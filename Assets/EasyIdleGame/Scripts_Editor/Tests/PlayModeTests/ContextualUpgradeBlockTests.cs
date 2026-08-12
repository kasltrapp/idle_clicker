using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class ContextualUpgradeBlockTests
    {
        private GameObject _managerObject;
        private readonly List<UnityEngine.Object> _createdObjects = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            ProductionModifierRegistry.Clear();
            _createdObjects.Clear();
            _managerObject = TestUtilities.SetupManagerObject();
        }

        [TearDown]
        public void TearDown()
        {
            ProductionModifierRegistry.Clear();
            TestUtilities.TearDownManagerObject(_managerObject);

            foreach (UnityEngine.Object createdObject in _createdObjects)
            {
                if (createdObject != null)
                    UnityEngine.Object.DestroyImmediate(createdObject);
            }
        }

        [Test]
        public void ProductionOutputModifiers_CanTargetActiveOfflineAndManualContexts()
        {
            Currency milk = CreateAsset<Currency>();
            Business cowshed = CreateProducingBusiness(milk, 10);
            cowshed.autoProduce = true;

            Upgrade upgrade = CreateUpgrade(
                Modifier(UpgradeType.production, 2, targetCurrency: milk),
                Modifier(UpgradeType.production, 3, targetCurrency: milk, runtimeFilter: UpgradeRuntimeFilter.Active),
                Modifier(UpgradeType.production, 5, targetCurrency: milk, runtimeFilter: UpgradeRuntimeFilter.Offline),
                Modifier(UpgradeType.production, 7, targetCurrency: milk, runtimeFilter: UpgradeRuntimeFilter.Manual));

            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, 1);
            BusinessesManager.Instance.ForceBuyItemsForFree(cowshed, 1);
            BusinessHolder holder = BusinessesManager.Instance.GetHolder(cowshed);

            Assert.AreEqual(new BigNumber(60), GetCurrencyOutput(holder.iProducable.GetOptimisticActualProduction(), milk));

            ProfitData offlineProfit = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(1));
            Assert.AreEqual(new BigNumber(100), offlineProfit.GetCurrencyAmount(milk));

            Assert.IsTrue(holder.TryProduceManually(1, out ProductionRound round));
            Assert.AreEqual(new BigNumber(420), GetCurrencyOutput(round.rolledOutputs, milk));
        }

        [Test]
        public void RuntimeFilter_CanSelectMultipleContexts()
        {
            Currency milk = CreateAsset<Currency>();
            Upgrade upgrade = CreateUpgrade(
                Modifier(UpgradeType.production, 2, targetCurrency: milk, runtimeFilter: UpgradeRuntimeFilter.Active | UpgradeRuntimeFilter.Offline),
                Modifier(UpgradeType.production, 3, targetCurrency: milk, runtimeFilter: UpgradeRuntimeFilter.Manual));
            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, 1);

            Assert.AreEqual(new BigNumber(2), GetProductionMultiplier(milk, isOffline: false, isManual: false));
            Assert.AreEqual(new BigNumber(2), GetProductionMultiplier(milk, isOffline: true, isManual: false));
            Assert.AreEqual(new BigNumber(6), GetProductionMultiplier(milk, isOffline: false, isManual: true));
        }
        [Test]
        public void ProductionInputCostModifier_AffectsAffordabilityAndPayment()
        {
            Currency feed = CreateAsset<Currency>();
            Currency milk = CreateAsset<Currency>();
            Business cowshed = CreateProducingBusiness(milk, 1);
            cowshed.scaleProductionCostWithAmount = true;
            cowshed.allowPartialProductionCost = true;
            cowshed.productionCost = new List<Input> { new Input { currencyInput = feed, inputAmount = 10 } };

            Upgrade upgrade = CreateUpgrade(Modifier(UpgradeType.productionInputCost, 0.5f, targetCurrency: feed));
            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, 1);

            CurrencyManager.Instance.AddCurrencyAmount(feed, 25);
            BusinessesManager.Instance.ForceBuyItemsForFree(cowshed, 10);
            BusinessHolder holder = BusinessesManager.Instance.GetHolder(cowshed);

            Assert.IsTrue(holder.TryStartProduction());
            Assert.AreEqual(new BigNumber(5), holder.ActiveProductions[0].productionAmount);
            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(feed));
        }

        [Test]
        public void RewardOutputAndDropChanceModifiers_RespectRewardSourceAndTarget()
        {
            Currency gems = CreateAsset<Currency>();
            DropTable table = new DropTable
            {
                outputs = new List<Output>
                {
                    new Output { currencyOutput = gems, outputAmount = 10, outputMaxAmount = 10, dropChance = 0 }
                }
            };

            Upgrade upgrade = CreateUpgrade(
                Modifier(UpgradeType.dropChance, 1, UpgradeBlockOperation.Add, targetCurrency: gems, sourceType: SourceType.RecurringReward),
                Modifier(UpgradeType.production, 3, targetCurrency: gems, sourceType: SourceType.RecurringReward));
            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, 1);

            List<Output> dailyExpected = RewardCalculator.GetAverageOutputs(new List<DropTable> { table }, 1, RewardCalculator.CreateContext(SourceType.RecurringReward));
            List<Output> genericExpected = RewardCalculator.GetAverageOutputs(new List<DropTable> { table }, 1, RewardCalculator.CreateContext(SourceType.Generic));

            Assert.AreEqual(new BigNumber(30), GetCurrencyOutput(dailyExpected, gems));
            Assert.AreEqual(new BigNumber(0), GetCurrencyOutput(genericExpected, gems));
        }

        [Test]
        public void MergeModifiers_AdjustInputsOutputsDurationAndCooldown()
        {
            Business calf = CreateBusiness("Calf");
            Business cow = CreateBusiness("Cow");
            BusinessMergeRecipe recipe = CreateAsset<BusinessMergeRecipe>();
            recipe.inputs = new List<Input> { new Input { businessInput = calf, inputAmount = 10 } };
            recipe.outputTables = new List<DropTable>
            {
                new DropTable { outputs = new List<Output> { new Output { businessOutput = cow, outputAmount = 1, outputMaxAmount = 1 } } }
            };
            recipe.mergeDurationSeconds = 10;
            recipe.mergeCooldownSeconds = 10;

            Upgrade upgrade = CreateUpgrade(
                Modifier(UpgradeType.mergeInputCost, 0.5f, targetBusiness: calf, targetMergeRecipe: recipe),
                Modifier(UpgradeType.production, 2, targetBusiness: cow, sourceType: SourceType.Conversion),
                Modifier(UpgradeType.speed, 2, targetMergeRecipe: recipe, sourceType: SourceType.Conversion),
                Modifier(UpgradeType.cooldown, 0.5f, targetMergeRecipe: recipe, sourceType: SourceType.Conversion));
            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, 1);

            BusinessesManager.Instance.ForceBuyItemsForFree(calf, 5);

            Assert.IsTrue(BusinessesManager.Instance.CanMergeBusinesses(recipe));
            Assert.AreEqual(5f, BusinessesManager.Instance.GetEffectiveMergeDuration(recipe));
            Assert.AreEqual(5f, BusinessesManager.Instance.GetEffectiveMergeCooldown(recipe));

            Assert.IsTrue(BusinessesManager.Instance.TryMergeBusinesses(recipe, out List<Output> outputs));
            Assert.AreEqual(new BigNumber(0), BusinessesManager.Instance.GetOrAddHolder(calf).amount);
            Assert.AreEqual(new BigNumber(2), BusinessesManager.Instance.GetOrAddHolder(cow).amount);
            Assert.AreEqual(new BigNumber(2), GetBusinessOutput(outputs, cow));
        }

        [Test]
        public void BoostAndManagerModifiers_AdjustDurationCooldownAndEffectStrength()
        {
            Business business = CreateBusiness("ManagedBusiness");
            GenericMultiplierBoost boost = CreateAsset<GenericMultiplierBoost>();
            boost.upgradeBlocks = new[] { new UpgradeBlock(UpgradeType.production, 2) };
            boost.duration = new Duration(0, 0, 0, 10);

            Manager manager = CreateAsset<Manager>();
            manager.passiveBoosts = new List<UpgradeBlock> { new UpgradeBlock(UpgradeType.production, 2) };
            manager.activeUpgrade = new List<UpgradeBlock> { new UpgradeBlock(UpgradeType.production, 2) };
            manager.activeDuration = new Duration(0, 0, 0, 10);
            manager.cooldownDuration = new Duration(0, 0, 0, 20);
            business.manager = manager;

            Upgrade upgrade = CreateUpgrade(
                Modifier(UpgradeType.duration, 2, targetBoost: boost),
                Modifier(UpgradeType.production, 3, targetBoost: boost),
                Modifier(UpgradeType.production, 3, targetManager: manager),
                Modifier(UpgradeType.duration, 3, targetManager: manager),
                Modifier(UpgradeType.cooldown, 0.5f, targetManager: manager));
            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, 1);

            BoostsManager.Instance.AddBoosts(boost, 1);
            Assert.IsTrue(BoostsManager.Instance.TryUseBoost(boost));
            Assert.AreEqual(20f, BoostsManager.Instance.activeBoosts[0].timeLeft);
            Assert.AreEqual(new BigNumber(6), BoostsManager.GetMultiplierOfTypeStatic(UpgradeType.production, business.GetEffectiveBusinessGroups(), business.GetEffectiveBusinessGroupName()));

            ManagerHolder managerHolder = ManagersManager.Instance.GetOrAddHolder(manager);
            managerHolder.level = 1;
            Assert.IsTrue(managerHolder.TryActivate());
            Assert.AreEqual(30f, managerHolder.activeTimeLeft);
            Assert.AreEqual(6f, ManagersManager.GetPassiveMultiplierOfType(UpgradeType.production, business));
            Assert.AreEqual(6f, ManagersManager.GetActiveMultiplierOfType(UpgradeType.production, business));

            managerHolder.OnTimePassed(1f);
            Assert.AreEqual(29f, managerHolder.activeTimeLeft);
            Assert.AreEqual(10f, managerHolder.cooldownTimeLeft);
        }

        [Test]
        public void PurchaseAndLevelUpCostModifiers_RespectTargetsGroupsAndLevelRanges()
        {
            Currency coins = CreateAsset<Currency>();

            BusinessGroup shopGroup = CreateAsset<BusinessGroup>();
            Business shop = CreateBusiness("Shop");
            shop.businessGroups.Add(shopGroup);
            shop.cost = new List<Input> { new Input { currencyInput = coins, inputAmount = 100 } };
            UpgradeGroup researchGroup = CreateAsset<UpgradeGroup>();
            Upgrade targetUpgrade = CreateUpgrade();
            targetUpgrade.upgradeGroups.Add(researchGroup);
            targetUpgrade.cost = new List<Input> { new Input { currencyInput = coins, inputAmount = 100 } };

            BusinessGroup cowshedGroup = CreateAsset<BusinessGroup>();
            Business cowshed = CreateBusiness("Cowshed");
            cowshed.businessGroups.Add(cowshedGroup);
            cowshed.autoUpgradeForFree = false;
            cowshed.autoUpgradeToFirstLevelFree = false;
            cowshed.upgradeCost = new List<Input> { new Input { currencyInput = coins, inputAmount = 100 } };

            Upgrade discountUpgrade = CreateUpgrade(
                Modifier(UpgradeType.purchaseCost, 0.5f, targetBusinessGroup: shopGroup),
                Modifier(UpgradeType.upgradePurchaseCost, 0.5f, targetUpgradeGroup: researchGroup),
                Modifier(UpgradeType.levelUpCost, 0.25f, targetBusinessGroup: cowshedGroup, minBusinessLevel: 0, maxBusinessLevel: 0));
            UpgradesManager.Instance.ForceBuyItemsForFree(discountUpgrade, 1);

            CurrencyManager.Instance.AddCurrencyAmount(coins, 50);
            Assert.IsTrue(BusinessesManager.Instance.iBuyable.TryBuyItems(shop, 1));
            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(coins));

            CurrencyManager.Instance.AddCurrencyAmount(coins, 50);
            Assert.IsTrue(UpgradesManager.Instance.iBuyable.TryBuyItems(targetUpgrade, 1));
            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(coins));

            CurrencyManager.Instance.AddCurrencyAmount(coins, 25);
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(cowshed);
            holder.iUpgradable.AddCount(1);
            Assert.IsTrue(holder.iUpgradable.TryUpgrade());
            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(coins));
            Assert.AreEqual(new BigNumber(1), holder.iUpgradable.Level);
        }

        [Test]
        public void ProductionOutputScalingModifier_CanBeImprovedByUpgrade()
        {
            Currency milk = CreateAsset<Currency>();
            Business cowshed = CreateProducingBusiness(milk, 1);
            ProductionBusinessOutputScalingModifierAsset scaling = CreateAsset<ProductionBusinessOutputScalingModifierAsset>();
            scaling.producerBusiness = cowshed;
            scaling.outputBusiness = cowshed;
            scaling.scalingMode = ProductionBusinessOutputScalingMode.LogarithmicFloor;
            scaling.scalingSource = ProductionBusinessOutputScalingSource.OwnedAmount;
            scaling.scalingBase = 10;
            scaling.outputAmountPerStep = 1;

            Upgrade upgrade = CreateUpgrade(Modifier(UpgradeType.productionOutputScaling, 3, targetBusiness: cowshed));
            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, 1);
            ProductionModifierRegistry.Register(scaling);

            BusinessesManager.Instance.ForceBuyItemsForFree(cowshed, 100);
            BusinessHolder holder = BusinessesManager.Instance.GetHolder(cowshed);

            Assert.AreEqual(new BigNumber(6), GetBusinessOutput(holder.iProducable.GetOptimisticActualProduction(), cowshed));
        }

        private T CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            _createdObjects.Add(asset);
            return asset;
        }

        private Upgrade CreateUpgrade(params UpgradeBlock[] modifiers)
        {
            Upgrade upgrade = CreateAsset<Upgrade>();
            upgrade.upgrades = new List<UpgradeBlock>(modifiers);
            upgrade.cost = new List<Input>();
            upgrade.locks = new List<Lock>();
            upgrade.maxBuyableAmount = 10;
            return upgrade;
        }

        private Business CreateProducingBusiness(Currency outputCurrency, BigNumber outputAmount)
        {
            Business business = CreateBusiness("Producer");
            business.outputTables = new List<DropTable>
            {
                new DropTable
                {
                    outputs = new List<Output>
                    {
                        new Output { currencyOutput = outputCurrency, outputAmount = outputAmount, outputMaxAmount = outputAmount }
                    }
                }
            };
            return business;
        }

        private Business CreateBusiness(string name)
        {
            Business business = CreateAsset<Business>();
            business.businessName = name;
            business.cost = new List<Input>();
            business.productionCost = new List<Input>();
            business.outputTables = new List<DropTable>();
            business.timeToProduce = 1;
            business.autoCollect = true;
            business.autoLevelUp = false;
            business.upgradeCost = new List<Input>();
            return business;
        }

        private static UpgradeBlock Modifier(
            UpgradeType type,
            float value,
            UpgradeBlockOperation operation = UpgradeBlockOperation.Multiply,
            Currency targetCurrency = null,
            Business targetBusiness = null,
            BusinessGroup targetBusinessGroup = null,
            UpgradeGroup targetUpgradeGroup = null,
            Boost targetBoost = null,
            Manager targetManager = null,
            BusinessMergeRecipe targetMergeRecipe = null,
            SourceType sourceType = SourceType.Any,
            int minBusinessLevel = 0,
            int maxBusinessLevel = -1,
            UpgradeRuntimeFilter runtimeFilter = (UpgradeRuntimeFilter)0)
        {
            UpgradeBlock block = new UpgradeBlock
            {
                upgradeType = type,
                operation = operation,
                value = value,
                filters = new UpgradeBlockFilter
                {
                    targetCurrency = targetCurrency,
                    targetBusiness = targetBusiness,
                    targetBoost = targetBoost,
                    targetManager = targetManager,
                    targetMergeRecipe = targetMergeRecipe,
                    source = sourceType,
                    runtimeFilter = runtimeFilter
                }
            };

            if (targetBusinessGroup != null) block.filters.targetBusinessGroups.Add(targetBusinessGroup);
            if (targetUpgradeGroup != null) block.filters.targetUpgradeGroups.Add(targetUpgradeGroup);
            block.filters.minBusinessLevel = Math.Max(0, minBusinessLevel);
            block.filters.maxBusinessLevel = maxBusinessLevel;

            return block;
        }

        private static BigNumber GetProductionMultiplier(Currency currency, bool isOffline, bool isManual)
        {
            return UpgradeEffectResolver.GetMultiplier(UpgradeType.production, new UpgradeEffectContext
            {
                TargetCurrency = currency,
                IsOffline = isOffline,
                IsManual = isManual
            });
        }
        private static BigNumber GetCurrencyOutput(List<Output> outputs, Currency currency)
        {
            Output output = outputs.Find(o => o.currencyOutput == currency);
            return output != null ? output.outputAmount : 0;
        }

        private static BigNumber GetBusinessOutput(List<Output> outputs, Business business)
        {
            Output output = outputs.Find(o => o.businessOutput == business);
            return output != null ? output.outputAmount : 0;
        }
    }
}
