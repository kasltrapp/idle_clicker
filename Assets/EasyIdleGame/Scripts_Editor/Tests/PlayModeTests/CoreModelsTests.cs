using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace EasyIdleGame.Tests
{
    public class CoreModelsTests
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
        public void Level_AddXp_HandlesLevelUpSpillover()
        {
            Level level = new Level(100, 2); // Target: 100 * 2^(0) = 100 for level 0. Level 1 will be 100*2 = 200

            level.AddXp(350); // Level 0 costs 100. Next level costs 200. Total = 300. Remainder 50.

            Assert.AreEqual(2, level.unclaimedLevelups, "Should have 2 unclaimed levelups.");
            Assert.AreEqual(new BigNumber(50), level.currentXp, "Remaining XP should be 50.");
        }

        [Test]
        public void Reward_OutputApplication_AppliesToManagers()
        {
            Reward reward = ScriptableObject.CreateInstance<Reward>();
            reward.rewardTables.Add(new DropTable {
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output
                {
                    currencyOutput = TestUtilities.TestCurrency,
                    outputAmount = 100
                } }
            });

            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Initial amount should be 0.");

            reward.rewardTables.ApplyOutputs(2); // 100 * 2 = 200

            Assert.AreEqual(new BigNumber(200), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Reward should be applied properly.");

            Object.DestroyImmediate(reward);
        }

        [Test]
        public void Output_GetAmount_ReturnsCorrectValues()
        {
            Output output = new Output { outputAmount = 10, outputMaxAmount = 10 };
            Assert.AreEqual(new BigNumber(10), output.GetAmount(), "GetAmount should return exact amount if max is not greater than base amount.");
            Assert.AreEqual(new BigNumber(10), output.GetAverageAmount(), "GetAverageAmount should return exact amount if max is not greater than base amount.");

            output.outputMaxAmount = 20;
            BigNumber randomVal = output.GetAmount();
            Assert.IsTrue(randomVal >= 10 && randomVal <= 20, "GetAmount should return value between outputAmount and outputMaxAmount when randomized.");

            Assert.AreEqual(new BigNumber(15), output.GetAverageAmount(), "GetAverageAmount should return the exact average between outputAmount and outputMaxAmount.");
        }

        [Test]
        public void OutputListExtension_Squash_SumsAmountAndMaxAmount()
        {
            Business testBusiness = ScriptableObject.CreateInstance<Business>();

            var outputs = new System.Collections.Generic.List<Output>
            {
                new Output { businessOutput = testBusiness, outputAmount = 5, outputMaxAmount = 10 },
                new Output { businessOutput = testBusiness, outputAmount = 15, outputMaxAmount = 15 },
                new Output { businessOutput = testBusiness, outputAmount = 2, outputMaxAmount = 2 },
            };

            var squashed = outputs.Squash();

            Assert.AreEqual(1, squashed.Count, "Should squash into 1 group based on the business type.");

            var squashedGroup = squashed[0];
            Assert.AreEqual(new BigNumber(22), squashedGroup.outputAmount, "Should add up all outputAmount values (5 + 15 + 2).");
            Assert.AreEqual(new BigNumber(27), squashedGroup.outputMaxAmount, "Should add up all outputMaxAmount values (10 + 15 + 2).");

            Object.DestroyImmediate(testBusiness);
        }
        [Test]
        public void Output_Integration_ProductionCycleRandomizesAmount()
        {
            Business testBusiness = ScriptableObject.CreateInstance<Business>();
            testBusiness.timeToProduce = 1;
            testBusiness.autoCollect = true;

            testBusiness.outputTables = new System.Collections.Generic.List<DropTable>();
            testBusiness.productionCost = new System.Collections.Generic.List<Input>();
            testBusiness.cost = new System.Collections.Generic.List<Input>();
            
            DropTable t1 = new DropTable();
            t1.outputs.Add(new Output
            {
                currencyOutput = TestUtilities.TestCurrency,
                outputAmount = 100,
                outputMaxAmount = 200
            });
            testBusiness.outputTables.Add(t1);

            BusinessHolder holder = new BusinessHolder(testBusiness, 1);
            CurrencyManager.Instance.holders.Add(new CurrencyHolder(TestUtilities.TestCurrency, 0));

            holder.TryStartProduction();
            holder.activeProductions[0].productionTime = 1;
            holder.HandleUpdate(); // Finishes production and auto-collects

            BigNumber currencyAmount = CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency);

            Assert.IsTrue(currencyAmount >= 100 && currencyAmount <= 200, $"Integration test: Production should yield a random amount between 100 and 200, got {currencyAmount}.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void DropTable_Integration_WeightedRandomAppliesOutputs()
        {
            Business testBusiness = ScriptableObject.CreateInstance<Business>();
            testBusiness.timeToProduce = 1;
            testBusiness.autoCollect = true;

            testBusiness.outputTables = new System.Collections.Generic.List<DropTable>();
            testBusiness.productionCost = new System.Collections.Generic.List<Input>();
            testBusiness.cost = new System.Collections.Generic.List<Input>();
            
            DropTable t1 = new DropTable();
            t1.strategy = DropStrategy.WeightedRandom;
            t1.totalDropAmount = 1;
            
            // 100% weight to this item
            t1.outputs.Add(new Output
            {
                currencyOutput = TestUtilities.TestCurrency,
                outputAmount = 50,
                outputMaxAmount = 50,
                weight = 1
            });
            testBusiness.outputTables.Add(t1);

            BusinessHolder holder = new BusinessHolder(testBusiness, 1);
            CurrencyManager.Instance.holders.Add(new CurrencyHolder(TestUtilities.TestCurrency, 0));

            holder.TryStartProduction();
            holder.activeProductions[0].productionTime = 1;
            holder.HandleUpdate(); // Finishes production and auto-collects

            BigNumber currencyAmount = CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency);

            Assert.AreEqual(new BigNumber(50), currencyAmount, $"Integration test: Weighted random should have produced exactly 50, got {currencyAmount}.");

            Object.DestroyImmediate(testBusiness);
        }
        [Test]
        public void Business_Integration_EvaluatePerInstance_TinyDropChance_TinyAmount()
        {
            Business testBusiness = ScriptableObject.CreateInstance<Business>();
            testBusiness.timeToProduce = 1;
            testBusiness.autoCollect = true;
            testBusiness.dropEvaluationMode = DropEvaluationMode.EvaluatePerInstance; // Normalization

            testBusiness.outputTables = new System.Collections.Generic.List<DropTable>();
            testBusiness.productionCost = new System.Collections.Generic.List<Input>();
            testBusiness.cost = new System.Collections.Generic.List<Input>();
            
            DropTable t1 = new DropTable();
            t1.strategy = DropStrategy.All;
            t1.outputs.Add(new Output
            {
                currencyOutput = TestUtilities.TestCurrency,
                outputAmount = new BigNumber(0.001d),
                outputMaxAmount = new BigNumber(0.001d),
                dropChance = 0.001f
            });
            testBusiness.outputTables.Add(t1);

            // 1,000,000 multiplier guarantees we hit EV scaling
            BusinessHolder holder = new BusinessHolder(testBusiness, 1000000);
            CurrencyManager.Instance.holders.Add(new CurrencyHolder(TestUtilities.TestCurrency, 0));

            holder.TryStartProduction();
            holder.activeProductions[0].productionTime = 1;
            holder.HandleUpdate(); // Finishes production and auto-collects

            BigNumber currencyAmount = CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency);

            // 1,000,000 * 0.001 * 0.001 = 1
            Assert.AreEqual(new BigNumber(1), currencyAmount, $"Integration test: EvaluatePerInstance should have calculated exactly 1 based on EV for tiny numbers, got {currencyAmount}.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void BusinessHolder_ManualProduction_TracksConcurrentRounds()
        {
            Business testBusiness = ScriptableObject.CreateInstance<Business>();
            testBusiness.timeToProduce = 2;
            testBusiness.autoCollect = true;
            testBusiness.consumeOnProduce = false;
            testBusiness.productionRoundMode = ProductionRoundMode.ConcurrentRounds;

            testBusiness.outputTables = new System.Collections.Generic.List<DropTable>();
            testBusiness.productionCost = new System.Collections.Generic.List<Input>();
            testBusiness.cost = new System.Collections.Generic.List<Input>();
            
            DropTable t2 = new DropTable();
            t2.outputs.Add(new Output
            {
                currencyOutput = TestUtilities.TestCurrency,
                outputAmount = 10,
                outputMaxAmount = 10
            });
            testBusiness.outputTables.Add(t2);

            BusinessHolder holder = new BusinessHolder(testBusiness, 10);
            CurrencyManager.Instance.holders.Add(new CurrencyHolder(TestUtilities.TestCurrency, 0));

            bool success = holder.TryProduceManually(3, out _);
            Assert.IsTrue(success, "Manual production should successfully start.");
            Assert.AreEqual(new BigNumber(3), holder.CurrentlyProducing, "Currently producing should be 3.");
            Assert.AreEqual(1, holder.activeProductions.Count, "There should be exactly 1 active production round.");

            bool failSuccess = holder.TryProduceManually(8, out _);
            Assert.IsFalse(failSuccess, "Manual production should fail if requesting more than available (non-busy) instances.");

            bool success2 = holder.TryProduceManually(2, out _);
            Assert.IsTrue(success2, "Second manual production should successfully start.");
            Assert.AreEqual(new BigNumber(5), holder.CurrentlyProducing, "Currently producing should be 5.");
            Assert.AreEqual(2, holder.activeProductions.Count, "There should be exactly 2 active production rounds.");

            holder.activeProductions[0].productionTime += 1;
            holder.activeProductions[1].productionTime += 0.5f;
            holder.HandleUpdate();

            Assert.AreEqual(new BigNumber(10), holder.amount, "Amount should still be 10 as production hasn't finished.");
            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "No currency should be produced yet.");

            holder.activeProductions[0].productionTime += 1.1f;
            holder.HandleUpdate();

            Assert.AreEqual(new BigNumber(10), holder.amount, "Amount should be 10 because consume on produce is false.");
            Assert.AreEqual(new BigNumber(30), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Currency should increase by 30 (10 * 3 instances).");
            Assert.AreEqual(1, holder.activeProductions.Count, "There should be 1 active production round remaining.");
            Assert.AreEqual(new BigNumber(2), holder.CurrentlyProducing, "Currently producing should be 2.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void BusinessHolder_ManualProduction_DefaultSingleRoundRejectsConcurrentRound()
        {
            Business testBusiness = CreateProductionAmountTestBusiness(ProductionRoundMode.SingleRoundLiveAmount);
            BusinessHolder holder = new BusinessHolder(testBusiness, 5);
            CurrencyManager.Instance.holders.Add(new CurrencyHolder(TestUtilities.TestCurrency, 0));

            Assert.IsTrue(holder.TryProduceManually(2, out _), "First manual production round should start.");
            Assert.IsFalse(holder.TryProduceManually(1, out _), "Default single-round businesses should reject another manual round while one is active.");
            Assert.AreEqual(1, holder.activeProductions.Count, "Only the first production round should be active.");
            Assert.AreEqual(new BigNumber(2), holder.CurrentlyProducing, "Only the first requested amount should be producing.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void BusinessHolder_ConsumeOnProduce_DeductsInstances()
        {
            Business testBusiness = ScriptableObject.CreateInstance<Business>();
            testBusiness.timeToProduce = 2;
            testBusiness.autoCollect = true;
            testBusiness.consumeOnProduce = true;

            testBusiness.outputTables = new System.Collections.Generic.List<DropTable>();
            testBusiness.productionCost = new System.Collections.Generic.List<Input>();
            testBusiness.cost = new System.Collections.Generic.List<Input>();
            
            DropTable t3 = new DropTable();
            t3.outputs.Add(new Output
            {
                currencyOutput = TestUtilities.TestCurrency,
                outputAmount = 10,
                outputMaxAmount = 10
            });
            testBusiness.outputTables.Add(t3);

            BusinessHolder holder = new BusinessHolder(testBusiness, 10);
            CurrencyManager.Instance.holders.Add(new CurrencyHolder(TestUtilities.TestCurrency, 0));

            holder.TryProduceManually(3, out _);

            holder.activeProductions[0].productionTime += 2.1f;
            holder.HandleUpdate();

            Assert.AreEqual(new BigNumber(7), holder.amount, "Amount should be 7 because 3 instances were consumed on produce.");
            Assert.AreEqual(new BigNumber(30), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Currency should increase by 30 (10 * 3 instances).");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void BusinessHolder_PartialProductionCostDisabled_RequiresFullScaledCost()
        {
            Business testBusiness = ScriptableObject.CreateInstance<Business>();
            testBusiness.timeToProduce = 1;
            testBusiness.scaleProductionCostWithAmount = true;
            testBusiness.allowPartialProductionCost = false;
            testBusiness.productionCost = new List<Input>
            {
                new Input { currencyInput = TestUtilities.TestCurrency, inputAmount = 10 }
            };
            testBusiness.cost = new List<Input>();
            testBusiness.outputTables = new List<DropTable>();

            BusinessHolder holder = new BusinessHolder(testBusiness, 5);
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 30);

            Assert.IsFalse(holder.CanProduce(), "Default behavior should still require enough inputs for the full production amount.");
            Assert.IsFalse(holder.TryStartProduction(), "Production should not start when only a partial scaled cost can be paid.");
            Assert.AreEqual(0, holder.activeProductions.Count, "No production round should be started.");
            Assert.AreEqual(new BigNumber(30), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Currency should not be consumed.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void BusinessHolder_PartialProductionCostEnabled_StartsAffordableScaledAmount()
        {
            Business costBusiness = ScriptableObject.CreateInstance<Business>();
            Business testBusiness = ScriptableObject.CreateInstance<Business>();
            testBusiness.timeToProduce = 1;
            testBusiness.scaleProductionCostWithAmount = true;
            testBusiness.allowPartialProductionCost = true;
            testBusiness.productionCost = new List<Input>
            {
                new Input { currencyInput = TestUtilities.TestCurrency, inputAmount = 10 },
                new Input { businessInput = costBusiness, inputAmount = 2 }
            };
            testBusiness.cost = new List<Input>();
            testBusiness.outputTables = new List<DropTable>();

            BusinessHolder holder = new BusinessHolder(testBusiness, 5);
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 40);
            BusinessesManager.Instance.ForceBuyItemsForFree(costBusiness, 6);

            Assert.IsTrue(holder.CanProduce(), "Partial production should be possible when at least one instance can be paid for.");
            Assert.IsTrue(holder.TryStartProduction(), "Production should start for the affordable amount.");

            Assert.AreEqual(1, holder.activeProductions.Count, "One partial production round should be active.");
            Assert.AreEqual(new BigNumber(3), holder.activeProductions[0].productionAmount, "Business input cost should cap the production amount at 3.");
            Assert.AreEqual(new BigNumber(10), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Only the partial currency cost should be consumed.");
            Assert.AreEqual(new BigNumber(0), BusinessesManager.Instance.GetHolder(costBusiness).amount, "Only the partial business cost should be consumed.");

            Object.DestroyImmediate(costBusiness);
            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void BusinessHolder_PartialProductionCostEnabled_ActualProductionUsesAffordableRoundAmount()
        {
            Business testBusiness = CreateProductionAmountTestBusiness();
            testBusiness.scaleProductionCostWithAmount = true;
            testBusiness.allowPartialProductionCost = true;
            testBusiness.productionCost = new List<Input>
            {
                new Input { currencyInput = TestUtilities.TestCurrency, inputAmount = 10 }
            };

            BusinessHolder holder = new BusinessHolder(testBusiness, 5);
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 30);

            Assert.IsTrue(holder.TryStartProduction(), "Production should start for the largest affordable amount.");
            Assert.AreEqual(new BigNumber(3), holder.activeProductions[0].productionAmount, "The active round should store the affordable amount.");

            List<Output> actualProduction = holder.iProducable.GetActualProduction();
            List<Output> optimisticProduction = holder.iProducable.GetOptimisticActualProduction();

            Assert.AreEqual(1, actualProduction.Count, "Actual production should include the active partial round.");
            Assert.AreEqual(new BigNumber(30), actualProduction[0].outputAmount, "Actual production should use the partial round amount, not the owned amount.");
            Assert.AreEqual(new BigNumber(50), optimisticProduction[0].outputAmount, "Optimistic production should still describe all owned copies.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void BusinessHolder_GetActualProduction_SumsAllActiveProductionRounds()
        {
            Business testBusiness = CreateProductionAmountTestBusiness(ProductionRoundMode.ConcurrentRounds);
            BusinessHolder holder = new BusinessHolder(testBusiness, 10);

            Assert.IsTrue(holder.TryProduceManually(3, out _), "First concurrent production round should start.");
            Assert.IsTrue(holder.TryProduceManually(2, out _), "Second concurrent production round should start.");

            List<Output> actualProduction = holder.iProducable.GetActualProduction();

            Assert.AreEqual(1, actualProduction.Count, "Actual production should squash matching outputs from active rounds.");
            Assert.AreEqual(new BigNumber(50), actualProduction[0].outputAmount, "Actual production should sum every active round's rolled output.");

            Object.DestroyImmediate(testBusiness);
        }
        [Test]
        public void BusinessHolder_DefaultSingleRoundLiveAmount_RefreshesActiveRoundOutputs()
        {
            Business testBusiness = CreateProductionAmountTestBusiness();
            BusinessHolder holder = new BusinessHolder(testBusiness, 1);
            CurrencyManager.Instance.holders.Add(new CurrencyHolder(TestUtilities.TestCurrency, 0));

            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency));

            Assert.AreEqual(ProductionRoundMode.SingleRoundLiveAmount, testBusiness.ProductionRoundMode, "Single round live amount should be the default production round mode.");
            Assert.IsTrue(holder.TryStartProduction(), "Initial production should start.");
            Assert.AreEqual(new BigNumber(1), holder.activeProductions[0].productionAmount, "Initial round should start with the original amount.");

            holder.AddAmount(4);

            Assert.AreEqual(new BigNumber(5), holder.amount, "Owned amount should increase immediately.");
            Assert.AreEqual(new BigNumber(5), holder.activeProductions[0].productionAmount, "Default live-amount mode should refresh the active round to include newly added copies.");
            Assert.IsFalse(holder.TryStartProduction(), "Default single-round businesses should not start another round while one is active.");

            holder.activeProductions[0].productionTime = 1f;
            holder.HandleUpdate();

            Assert.AreEqual(new BigNumber(50), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Finished round should pay for all copies after the active round was refreshed.");
            Assert.AreEqual(0, holder.activeProductions.Count, "No extra round should be required for the newly added copies.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void BusinessHolder_SingleRoundSnapshot_KeepsAddedCopiesForNextRound()
        {
            Business testBusiness = CreateProductionAmountTestBusiness(ProductionRoundMode.SingleRoundSnapshot);
            BusinessHolder holder = new BusinessHolder(testBusiness, 1);
            CurrencyManager.Instance.holders.Add(new CurrencyHolder(TestUtilities.TestCurrency, 0));

            Assert.IsTrue(holder.TryStartProduction(), "Initial production should start.");
            Assert.AreEqual(new BigNumber(1), holder.activeProductions[0].productionAmount, "Initial round should start with the original amount.");

            holder.AddAmount(4);

            Assert.AreEqual(new BigNumber(5), holder.amount, "Owned amount should increase immediately.");
            Assert.AreEqual(new BigNumber(1), holder.activeProductions[0].productionAmount, "Snapshot mode should keep the active round's original amount.");
            Assert.IsFalse(holder.TryStartProduction(), "Snapshot mode is still a single-round mode and should not start another round while one is active.");

            holder.activeProductions[0].productionTime = 1f;
            holder.HandleUpdate();

            Assert.AreEqual(new BigNumber(10), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Finished round should only pay for the original producing copy.");
            Assert.AreEqual(new BigNumber(5), holder.ProductionAmount, "Production amount should refresh to the owned amount after the round ends.");
            Assert.IsTrue(holder.TryStartProduction(), "Next round should be allowed to start with all owned copies.");
            Assert.AreEqual(new BigNumber(5), holder.activeProductions[0].productionAmount, "Next round should include the added copies.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void BusinessHolder_ConcurrentRounds_StartsAddedCopiesSeparately()
        {
            Business testBusiness = CreateProductionAmountTestBusiness(ProductionRoundMode.ConcurrentRounds);
            BusinessHolder holder = new BusinessHolder(testBusiness, 1);
            CurrencyManager.Instance.holders.Add(new CurrencyHolder(TestUtilities.TestCurrency, 0));

            Assert.IsTrue(holder.TryStartProduction(), "Initial production should start.");

            holder.AddAmount(4);

            Assert.AreEqual(new BigNumber(1), holder.activeProductions[0].productionAmount, "Existing round should keep its original amount.");
            Assert.IsTrue(holder.TryStartProduction(), "Added copies should be able to start a separate round when concurrent rounds are enabled.");
            Assert.AreEqual(2, holder.activeProductions.Count, "The added copies should start their own production round.");
            Assert.AreEqual(new BigNumber(4), holder.activeProductions[1].productionAmount, "The new round should only include copies that are not already producing.");

            holder.activeProductions[0].productionTime = 1f;
            holder.HandleUpdate();

            Assert.AreEqual(new BigNumber(10), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "First round should only pay for the original producing copy.");
            Assert.AreEqual(1, holder.activeProductions.Count, "The added copies should still be producing in their separate round.");
            Assert.AreEqual(new BigNumber(4), holder.activeProductions[0].productionAmount, "The separate round should keep the added copies.");

            holder.activeProductions[0].productionTime = 1f;
            holder.HandleUpdate();

            Assert.AreEqual(new BigNumber(50), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Second round should pay for the added copies.");
            Assert.AreEqual(new BigNumber(5), holder.ProductionAmount, "Production amount should refresh to the owned amount after all rounds end.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void BusinessHolder_ConcurrentRoundsAutoProduction_StartsNewRoundWhenBusinessIsBought()
        {
            Business testBusiness = CreateProductionAmountTestBusiness(ProductionRoundMode.ConcurrentRounds);
            testBusiness.timeToProduce = 10f;
            testBusiness.autoProduce = true;
            BusinessHolder holder = new BusinessHolder(testBusiness, 1);
            CurrencyManager.Instance.holders.Add(new CurrencyHolder(TestUtilities.TestCurrency, 0));

            Assert.IsTrue(holder.TryStartProduction(), "Initial production should start.");

            holder.AddAmount(4);
            holder.HandleUpdate();

            Assert.AreEqual(2, holder.activeProductions.Count, "Concurrent auto-production should start a new round for bought copies on update.");
            Assert.AreEqual(new BigNumber(1), holder.activeProductions[0].productionAmount, "Original round should keep the original amount.");
            Assert.AreEqual(new BigNumber(4), holder.activeProductions[1].productionAmount, "Auto-started round should contain the newly bought idle copies.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void Business_ProductionRoundMode_MigratesLegacyIncreaseProductionAmountAfterRoundEnd()
        {
            Business snapshotBusiness = ScriptableObject.CreateInstance<Business>();
            Business liveAmountBusiness = ScriptableObject.CreateInstance<Business>();

#pragma warning disable 0618
            snapshotBusiness.increaseProductionAmountAfterRoundEnd = true;
            liveAmountBusiness.increaseProductionAmountAfterRoundEnd = false;
#pragma warning restore 0618

            snapshotBusiness.OnAfterDeserialize();
            liveAmountBusiness.OnAfterDeserialize();

            Assert.AreEqual(ProductionRoundMode.SingleRoundSnapshot, snapshotBusiness.ProductionRoundMode, "Legacy enabled flag should migrate to snapshot single-round mode.");
            Assert.AreEqual(ProductionRoundMode.SingleRoundLiveAmount, liveAmountBusiness.ProductionRoundMode, "Legacy disabled flag should migrate to live-amount single-round mode.");

            Object.DestroyImmediate(snapshotBusiness);
            Object.DestroyImmediate(liveAmountBusiness);
        }

        [Test]
        public void Business_ProductionRoundMode_UsesLegacyIncreaseFlagBeforeMigration()
        {
            Business testBusiness = ScriptableObject.CreateInstance<Business>();

#pragma warning disable 0618
            testBusiness.increaseProductionAmountAfterRoundEnd = true;
#pragma warning restore 0618

            Assert.AreEqual(ProductionRoundMode.SingleRoundSnapshot, testBusiness.ProductionRoundMode, "Legacy enabled flag should still read as snapshot mode before serialization migration runs.");

            testBusiness.OnBeforeSerialize();

#pragma warning disable 0618
            Assert.IsTrue(testBusiness.increaseProductionAmountAfterRoundEnd, "Serializing should preserve the legacy enabled flag for old assets and code.");
#pragma warning restore 0618
            Assert.AreEqual(ProductionRoundMode.SingleRoundSnapshot, testBusiness.ProductionRoundMode, "Serialization should not flatten the legacy snapshot mode back to the default.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void Business_ProductionRoundMode_TracksLegacyIncreaseFlagAfterMigration()
        {
            Business testBusiness = ScriptableObject.CreateInstance<Business>();
            testBusiness.OnBeforeSerialize();

#pragma warning disable 0618
            testBusiness.increaseProductionAmountAfterRoundEnd = true;
#pragma warning restore 0618

            Assert.AreEqual(ProductionRoundMode.SingleRoundSnapshot, testBusiness.ProductionRoundMode, "Legacy enabled flag should still switch migrated assets to snapshot mode.");

#pragma warning disable 0618
            testBusiness.increaseProductionAmountAfterRoundEnd = false;
#pragma warning restore 0618

            Assert.AreEqual(ProductionRoundMode.SingleRoundLiveAmount, testBusiness.ProductionRoundMode, "Legacy disabled flag should still switch migrated assets to live-amount mode.");

            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void Business_HasGroupMatch_EvaluatesCorrectly()
        {
            var bg1 = ScriptableObject.CreateInstance<BusinessGroup>(); bg1.name = "bg1";
            var bg2 = ScriptableObject.CreateInstance<BusinessGroup>(); bg2.name = "bg2";
            var bg3 = ScriptableObject.CreateInstance<BusinessGroup>(); bg3.name = "bg3";

            var sourceListEmpty = new List<BusinessGroup>();
            var sourceList1 = new List<BusinessGroup> { bg1, bg2 };

            var targetListEmpty = new List<BusinessGroup>();
            var targetList1 = new List<BusinessGroup> { bg2, bg3 };
            var targetList2 = new List<BusinessGroup> { bg3 };

            // Both empty -> true
            Assert.IsTrue(Business.HasGroupMatch(sourceListEmpty, "", targetListEmpty, ""));

            // 1. String -> String
            Assert.IsTrue(Business.HasGroupMatch(sourceListEmpty, "stringA", targetListEmpty, "stringA"));
            Assert.IsFalse(Business.HasGroupMatch(sourceListEmpty, "stringA", targetListEmpty, "stringB"));

            // 2. String -> List (Target uses String, Source uses List)
            Assert.IsTrue(Business.HasGroupMatch(sourceList1, "", targetListEmpty, "bg1"));
            Assert.IsFalse(Business.HasGroupMatch(sourceList1, "", targetListEmpty, "bg3"));

            // 3. List -> String (Target uses List, Source uses String)
            Assert.IsTrue(Business.HasGroupMatch(sourceListEmpty, "bg2", targetList1, ""));
            Assert.IsFalse(Business.HasGroupMatch(sourceListEmpty, "bg1", targetList1, ""));

            // 4. List -> List (Both use List)
            Assert.IsTrue(Business.HasGroupMatch(sourceList1, "", targetList1, "")); // bg2 is shared
            Assert.IsFalse(Business.HasGroupMatch(sourceList1, "", targetList2, "")); // No shared groups
            
            // 5. String + List Mixed (String has priority in target)
            // Target uses string, so lists are ignored. Source uses string that matches target.
            Assert.IsTrue(Business.HasGroupMatch(sourceListEmpty, "bg1", targetList1, "bg1"));
            // Target uses string, source uses list that contains target string
            Assert.IsTrue(Business.HasGroupMatch(sourceList1, "", targetList1, "bg1"));

            Object.DestroyImmediate(bg1);
            Object.DestroyImmediate(bg2);
            Object.DestroyImmediate(bg3);
        }

        private static Business CreateProductionAmountTestBusiness()
        {
            Business testBusiness = ScriptableObject.CreateInstance<Business>();
            testBusiness.timeToProduce = 1;
            testBusiness.autoCollect = true;
            testBusiness.productionCost = new List<Input>();
            testBusiness.cost = new List<Input>();
            testBusiness.outputTables = new List<DropTable>();

            DropTable table = new DropTable();
            table.outputs.Add(new Output
            {
                currencyOutput = TestUtilities.TestCurrency,
                outputAmount = 10,
                outputMaxAmount = 10
            });
            testBusiness.outputTables.Add(table);

            return testBusiness;
        }

        private static Business CreateProductionAmountTestBusiness(ProductionRoundMode productionRoundMode)
        {
            Business testBusiness = CreateProductionAmountTestBusiness();
            testBusiness.productionRoundMode = productionRoundMode;
            return testBusiness;
        }
    }
}

