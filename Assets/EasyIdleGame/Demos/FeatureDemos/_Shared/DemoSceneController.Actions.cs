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
        protected void BuyBusiness(string businessName, int amount)
        {
            Business business = Load<Business>(businessName);
            bool bought = BusinessesManager.Instance.iBuyable.TryBuyItems(business, amount);
            Log($"TryBuyItems {business.businessName} x{amount}: {bought}.");
        }

        protected void GrantBusiness(string businessName, BigNumber amount)
        {
            Business business = LoadOptional<Business>(businessName);
            if (business == null) return;

            BusinessesManager.Instance.ForceBuyItemsForFree(business, amount);
            Log($"Granted {business.businessName} x{amount}.");
        }

        protected void AddCurrency(string currencyName, BigNumber amount)
        {
            Currency currency = Load<Currency>(currencyName);
            CurrencyManager.Instance.AddCurrencyAmount(currency, amount);
            Log($"Added {currency.currencyName} x{amount}.");
        }

        protected void StartBusiness(string businessName, BigNumber amount, bool log = true)
        {
            Business business = Load<Business>(businessName);
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            bool started = holder.TryProduceManually(amount, out _);

            if (log)
            {
                Log($"TryProduceManually {business.businessName} x{amount}: {started}.");
            }
        }

        protected void StartAllOwnedBusinessCopies(string businessName)
        {
            Business business = Load<Business>(businessName);
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            BigNumber amount = holder.amount - holder.CurrentlyProducing;
            if (amount <= 0)
            {
                Log($"{business.businessName} has no idle owned copies to start.");
                return;
            }

            bool started = holder.TryProduceManually(amount, out _);
            Log($"TryProduceManually {business.businessName} all idle owned x{amount}: {started}.");
        }

        protected void FinishAllLoadedProductions()
        {
            int finished = 0;
            foreach (Business business in LoadAllPrefixed<Business>())
            {
                BusinessHolder holder = BusinessesManager.Instance.GetHolder(business);
                if (holder == null || holder.ActiveProductions.Count == 0) continue;

                List<ProductionRound> rounds = holder.ActiveProductions.ToList();
                holder.ActiveProductions.Clear();

                foreach (ProductionRound round in rounds)
                {
                    holder.OnProductionRoundFinished(round);
                    finished++;
                }
            }

            Log($"Finished {finished} active production round(s).");
        }

        protected void FinishBusinessProductions(string businessName)
        {
            Business business = Load<Business>(businessName);
            BusinessHolder holder = BusinessesManager.Instance.GetHolder(business);
            int finished = FinishBusinessProductions(holder);
            Log($"Finished {finished} {business.businessName} production round(s).");
        }

        private int FinishBusinessProductions(BusinessHolder holder)
        {
            if (holder == null || holder.ActiveProductions.Count == 0) return 0;

            List<ProductionRound> rounds = holder.ActiveProductions.ToList();
            holder.ActiveProductions.Clear();

            foreach (ProductionRound round in rounds)
            {
                holder.OnProductionRoundFinished(round);
            }

            return rounds.Count;
        }

        protected void ClaimAllLoadedOutputs()
        {
            List<Output> claimed = new List<Output>();

            foreach (Business business in LoadAllPrefixed<Business>())
            {
                BusinessHolder holder = BusinessesManager.Instance.GetHolder(business);
                if (holder == null || holder.unclaimedOutputs.Count == 0) continue;

                claimed.AddRange(holder.ClaimOutputs());
            }

            LastOutputs = claimed.Squash();
            Log($"Claimed stored outputs: {DescribeOutputs(LastOutputs)}.");
        }

        protected void ClaimBusinessOutputs(string businessName)
        {
            Business business = Load<Business>(businessName);
            BusinessHolder holder = BusinessesManager.Instance.GetHolder(business);
            LastOutputs = holder == null ? new List<Output>() : holder.ClaimOutputs().Squash();
            Log($"Claimed {business.businessName} outputs: {DescribeOutputs(LastOutputs)}.");
        }

        protected void FinishAndClaimAllLoaded()
        {
            FinishAllLoadedProductions();
            ClaimAllLoadedOutputs();
        }

        protected void MergePairRecipe()
        {
            Business input = Load<Business>("ScrapCart");
            BusinessesManager.TryGetMergeRecipe(input, input, out BusinessMergeRecipe recipe);
            bool merged = recipe != null && BusinessesManager.Instance.TryMergeBusinesses(recipe, out LastOutputs);
            Log($"TryMergeBusinesses pair recipe: {merged}, outputs={DescribeOutputs(LastOutputs)}.");
        }

        protected void MergeCurrencyRecipe()
        {
            BusinessMergeRecipe recipe = Load<BusinessMergeRecipe>("CurrencyRecipe");
            bool merged = BusinessesManager.Instance.TryMergeBusinesses(recipe, out LastOutputs);
            Log($"TryMergeBusinesses mixed currency/business recipe: {merged}, outputs={DescribeOutputs(LastOutputs)}.");
        }

        protected void ProduceWrapperPair()
        {
            StartAllOwnedBusinessCopies("BaseBusiness");
            StartAllOwnedBusinessCopies("WrappedBusiness");
            FinishAllLoadedProductions();
        }

        protected void ProduceBusinessNow(string businessName)
        {
            Business business = Load<Business>(businessName);
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            BigNumber amount = holder.amount;
            if (amount <= 0)
            {
                LastOutputs = new List<Output>();
                Log($"{business.businessName} produce now skipped because no copies are owned.");
                return;
            }

            bool started = holder.TryProduceManually(amount, out _);
            FinishBusinessProductions(holder);

            LastOutputs = holder.ClaimOutputs().Squash();
            Log($"{business.businessName} produce all now x{amount}: started={started}, claimed={DescribeOutputs(LastOutputs)}.");
        }

        protected void ShowDropAverages()
        {
            List<string> lines = new List<string>();
            foreach (BusinessMergeRecipe recipe in LoadAllPrefixed<BusinessMergeRecipe>())
            {
                lines.Add($"{recipe.name}: {DescribeOutputs(recipe.outputTables.GetAverageOutputs(1))}");
            }

            Log(lines.Count == 0 ? "No drop-table recipe assets found." : string.Join(" | ", lines));
        }

        protected void RollDropRecipe(string recipeName, int amount)
        {
            BusinessMergeRecipe recipe = Load<BusinessMergeRecipe>(recipeName);
            LastOutputs = recipe.outputTables.SelectMany(table => table.GenerateDrops(amount)).ToList().Squash();
            Log($"Rolled {recipe.name} x{amount}: {DescribeOutputs(LastOutputs)}.");
        }

        protected void ApplyDropRecipe(string recipeName, int amount)
        {
            BusinessMergeRecipe recipe = Load<BusinessMergeRecipe>(recipeName);
            LastOutputs = recipe.outputTables.ApplyOutputs(amount);
            Log($"Applied {recipe.name} x{amount}: {DescribeOutputs(LastOutputs)}.");
        }

        protected void BuyBoostForFree(string boostName, BigNumber amount)
        {
            Boost boost = LoadOptional<Boost>(boostName);
            if (boost == null) return;

            BoostsManager.Instance.ForceBuyItemsForFree(boost, amount);
        }

        protected void GrantBoost(Boost boost, BigNumber amount)
        {
            BoostsManager.Instance.ForceBuyItemsForFree(boost, amount);
            Log($"Added {boost.boostName} x{amount} to inventory.");
        }

        protected void UseBoost(string boostName)
        {
            Boost boost = Load<Boost>(boostName);
            UseBoost(boost);
        }

        protected void UseBoost(Boost boost)
        {
            BigNumber inventory = BoostsManager.Instance.GetBoostAmountInInventory(boost);
            if (inventory <= 0)
            {
                LastBoostResult = $"{boost.boostName} cannot be used because inventory is 0.";
                Log(LastBoostResult);
                return;
            }

            int before = BoostsManager.Instance.activeBoosts.Count;
            bool used = BoostsManager.Instance.TryUseBoost(boost);
            int after = BoostsManager.Instance.activeBoosts.Count;

            Log($"TryUseBoost {boost.boostName}: {used}, active count {before}->{after}.");
        }

        protected void BuyDemoUpgrades()
        {
            ForceBuyUpgrade("ProductionUpgrade", 1);
            ForceBuyUpgrade("SpeedUpgrade", 1);
            ForceBuyUpgrade("CostUpgrade", 1);
            Log("Applied production, speed, and cost upgrades.");
        }

        protected void ApplyOverrideUpgrades()
        {
            ForceBuyUpgrade("WalletCapUpgrade", 1);
            ForceBuyUpgrade("WalletCapModifier", 1);

            Currency credits = Load<Currency>("Credits");
            CurrencyManager.Instance.AddCurrencyAmount(credits, 500);
            Log($"Applied override cap upgrades, {credits.currencyName} max is now {CurrencyManager.Instance.GetMaxCurrencyConstrain(credits)}.");
        }

        protected void BuyUpgrade(string upgradeName)
        {
            Upgrade upgrade = Load<Upgrade>(upgradeName);
            BuyUpgrade(upgrade);
        }

        protected void BuyUpgrade(Upgrade upgrade)
        {
            bool bought = UpgradesManager.Instance.iBuyable.TryBuyItems(upgrade, 1);
            LastUpgradeResult = $"TryBuyItems {upgrade.upgradeName}: {bought}";
            Log(LastUpgradeResult);
        }

        protected void ForceBuyUpgrade(string upgradeName, BigNumber amount)
        {
            Upgrade upgrade = LoadOptional<Upgrade>(upgradeName);
            if (upgrade == null) return;

            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, amount);
        }

        protected void CompleteRewardPurchase()
        {
            CompleteRewardPurchase("RewardCrate");
        }

        protected void CompleteRewardPurchase(string itemName)
        {
            ShopItem item = Load<ShopItem>(itemName);
            CompleteRewardPurchase(item);
        }

        protected void CompleteRewardPurchase(ShopItem item)
        {
            LastOutputs = ShopManager.Instance.CompleteExternalPurchase(item, 1, SourceType.Purchase);
            Log($"CompleteExternalPurchase returned {DescribeOutputs(LastOutputs)}.");
        }

        protected void AttemptRewardAd()
        {
            AttemptRewardAd("RewardCrate");
        }

        protected void AttemptRewardAd(string itemName)
        {
            ShopItem item = Load<ShopItem>(itemName);
            AttemptRewardAd(item);
        }

        protected void AttemptRewardAd(ShopItem item)
        {
            ShopManager.Instance.AttemptPurchaseWithAd(item);
            Log("AttemptPurchaseWithAd invoked; complete it with the purchase action.");
        }

        protected void CompleteRewardAd(ShopItem item)
        {
            ShopManager.Instance.AttemptPurchaseWithAd(item);
            LastOutputs = ShopManager.Instance.CompleteExternalPurchase(item, 1, SourceType.Advertisement).Squash();
            LastShopRewardResult = $"Mock rewarded ad only. In a game, subscribe to ShopManager.OnAttemptAdPurchase and call CompleteExternalPurchase after the ad SDK reward callback. Outputs={DescribeOutputs(LastOutputs)}.";
            Log(LastShopRewardResult);
        }

        protected void ConsumeRewardCrate()
        {
            ConsumeRewardCrate("RewardCrate");
        }

        protected void ConsumeRewardCrate(string itemName)
        {
            ShopItem item = Load<ShopItem>(itemName);
            ConsumeRewardCrate(item);
        }

        protected void ConsumeRewardCrate(ShopItem item)
        {
            bool consumed = ShopManager.Instance.TryConsumeItem(item, 1, out LastOutputs);
            Log($"TryConsumeItem {item.metadata.name}: {consumed}, outputs={DescribeOutputs(LastOutputs)}.");
        }

        protected void CalculateOfflineProfit()
        {
            LastCalculatedProfit = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(OfflineMinutes));
            Log($"Calculated offline profit for {LastCalculatedProfit.ElapsedTime()}: {DescribeProfit(LastCalculatedProfit)}.");
        }

        protected void ApplyCalculatedProfit()
        {
            if (LastCalculatedProfit == null)
            {
                CalculateOfflineProfit();
            }

            LastProfitSummary = ProfitCalculator.ApplyProfit(LastCalculatedProfit);
            Log($"Applied calculated profit: {DescribeSummary(LastProfitSummary)}.");
        }

        protected void Travel(string locationName)
        {
            Location location = Load<Location>(locationName);
            Travel(location);
        }

        protected void Travel(Location location)
        {
            bool traveled = LocationsManager.Instance.TryTravelTo(location);
            Log($"TryTravelTo {location.metadata.name}: {traveled}.");
        }

        protected void ListTypedLocations()
        {
            List<Location> all = LocationsManager.Instance.GetAllLocations<Location>();
            List<Location> unlocked = LocationsManager.Instance.GetUnlockedLocations<Location>();
            bool hasActive = LocationsManager.Instance.TryGetActiveLocation(out Location active);

            Log($"GetAllLocations<Location>={all.Count}, GetUnlockedLocations<Location>={unlocked.Count}, active={(hasActive ? active.metadata.name : "None")}.");
        }

        protected void CycleBuyAmount()
        {
            BusinessesManager.Instance.ChangeBuyAmount();
            Log($"Current buy amount is now {BusinessesManager.Instance.GetBuyAmount()}.");
        }

        protected void BuyTargetAmount(string businessName)
        {
            Business business = Load<Business>(businessName);
            BigNumber amount = BusinessesManager.Instance.GetTargetBusinessBuyAmount(business);
            bool bought = amount > 0 && BusinessesManager.Instance.iBuyable.TryBuyItems(business, amount);
            Log($"Target buy {business.businessName}: amount={amount}, bought={bought}.");
        }

        protected void GrantLockInputs()
        {
            AddCurrency("Credits", 160);
            AddCurrency("Gems", 5);
            GrantBusiness("StarterStand", 3);
            AddPlayerXp(360);
            BusinessesManager.Instance.iUnlockable.CheckAutoUnlocks();
            Log("Granted the currencies, starter copies, and player level used by lock examples.");
        }

        protected void BuyAutomationManager()
        {
            BuyAutomationManager("Foreman");
        }

        protected void BuyAutomationManager(string managerName)
        {
            Manager manager = Load<Manager>(managerName);
            BuyAutomationManager(manager);
        }

        protected void BuyAutomationManager(Manager manager)
        {
            bool bought = ManagersManager.Instance.iBuyable.TryBuyItems(manager, 1);
            LastManagerResult = $"TryBuyItems {manager.managerName}: {bought}";
            Log(LastManagerResult);
        }

        protected void GrantAutomationManagerLevel(Manager manager)
        {
            ManagerHolder holder = ManagersManager.Instance.GetOrAddHolder(manager);
            BigNumber countToNextLevel = holder.iUpgradable.GetCountToNextLevel();
            holder.AddManagerCount(countToNextLevel);
            LastManagerResult = $"{manager.managerName} level is now {holder.level:0}.";
            Log(LastManagerResult);
        }

        protected void ActivateAutomationManager()
        {
            ActivateAutomationManager("Foreman");
        }

        protected void ActivateAutomationManager(string managerName)
        {
            Manager manager = Load<Manager>(managerName);
            ActivateAutomationManager(manager);
        }

        protected void ActivateAutomationManager(Manager manager)
        {
            bool activated = ManagersManager.Instance.TryActivateManager(manager);
            LastManagerResult = $"TryActivateManager {manager.managerName}: {activated}";
            Log(LastManagerResult);
        }

        protected void TickManagersAndBusinesses(float seconds)
        {
            ManagersManager.Instance.OnTimePassed(seconds);

            int started = 0;
            int finished = 0;
            foreach (Business business in LoadAllPrefixed<Business>())
            {
                BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
                if (holder.CanAutoProduce() && holder.ActiveProductions.Count == 0)
                {
                    holder.TryStartProduction();
                    started++;
                }

                float speedMultiplier = (float)holder.GetSpeedMultiplier().ToDouble();
                for (int i = holder.ActiveProductions.Count - 1; i >= 0; i--)
                {
                    ProductionRound round = holder.ActiveProductions[i];
                    round.productionTime += seconds * speedMultiplier;
                    if (round.productionTime < business.timeToProduce) continue;

                    holder.ActiveProductions.RemoveAt(i);
                    holder.OnProductionRoundFinished(round);
                    finished++;
                }
            }

            LastManagerResult = $"Advanced managers and active production by {seconds:0.#}s, started={started}, finished={finished}.";
            Log(LastManagerResult);
        }

        protected void AddPlayerXp(BigNumber amount)
        {
            PlayerStats.Instance.AddXp(amount);
            LastPlayerStatsResult = $"Added XP x{amount}; level {PlayerStats.Instance.GetPlayerLevel()}, current XP {PlayerStats.Instance.level.currentXp}.";
            Log(LastPlayerStatsResult);
        }

        protected void IncrementCustomStat(string statName, BigNumber amount)
        {
            CustomStat stat = Load<CustomStat>(statName);
            IncrementCustomStat(stat, amount);
        }

        protected void IncrementCustomStat(CustomStat stat, BigNumber amount)
        {
            PlayerStats.Instance.IncrementStat(stat, amount);
            LastPlayerStatsResult = $"{stat.metadata.name} is now {PlayerStats.Instance.GetStat(stat)}.";
            Log(LastPlayerStatsResult);
        }

        protected void UpgradeProgressionBusiness()
        {
            Business business = Load<Business>("ProgressionStand");
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            holder.iUpgradable.ForceUpgradeToNextLevel();
            LastPlayerStatsResult = $"{business.businessName} business upgrade level is now {holder.iUpgradable.Level}.";
            Log(LastPlayerStatsResult);
        }

        protected void ClaimDemoAchievement()
        {
            ClaimDemoAchievement("QuestAchievement");
        }

        protected void ClaimDemoAchievement(string achievementName)
        {
            Achievement achievement = Load<Achievement>(achievementName);
            ClaimDemoAchievement(achievement);
        }

        protected void ClaimDemoAchievement(Achievement achievement)
        {
            AchievementHolder holder = AchievementsManager.Instance.GetOrAddHolder(achievement);
            holder.iUnlockable.IsUnlocked();
            bool claimed = AchievementsManager.Instance.TryClaimAchievement(achievement, out List<Output> outputs);
            LastOutputs = outputs == null ? new List<Output>() : outputs.Squash();
            LastAchievementResult = $"TryClaimAchievement {achievement.metadata.name}: {claimed}";
            Log($"{LastAchievementResult}, outputs={DescribeOutputs(LastOutputs)}.");
        }

        protected void SimulateNextRewardDay()
        {
            DailyRewardsManager.Instance.SimulateNextDay();
            LastDailyRewardResult = $"Current reward day is now {DailyRewardsManager.Instance.GetCurrentDay()}.";
            Log(LastDailyRewardResult);
        }

        protected void ClaimCurrentDailyReward()
        {
            ClaimDailyReward(DailyRewardsManager.Instance.GetCurrentDay());
        }

        protected void ClaimDailyReward(int day)
        {
            bool claimed = DailyRewardsManager.Instance.TryClaimDailyRewards(day, out List<Output> outputs);
            LastOutputs = outputs == null ? new List<Output>() : outputs.Squash();
            LastDailyRewardResult = $"TryClaimDailyRewards day {day}: {claimed}";
            Log($"{LastDailyRewardResult}, outputs={DescribeOutputs(LastOutputs)}.");
        }

        protected virtual void RunPrestige()
        {
            bool prestiged = PrestigeManager.Instance.TryPrestigeGame(out List<Output> outputs);
            LastOutputs = outputs == null ? new List<Output>() : outputs.Squash();
            LastPrestigeResult = $"TryPrestigeGame: {prestiged}, count={PrestigeManager.Instance.prestiges.prestigeCount}.";
            Log($"{LastPrestigeResult} rewards={DescribeOutputs(LastOutputs)}.");
        }

        protected void TriggerOffer()
        {
            OffersManager.Instance.TriggerOffer();
            OfferPoolItem active = OffersManager.Instance.ActiveOffer;
            LastOfferResult = active == null ? "No valid offer became active." : $"Active offer: {active.item.metadata.name} for {active.durationSeconds:0}s.";
            Log(LastOfferResult);
        }

        protected void ExpireOffer()
        {
            OffersManager.Instance.ExpireCurrentOffer();
            LastOfferResult = "Expired the active offer.";
            Log(LastOfferResult);
        }

        protected void BuyActiveOffer()
        {
            OfferPoolItem active = OffersManager.Instance.ActiveOffer;
            if (active == null)
            {
                LastOfferResult = "No active offer to buy.";
                Log(LastOfferResult);
                return;
            }

            LastOutputs = ShopManager.Instance.CompleteExternalPurchase(active.item, SourceType.Purchase).Squash();
            bool consumed = OffersManager.Instance.TryConsumeCurrentOffer();
            LastOfferResult = $"Completed active offer {active.item.metadata.name}, consumed={consumed}, outputs={DescribeOutputs(LastOutputs)}.";
            Log(LastOfferResult);
        }
    }
}

