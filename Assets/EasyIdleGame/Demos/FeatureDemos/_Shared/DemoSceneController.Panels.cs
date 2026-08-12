using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EasyIdleGame.Demos
{
    public abstract partial class DemoSceneController
    {
        protected void AddOverview()
        {
            VisualElement card = CreateCard("What this demo shows", false);
            card.style.minHeight = 0;
            card.style.paddingTop = 3;
            card.style.paddingBottom = 3;

            VisualElement header = card.Q<VisualElement>("CardHeader");
            Label label = card.Q<Label>("CardTitle");
            if (label != null)
            {
                label.RemoveFromHierarchy();
                label.style.fontSize = 12;
                label.style.marginBottom = 0;
                card.Add(label);
            }

            header?.RemoveFromHierarchy();

            string summaryText = readme != null ? readme.summary : string.Empty;
            if (!string.IsNullOrWhiteSpace(summaryText))
            {
                Label summary = new Label(summaryText);
                StyleText(summary, 11, TextColor);
                summary.style.whiteSpace = WhiteSpace.Normal;
                summary.style.marginTop = 1;
                card.Add(summary);
            }

            Button reset = CreateDemoButton("Reset", ResetDemoState, 0, 20);
            reset.style.alignSelf = Align.FlexStart;
            reset.style.marginTop = 4;
            reset.style.marginBottom = 0;
            card.Add(reset);
            dashboard.Add(card);
        }

        protected void AddCurrencyTotalsPanel()
        {
            List<Currency> currencies = LoadAllPrefixed<Currency>();
            if (currencies.Count == 0) return;

            VisualElement section = CreateSection("Currency Totals");
            foreach (Currency currency in currencies)
            {
                Currency currencyAsset = currency;
                VisualElement card = CreateObjectCard(currency.currencyName, "Currency", AccentColor, configurationObject: currencyAsset);
                AddLiveKeyValue(card, "Total", () => CurrencyManager.Instance.GetCurrencyAmount(currencyAsset).ToString(), AccentColor, true);
                AddLiveLimit(card, "Max", () => CurrencyManager.Instance.GetMaxCurrencyConstrain(currencyAsset));
                AddDefaultCurrencyAction(card, currencyAsset);
                AddSceneCurrencyActions(card, currencyAsset);
                section.Add(card);
            }
        }

        private void AddDefaultCurrencyAction(VisualElement card, Currency currency)
        {
            if (currency == null || !ShouldAddDefaultCurrencyAction(currency)) return;

            BigNumber amount = GetDefaultCurrencyGrantAmount(currency);
            if (amount <= 0) return;

            string currencyName = GetResourceNameWithoutPrefix(currency);
            AddCardAction(card, GetDefaultCurrencyGrantLabel(currency, amount), () => AddCurrency(currencyName, amount));
        }

        protected virtual bool ShouldAddDefaultCurrencyAction(Currency currency)
        {
            return ResourceNameEquals(currency, "Credits");
        }

        protected virtual BigNumber GetDefaultCurrencyGrantAmount(Currency currency)
        {
            return 100;
        }

        protected virtual string GetDefaultCurrencyGrantLabel(Currency currency, BigNumber amount)
        {
            return $"Add +{amount}";
        }

        protected virtual void AddSceneCurrencyActions(VisualElement card, Currency currency)
        {
        }

        protected void AddBusinessPanel(string title, BusinessPanelMode mode, params string[] businessNames)
        {
            VisualElement section = CreateSection(title);

            foreach (string businessName in businessNames)
            {
                Business business = LoadOptional<Business>(businessName);
                if (business == null) continue;

                BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
                VisualElement card = CreateObjectCard(GetBusinessCardTitle(business), "Business", AccentColor, configurationObject: business);
                AddBusinessRenderedData(card, holder);
                AddLiveKeyValue(card, "Owned", () => holder.amount.ToString(), () => holder.amount > 0 ? PositiveColor : WarningColor, true);

                switch (mode)
                {
                    case BusinessPanelMode.Inventory:
                        AddLiveKeyValue(card, "Bought", () => holder.boughtAmount.ToString());
                        if (HasLocks(business))
                        {
                            AddLiveKeyValue(card, "Unlocked", () => holder.iUnlockable.IsUnlocked() ? "yes" : "no", () => holder.iUnlockable.IsUnlocked() ? PositiveColor : WarningColor);
                            AddLiveKeyValue(card, "Requirements", () => DescribeBusinessLocks(business), () => holder.iUnlockable.IsUnlocked() ? PositiveColor : WarningColor);
                            if (UsesPlayerLevelLock(business))
                            {
                                AddLiveKeyValue(card, "Player level", DescribeCurrentPlayerLevel, () => PlayerStats.Instance && PlayerStats.Instance.GetPlayerLevel() > 0 ? PositiveColor : WarningColor);
                            }
                        }
                        if (business.businessGroups != null && business.businessGroups.Count > 0)
                        {
                            AddLiveKeyValue(card, "Shared cap", () => DescribeBusinessGroupCapacity(business), () => holder.GetRemainingAmountConstrain() > 0 ? TextColor : WarningColor);
                        }
                        if (ShouldShowInventoryBuyState(business))
                        {
                            AddLiveKeyValue(card, "Buy target", () => DescribeBusinessBuyTarget(business, BusinessesManager.Instance.GetTargetBusinessBuyAmount(business)));
                            AddLiveKeyValue(card, "Cost/target purchase", () =>
                            {
                                BigNumber target = BusinessesManager.Instance.GetTargetBusinessBuyAmount(business);
                                return DescribeBuyCost(holder.iBuyable, target > 0 ? target : 1);
                            }, () =>
                            {
                                BigNumber target = BusinessesManager.Instance.GetTargetBusinessBuyAmount(business);
                                return GetBuyCostColor(holder.iBuyable, target > 0 ? target : 1);
                            });
                            AddLiveKeyValue(card, "Buy status", () =>
                            {
                                BigNumber target = BusinessesManager.Instance.GetTargetBusinessBuyAmount(business);
                                return DescribeBuyAvailability(holder.iBuyable, target);
                            }, () =>
                            {
                                BigNumber target = BusinessesManager.Instance.GetTargetBusinessBuyAmount(business);
                                return CanBuyAmount(holder.iBuyable, target) ? PositiveColor : WarningColor;
                            });
                        }
                        break;
                    case BusinessPanelMode.Production:
                        AddKeyValue(card, "Mode", $"{(business.autoProduce ? "automatic" : "manual")} start, {(business.autoCollect ? "automatic collection" : "store until claimed")}", business.autoProduce || business.autoCollect ? AccentColor : TextColor);
                        AddProductionRuntimeState(card, holder, !CanAutoCollectOutput(holder));
                        AddDropTableVisualization(card, business.OutputTables, "production round");
                        break;
                    case BusinessPanelMode.ProductionCost:
                        AddKeyValue(card, "Group", DescribeGroups(business.businessGroups));
                        AddLiveKeyValue(card, "Cost/round", () => DescribeInputs(holder.iProducable.GetProductionCost(0, true)), () => GetInputCostColor(holder.iProducable.GetProductionCost(0, true)));
                        AddKeyValue(card, "Consume own copy", business.ConsumeOnProduce ? "on production finished" : "no", business.ConsumeOnProduce ? WarningColor : TextColor, business.ConsumeOnProduce);
                        AddKeyValue(card, "Partial batches", business.allowPartialProductionCost ? "enabled" : "disabled");
                        if (HasProductionCost(holder))
                        {
                            AddLiveKeyValue(card, "Available/required", () => DescribeInputProgress(holder.iProducable.GetProductionCost(0, true)), () => GetInputCostColor(holder.iProducable.GetProductionCost(0, true)));
                        }
                        AddProductionRuntimeState(card, holder, !CanAutoCollectOutput(holder));
                        break;
                    case BusinessPanelMode.BoostTarget:
                        AddKeyValue(card, "Group", DescribeGroups(business.businessGroups));
                        AddLiveKeyValue(card, "Output multiplier", () => $"x{holder.GetProductionMultiplier()}");
                        AddLiveKeyValue(card, "Speed multiplier", () => $"x{holder.GetSpeedMultiplier()}");
                        AddProductionRuntimeState(card, holder, !CanAutoCollectOutput(holder));
                        break;
                    case BusinessPanelMode.Automation:
                        Manager manager = business.manager;
                        ManagerHolder managerHolder = manager ? ManagersManager.Instance.GetOrAddHolder(manager) : null;
                        bool autoProduce = holder.IsAutoProductionPossible();
                        bool autoCollect = CanAutoCollectOutput(holder);
                        AddKeyValue(card, "Mode", $"{(business.autoProduce ? "built-in auto" : manager ? "manager gated" : "manual")} produce, {(autoCollect ? "auto collect" : "stored output")}", autoProduce || autoCollect ? AccentColor : WarningColor);
                        AddKeyValue(card, "Manager", manager ? manager.managerName : "none", managerHolder != null && managerHolder.level > 0 ? PositiveColor : MutedTextColor);
                        if (managerHolder != null)
                        {
                            AddKeyValue(card, "Manager thresholds", $"current L{managerHolder.level:0}; auto-start L{manager.autoProduceOnLevel}, auto-collect L{manager.autoCollectOnLevel}", managerHolder.level > 0 ? PositiveColor : WarningColor);
                        }
                        AddKeyValue(card, "Auto start", autoProduce ? "enabled" : "locked", autoProduce ? PositiveColor : WarningColor, autoProduce);
                        AddKeyValue(card, "Auto collect", autoCollect ? "enabled" : "locked", autoCollect ? PositiveColor : WarningColor, autoCollect);
                        AddProductionRuntimeState(card, holder, !CanAutoCollectOutput(holder));
                        break;
                    case BusinessPanelMode.Progression:
                        AddLiveKeyValue(card, "Bought", () => holder.boughtAmount.ToString());
                        AddLiveKeyValue(card, "Cost/purchase", () => DescribeBuyCost(holder.iBuyable, 1), () => GetBuyCostColor(holder.iBuyable, 1));
                        AddLiveKeyValue(card, "Buy status", () => DescribeBusinessBuyAvailability(businessName, 1), () => CanBuyBusinessAmount(businessName, 1) ? PositiveColor : WarningColor);
                        AddLiveKeyValue(card, "Output multiplier", () => $"x{holder.GetProductionMultiplier()}");
                        AddLiveKeyValue(card, "Speed multiplier", () => $"x{holder.GetSpeedMultiplier()}");
                        AddKeyValue(card, "XP on buy", business.xpsAddPerPurchaseToPlayer.ToString());
                        AddLiveKeyValue(card, "Level", () => $"{holder.Level.currentLevel}, xp {holder.Level.currentXp}");
                        AddProductionRuntimeState(card, holder, !CanAutoCollectOutput(holder));
                        break;
                    case BusinessPanelMode.Offline:
                        AddKeyValue(card, "Offline role", business.autoProduce && business.autoCollect ? "applies directly" : "stockpiles");
                        AddProductionRuntimeState(card, holder, !CanAutoCollectOutput(holder));
                        break;
                }

                AddSceneBusinessDetails(card, businessName, mode);
                AddBusinessActions(card, businessName, mode);
                section.Add(card);
            }
        }

        protected virtual bool ShouldShowInventoryBuyState(Business business)
        {
            return true;
        }

        private void AddBusinessRenderedData(VisualElement card, BusinessHolder holder)
        {
            if (holder == null || holder.business == null || holder.business.upgradeLevels == null || holder.business.upgradeLevels.Length == 0) return;

            Label title = card.Q<Label>("CardTitle");
            void RefreshTitle()
            {
                if (title != null)
                {
                    title.text = GetBusinessRuntimeDisplayName(holder);
                }
            }

            RefreshTitle();
            liveUiRefreshers.Add(RefreshTitle);
        }

        private bool HasProductionCost(BusinessHolder holder)
        {
            List<Input> cost = holder?.iProducable.GetProductionCost(0, true);
            return cost != null && cost.Count > 0;
        }

        protected void AddProductionRuntimeState(VisualElement card, BusinessHolder holder, bool showStoredOutputs)
        {
            AddLiveKeyValue(card, "Active", () => holder.ActiveProductions.Count == 0 ? "idle" : $"{holder.CurrentlyProducing} producing", () => holder.ActiveProductions.Count == 0 ? InactiveColor : AccentColor);
            AddLiveProgress(card, holder.GetProductionProgress);
            if (showStoredOutputs)
            {
                AddLiveKeyValue(card, "Stored to claim", () => DescribeStoredOutputs(holder));
            }
            AddLiveKeyValue(card, "Round duration", () => $"{holder.iProducable.GetActualTimeToProduce():0.##}s");
            AddLiveKeyValue(card, "Average output", () => $"{DescribeOutputs(GetAverageOutputPerRound(holder))}/round ({DescribeOutputs(holder.iProducable.GetActualProductionPerSecond(true))}/sec)");
        }

        private List<Output> GetAverageOutputPerRound(BusinessHolder holder)
        {
            if (holder == null) return new List<Output>();

            return ProductionOutputCalculator.CalculateAverageProduction(
                holder,
                holder.amount,
                ProductionOutputCalculationMode.OptimisticPreview,
                0,
                true);
        }

        protected virtual void AddBusinessActions(VisualElement card, string businessName, BusinessPanelMode mode)
        {
            if (UsesProductionControls(mode))
            {
                Business business = Load<Business>(businessName);
                BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
                AddCardAction(card, "Start all", () => StartAllOwnedBusinessCopies(businessName), () => CanStartAnyBusinessProduction(businessName), false, () => DescribeStartAllProductionAvailability(businessName));
                if (!CanAutoCollectOutput(holder))
                {
                    AddCardAction(card, "Claim", () => ClaimBusinessOutputs(businessName), () => HasStoredBusinessOutputs(businessName), false, () => DescribeClaimAvailability(businessName));
                }
            }

            AddSceneBusinessActions(card, businessName, mode);
        }

        protected virtual void AddSceneBusinessDetails(VisualElement card, string businessName, BusinessPanelMode mode)
        {
        }

        protected virtual void AddSceneBusinessActions(VisualElement card, string businessName, BusinessPanelMode mode)
        {
        }

        protected bool UsesProductionControls(BusinessPanelMode mode)
        {
            return mode == BusinessPanelMode.Production
                || mode == BusinessPanelMode.ProductionCost
                || mode == BusinessPanelMode.BoostTarget
                || mode == BusinessPanelMode.Automation
                || mode == BusinessPanelMode.Offline
                || mode == BusinessPanelMode.Progression;
        }

        protected void AddRecipePanel(string title, params string[] recipeNames)
        {
            VisualElement section = CreateSection(title);

            foreach (string recipeName in recipeNames)
            {
                BusinessMergeRecipe recipe = LoadOptional<BusinessMergeRecipe>(recipeName);
                if (recipe == null) continue;

                string recipeTitle = string.IsNullOrWhiteSpace(recipe.Metadata?.name)
                    ? GetAssetRoleName(recipe.name)
                    : recipe.Metadata.name;
                VisualElement card = CreateObjectCard(recipeTitle, RecipeObjectType, AccentColor, configurationObject: recipe);
                AddLiveKeyValue(card, $"Inputs/{RecipeTriggerName}", () => DescribeInputs(recipe.inputs), () => GetInputCostColor(recipe.inputs));
                AddLiveKeyValue(card, "Inputs held/needed", () => DescribeInputProgress(recipe.inputs), () => GetInputCostColor(recipe.inputs));
                if (ShowMergeRecipeReadiness)
                {
                    AddLiveKeyValue(card, "Merge status", () => DescribeMergeAvailability(recipe), () => BusinessesManager.Instance.CanMergeBusinesses(recipe) ? PositiveColor : WarningColor);
                }
                AddKeyValue(card, "Reward selection", DescribeDropTableStrategies(recipe.outputTables));
                AddKeyValue(card, $"Average output/{RecipeTriggerName}", DescribeOutputs(recipe.outputTables.GetAverageOutputs(1)));
                AddDropTableVisualization(card, recipe.outputTables, RecipeTriggerName);
                AddRecipeActions(card, recipeName);
                section.Add(card);
            }
        }

        protected virtual string RecipeObjectType => "Merge Recipe";

        protected virtual string RecipeTriggerName => "merge";

        protected virtual bool ShowMergeRecipeReadiness => false;

        protected virtual void AddRecipeActions(VisualElement card, string recipeName)
        {
        }

        protected void AddBoostPanel()
        {
            VisualElement section = CreateSection("Boosts");

            foreach (Boost boost in LoadAllPrefixed<Boost>())
            {
                if (!ShouldShowBoost(boost)) continue;

                BoostHolder holder = BoostsManager.Instance.GetOrAddHolder(boost);
                VisualElement card = CreateObjectCard(boost.boostName, "Boost", AccentColor, configurationObject: boost);
                AddLiveKeyValue(card, "Inventory", () => holder.amount.ToString(), () => holder.amount > 0 ? PositiveColor : WarningColor, holder.amount > 0);

                GenericMultiplierBoost multiplierBoost = boost as GenericMultiplierBoost;
                if (multiplierBoost != null)
                {
                    AddKeyValue(card, "Effect", DescribeUpgradeBlocks(multiplierBoost.upgradeBlocks));
                    AddKeyValue(card, "Duration", $"{multiplierBoost.duration.TotalSeconds:0}s");
                    if (multiplierBoost.targetBusinessGroups != null && multiplierBoost.targetBusinessGroups.Count > 0)
                    {
                        AddKeyValue(card, "Targets", DescribeGroups(multiplierBoost.targetBusinessGroups));
                    }
                }

                AutoProduceBoost autoProduceBoost = boost as AutoProduceBoost;
                if (autoProduceBoost != null)
                {
                    AddKeyValue(card, "Effect", "auto production");
                    AddKeyValue(card, "Duration", $"{autoProduceBoost.duration.TotalSeconds:0}s");
                    if (autoProduceBoost.targetBusinessGroups != null && autoProduceBoost.targetBusinessGroups.Count > 0)
                    {
                        AddKeyValue(card, "Targets", DescribeGroups(autoProduceBoost.targetBusinessGroups));
                    }
                }

                Boost boostAsset = boost;
                AddCardAction(card, "Grant +1", () => GrantBoost(boostAsset, 1));
                AddCardAction(card, "Use", () => UseBoost(boostAsset), () => CanUseBoost(boostAsset), false, () => DescribeBoostUseAvailability(boostAsset, 1));
                section.Add(card);
            }

            VisualElement activeCard = CreateObjectCard("Active Boosts", "Runtime State", MutedTextColor);
            AddLiveKeyValue(activeCard, "Active", () => BoostsManager.Instance.activeBoosts.Count == 0 ? "none" : $"{BoostsManager.Instance.activeBoosts.Count} active", () => BoostsManager.Instance.activeBoosts.Count == 0 ? InactiveColor : AccentColor);
            AddLiveKeyValue(activeCard, "Timers", DescribeActiveBoostTimers, () => BoostsManager.Instance.activeBoosts.Count == 0 ? MutedTextColor : AccentColor);
            AddLiveMessage(activeCard, "Last boost event", () => LastBoostResult);
            section.Add(activeCard);

            if (LoadAllPrefixed<Boost>().Any(boost => boost is TimeSkipBoost))
            {
                VisualElement skipCard = CreateObjectCard("Time Skip Event", "Time Skip", AccentColor);
                AddLiveMessage(skipCard, "Last time skip", () => LastTimeSkipResult);
                AddLiveMessage(skipCard, "Last profit event", () => LastProfitResult);
                void RefreshSkipCard()
                {
                    skipCard.style.display = HasRuntimeMessage(LastTimeSkipResult) || HasRuntimeMessage(LastProfitResult)
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                }

                RefreshSkipCard();
                liveUiRefreshers.Add(RefreshSkipCard);
                section.Add(skipCard);
            }
        }

        protected virtual bool ShouldShowBoost(Boost boost)
        {
            return true;
        }

        protected void AddUpgradePanel()
        {
            VisualElement section = CreateSection("Upgrades And Overrides");

            foreach (Upgrade upgrade in LoadAllPrefixed<Upgrade>())
            {
                if (!ShouldShowUpgrade(upgrade)) continue;

                UpgradeHolder holder = UpgradesManager.Instance.GetOrAddHolder(upgrade);
                VisualElement card = CreateObjectCard(upgrade.upgradeName, "Upgrade", AccentColor, configurationObject: upgrade);
                AddLiveKeyValue(card, "Owned", () => holder.amount.ToString(), () => holder.amount > 0 ? PositiveColor : WarningColor, holder.amount > 0);
                if (upgrade.upgradeDurationInSeconds > 0f)
                {
                    AddKeyValue(card, "Delay", $"{upgrade.upgradeDurationInSeconds:0.#}s, {UpgradesManager.Instance.queueMode}");
                    AddLiveKeyValue(card, "Pending", () => DescribePendingUpgrades(holder), () => holder.pendingUpgrades.Count > 0 ? AccentColor : MutedTextColor, holder.pendingUpgrades.Count > 0);
                    AddLiveProgress(card, () => GetPendingUpgradeProgress(upgrade, holder));
                }
                AddLiveKeyValue(card, "Cost/purchase", () => DescribeBuyCost(holder.iBuyable, 1), () => GetBuyCostColor(holder.iBuyable, 1));
                AddLiveKeyValue(card, "Buy status", () => DescribeUpgradeBuyAvailability(upgrade), () => CanBuyAmount(holder.iBuyable, 1) ? PositiveColor : WarningColor);
                string multipliers = DescribeUpgradeBlocks(upgrade.upgrades);
                string overrides = DescribeOverrideBlocks(upgrade.overrideUpgrades);
                string overrideModifiers = DescribeOverrideModifiers(upgrade.overrideUpgradeModifiers);
                if (multipliers != "none") AddKeyValue(card, "Effect", multipliers);
                if (overrides != "none") AddKeyValue(card, "Overrides", overrides);
                if (overrideModifiers != "none") AddKeyValue(card, "Override mods", overrideModifiers);
                if (upgrade.targetBusinessGroups != null && upgrade.targetBusinessGroups.Count > 0)
                {
                    AddKeyValue(card, "Targets", DescribeGroups(upgrade.targetBusinessGroups));
                }
                Upgrade upgradeAsset = upgrade;
                AddCardAction(card, "Buy", () => BuyUpgrade(upgradeAsset), () => CanBuyUpgrade(upgradeAsset), false, () => DescribeUpgradeBuyAvailability(upgradeAsset), false);
                AddSceneUpgradeActions(card, upgradeAsset);
                section.Add(card);
            }

            VisualElement eventCard = CreateObjectCard("Last Upgrade", "Runtime State", MutedTextColor);
            AddLiveMessage(eventCard, "Result", () => LastUpgradeResult, true);
            section.Add(eventCard);

            Currency credits = LoadOptional<Currency>("Credits");
            if (credits != null)
            {
                VisualElement card = CreateObjectCard("Resolved Currency Cap", "Resolved Value", MutedTextColor);
                AddLiveLimit(card, credits.currencyName, () => CurrencyManager.Instance.GetMaxCurrencyConstrain(credits), true);
                section.Add(card);
            }
        }

        protected void AddShopPanel()
        {
            VisualElement section = CreateSection("Shop Reward Events");

            foreach (ShopItem item in LoadAllPrefixed<ShopItem>())
            {
                ShopItemHolder holder = ShopManager.Instance.GetOrAddHolder(item);
                VisualElement card = CreateObjectCard(item.metadata.name, "Shop Item", AccentColor, configurationObject: item);
                AddLiveKeyValue(card, "Owned", () => holder.amount.ToString(), () => holder.amount > 0 ? PositiveColor : WarningColor, holder.amount > 0);
                AddKeyValue(card, "Purchase method", item.buyWithAd ? "rewarded-ad callback" : string.IsNullOrEmpty(item.productId) ? "external product id not set" : item.productId);
                if (item.maxPurchases > 0)
                {
                    AddKeyValue(card, "Purchase limit", item.maxPurchases.ToString());
                    AddLiveKeyValue(card, "Purchases left", () => holder.iBuyable.Maxed ? "0 (limit reached)" : item.GetMaxBuyableAmount(holder.iBuyable.GetGeometricCurrentAmount()).ToString(), () => holder.iBuyable.Maxed ? WarningColor : PositiveColor);
                }
                AddLiveKeyValue(card, "Unlocked", () => holder.iUnlockable.IsUnlocked() ? "yes" : "no", () => holder.iUnlockable.IsUnlocked() ? PositiveColor : WarningColor);
                if (item.inGameCost != null && item.inGameCost.Count > 0)
                {
                    AddLiveKeyValue(card, "Cost/purchase", () => DescribeBuyCost(holder.iBuyable, 1), () => GetBuyCostColor(holder.iBuyable, 1));
                }
                AddKeyValue(card, "Rewards/completion", DescribeDropTables(item.rewardTables));
                AddLiveMessage(card, "Last reward", () => LastShopRewardResult);
                AddLiveKeyValue(card, "Purchase status", () => item.buyWithAd ? DescribeShopAdAvailability(item) : DescribeShopCompleteAvailability(item), () => item.buyWithAd ? CanAttemptRewardAd(item) ? PositiveColor : WarningColor : CanCompleteShopPurchase(item) ? PositiveColor : WarningColor);
                AddShopItemActions(card, item);
                section.Add(card);
            }
        }

        protected virtual void AddShopItemActions(VisualElement card, ShopItem item)
        {
        }

        protected virtual void AddSceneUpgradeActions(VisualElement card, Upgrade upgrade)
        {
        }

        protected void AddProfitPanel()
        {
            VisualElement section = CreateSection("Profit Summary");

            VisualElement calculated = CreateObjectCard("Calculated ProfitData", "Profit Data", AccentColor);
            AddLiveKeyValue(calculated, "Simulated time", () => LastCalculatedProfit == null ? "not calculated" : LastCalculatedProfit.ElapsedTime());
            AddLiveKeyValue(calculated, "Generated over period", () => LastCalculatedProfit == null ? "none" : DescribeProfit(LastCalculatedProfit));
            if (ShowManualProfitControls)
            {
                AddLiveKeyValue(calculated, "Automatic producer", DescribeOfflineProfitEligibility,
                    () => HasOfflineProfitEligibleBusiness() ? PositiveColor : WarningColor);
                AddIntSlider(calculated, "Minutes", 5, 180, OfflineMinutes, value => OfflineMinutes = value);
                AddCardAction(calculated, "Calculate", CalculateOfflineProfit,
                    HasOfflineProfitEligibleBusiness, false, DescribeOfflineProfitEligibility);
            }
            section.Add(calculated);

            VisualElement applied = CreateObjectCard("Last ProfitApplicationSummary", "Profit Summary", AccentColor);
            AddLiveKeyValue(applied, "Added to balances", () => LastProfitSummary == null ? "none" : DescribeSummary(LastProfitSummary));
            AddLiveKeyValue(applied, "Stockpiles", () => LastProfitSummary == null ? "none" : DescribeStockpiles(LastProfitSummary.AppliedStockpiles));
            AddLiveMessage(applied, "Result", () => LastProfitResult);
            if (ShowManualProfitControls)
            {
                AddCardAction(applied, "Apply calculated", ApplyCalculatedProfit);
            }
            section.Add(applied);
        }

        protected virtual bool ShowManualProfitControls => false;

        private bool HasOfflineProfitEligibleBusiness()
        {
            return BusinessesManager.Instance && BusinessesManager.Instance.holders.Any(holder =>
                holder != null && holder.business != null && holder.amount > 0 && holder.IsAutoProductionPossible());
        }

        private string DescribeOfflineProfitEligibility()
        {
            return HasOfflineProfitEligibleBusiness()
                ? "ready"
                : "none; activate an Auto Produce Boost, enable Auto Produce on the Business, or unlock its automation Manager";
        }

        protected void AddSaveEventPanel()
        {
            VisualElement section = CreateSection("Save-Worthy Events");
            VisualElement card = CreateObjectCard("EconomyChangedEvents.OnSaveWorthyStateChanged", "Event Source", AccentColor);
            AddKeyValue(card, "Debounce", "first save signal per frame is dispatched; later same-frame signals are suppressed");
            AddLiveMessage(card, "Last source", () => LastSaveEventSource);
            AddLiveKeyValue(card, "Save requests", () => saveEventCount.ToString(), () => saveEventCount > 0 ? AccentColor : MutedTextColor, true);
            AddLiveKeyValue(card, "Suppressed this frame", () => EconomyChangedEvents.SuppressedSaveWorthyChangesThisFrame.ToString(), () => EconomyChangedEvents.SuppressedSaveWorthyChangesThisFrame > 0 ? WarningColor : MutedTextColor);
            AddLiveKeyValue(card, "Sources this frame", () => EconomyChangedEvents.SaveWorthySourcesThisFrame);
            AddLiveKeyValue(card, "Recent sources", () => saveEventSources.Count == 0 ? "none" : string.Join(" <- ", saveEventSources.Take(6)));
            AddCardAction(card, "Buy +1", () => BuyBusiness("TrackedBusiness", 1), () => CanBuyBusinessAmount("TrackedBusiness", 1), false, () => DescribeBusinessBuyAvailability("TrackedBusiness", 1), false);
            AddCardAction(card, "Produce now", () => ProduceBusinessNow("TrackedBusiness"), () => CanStartAnyBusinessProduction("TrackedBusiness"), false, () => DescribeStartAllProductionAvailability("TrackedBusiness"));
            AddCardAction(card, "Travel", () => Travel("TrackedLocation"), () => CanTravelTo(LoadOptional<Location>("TrackedLocation")), false, () => DescribeTravelAvailability(LoadOptional<Location>("TrackedLocation")));
            section.Add(card);
        }

        protected void AddLocationPanel()
        {
            VisualElement section = CreateSection("Locations");
            List<Location> all = LocationsManager.Instance.GetAllLocations<Location>()
                .Where(IsResourceInCurrentDemo)
                .ToList();

            VisualElement summary = CreateObjectCard("Current Location", "Location State", AccentColor);
            AddLiveKeyValue(summary, "Active", () => LocationsManager.Instance.ActiveLocation ? LocationsManager.Instance.ActiveLocation.metadata.name : "None", AccentColor, true);
            AddLiveMessage(summary, "Last location event", () => LastLocationResult);
            section.Add(summary);

            foreach (Location location in all)
            {
                LocationHolder holder = LocationsManager.Instance.GetOrAddHolder(location);
                VisualElement card = CreateObjectCard(location.metadata.name, "Location", AccentColor, configurationObject: location);
                AddLiveKeyValue(card, "Unlocked", () => holder.AlreadyUnlocked ? "yes" : "no", () => holder.AlreadyUnlocked ? PositiveColor : WarningColor, holder.AlreadyUnlocked);
                if (location.locks != null && location.locks.Count > 0)
                {
                    AddLiveKeyValue(card, "Locks", () => DescribeLocks(location.locks), () => holder.iUnlockable.IsUnlocked() ? PositiveColor : WarningColor);
                }
                AddLiveKeyValue(card, "Cost/travel", () => DescribeInputs(location.cost), () => GetInputCostColor(location.cost));
                AddLiveKeyValue(card, "Cost held/needed", () => DescribeInputProgress(location.cost), () => GetInputCostColor(location.cost));
                AddLiveKeyValue(card, "Travel status", () => DescribeTravelAvailability(location), () => CanTravelTo(location) ? PositiveColor : WarningColor);
                AddKeyValue(card, "Starter rewards/first visit", DescribeDropTables(location.starterOutputTables));
                Location locationAsset = location;
                AddCardAction(card, "Travel", () => Travel(locationAsset), () => CanTravelTo(locationAsset), false, () => DescribeTravelAvailability(locationAsset), false);
                AddSceneLocationActions(card, locationAsset);
                section.Add(card);
            }
        }

        protected virtual void AddSceneLocationActions(VisualElement card, Location location)
        {
        }

        protected void AddAdvancedProductionPanel()
        {
            VisualElement section = CreateSection("Production Cost Details");

            Business assembly = LoadOptional<Business>("AssemblyLine");

            if (assembly != null)
            {
                BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(assembly);
                VisualElement card = CreateObjectCard("Affordable Production", "Production Cost", AccentColor, configurationObject: assembly);
                AddLiveKeyValue(card, "Copies requested/round", () => holder.ProductionAmount.ToString());
                AddLiveKeyValue(card, "Copies affordable now", () => assembly.iProducable.GetAffordableProductionAmount(holder.ProductionAmount).ToString(), () => assembly.iProducable.GetAffordableProductionAmount(holder.ProductionAmount) > 0 ? PositiveColor : WarningColor);
                AddKeyValue(card, "Scale cost with batch size", assembly.scaleProductionCostWithAmount ? "yes" : "no");
                AddKeyValue(card, "Allow partial batches", assembly.allowPartialProductionCost ? "yes" : "no");
                AddLiveKeyValue(card, "Cost per full round", () => DescribeInputs(assembly.iProducable.GetProductionCost(holder.ProductionAmount)), () => GetInputCostColor(assembly.iProducable.GetProductionCost(holder.ProductionAmount)));
                AddLiveKeyValue(card, "Production status", () => DescribeStartProductionAvailability("AssemblyLine", holder.ProductionAmount), () => CanStartBusinessProduction("AssemblyLine", holder.ProductionAmount) ? PositiveColor : WarningColor);
                section.Add(card);
            }
        }

        protected virtual void SeedProductionCostInputs()
        {
            Log("No production-cost input seed action is configured for this demo.");
        }

        protected void AddBusinessGroupTargetingPanel()
        {
            VisualElement section = CreateSection("Targeted Multiplier Check");
            Business grouped = LoadOptional<Business>("GroupedBusiness");
            Business ungrouped = LoadOptional<Business>("UngroupedBusiness");
            GenericMultiplierBoost boost = LoadOptional<GenericMultiplierBoost>("TargetedBoost");

            if (boost != null && grouped != null && ungrouped != null)
            {
                VisualElement card = CreateObjectCard("Targeted Multiplier", "Business Group", AccentColor, configurationObject: boost);
                AddKeyValue(card, "Boost targets", DescribeGroups(boost.targetBusinessGroups));
                AddKeyValue(card, $"{grouped.businessName} output", $"x{boost.iMultiplierProvider.GetMultiplierOfType(UpgradeType.production, grouped.businessGroups)}");
                AddKeyValue(card, $"{ungrouped.businessName} output", $"x{boost.iMultiplierProvider.GetMultiplierOfType(UpgradeType.production, ungrouped.businessGroups)}");
                AddKeyValue(card, "Expected", "grouped changes, ungrouped stays x1", AccentColor, true);
                section.Add(card);
            }
        }

        protected void AddLocksAndBuyAmountsPanel()
        {
            VisualElement section = CreateSection("Scene Controls And Shared Caps");

            VisualElement buyCard = CreateCard("Buy Amount Controls");
            AddLiveKeyValue(buyCard, "Current buy setting", () => BusinessesManager.Instance.GetBuyAmount().ToString(), AccentColor, true);
            AddKeyValue(buyCard, "Affects", "the Buy target row on each business card");
            AddCardAction(buyCard, "Cycle buy", CycleBuyAmount);
            section.Add(buyCard);

            foreach (BusinessGroup group in LoadAllPrefixed<BusinessGroup>())
            {
                VisualElement card = CreateObjectCard(group.metadata.name, "Business Group", AccentColor, configurationObject: group);
                AddLiveKeyValue(
                    card,
                    IsUnlimited(group.capacity) ? "Used" : "Used / cap",
                    () =>
                    {
                        BigNumber used = BusinessesManager.Instance.GetBusinessGroupUsedCapacity(group);
                        return IsUnlimited(group.capacity) ? used.ToString() : $"{used}/{DescribeLimit(group.capacity)}";
                    });
                section.Add(card);
            }

            foreach (CurrencyGroup group in LoadAllPrefixed<CurrencyGroup>())
            {
                VisualElement card = CreateObjectCard(group.metadata.name, "Currency Group", AccentColor, configurationObject: group);
                AddLiveKeyValue(
                    card,
                    IsUnlimited(group.capacity) ? "Used" : "Used / cap",
                    () =>
                    {
                        BigNumber used = CurrencyManager.Instance.GetCurrencyGroupUsedCapacity(group);
                        return IsUnlimited(group.capacity) ? used.ToString() : $"{used}/{DescribeLimit(group.capacity)}";
                    });
                section.Add(card);
            }
        }

        protected virtual bool ShouldShowUpgrade(Upgrade upgrade)
        {
            return true;
        }

        protected void AddManagerAutomationPanel()
        {
            VisualElement section = CreateSection("Managers And Automation");

            foreach (Manager manager in LoadAllPrefixed<Manager>())
            {
                ManagerHolder holder = ManagersManager.Instance.GetOrAddHolder(manager);
                VisualElement card = CreateObjectCard(manager.managerName, "Manager", AccentColor, configurationObject: manager);
                AddLiveKeyValue(card, "Copies owned", () => holder.totalCount.ToString(), () => holder.level > 0 ? PositiveColor : WarningColor, holder.level > 0);
                AddLiveKeyValue(card, "Level", () => $"{holder.level:0}", () => holder.level > 0 ? PositiveColor : WarningColor, holder.level > 0);
                AddLiveKeyValue(card, "Unlocked", () => holder.iUnlockable.IsUnlocked() ? "yes" : "no", () => holder.iUnlockable.IsUnlocked() ? PositiveColor : WarningColor);
                AddLiveKeyValue(card, "Cost/purchase", () => DescribeBuyCost(holder.iBuyable, 1), () => GetBuyCostColor(holder.iBuyable, 1));
                AddLiveKeyValue(card, "Buy status", () => DescribeManagerBuyAvailability(manager), () => CanBuyAmount(holder.iBuyable, 1) ? PositiveColor : WarningColor);
                AddKeyValue(card, "Automation thresholds", $"auto-start at L{manager.autoProduceOnLevel}; auto-collect at L{manager.autoCollectOnLevel}");
                string passive = DescribeUpgradeBlocks(manager.passiveBoosts);
                string active = DescribeUpgradeBlocks(manager.activeUpgrade);
                if (passive != "none") AddKeyValue(card, "Passive", passive);
                if (active != "none") AddKeyValue(card, "Active", active);
                AddLiveKeyValue(card, "Timer", () => holder.IsActive ? $"active {holder.activeTimeLeft:0.0}s" : holder.IsOnCooldown ? $"cooldown {holder.cooldownTimeLeft:0.0}s" : "ready", holder.IsActive ? AccentColor : holder.IsOnCooldown ? WarningColor : PositiveColor);
                AddLiveMessage(card, "Result", () => LastManagerResult);
                Manager managerAsset = manager;
                AddCardAction(card, "Buy", () => BuyAutomationManager(managerAsset), () => CanBuyManager(managerAsset), false, () => DescribeManagerBuyAvailability(managerAsset), false);
                AddCardAction(card, "Grant +1", () => GrantAutomationManagerLevel(managerAsset));
                AddCardAction(card, "Activate", () => ActivateAutomationManager(managerAsset), () => CanActivateManager(managerAsset), false, () => DescribeManagerActivationAvailability(managerAsset));
                AddCardAction(card, "Tick 15s", () => TickManagersAndBusinesses(15f));
                section.Add(card);
            }
        }

        protected void AddPlayerProgressionPanel()
        {
            VisualElement section = CreateSection("Player Progression And Achievements");

            VisualElement playerCard = CreateObjectCard("Player Stats", "Player Level", AccentColor);
            AddLiveKeyValue(playerCard, "Level", () => PlayerStats.Instance.GetPlayerLevel().ToString(), AccentColor, true);
            AddLiveKeyValue(playerCard, "Current XP", DescribePlayerXp);
            AddLiveKeyValue(playerCard, "Unclaimed level rewards", () => PlayerStats.Instance.level.unclaimedLevelups.ToString(), () => PlayerStats.Instance.level.unclaimedLevelups > 0 ? AccentColor : MutedTextColor);
            AddLiveMessage(playerCard, "Last player event", () => LastPlayerStatsResult);
            AddIntSlider(playerCard, "XP to add", 10, 500, PlayerXpGrantAmount, value => PlayerXpGrantAmount = value);
            AddLiveCardAction(playerCard, () => $"Add +{PlayerXpGrantAmount} XP", () => AddPlayerXp(PlayerXpGrantAmount));
            section.Add(playerCard);

            foreach (CustomStat stat in LoadAllPrefixed<CustomStat>())
            {
                VisualElement card = CreateObjectCard(stat.metadata.name, "Custom Stat", AccentColor, configurationObject: stat);
                AddLiveKeyValue(card, "Value", () => PlayerStats.Instance.GetStat(stat).ToString(), () => PlayerStats.Instance.GetStat(stat) > 0 ? AccentColor : MutedTextColor, PlayerStats.Instance.GetStat(stat) > 0);
                if (!string.IsNullOrEmpty(stat.metadata.description)) AddKeyValue(card, "Description", stat.metadata.description);
                CustomStat statAsset = stat;
                AddCardAction(card, "Add +1", () => IncrementCustomStat(statAsset, 1));
                section.Add(card);
            }

            foreach (Achievement achievement in LoadAllPrefixed<Achievement>())
            {
                AchievementHolder holder = AchievementsManager.Instance.GetOrAddHolder(achievement);
                VisualElement card = CreateObjectCard(achievement.metadata.name, "Achievement", AccentColor, configurationObject: achievement);
                AddLiveKeyValue(card, "Unlocked", () => holder.iUnlockable.IsUnlocked() ? "yes" : "no", () => holder.iUnlockable.IsUnlocked() ? PositiveColor : WarningColor);
                AddLiveKeyValue(card, "Claimed tiers", () => $"{holder.ClaimedAmount}/{holder.TotalClaimAmount}", () => holder.IsFullyClaimed ? PositiveColor : MutedTextColor);
                AddLiveKeyValue(card, "Requirements", () => DescribeLocks(holder.GetCurrentTierLocks()), () => holder.iUnlockable.IsUnlocked() ? PositiveColor : WarningColor);
                AddKeyValue(card, "Rewards/claim", DescribeDropTables(achievement.rewardTables));
                AddLiveMessage(card, "Result", () => LastAchievementResult);
                Achievement achievementAsset = achievement;
                AddCardAction(card, "Claim", () => ClaimDemoAchievement(achievementAsset), () => CanClaimAchievement(achievementAsset), false, () => DescribeAchievementClaimAvailability(achievementAsset));
                section.Add(card);
            }
        }

        protected void AddDailyRewardsPanel()
        {
            VisualElement section = CreateSection("Daily Rewards");
            DailyRewardsManager manager = DailyRewardsManager.Instance;
            lastRenderedDailyDay = manager.GetCurrentDay();

            VisualElement summary = CreateObjectCard("Calendar Summary", "Daily Calendar", AccentColor, configurationObject: manager);
            AccentCard(summary, AccentColor);
            AddLiveKeyValue(summary, "Current day", () => manager.GetCurrentDay().ToString(), AccentColor, true);
            AddLiveKeyValue(summary, "Time to next reward", () => FormatRewardCountdown(manager.GetTimeToNextDay()), AccentColor, true);
            AddKeyValue(summary, "Day length in this demo", $"{manager.dayLengthHours * 3600f:0}s");
            AddLiveKeyValue(summary, "Claimed days", () => manager.claimedDays.Count == 0 ? "none" : string.Join(", ", manager.claimedDays));
            AddLiveMessage(summary, "Result", () => LastDailyRewardResult);
            AddCardAction(summary, "Next day", SimulateNextRewardDay);
            section.Add(summary);

            foreach (DailyRewardHolder holder in manager.GetCalendarView(2, 4))
            {
                Reward rewardConfig = ResolveDailyRewardConfig(manager, holder.day);
                VisualElement card = CreateObjectCard("Day " + holder.day, "Daily Reward", holder.isAvailableToday ? AccentColor : holder.isMissed ? WarningColor : MutedTextColor, configurationObject: rewardConfig);
                if (holder.isAvailableToday) AccentCard(card, AccentColor);
                else if (holder.isMissed) AccentCard(card, WarningColor);
                AddLiveKeyValue(card, "State", () => holder.isClaimed ? "claimed" : holder.isAvailableToday ? "available today" : holder.isMissed ? "missed" : "upcoming", () => holder.isClaimed ? PositiveColor : holder.isAvailableToday ? AccentColor : holder.isMissed ? WarningColor : MutedTextColor, holder.isAvailableToday || holder.isClaimed);
                AddKeyValue(card, "Config", rewardConfig ? rewardConfig.name : "generated calendar placeholder");
                AddKeyValue(card, "Expected reward/claim", DescribeOutputs(holder.expectedOutputs));
                AddLiveKeyValue(card, "Claim status", () => DescribeDailyRewardClaimAvailability(holder), () => CanClaimDailyReward(holder) ? PositiveColor : WarningColor);
                int day = holder.day;
                DailyRewardHolder dailyHolder = holder;
                AddCardAction(card, "Claim", () => ClaimDailyReward(day), () => CanClaimDailyReward(dailyHolder), false, () => DescribeDailyRewardClaimAvailability(dailyHolder), false);
                section.Add(card);
            }
        }

        private Reward ResolveDailyRewardConfig(DailyRewardsManager manager, int day)
        {
            if (manager == null || day <= 0) return null;

            DailyRewardHolder explicitReward = manager.dailyRewards?.FirstOrDefault(holder => holder != null && holder.day == day && holder.reward != null);
            if (explicitReward != null)
            {
                return explicitReward.reward;
            }

            LevelRewardHolder repeatingReward = manager.eachDayReward?.FirstOrDefault(holder => holder != null && holder.level > 0 && day % holder.level == 0 && holder.levelReward != null);
            return repeatingReward?.levelReward;
        }

        protected void AddPrestigePanel()
        {
            VisualElement section = CreateSection("Prestige");
            PrestigeManager manager = PrestigeManager.Instance;
            Currency relics = LoadOptional<Currency>("Relics");

            VisualElement card = CreateObjectCard("Prestige Flow", "Prestige", AccentColor);
            AddCardSeparator(card, "Run state");
            AddLiveKeyValue(card, "Can prestige", () => manager.CanPrestige() ? "yes" : "no", () => manager.CanPrestige() ? PositiveColor : WarningColor, manager.CanPrestige());
            AddLiveKeyValue(card, "Prestige count", () => manager.prestiges.prestigeCount.ToString(), AccentColor, true);
            AddCardSeparator(card, "Requirements and rewards");
            AddLiveKeyValue(card, "Requirements/this prestige", () => DescribeLocks(manager.currentPrestigeRequirements.Count == 0 ? manager.prestigeRequirements : manager.currentPrestigeRequirements), () => manager.CanPrestige() ? PositiveColor : WarningColor);
            AddLiveKeyValue(card, "Rewards/this prestige", () => DescribeOutputs(manager.GetExpectedPrestigeRewards()));
            AddKeyValue(card, "Excluded from reset", relics != null ? relics.currencyName : "none");
            AddCardSeparator(card, "Persistent effects");
            AddLiveKeyValue(card, "Persistent multipliers", () => DescribeUpgradeBlocks(manager.GetPrestigeMultipliers()));
            AddLiveMessage(card, "Result", () => LastPrestigeResult);
            AddCardAction(card, "Prestige", RunPrestige, () => PrestigeManager.Instance.CanPrestige(), false, () => DescribePrestigeAvailability(), false);
            section.Add(card);
        }

        protected void AddOfferPanel()
        {
            VisualElement section = CreateSection("Offers");

            VisualElement offerCard = CreateObjectCard("Active Offer", "Offer Runtime", AccentColor);
            AddLiveKeyValue(offerCard, "Active", () => OffersManager.Instance.ActiveOffer == null || OffersManager.Instance.ActiveOffer.item == null ? "none" : OffersManager.Instance.ActiveOffer.item.metadata.name, () => OffersManager.Instance.ActiveOffer != null ? AccentColor : WarningColor, OffersManager.Instance.ActiveOffer != null);
            AddLiveKeyValue(offerCard, "Time left", DescribeOfferTimer, () => OffersManager.Instance.ActiveOffer != null ? AccentColor : MutedTextColor);
            AddLiveProgress(offerCard, GetOfferTimerProgress);
            AddLiveKeyValue(offerCard, "Buy status", DescribeActiveOfferAvailability, () => OffersManager.Instance.ActiveOffer != null ? PositiveColor : WarningColor, OffersManager.Instance.ActiveOffer != null);
            AddLiveKeyValue(offerCard, "Expire status", () => OffersManager.Instance.ActiveOffer == null ? "no active offer exists; press Trigger first" : "ready, clears the active offer and schedules another", () => OffersManager.Instance.ActiveOffer != null ? PositiveColor : WarningColor, OffersManager.Instance.ActiveOffer != null);
            AddLiveMessage(offerCard, "Result", () => LastOfferResult);
            AddCardAction(offerCard, "Trigger", TriggerOffer);
            AddCardAction(offerCard, "Buy", BuyActiveOffer, () => OffersManager.Instance.ActiveOffer != null, false, () => DescribeActiveOfferAvailability(), false);
            AddCardAction(offerCard, "Expire", ExpireOffer, () => OffersManager.Instance.ActiveOffer != null, false, () => OffersManager.Instance.ActiveOffer == null ? "no active offer exists; press Trigger first" : "ready, clears the active offer and schedules another", false);
            section.Add(offerCard);

            foreach (OfferPoolItem offer in OffersManager.Instance.possibleOffers)
            {
                if (offer == null || offer.item == null) continue;

                ShopItemHolder holder = ShopManager.Instance.GetOrAddHolder(offer.item);
                VisualElement card = CreateObjectCard(offer.item.metadata.name, "Offer Pool Item", AccentColor, configurationObject: offer.item);
                AddKeyValue(card, "Weight", offer.weight.ToString());
                AddKeyValue(card, "Duration", $"{offer.durationSeconds:0}s");
                AddLiveKeyValue(card, "Unlocked", () => holder.iUnlockable.IsUnlocked() ? "yes" : "no", () => holder.iUnlockable.IsUnlocked() ? PositiveColor : WarningColor);
                AddLiveKeyValue(card, "Eligibility", () => DescribeOfferEligibility(offer), () => CanOfferEnterPool(offer) ? PositiveColor : WarningColor, CanOfferEnterPool(offer));
                AddKeyValue(card, "Rewards/purchase", DescribeDropTables(offer.item.rewardTables));
                section.Add(card);
            }
        }

        protected void AddOutputPanel(string title, List<Output> outputs)
        {
            if (outputs == null || outputs.Count == 0) return;

            List<Output> displayOutputs = outputs.Squash();
            if (displayOutputs.Count == 0) return;

            VisualElement card = CreateCard(title, false);
            AddKeyValue(card, "Outputs", DescribeOutputs(displayOutputs));
            dashboard.Add(card);
        }
    }
}

