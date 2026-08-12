using NUnit.Framework;
using System;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class IdleProfitCalculatorTests
    {
        private GameObject _managerObject;

        [SetUp]
        public void SetUp()
        {
            _managerObject = TestUtilities.SetupManagerObject();
        }

        [TearDown]
        public void TearDown()
        {
            TestUtilities.TearDownManagerObject(_managerObject);
        }

        [Test]
        public void SimpleIdleProfit()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessAutoProduce, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(10));
            Assert.IsTrue(data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 600,
                $"Idle profit should be 600 after 10 minutes of production. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");

            ProfitCalculator.ApplyProfit(data);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 600,
                $"Idle profit should be 600 after applying profit. Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");
        }

        [Test]
        public void SimpleIdleProfitWithManager()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessManagerProfit, 1);
            ManagersManager.Instance.AddManagers(TestUtilities.TestManagerProfit, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(10));
            Assert.IsTrue(data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 600,
                $"Idle profit should be 600 after 10 minutes of production. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");

            ProfitCalculator.ApplyProfit(data);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 600,
                $"Idle profit should be 600 after applying profit. Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");
        }

        [Test]
        public void IdleProfitWithBusinessOutput()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessOutput, 1);
            ManagersManager.Instance.AddManagers(TestUtilities.TestManagerProfit, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(10));
            Assert.IsTrue(data.GetAdditionalBusinessAmount(TestUtilities.TestBusinessAutoProduce) == 600,
                $"Idle business profit should be 600 after 10 minutes of production. Actual: {data.GetAdditionalBusinessAmount(TestUtilities.TestBusinessAutoProduce)}");

            ProfitCalculator.ApplyProfit(data);
            Assert.IsTrue(BusinessesManager.Instance.iBuyable.GetItemAmount(TestUtilities.TestBusinessAutoProduce) == 600,
                $"Idle business profit should be 600 after applying profit. Actual: {BusinessesManager.Instance.iBuyable.GetItemAmount(TestUtilities.TestBusinessAutoProduce)}");
        }

        [Test]
        public void IdleProfitWithBusinessOutputExponential()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessOutput, 1);
            ManagersManager.Instance.AddManagers(TestUtilities.TestManagerProfit, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(1));
            Assert.IsTrue(data.GetAdditionalBusinessAmount(TestUtilities.TestBusinessAutoProduce) == 60,
                $"Idle business profit should be 60 after 1 minute of production. Actual: {data.GetAdditionalBusinessAmount(TestUtilities.TestBusinessAutoProduce)}");

            BigNumber cOutput = data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit);
            Assert.IsTrue(cOutput > 1820 && cOutput < 1980,
                $"Idle profit should be around 1900 after exponential calculation. Actual: {cOutput}");
        }

        [Test]
        public void IdleProfitWithProductionCostSimple()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessProductionCost, 1);
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrencyProfit, 300);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(10));
            Assert.IsTrue(data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == -300,
                $"Idle currency profit should be -300 (consumed). Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");
            Assert.IsTrue(data.GetCurrencyAmount(TestUtilities.TestCurrency2Profit) == 300,
                $"Idle currency2 profit should be 300. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrency2Profit)}");
            Assert.IsTrue(data.GetAbsoluteCurrencyAmount(TestUtilities.TestCurrencyProfit) == 0,
                "Absolute currency amount for consumed currency should be 0");
            Assert.IsTrue(data.GetAbsoluteCurrencyAmount(TestUtilities.TestCurrency2Profit) == 300,
                "Absolute currency amount for produced currency should be 300");

            ProfitCalculator.ApplyProfit(data);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 0,
                $"Idle currency profit should be 0 after applying. Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency2Profit) == 300,
                $"Idle currency2 profit should be 300 after applying. Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency2Profit)}");
        }

        [Test]
        public void IdleProfitWithProductionCost()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessProductionCost, 1);
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessAutoProduce, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(30));

            BigNumber cOutput = data.GetCurrencyAmount(TestUtilities.TestCurrency2Profit);
            Assert.IsTrue(cOutput >= 1740 && cOutput <= 1860,
                $"Idle currency profit should be about 1800. Actual: {cOutput}");
        }

        [Test]
        public void IdleProfitWithStockpileProduction()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessStockpile, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(10));
            Assert.IsTrue(data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 0,
                $"All outputs should be in the stockpile. Currency amount should be 0. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");
            Assert.IsTrue(data.GetBusinessStockpile(TestUtilities.TestBusinessStockpile).Count != 0,
                "The stockpile should not be empty");
            Assert.IsTrue(data.GetBusinessStockpile(TestUtilities.TestBusinessStockpile)[0].outputAmount == 600,
                "Stockpile output amount should be 600");

            ProfitCalculator.ApplyProfit(data);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 0,
                $"Currency amount should still be 0 after applying profit (unclaimed). Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");

            Assert.IsTrue(BusinessesManager.Instance.GetHolder(TestUtilities.TestBusinessStockpile).unclaimedOutputs[0].outputAmount == 600,
                "Unclaimed outputs were not applied properly to the business holder");

            BusinessesManager.Instance.GetHolder(TestUtilities.TestBusinessStockpile).ClaimOutputs();
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 600,
                $"Idle profit should be 600 after claiming from stockpile. Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");
        }

        [Test]
        public void IdleProfitWithProductionBoost()
        {
            BoostsManager.Instance.AddBoosts(TestUtilities.TestProductionBoost, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestProductionBoost);

            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessAutoProduce, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(10));
            Assert.IsTrue(data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 6000,
                $"Idle profit should be 6000 after 10 minutes with production boost. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");

            ProfitCalculator.ApplyProfit(data);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 6000,
                $"Idle profit should be 6000 after applying profit. Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");
        }

        [Test]
        public void IdleProfitWithSpeedBoost()
        {
            BoostsManager.Instance.AddBoosts(TestUtilities.TestSpeedBoost, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestSpeedBoost);

            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessAutoProduce, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(10));
            Assert.IsTrue(data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 6000,
                $"Idle profit should be 6000 after 10 minutes with speed boost. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");

            ProfitCalculator.ApplyProfit(data);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 6000,
                $"Idle profit should be 6000 after applying profit. Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");
        }

        [Test]
        public void IdleProfitWithUpgrade()
        {
            UpgradesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestProductionUpgrade, 1);
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessAutoProduce, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(10));
            Assert.IsTrue(data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 6000,
                $"Idle profit should be 6000 after 10 minutes with upgrade. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");

            ProfitCalculator.ApplyProfit(data);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) == 6000,
                $"Idle profit should be 6000 after applying profit. Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");
        }

        [Test]
        public void IdleProfitWithMaxBusinessAndCurrencyAmount()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessMaxAmountCurBusOutput, 1);
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessMaxAmountCurOutput, 2);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(10));

            Assert.IsTrue(data.GetCurrencyAmount(TestUtilities.TestCurrencyMaxAmount) == 400,
                $"Calculated idle profit should be capped during simulation. Expected 400, Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyMaxAmount)}");

            ProfitCalculator.ApplyProfit(data);

            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyMaxAmount) == 400,
                "Currency amount should be capped at 400 after application");
            Assert.IsTrue(BusinessesManager.Instance.iBuyable.GetItemAmount(TestUtilities.TestBusinessMaxAmountCurOutput) == 2,
                "Business amount should be capped at 2");
        }

        [Test]
        public void IdleProfitWithSharedCapacityGroups()
        {
            // Setup shared capacity group
            CurrencyGroup sharedGroup = ScriptableObject.CreateInstance<CurrencyGroup>();
            sharedGroup.name = "SharedGroup";
            sharedGroup.capacity = 100;

            Currency sharedCurrency1 = ScriptableObject.CreateInstance<Currency>();
            sharedCurrency1.currencyName = "SharedCurrency1";
            sharedCurrency1.currencyGroups = new System.Collections.Generic.List<CurrencyGroup> { sharedGroup };

            Currency sharedCurrency2 = ScriptableObject.CreateInstance<Currency>();
            sharedCurrency2.currencyName = "SharedCurrency2";
            sharedCurrency2.currencyGroups = new System.Collections.Generic.List<CurrencyGroup> { sharedGroup };

            Business producer1 = ScriptableObject.CreateInstance<Business>();
            producer1.businessName = "Producer1";
            producer1.timeToProduce = 1;
            producer1.autoProduce = true;
            producer1.productionCost = new System.Collections.Generic.List<Input>();
            producer1.cost = new System.Collections.Generic.List<Input>();
            DropTable table1 = new DropTable();
            table1.outputs.Add(new Output { currencyOutput = sharedCurrency1, outputAmount = 10, outputMaxAmount = 10 });
            producer1.outputTables = new System.Collections.Generic.List<DropTable> { table1 };

            Business producer2 = ScriptableObject.CreateInstance<Business>();
            producer2.businessName = "Producer2";
            producer2.timeToProduce = 1;
            producer2.autoProduce = true;
            producer2.productionCost = new System.Collections.Generic.List<Input>();
            producer2.cost = new System.Collections.Generic.List<Input>();
            DropTable table2 = new DropTable();
            table2.outputs.Add(new Output { currencyOutput = sharedCurrency2, outputAmount = 20, outputMaxAmount = 20 });
            producer2.outputTables = new System.Collections.Generic.List<DropTable> { table2 };

            BusinessesManager.Instance.ForceBuyItemsForFree(producer1, 1);
            BusinessesManager.Instance.ForceBuyItemsForFree(producer2, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(10));

            BigNumber totalProduced = data.GetCurrencyAmount(sharedCurrency1) + data.GetCurrencyAmount(sharedCurrency2);
            Assert.IsTrue(totalProduced == 100, $"Total produced currencies across shared group should be capped at 100, actual: {totalProduced}");

            ProfitCalculator.ApplyProfit(data);
            BigNumber actualTotal = CurrencyManager.Instance.GetCurrencyAmount(sharedCurrency1) + CurrencyManager.Instance.GetCurrencyAmount(sharedCurrency2);
            Assert.IsTrue(actualTotal == 100, $"Total actual currencies should be capped at 100, actual: {actualTotal}");

            UnityEngine.Object.DestroyImmediate(sharedGroup);
            UnityEngine.Object.DestroyImmediate(sharedCurrency1);
            UnityEngine.Object.DestroyImmediate(sharedCurrency2);
            UnityEngine.Object.DestroyImmediate(producer1);
            UnityEngine.Object.DestroyImmediate(producer2);
        }

        [Test]
        public void CalculatingProfitDoesNotAffectRealWorld()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessAutoProduce, 1);
            BigNumber initialCurrency = CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit);

            bool currencyChangedFired = false;
            UnityEngine.Events.UnityAction<Currency> listener = (c) => { currencyChangedFired = true; };
            CurrencyManager.Instance.OnCurrencyAmountChanged.AddListener(listener);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(10));

            CurrencyManager.Instance.OnCurrencyAmountChanged.RemoveListener(listener);

            Assert.IsFalse(currencyChangedFired, "Calculating profit should NOT fire CurrencyManager events.");
            Assert.AreEqual(initialCurrency, CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit), "Calculating profit should NOT change live currency amounts.");
            Assert.IsTrue(data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit) > 0, "Simulation should have produced some currency internally.");
        }

        [Test]
        public void IdleProfitPreservesBusinessUpgradeLevelDuringSimulation()
        {
            // Regression test: the offline simulation clones live BusinessHolders, but used to only
            // copy `business` and `amount`, silently resetting `upgradableLevel` to 0. Any business
            // whose real upgrade level raises its max-amount capacity above its current amount would
            // then simulate as if it had no room left to produce, reporting zero output.
            Business upgradedBusiness = ScriptableObject.CreateInstance<Business>();
            upgradedBusiness.businessName = "TestUpgradedCapacityBusiness";
            upgradedBusiness.timeToProduce = 1;
            upgradedBusiness.autoProduce = true;
            upgradedBusiness.autoLevelUp = false; // avoid unrelated business-level system triggering on the large ForceBuyItemsForFree grant
            upgradedBusiness.productionCost = new System.Collections.Generic.List<Input>();
            upgradedBusiness.cost = new System.Collections.Generic.List<Input>();
            upgradedBusiness.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { businessOutput = upgradedBusiness, outputAmount = 1, outputMaxAmount = 1 });
            upgradedBusiness.outputTables.Add(table);

            // Level 0 caps ownership well below the amount we'll own; level 1 raises it far above.
            upgradedBusiness.upgradeLevels = new BusinessUpgradeLevel[]
            {
                new BusinessUpgradeLevel { level = 0, maxAmount_ = 1000 },
                new BusinessUpgradeLevel { level = 1, maxAmount_ = 200000 },
            };

            // Set the holder's upgrade level to 1 BEFORE granting the amount, since AddAmount clamps
            // to the current level's capacity.
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(upgradedBusiness);
            holder.iUpgradable.Level = 1;
            BusinessesManager.Instance.ForceBuyItemsForFree(upgradedBusiness, 95294);

            Assert.AreEqual(new BigNumber(95294), holder.amount, "Sanity check: real holder should own 95294 at upgrade level 1.");

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(10));

            Assert.IsTrue(data.GetAdditionalBusinessAmount(upgradedBusiness) > 0,
                $"Simulated production should use the holder's real upgrade level (capacity 200000), not reset to level 0 (capacity 1000). " +
                $"Actual additional amount: {data.GetAdditionalBusinessAmount(upgradedBusiness)}");

            UnityEngine.Object.DestroyImmediate(upgradedBusiness);
        }

        [Test]
        public void CalculatingProfitDoesNotMutateRealManagerActiveTimer()
        {
            // Regression test: unlike currencies/businesses (cloned) and boosts (cloned, then
            // restored), ManagersManager.Instance.OnTimePassed() was invoked directly on the real,
            // live ManagerHolders with no save/restore. Merely calculating (not applying) offline
            // profit would permanently drain a manager's real active-boost timer / cooldown.
            Manager activeManager = ScriptableObject.CreateInstance<Manager>();
            activeManager.managerName = "TestActiveTimerManager";
            activeManager.cost = new System.Collections.Generic.List<Input>();
            activeManager.upgradeCost = new System.Collections.Generic.List<Input>();
            activeManager.locks = new System.Collections.Generic.List<Lock>();
            activeManager.passiveBoosts = new System.Collections.Generic.List<UpgradeBlock>();
            activeManager.activeUpgrade = new System.Collections.Generic.List<UpgradeBlock>();
            activeManager.activeDuration = new Duration(0, 0, 10, 0);
            activeManager.cooldownDuration = new Duration(0, 0, 0, 0);

            ManagersManager.Instance.AddManagers(activeManager, 1);
            ManagersManager.Instance.TryActivateManager(activeManager);

            ManagerHolder holder = ManagersManager.Instance.GetOrAddHolder(activeManager);
            double activeTimeBefore = holder.activeTimeLeft;
            Assert.IsTrue(activeTimeBefore > 0, "Sanity check: manager should be active with time remaining.");

            ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(2));

            Assert.AreEqual(activeTimeBefore, holder.activeTimeLeft, 0.0001,
                $"CalculateProfit only calculates and must not mutate the real manager's active timer. Before: {activeTimeBefore}, After: {holder.activeTimeLeft}");

            UnityEngine.Object.DestroyImmediate(activeManager);
        }

        [Test]
        public void IdleProfitWithConsumeOnProduce()
        {
            Business consumeBusiness = ScriptableObject.CreateInstance<Business>();
            consumeBusiness.businessName = "ConsumeBusiness";
            consumeBusiness.timeToProduce = 1; // 1 second
            consumeBusiness.autoProduce = true;
            consumeBusiness.autoCollect = true;
            consumeBusiness.consumeOnProduce = true;

            consumeBusiness.outputTables = new System.Collections.Generic.List<DropTable>();
            consumeBusiness.productionCost = new System.Collections.Generic.List<Input>();
            consumeBusiness.cost = new System.Collections.Generic.List<Input>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output
            {
                currencyOutput = TestUtilities.TestCurrencyProfit,
                outputAmount = 10,
                outputMaxAmount = 10
            });
            consumeBusiness.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(consumeBusiness, 5); // Start with 5 instances

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(10));
            
            Assert.AreEqual(new BigNumber(50), data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit), "Should only produce once with the available 5 instances because they are consumed on produce.");
            Assert.AreEqual(new BigNumber(-5), data.GetAdditionalBusinessAmount(consumeBusiness), "Should record that 5 instances were consumed.");

            ProfitCalculator.ApplyProfit(data);

            Assert.AreEqual(new BigNumber(50), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit), "Applied currency should be 50.");
            Assert.AreEqual(new BigNumber(0), BusinessesManager.Instance.iBuyable.GetItemAmount(consumeBusiness), "Business amount should be 0 because instances were consumed.");

            UnityEngine.Object.DestroyImmediate(consumeBusiness);
        }

        [Test]
        public void IdleProfitWithScaledProductionCost()
        {
            Business scaledCostBusiness = ScriptableObject.CreateInstance<Business>();
            scaledCostBusiness.businessName = "ScaledCostBusiness";
            scaledCostBusiness.timeToProduce = 1; // 1 second
            scaledCostBusiness.autoProduce = true;
            scaledCostBusiness.scaleProductionCostWithAmount = true;

            scaledCostBusiness.productionCost = new System.Collections.Generic.List<Input>
            {
                new Input { currencyInput = TestUtilities.TestCurrencyProfit, inputAmount = 10 }
            };
            scaledCostBusiness.cost = new System.Collections.Generic.List<Input>();
            scaledCostBusiness.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { currencyOutput = TestUtilities.TestCurrency2Profit, outputAmount = 100, outputMaxAmount = 100 });
            scaledCostBusiness.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(scaledCostBusiness, 5); // 5 instances
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrencyProfit, 150); // Enough for 3 rounds (5 instances * 10 cost = 50 per round)

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(10));
            
            Assert.AreEqual(new BigNumber(-150), data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit), "Should have consumed 150 currency.");
            Assert.AreEqual(new BigNumber(1500), data.GetCurrencyAmount(TestUtilities.TestCurrency2Profit), "Should have produced 1500 output.");

            UnityEngine.Object.DestroyImmediate(scaledCostBusiness);
        }

        [Test]
        public void IdleProfitWithBusinessAsCost()
        {
            Business businessAsCost = ScriptableObject.CreateInstance<Business>();
            businessAsCost.businessName = "BusinessAsCost";
            businessAsCost.timeToProduce = 1;
            businessAsCost.autoProduce = true;

            businessAsCost.productionCost = new System.Collections.Generic.List<Input>
            {
                new Input { businessInput = TestUtilities.TestBusinessAutoProduce, inputAmount = 5 }
            };
            businessAsCost.cost = new System.Collections.Generic.List<Input>();
            businessAsCost.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { currencyOutput = TestUtilities.TestCurrency2Profit, outputAmount = 10, outputMaxAmount = 10 });
            businessAsCost.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(businessAsCost, 1);
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessAutoProduce, 12); // Enough for 2 rounds

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(10));
            
            Assert.AreEqual(new BigNumber(-10), data.GetAdditionalBusinessAmount(TestUtilities.TestBusinessAutoProduce), "Should have consumed 10 businesses.");
            Assert.AreEqual(new BigNumber(20), data.GetCurrencyAmount(TestUtilities.TestCurrency2Profit), "Should have produced 20 currency (2 rounds).");

            UnityEngine.Object.DestroyImmediate(businessAsCost);
        }

        [Test]
        public void IdleProfitWithIncreaseProductionAmountAfterRoundEnd()
        {
            Business expBusiness = ScriptableObject.CreateInstance<Business>();
            expBusiness.businessName = "ExponentialBusiness";
            expBusiness.timeToProduce = 10; // 10 seconds per round
            expBusiness.autoProduce = true;
            expBusiness.productionRoundMode = ProductionRoundMode.SingleRoundSnapshot;
            expBusiness.productionCost = new System.Collections.Generic.List<Input>();
            expBusiness.cost = new System.Collections.Generic.List<Input>();
            expBusiness.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { businessOutput = expBusiness, outputAmount = 1, outputMaxAmount = 1 });
            expBusiness.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(expBusiness, 1);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(15));
            Assert.AreEqual(new BigNumber(1), data.GetAdditionalBusinessAmount(expBusiness), "Should produce exactly 1 additional unit after 1 round, despite extra time passing, because mid-round additions don't increase production instantly here.");

            UnityEngine.Object.DestroyImmediate(expBusiness);
        }

        [Test]
        public void IdleProfitWithAutoProduceOverrideBoost()
        {
            Business overrideBusiness = ScriptableObject.CreateInstance<Business>();
            overrideBusiness.businessName = "OverrideBusiness";
            overrideBusiness.timeToProduce = 1;
            overrideBusiness.autoProduce = false;
            overrideBusiness.businessGroup = "OverrideGroupString";
            
            overrideBusiness.productionCost = new System.Collections.Generic.List<Input>();
            overrideBusiness.cost = new System.Collections.Generic.List<Input>();
            overrideBusiness.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { currencyOutput = TestUtilities.TestCurrencyProfit, outputAmount = 10, outputMaxAmount = 10 });
            overrideBusiness.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(overrideBusiness, 1);

            ProfitData dataWithout = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(5));
            Assert.AreEqual(new BigNumber(0), dataWithout.GetCurrencyAmount(TestUtilities.TestCurrencyProfit), "Should not produce without boost or manager.");

            AutoProduceBoost boost = ScriptableObject.CreateInstance<AutoProduceBoost>();
            boost.group = "OverrideGroupString";
            boost.duration = new Duration(0, 0, 0, 10);
            
            BoostsManager.Instance.AddBoosts(boost, 1);
            BoostsManager.Instance.TryUseBoost(boost);
            
            ProfitData dataWith = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(5));
            Assert.AreEqual(new BigNumber(50), dataWith.GetCurrencyAmount(TestUtilities.TestCurrencyProfit), "Should produce 50 with override active.");

            UnityEngine.Object.DestroyImmediate(boost);
            UnityEngine.Object.DestroyImmediate(overrideBusiness);
        }

        [Test]
        public void ProfitDataMultiplyProfits()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessAutoProduce, 1);
            
            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromMinutes(1)); // 60 produced
            Assert.AreEqual(new BigNumber(60), data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit));

            ProfitData multipliedData = data.MultiplyProfits(2.5f);
            
            Assert.AreEqual(new BigNumber(150), multipliedData.GetCurrencyAmount(TestUtilities.TestCurrencyProfit), "Should multiply profits by 2.5.");
            Assert.AreEqual(data.elapsedTime, multipliedData.elapsedTime, "Elapsed time should remain unchanged.");
        }

        [Test]
        public void IdleProfitWithZeroCostUnownedBusinessInputDoesNotThrow()
        {
            // Regression test: GetMaxProductionRounds/RemoveInput indexed the simulated business
            // dictionary directly (`businesses[item.businessInput]`). That dictionary only contains
            // businesses the player owns plus businesses produced as someone's output, so a
            // productionCost referencing a business the player never owns (and that nothing
            // produces) is absent from it. As long as the cost's inputAmount is <= 0, the
            // affordability pre-check in CanAutoProduce (which safely defaults missing entries to 0)
            // still passes, so the business proceeds into production and used to crash with a
            // KeyNotFoundException instead of just treating the missing business as owning 0.
            Business neverOwnedBusiness = ScriptableObject.CreateInstance<Business>();
            neverOwnedBusiness.businessName = "NeverOwnedCostBusiness";

            Business consumer = ScriptableObject.CreateInstance<Business>();
            consumer.businessName = "ConsumerWithZeroCostBusinessInput";
            consumer.timeToProduce = 1;
            consumer.autoProduce = true;
            consumer.productionCost = new System.Collections.Generic.List<Input>
            {
                new Input { businessInput = neverOwnedBusiness, inputAmount = 0 }
            };
            consumer.cost = new System.Collections.Generic.List<Input>();
            consumer.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { currencyOutput = TestUtilities.TestCurrencyProfit, outputAmount = 10, outputMaxAmount = 10 });
            consumer.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(consumer, 1);

            ProfitData data = null;
            Assert.DoesNotThrow(() => data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(5)),
                "CalculateProfit must not throw when a production cost references a business the player never owns with inputAmount <= 0.");
            Assert.AreEqual(new BigNumber(50), data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit),
                "Production should proceed treating the missing business input as owning 0.");

            UnityEngine.Object.DestroyImmediate(neverOwnedBusiness);
            UnityEngine.Object.DestroyImmediate(consumer);
        }

        [Test]
        public void IdleProfitWithZeroCostUntouchedCurrencyInputDoesNotThrow()
        {
            // Same bug as above, but for currencyInput: the simulated currency dictionary only
            // contains currencies the player has ever held (CurrencyHolders are created lazily), so
            // a productionCost referencing a currency the player has never touched, with
            // inputAmount <= 0, used to crash with a KeyNotFoundException instead of treating the
            // missing currency as an amount of 0.
            Currency neverTouchedCurrency = ScriptableObject.CreateInstance<Currency>();
            neverTouchedCurrency.currencyName = "NeverTouchedCurrency";

            Business consumer = ScriptableObject.CreateInstance<Business>();
            consumer.businessName = "ConsumerWithZeroCostCurrencyInput";
            consumer.timeToProduce = 1;
            consumer.autoProduce = true;
            consumer.productionCost = new System.Collections.Generic.List<Input>
            {
                new Input { currencyInput = neverTouchedCurrency, inputAmount = 0 }
            };
            consumer.cost = new System.Collections.Generic.List<Input>();
            consumer.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { currencyOutput = TestUtilities.TestCurrencyProfit, outputAmount = 10, outputMaxAmount = 10 });
            consumer.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(consumer, 1);

            ProfitData data = null;
            Assert.DoesNotThrow(() => data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(5)),
                "CalculateProfit must not throw when a production cost references a currency the player has never touched with inputAmount <= 0.");
            Assert.AreEqual(new BigNumber(50), data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit),
                "Production should proceed treating the missing currency input as an amount of 0.");

            UnityEngine.Object.DestroyImmediate(neverTouchedCurrency);
            UnityEngine.Object.DestroyImmediate(consumer);
        }

        [Test]
        public void CalculateProfitWithZeroOrNegativeElapsedTimeReturnsEmptyData()
        {
            // The early-return branch (elapsedTime.TotalSeconds <= 0) was never exercised by any test.
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessAutoProduce, 1);

            ProfitData zeroData = ProfitCalculator.CalculateProfit(TimeSpan.Zero);
            Assert.AreEqual(new BigNumber(0), zeroData.GetCurrencyAmount(TestUtilities.TestCurrencyProfit),
                "Zero elapsed time should produce no profit.");

            ProfitData negativeData = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(-5));
            Assert.AreEqual(new BigNumber(0), negativeData.GetCurrencyAmount(TestUtilities.TestCurrencyProfit),
                "Negative elapsed time should produce no profit.");
        }

        [Test]
        public void CalculateAndApplyProfitAppliesComputedProfitToRealState()
        {
            // CalculateAndApplyProfit (the combined convenience entrypoint) had zero test coverage.
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusinessAutoProduce, 1);

            ProfitApplicationSummary summary = ProfitCalculator.CalculateAndApplyProfit(TimeSpan.FromMinutes(10));

            Assert.AreEqual(new BigNumber(600), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrencyProfit),
                "CalculateAndApplyProfit should compute and apply the idle profit in one call.");
            Assert.AreEqual(new BigNumber(600), summary.AppliedCurrencies[TestUtilities.TestCurrencyProfit],
                "Summary should report the applied currency amount.");
        }

        [Test]
        public void IdleProfitWithBoostExpiringMidSimulation()
        {
            // No existing test exercised a boost expiring PARTWAY through the offline elapsed time
            // (the existing boost tests all use boosts with a 10-day duration, far outliving the
            // simulated period). This is the core behavior `simulateBoosts` exists for, so it deserves
            // direct coverage: with a 300s boost over a 600s (30-segment, 20s/segment) simulation, the
            // boost should cover exactly the first half and expire for the second half.
            GenericMultiplierBoost expiringBoost = ScriptableObject.CreateInstance<GenericMultiplierBoost>();
            expiringBoost.upgradeBlocks = new UpgradeBlock[] { new UpgradeBlock(UpgradeType.production, 5) };
            expiringBoost.duration = new Duration(0, 0, 5, 0); // 300 seconds

            Business boostedBusiness = ScriptableObject.CreateInstance<Business>();
            boostedBusiness.businessName = "BoostExpiryBusiness";
            boostedBusiness.timeToProduce = 1;
            boostedBusiness.autoProduce = true;
            boostedBusiness.productionCost = new System.Collections.Generic.List<Input>();
            boostedBusiness.cost = new System.Collections.Generic.List<Input>();
            boostedBusiness.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { currencyOutput = TestUtilities.TestCurrencyProfit, outputAmount = 10, outputMaxAmount = 10 });
            boostedBusiness.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(boostedBusiness, 1);
            BoostsManager.Instance.AddBoosts(expiringBoost, 1);
            BoostsManager.Instance.TryUseBoost(expiringBoost);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(600));

            // First 300s boosted (10/s * 5x = 50/s -> 15000), last 300s unboosted (10/s -> 3000).
            Assert.AreEqual(new BigNumber(18000), data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit),
                $"Boost should apply only until it expires partway through the simulation. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");

            UnityEngine.Object.DestroyImmediate(expiringBoost);
            UnityEngine.Object.DestroyImmediate(boostedBusiness);
        }

        [Test]
        public void IdleProfitWithSimulateBoostsDisabledKeepsBoostFullyActiveForWholeDuration()
        {
            // Covers the simulateBoosts:false parameter, which previously had zero test coverage.
            GenericMultiplierBoost expiringBoost = ScriptableObject.CreateInstance<GenericMultiplierBoost>();
            expiringBoost.upgradeBlocks = new UpgradeBlock[] { new UpgradeBlock(UpgradeType.production, 5) };
            expiringBoost.duration = new Duration(0, 0, 5, 0); // 300 seconds, shorter than the 600s simulated

            Business boostedBusiness = ScriptableObject.CreateInstance<Business>();
            boostedBusiness.businessName = "BoostNoDecayBusiness";
            boostedBusiness.timeToProduce = 1;
            boostedBusiness.autoProduce = true;
            boostedBusiness.productionCost = new System.Collections.Generic.List<Input>();
            boostedBusiness.cost = new System.Collections.Generic.List<Input>();
            boostedBusiness.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { currencyOutput = TestUtilities.TestCurrencyProfit, outputAmount = 10, outputMaxAmount = 10 });
            boostedBusiness.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(boostedBusiness, 1);
            BoostsManager.Instance.AddBoosts(expiringBoost, 1);
            BoostsManager.Instance.TryUseBoost(expiringBoost);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(600), simulateBoosts: false);

            // With decay disabled, the full 600s should be boosted despite the boost's 300s duration.
            Assert.AreEqual(new BigNumber(30000), data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit),
                $"simulateBoosts: false should keep the boost fully active for the whole simulated duration. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");

            UnityEngine.Object.DestroyImmediate(expiringBoost);
            UnityEngine.Object.DestroyImmediate(boostedBusiness);
        }

        [Test]
        public void IdleProfitWithManagerActiveBoostExpiringMidSimulation()
        {
            // Mirrors the boost-expiry gap above but for a manager's active-boost timer, the exact
            // mechanism CalculatingProfitDoesNotMutateRealManagerActiveTimer verified was no longer
            // mutating real state for. That test only checked the timer wasn't leaked into real state -
            // this one checks the simulated decay actually produces the right numeric result.
            Manager activeManager = ScriptableObject.CreateInstance<Manager>();
            activeManager.managerName = "TestExpiringActiveManager";
            activeManager.cost = new System.Collections.Generic.List<Input>();
            activeManager.upgradeCost = new System.Collections.Generic.List<Input>();
            activeManager.locks = new System.Collections.Generic.List<Lock>();
            activeManager.passiveBoosts = new System.Collections.Generic.List<UpgradeBlock>();
            activeManager.activeUpgrade = new System.Collections.Generic.List<UpgradeBlock> { new UpgradeBlock(UpgradeType.production, 3) };
            activeManager.activeDuration = new Duration(0, 0, 5, 0); // 300 seconds
            activeManager.cooldownDuration = new Duration(0, 0, 0, 0);

            Business managedBusiness = ScriptableObject.CreateInstance<Business>();
            managedBusiness.businessName = "ManagerActiveBoostBusiness";
            managedBusiness.timeToProduce = 1;
            managedBusiness.autoProduce = true;
            managedBusiness.manager = activeManager;
            managedBusiness.productionCost = new System.Collections.Generic.List<Input>();
            managedBusiness.cost = new System.Collections.Generic.List<Input>();
            managedBusiness.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { currencyOutput = TestUtilities.TestCurrencyProfit, outputAmount = 10, outputMaxAmount = 10 });
            managedBusiness.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(managedBusiness, 1);
            ManagersManager.Instance.AddManagers(activeManager, 1);
            ManagersManager.Instance.TryActivateManager(activeManager);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(600));

            // First 300s boosted (10/s * 3x = 30/s -> 9000), last 300s unboosted (10/s -> 3000).
            Assert.AreEqual(new BigNumber(12000), data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit),
                $"Manager's active boost should apply only until it expires partway through the simulation. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");

            UnityEngine.Object.DestroyImmediate(activeManager);
            UnityEngine.Object.DestroyImmediate(managedBusiness);
        }

        [Test]
        public void IdleProfitWithSimulateActiveManagersDisabledKeepsManagerFullyActiveForWholeDuration()
        {
            // Covers the simulateActiveManagers:false parameter, which previously had zero test coverage.
            Manager activeManager = ScriptableObject.CreateInstance<Manager>();
            activeManager.managerName = "TestNoDecayActiveManager";
            activeManager.cost = new System.Collections.Generic.List<Input>();
            activeManager.upgradeCost = new System.Collections.Generic.List<Input>();
            activeManager.locks = new System.Collections.Generic.List<Lock>();
            activeManager.passiveBoosts = new System.Collections.Generic.List<UpgradeBlock>();
            activeManager.activeUpgrade = new System.Collections.Generic.List<UpgradeBlock> { new UpgradeBlock(UpgradeType.production, 3) };
            activeManager.activeDuration = new Duration(0, 0, 5, 0); // 300 seconds, shorter than the 600s simulated
            activeManager.cooldownDuration = new Duration(0, 0, 0, 0);

            Business managedBusiness = ScriptableObject.CreateInstance<Business>();
            managedBusiness.businessName = "ManagerNoDecayBusiness";
            managedBusiness.timeToProduce = 1;
            managedBusiness.autoProduce = true;
            managedBusiness.manager = activeManager;
            managedBusiness.productionCost = new System.Collections.Generic.List<Input>();
            managedBusiness.cost = new System.Collections.Generic.List<Input>();
            managedBusiness.outputTables = new System.Collections.Generic.List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output { currencyOutput = TestUtilities.TestCurrencyProfit, outputAmount = 10, outputMaxAmount = 10 });
            managedBusiness.outputTables.Add(table);

            BusinessesManager.Instance.ForceBuyItemsForFree(managedBusiness, 1);
            ManagersManager.Instance.AddManagers(activeManager, 1);
            ManagersManager.Instance.TryActivateManager(activeManager);

            ProfitData data = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(600), simulateActiveManagers: false);

            // With decay disabled, the full 600s should be boosted despite the manager's 300s active duration.
            Assert.AreEqual(new BigNumber(18000), data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit),
                $"simulateActiveManagers: false should keep the manager's active boost fully active for the whole simulated duration. Actual: {data.GetCurrencyAmount(TestUtilities.TestCurrencyProfit)}");

            UnityEngine.Object.DestroyImmediate(activeManager);
            UnityEngine.Object.DestroyImmediate(managedBusiness);
        }
    }
}
