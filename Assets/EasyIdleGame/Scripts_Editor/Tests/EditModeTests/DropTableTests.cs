using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class DropTableTests
    {
        private Currency _testCurrency1;
        private Currency _testCurrency2;

        [SetUp]
        public void SetUp()
        {
            _testCurrency1 = ScriptableObject.CreateInstance<Currency>();
            _testCurrency2 = ScriptableObject.CreateInstance<Currency>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testCurrency1);
            Object.DestroyImmediate(_testCurrency2);
        }

        [Test]
        public void DropTable_IndependentStrategy_CalculatesCorrectAverage()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.All;
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = 10, outputMaxAmount = 10, dropChance = 0.5f });

            List<Output> avgOutputs = table.GetAverageOutputs(1);

            Assert.AreEqual(1, avgOutputs.Count, "Independent strategy should return 1 output.");
            Assert.AreEqual(new BigNumber(5), avgOutputs[0].outputAmount, "Average amount should be exactly 5 (10 * 0.5).");
        }

        [Test]
        public void DropTable_WeightedRandomStrategy_CalculatesCorrectAverage()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.WeightedRandom;
            table.totalDropAmount = 2; // Pull 2 items
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = 10, outputMaxAmount = 10, weight = 1 });
            table.outputs.Add(new Output { currencyOutput = _testCurrency2, outputAmount = 20, outputMaxAmount = 20, weight = 3 });

            List<Output> avgOutputs = table.GetAverageOutputs(1);

            Assert.AreEqual(2, avgOutputs.Count, "Weighted strategy should return 2 outputs.");
            
            // Total weight = 4. 
            // Item 1 chance per pull = 1/4 = 0.25. Pulls = 2. Total chance = 0.5. Avg output = 10 * 0.5 = 5.
            // Item 2 chance per pull = 3/4 = 0.75. Pulls = 2. Total chance = 1.5. Avg output = 20 * 1.5 = 30.
            Assert.AreEqual(_testCurrency1, avgOutputs[0].currencyOutput, "First output should be currency 1.");
            Assert.AreEqual(new BigNumber(5), avgOutputs[0].outputAmount, "First average amount should be 5.");
            
            Assert.AreEqual(_testCurrency2, avgOutputs[1].currencyOutput, "Second output should be currency 2.");
            Assert.AreEqual(new BigNumber(30), avgOutputs[1].outputAmount, "Second average amount should be 30.");
        }

        [Test]
        public void DropTable_WeightedRandomStrategy_GeneratesCorrectAmountOfDrops()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.WeightedRandom;
            table.totalDropAmount = 5; 
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = 10, outputMaxAmount = 10, weight = 1 });
            table.outputs.Add(new Output { currencyOutput = _testCurrency2, outputAmount = 10, outputMaxAmount = 10, weight = 0 }); // 0 weight means it shouldn't drop

            List<Output> generated = table.GenerateDrops(1);
            
            Assert.AreEqual(1, generated.Count, "Should return exactly 1 type of output because the other has 0 weight.");
            Assert.AreEqual(_testCurrency1, generated[0].currencyOutput, "Should output currency 1.");
            Assert.AreEqual(new BigNumber(50), generated[0].outputAmount, "Total dropped amount should be 50 (10 * 5 pulls).");
        }
        
        [Test]
        public void DropTable_IndependentStrategy_EmptyState_ReturnsEmpty()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.All;
            
            Assert.AreEqual(0, table.GenerateDrops(1).Count, "GenerateDrops should return empty list for empty outputs.");
            Assert.AreEqual(0, table.GetAverageOutputs(1).Count, "GetAverageOutputs should return empty list for empty outputs.");
        }
        
        [Test]
        public void DropTable_WeightedRandomStrategy_EmptyState_ReturnsEmpty()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.WeightedRandom;
            
            Assert.AreEqual(0, table.GenerateDrops(1).Count, "GenerateDrops should return empty list for empty outputs.");
            Assert.AreEqual(0, table.GetAverageOutputs(1).Count, "GetAverageOutputs should return empty list for empty outputs.");
        }

        [Test]
        public void DropTable_IndependentStrategy_EvaluatePerInstance_SmallNumbers_RollsCorrectly()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.All;
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = 10, outputMaxAmount = 10, dropChance = 1f }); // 100% chance for test

            // amountMultiplier = 5
            List<Output> generated = table.GenerateDrops(5, evaluationMode: DropEvaluationMode.EvaluatePerInstance);

            Assert.AreEqual(1, generated.Count, "Should return 1 output type.");
            Assert.AreEqual(new BigNumber(50), generated[0].outputAmount, "Since chance is 100%, 5 rolls of 10 should yield 50.");
        }

        [Test]
        public void DropTable_IndependentStrategy_EvaluatePerInstance_LargeNumbers_UsesExpectedValue()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.All;
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = 10, outputMaxAmount = 10, dropChance = 0.5f });

            // amountMultiplier = 1000 (> 100 to trigger EV)
            List<Output> generated = table.GenerateDrops(1000, evaluationMode: DropEvaluationMode.EvaluatePerInstance);

            Assert.AreEqual(1, generated.Count, "Should return 1 output type.");
            // EV: 1000 * 0.5 * 10 = 5000
            Assert.AreEqual(new BigNumber(5000), generated[0].outputAmount, "Should use EV for large numbers (1000 * 0.5 * 10).");
        }

        [Test]
        public void DropTable_WeightedRandomStrategy_EvaluatePerInstance_SmallNumbers_RollsCorrectly()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.WeightedRandom;
            table.totalDropAmount = 1;
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = 10, outputMaxAmount = 10, weight = 1 });
            table.outputs.Add(new Output { currencyOutput = _testCurrency2, outputAmount = 10, outputMaxAmount = 10, weight = 0 }); // 0 weight ensures it's always Currency1

            // amountMultiplier = 5
            List<Output> generated = table.GenerateDrops(5, evaluationMode: DropEvaluationMode.EvaluatePerInstance);

            Assert.AreEqual(1, generated.Count, "Should return 1 output type.");
            Assert.AreEqual(new BigNumber(50), generated[0].outputAmount, "Since currency1 always wins, 5 rolls of 10 should yield 50.");
        }

        [Test]
        public void DropTable_WeightedRandomStrategy_EvaluatePerInstance_LargeNumbers_UsesExpectedValue()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.WeightedRandom;
            table.totalDropAmount = 2; // 2 drops per multiplier
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = 10, outputMaxAmount = 10, weight = 1 });
            table.outputs.Add(new Output { currencyOutput = _testCurrency2, outputAmount = 10, outputMaxAmount = 10, weight = 1 });

            // amountMultiplier = 1000 (> 100 to trigger EV)
            List<Output> generated = table.GenerateDrops(1000, evaluationMode: DropEvaluationMode.EvaluatePerInstance);

            Assert.AreEqual(2, generated.Count, "Should return both output types via EV.");
            
            // Total weight = 2. Each has 1/2 chance. 
            // Chance calculation per output: (weight / totalWeight) * totalDropAmount = (1/2) * 2 = 1.
            // Expected amount: outputAmount * chance * amountMultiplier = 10 * 1 * 1000 = 10000.
            Assert.AreEqual(new BigNumber(10000), generated[0].outputAmount, "Should calculate correct EV for first item.");
            Assert.AreEqual(new BigNumber(10000), generated[1].outputAmount, "Should calculate correct EV for second item.");
        }

        [Test]
        public void DropTable_IndependentStrategy_EvaluatePerInstance_TinyDropChance_LargeNumbers_UsesExpectedValue()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.All;
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = 10, outputMaxAmount = 10, dropChance = 0.001f });

            // amountMultiplier = 100,000 (> 100 to trigger EV)
            List<Output> generated = table.GenerateDrops(100000, evaluationMode: DropEvaluationMode.EvaluatePerInstance);

            Assert.AreEqual(1, generated.Count, "Should return 1 output type.");
            // EV: 100000 * 0.001 * 10 = 100 * 10 = 1000
            Assert.AreEqual(new BigNumber(1000), generated[0].outputAmount, "Should use EV for large numbers with tiny chance (100000 * 0.001 * 10 = 1000).");
        }

        [Test]
        public void DropTable_WeightedRandomStrategy_EvaluatePerInstance_TinyDropChance_LargeNumbers_UsesExpectedValue()
        {
            // For WeightedRandom, chance = (weight / totalWeight) * totalDropAmount
            DropTable table = new DropTable();
            table.strategy = DropStrategy.WeightedRandom;
            table.totalDropAmount = 1; 
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = 10, outputMaxAmount = 10, weight = 1 });
            table.outputs.Add(new Output { currencyOutput = _testCurrency2, outputAmount = 10, outputMaxAmount = 10, weight = 999 });

            // Chance for item 1 = 1 / 1000 * 1 = 0.001
            // amountMultiplier = 100,000 (> 100 to trigger EV)
            List<Output> generated = table.GenerateDrops(100000, evaluationMode: DropEvaluationMode.EvaluatePerInstance);

            // Item 1 EV: 100000 * 0.001 * 10 = 1000
            // Item 2 EV: 100000 * 0.999 * 10 = 999000
            
            Assert.AreEqual(2, generated.Count, "Should return 2 output types via EV.");
            
            var firstOutput = generated.Find(o => o.currencyOutput == _testCurrency1);
            var secondOutput = generated.Find(o => o.currencyOutput == _testCurrency2);

            Assert.IsNotNull(firstOutput, "First output should exist");
            Assert.IsNotNull(secondOutput, "Second output should exist");
            Assert.AreEqual(new BigNumber(1000), firstOutput.outputAmount, "Should calculate correct EV for rare item.");
            Assert.AreEqual(new BigNumber(999000), secondOutput.outputAmount, "Should calculate correct EV for common item.");
        }
        
        [Test]
        public void DropTable_IndependentStrategy_EvaluatePerInstance_TinyDropChance_SmallNumbers_RollsCorrectly()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.All;
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = 10, outputMaxAmount = 10, dropChance = 0.001f });

            // amountMultiplier = 10 (<= 100, so it actually rolls)
            // It will most likely yield 0 unless we are very lucky, but it shouldn't crash or behave improperly.
            List<Output> generated = table.GenerateDrops(10, evaluationMode: DropEvaluationMode.EvaluatePerInstance);

            if (generated.Count > 0)
            {
                Assert.AreEqual(1, generated.Count, "Should return 1 output type if rolled successfully.");
                Assert.AreEqual(new BigNumber(10), generated[0].outputAmount, "Should return exactly 10 for a single lucky roll (or a multiple if extremely lucky).");
            }
            else
            {
                Assert.AreEqual(0, generated.Count, "Most of the time a 0.001 drop chance over 10 rolls yields 0 items.");
            }
        }

        [Test]
        public void DropTable_IndependentStrategy_EvaluatePerInstance_TinyDropChance_And_TinyAmount_UsesExpectedValue()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.All;
            // outputAmount = 0.001, dropChance = 0.001
            table.outputs.Add(new Output { currencyOutput = _testCurrency1, outputAmount = new BigNumber(0.001d), outputMaxAmount = new BigNumber(0.001d), dropChance = 0.001f });

            // amountMultiplier = 1,000,000 (> 100 to trigger EV)
            // EV calculation:
            // average amount before chance: 1,000,000 * 0.001 = 1,000
            // total amount after chance: (1,000 * 0.001).Floor() = 1
            List<Output> generated = table.GenerateDrops(1000000, evaluationMode: DropEvaluationMode.EvaluatePerInstance);

            Assert.AreEqual(1, generated.Count, "Should return 1 output type via EV.");
            Assert.AreEqual(new BigNumber(1), generated[0].outputAmount, "Should calculate correct EV for tiny amount and tiny drop chance.");
        }

        [Test]
        public void DropTable_GenerateDrops_MultipleInstances_LocksInRandomAmounts()
        {
            DropTable table = new DropTable();
            table.strategy = DropStrategy.All;
            table.outputs.Add(new Output
            {
                currencyOutput = _testCurrency1,
                outputAmount = 2,
                outputMaxAmount = 6,
                dropChance = 1f
            });

            // Generate drops for 10 instances evaluating per instance
            var drops = table.GenerateDrops(10, DropEvaluationMode.EvaluatePerInstance);

            Assert.AreEqual(1, drops.Count, "Should return 1 grouped output.");
            
            Output generatedOutput = drops[0];
            
            // Should be exactly locked in now (min == max)
            Assert.AreEqual(generatedOutput.outputAmount, generatedOutput.outputMaxAmount, "Generated drop should have min == max to lock in the randomness.");

            BigNumber amount = generatedOutput.outputAmount;

            // Theoretical minimum is 2 * 10 = 20, max is 6 * 10 = 60.
            Assert.IsTrue(amount >= 20 && amount <= 60, $"Randomly rolled amount should be within theoretical bounds (20-60). Actual: {amount}");
        }
    }
}
