using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class ProductionOutputModifierTests
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
        public void BusinessProductionModifierScalesOnlyTargetBusinessOutputAcrossProductionModes()
        {
            Scenario scenario = CreateScenario(includeExistingSelfOutput: true);
            ProductionModifierRegistry.Register(scenario.Modifier);

            BusinessesManager.Instance.ForceBuyItemsForFree(scenario.Business, 100);
            BusinessHolder holder = BusinessesManager.Instance.GetHolder(scenario.Business);

            AssertOutputAmounts(holder.iProducable.GetOptimisticActualProduction(), scenario.Milk, 100, scenario.Business, 2, "optimistic preview");
            AssertOutputAmounts(holder.iProducable.GetActualProductionPerSecond(), scenario.Milk, 100, scenario.Business, 2, "production per second preview");

            ProfitData offlineProfit = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(1));
            Assert.AreEqual(new BigNumber(100), offlineProfit.GetCurrencyAmount(scenario.Milk), "Offline milk production should stay on the normal production path.");
            Assert.AreEqual(new BigNumber(2), offlineProfit.GetAdditionalBusinessAmount(scenario.Business), "Offline self-production should use the scaled modifier amount.");

            Assert.IsTrue(holder.TryProduceManually(100, out ProductionRound round), "Manual production should start for the full owned amount.");
            AssertOutputAmounts(round.rolledOutputs, scenario.Milk, 100, scenario.Business, 2, "actual production round");
            AssertOutputAmounts(holder.iProducable.GetCurrentProductionPerSecond(), scenario.Milk, 100, scenario.Business, 2, "current production preview");

            CompleteProductionRound(holder, round);

            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(scenario.Milk), "Actual runtime production should apply milk to CurrencyManager.");
            Assert.AreEqual(new BigNumber(102), BusinessesManager.Instance.iBuyable.GetItemAmount(scenario.Business), "Actual runtime production should add only the scaled self-output to BusinessesManager.");
            Assert.AreEqual(0, holder.unclaimedOutputs.Count, "Auto-collect should leave no stockpiled production outputs after applying them.");
        }

        [Test]
        public void BusinessProductionModifierCanAddTargetBusinessOutputWhenNoMatchingOutputExists()
        {
            Scenario scenario = CreateScenario(includeExistingSelfOutput: false);
            ProductionModifierRegistry.Register(scenario.Modifier);

            BusinessesManager.Instance.ForceBuyItemsForFree(scenario.Business, 100);
            BusinessHolder holder = BusinessesManager.Instance.GetHolder(scenario.Business);

            AssertOutputAmounts(holder.iProducable.GetOptimisticActualProduction(), scenario.Milk, 100, scenario.Business, 2, "added self-output preview");

            Assert.IsTrue(holder.TryProduceManually(100, out ProductionRound round), "Manual production should start when the source table has no matching business output.");
            AssertOutputAmounts(round.rolledOutputs, scenario.Milk, 100, scenario.Business, 2, "added self-output actual production round");

            CompleteProductionRound(holder, round);

            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(scenario.Milk), "Added self-output production should still apply the unrelated milk output.");
            Assert.AreEqual(new BigNumber(102), BusinessesManager.Instance.iBuyable.GetItemAmount(scenario.Business), "Added self-output production should apply the scaled business output.");
        }

        [Test]
        public void OfflineProfitApplicationAppliesScaledOutputsToManagers()
        {
            Scenario scenario = CreateScenario(includeExistingSelfOutput: true);
            ProductionModifierRegistry.Register(scenario.Modifier);

            BusinessesManager.Instance.ForceBuyItemsForFree(scenario.Business, 100);

            ProfitData offlineProfit = ProfitCalculator.CalculateProfit(TimeSpan.FromSeconds(1));
            Assert.AreEqual(new BigNumber(100), offlineProfit.GetCurrencyAmount(scenario.Milk), "Offline calculation should include milk output before application.");
            Assert.AreEqual(new BigNumber(2), offlineProfit.GetAdditionalBusinessAmount(scenario.Business), "Offline calculation should include scaled self-output before application.");

            ProfitCalculator.ApplyProfit(offlineProfit);

            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(scenario.Milk), "Offline profit application should add milk to CurrencyManager.");
            Assert.AreEqual(new BigNumber(102), BusinessesManager.Instance.iBuyable.GetItemAmount(scenario.Business), "Offline profit application should add only the scaled self-output to BusinessesManager.");
        }

        [Test]
        public void CurrencyScalingModifierAddsBonusCurrencyAcrossPreviewAndApplication()
        {
            CurrencyScenario scenario = CreateCurrencyScenario(replaceExistingMatchingOutput: false);
            ProductionModifierRegistry.Register(scenario.Modifier);

            BusinessesManager.Instance.ForceBuyItemsForFree(scenario.Business, 100);
            BusinessHolder holder = BusinessesManager.Instance.GetHolder(scenario.Business);

            // Linear base 10, 5 per step, 100 owned -> 10 steps -> +50 milk on top of the 100 from the tables.
            Assert.AreEqual(new BigNumber(150), SumCurrencyOutputs(holder.iProducable.GetOptimisticActualProduction(), scenario.Milk), "Optimistic preview should include the bonus currency.");

            Assert.IsTrue(holder.TryProduceManually(100, out ProductionRound round), "Manual production should start for the full owned amount.");
            Assert.AreEqual(new BigNumber(150), SumCurrencyOutputs(round.rolledOutputs, scenario.Milk), "The rolled round should include the bonus currency.");

            CompleteProductionRound(holder, round);

            Assert.AreEqual(new BigNumber(150), CurrencyManager.Instance.GetCurrencyAmount(scenario.Milk), "Applied production should add table output plus the bonus currency.");
            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(scenario.Gold), "Currencies the modifier does not target should stay unchanged.");
        }

        [Test]
        public void CurrencyScalingModifierCanReplaceMatchingCurrencyOutput()
        {
            CurrencyScenario scenario = CreateCurrencyScenario(replaceExistingMatchingOutput: true);
            ProductionModifierRegistry.Register(scenario.Modifier);

            BusinessesManager.Instance.ForceBuyItemsForFree(scenario.Business, 100);
            BusinessHolder holder = BusinessesManager.Instance.GetHolder(scenario.Business);

            List<Output> preview = holder.iProducable.GetOptimisticActualProduction();
            Assert.AreEqual(new BigNumber(50), SumCurrencyOutputs(preview, scenario.Milk), "Replace should remove the table's milk output and grant only the scaled amount.");
            Assert.AreEqual(new BigNumber(100), SumCurrencyOutputs(preview, scenario.Gold), "Replace should not touch outputs the modifier does not target.");
        }

        [Test]
        public void ProductionModifierManagerRegistersAndUnregistersInspectorModifiers()
        {
            Scenario scenario = CreateScenario(includeExistingSelfOutput: true);
            GameObject managerObject = new GameObject("ProductionModifierManagerTest");
            _createdObjects.Add(managerObject);
            managerObject.SetActive(false);

            ProductionModifierManager manager = managerObject.AddComponent<ProductionModifierManager>();
            manager.Modifiers.Add(scenario.Modifier);

            Assert.IsFalse(ProductionModifierRegistry.RegisteredModifiers.Contains(scenario.Modifier), "Inactive manager should not register modifiers yet.");

            managerObject.SetActive(true);
            Assert.IsTrue(ProductionModifierRegistry.RegisteredModifiers.Contains(scenario.Modifier), "Manager should register inspector-assigned modifiers on enable.");

            managerObject.SetActive(false);
            Assert.IsFalse(ProductionModifierRegistry.RegisteredModifiers.Contains(scenario.Modifier), "Manager should unregister inspector-assigned modifiers on disable.");
        }

        [Test]
        public void RegisteredProductionModifierDoesNotChangeGenericDropTableGeneration()
        {
            Scenario scenario = CreateScenario(includeExistingSelfOutput: true);
            ProductionModifierRegistry.Register(scenario.Modifier);

            DropTable rewardTable = new DropTable
            {
                outputs = new List<Output>
                {
                    new Output { businessOutput = scenario.Business, outputAmount = 1, outputMaxAmount = 1 }
                }
            };

            List<Output> generatedRewards = rewardTable.GenerateDrops(100);

            Assert.AreEqual(new BigNumber(100), GetBusinessOutputAmount(generatedRewards, scenario.Business), "Direct DropTable generation should not use production modifiers.");
        }

        private Scenario CreateScenario(bool includeExistingSelfOutput)
        {
            Currency milk = CreateAsset<Currency>();
            milk.currencyName = "Milk";

            Business business = CreateAsset<Business>();
            business.businessName = "Cowshed";
            business.timeToProduce = 1;
            business.autoProduce = true;
            business.autoCollect = true;
            business.autoLevelUp = false;
            business.cost = new List<Input>();
            business.productionCost = new List<Input>();
            business.outputTables = new List<DropTable>();

            List<Output> outputs = new List<Output>
            {
                new Output { currencyOutput = milk, outputAmount = 1, outputMaxAmount = 1 }
            };

            if (includeExistingSelfOutput)
                outputs.Add(new Output { businessOutput = business, outputAmount = 1, outputMaxAmount = 1 });

            business.outputTables.Add(new DropTable { outputs = outputs });

            ProductionBusinessOutputScalingModifierAsset modifier = CreateAsset<ProductionBusinessOutputScalingModifierAsset>();
            modifier.producerBusiness = business;
            modifier.outputBusiness = business;
            modifier.scalingMode = ProductionBusinessOutputScalingMode.LogarithmicFloor;
            modifier.scalingSource = ProductionBusinessOutputScalingSource.OwnedAmount;
            modifier.scalingBase = 10;
            modifier.outputAmountPerStep = 1;
            modifier.replaceExistingMatchingOutput = true;

            return new Scenario
            {
                Business = business,
                Milk = milk,
                Modifier = modifier
            };
        }

        private CurrencyScenario CreateCurrencyScenario(bool replaceExistingMatchingOutput)
        {
            Currency milk = CreateAsset<Currency>();
            milk.currencyName = "Milk";

            Currency gold = CreateAsset<Currency>();
            gold.currencyName = "Gold";

            Business business = CreateAsset<Business>();
            business.businessName = "Cowshed";
            business.timeToProduce = 1;
            business.autoProduce = true;
            business.autoCollect = true;
            business.autoLevelUp = false;
            business.cost = new List<Input>();
            business.productionCost = new List<Input>();
            business.outputTables = new List<DropTable>
            {
                new DropTable
                {
                    outputs = new List<Output>
                    {
                        new Output { currencyOutput = milk, outputAmount = 1, outputMaxAmount = 1 },
                        new Output { currencyOutput = gold, outputAmount = 1, outputMaxAmount = 1 }
                    }
                }
            };

            ProductionBusinessOutputScalingModifierAsset modifier = CreateAsset<ProductionBusinessOutputScalingModifierAsset>();
            modifier.producerBusiness = business;
            modifier.outputCurrency = milk;
            modifier.scalingMode = ProductionBusinessOutputScalingMode.LinearFloor;
            modifier.scalingSource = ProductionBusinessOutputScalingSource.OwnedAmount;
            modifier.scalingBase = 10;
            modifier.outputAmountPerStep = 5;
            modifier.replaceExistingMatchingOutput = replaceExistingMatchingOutput;

            return new CurrencyScenario
            {
                Business = business,
                Milk = milk,
                Gold = gold,
                Modifier = modifier
            };
        }

        private static BigNumber SumCurrencyOutputs(List<Output> outputs, Currency currency)
        {
            BigNumber total = 0;
            foreach (Output output in outputs)
            {
                if (output != null && output.currencyOutput == currency)
                    total += output.outputAmount;
            }

            return total;
        }

        private static void CompleteProductionRound(BusinessHolder holder, ProductionRound round)
        {
            holder.activeProductions.Remove(round);
            holder.OnProductionRoundFinished(round);
        }

        private T CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            _createdObjects.Add(asset);
            return asset;
        }

        private static void AssertOutputAmounts(List<Output> outputs, Currency currency, BigNumber currencyAmount, Business business, BigNumber businessAmount, string context)
        {
            Assert.AreEqual(currencyAmount, GetCurrencyOutputAmount(outputs, currency), $"Currency output mismatch for {context}.");
            Assert.AreEqual(businessAmount, GetBusinessOutputAmount(outputs, business), $"Business output mismatch for {context}.");
        }

        private static BigNumber GetCurrencyOutputAmount(List<Output> outputs, Currency currency)
        {
            Output output = outputs.Find(o => o.currencyOutput == currency);
            return output != null ? output.outputAmount : 0;
        }

        private static BigNumber GetBusinessOutputAmount(List<Output> outputs, Business business)
        {
            Output output = outputs.Find(o => o.businessOutput == business);
            return output != null ? output.outputAmount : 0;
        }

        private sealed class Scenario
        {
            public Business Business;
            public Currency Milk;
            public ProductionBusinessOutputScalingModifierAsset Modifier;
        }

        private sealed class CurrencyScenario
        {
            public Business Business;
            public Currency Milk;
            public Currency Gold;
            public ProductionBusinessOutputScalingModifierAsset Modifier;
        }
    }
}
