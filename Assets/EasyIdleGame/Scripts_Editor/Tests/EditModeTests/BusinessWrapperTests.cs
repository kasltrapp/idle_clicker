using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class BusinessWrapperTests
    {
        private Business _baseBusiness;
        private BusinessWrapper _wrapper;
        private BusinessWrapper _secondWrapper;
        private Currency _currency;
        private Texture2D _upgradeLevelTexture;
        private Sprite _upgradeLevelSprite;
        private GameObject _upgradeLevelModel;

        [SetUp]
        public void SetUp()
        {
            _baseBusiness = ScriptableObject.CreateInstance<Business>();
            _wrapper = ScriptableObject.CreateInstance<BusinessWrapper>();
            _secondWrapper = ScriptableObject.CreateInstance<BusinessWrapper>();
            _currency = ScriptableObject.CreateInstance<Currency>();

            _wrapper.underlyingBusiness = _baseBusiness;
            _secondWrapper.underlyingBusiness = _baseBusiness;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_upgradeLevelModel);
            Object.DestroyImmediate(_upgradeLevelSprite);
            Object.DestroyImmediate(_upgradeLevelTexture);
            Object.DestroyImmediate(_baseBusiness);
            Object.DestroyImmediate(_wrapper);
            Object.DestroyImmediate(_secondWrapper);
            Object.DestroyImmediate(_currency);
        }

        [Test]
        public void SyncData_DeepCopiesLists()
        {
            // Set up base business with a cost
            _baseBusiness.cost = new List<Input>
            {
                new Input { currencyInput = _currency, inputAmount = 10 }
            };

            // Act
            _wrapper.SyncData();

            // Assert
            Assert.IsNotNull(_wrapper.cost, "Cost list should not be null after sync.");
            Assert.AreEqual(1, _wrapper.cost.Count, "Cost list should have 1 item.");
            
            // Modify wrapper's cost to ensure it's a deep copy, not a shared reference
            _wrapper.cost[0].inputAmount = 20;
            
            Assert.AreEqual(new BigNumber(10), _baseBusiness.cost[0].inputAmount, "Base business cost should not be modified when wrapper cost changes.");
        }

        [Test]
        public void SyncData_ReplacesSelfReferences()
        {
            _baseBusiness.outputTables = new List<DropTable>
            {
                new DropTable
                {
                    strategy = DropStrategy.All,
                    outputs = new List<Output>
                    {
                        new Output { businessOutput = _baseBusiness, outputAmount = 1 }
                    }
                }
            };

            // Act
            _wrapper.SyncData();

            // Assert
            Assert.IsNotNull(_wrapper.outputTables, "Output tables should not be null.");
            Assert.AreEqual(1, _wrapper.outputTables.Count, "Output tables should have 1 item.");
            Assert.AreEqual(1, _wrapper.outputTables[0].outputs.Count, "Output table should have 1 output.");
            
            // The output should now reference the WRAPPER, not the base business
            Assert.AreEqual(_wrapper, _wrapper.outputTables[0].outputs[0].businessOutput, "Self-references to underlying business must be replaced with the wrapper itself.");
        }

        [Test]
        public void SyncData_UpgradeLevelsAreIsolatedBetweenSourceAndWrappers()
        {
            CreateUpgradeLevelAssets();

            _baseBusiness.upgradeLevels = new[]
            {
                new BusinessUpgradeLevel
                {
                    level = 2,
                    businessName = "Base tier",
                    image = _upgradeLevelSprite,
                    iconString = "BT",
                    model = _upgradeLevelModel,
                    description = "Base tier description",
                    maxAmount_ = 100,
                    maxBuyableAmount = 25
                }
            };

            _wrapper.SyncData();
            _secondWrapper.SyncData();

            Assert.AreEqual(1, _baseBusiness.upgradeLevels.Length, "Source business should keep its configured upgrade level.");
            Assert.AreEqual(1, _wrapper.upgradeLevels.Length, "Wrapper should copy the source upgrade level.");
            Assert.AreEqual(1, _secondWrapper.upgradeLevels.Length, "Second wrapper should copy the source upgrade level.");
            Assert.AreNotSame(_baseBusiness.upgradeLevels, _wrapper.upgradeLevels, "Wrapper should not share the upgrade-level array with the source business.");
            Assert.AreNotSame(_baseBusiness.upgradeLevels, _secondWrapper.upgradeLevels, "Second wrapper should not share the upgrade-level array with the source business.");
            Assert.AreNotSame(_wrapper.upgradeLevels, _secondWrapper.upgradeLevels, "Wrappers should not share the same upgrade-level array.");
            Assert.AreNotSame(_baseBusiness.upgradeLevels[0], _wrapper.upgradeLevels[0], "Wrapper should not share the upgrade-level object with the source business.");
            Assert.AreNotSame(_wrapper.upgradeLevels[0], _secondWrapper.upgradeLevels[0], "Wrappers should not share the same upgrade-level object.");

            _wrapper.upgradeLevels[0].level = 7;
            _wrapper.upgradeLevels[0].businessName = "Wrapper tier";
            _wrapper.upgradeLevels[0].image = null;
            _wrapper.upgradeLevels[0].iconString = "WT";
            _wrapper.upgradeLevels[0].model = null;
            _wrapper.upgradeLevels[0].description = "Wrapper tier description";
            _wrapper.upgradeLevels[0].maxAmount_ = 200;
            _wrapper.upgradeLevels[0].maxBuyableAmount = 75;

            AssertUpgradeLevel(
                _baseBusiness.upgradeLevels[0],
                2,
                "Base tier",
                _upgradeLevelSprite,
                "BT",
                _upgradeLevelModel,
                "Base tier description",
                100,
                25,
                "Source business");

            AssertUpgradeLevel(
                _secondWrapper.upgradeLevels[0],
                2,
                "Base tier",
                _upgradeLevelSprite,
                "BT",
                _upgradeLevelModel,
                "Base tier description",
                100,
                25,
                "Second wrapper");

            AssertUpgradeLevel(
                _wrapper.upgradeLevels[0],
                7,
                "Wrapper tier",
                null,
                "WT",
                null,
                "Wrapper tier description",
                200,
                75,
                "Mutated wrapper");
        }

        [Test]
        public void SyncData_AppliesScaleCorrectly()
        {
            _baseBusiness.cost = new List<Input> { new Input { inputAmount = 10 } };
            _baseBusiness.productionCost = new List<Input> { new Input { inputAmount = 10 } };
            _baseBusiness.upgradeCost = new List<Input> { new Input { inputAmount = 10 } };
            _baseBusiness.timeToProduce = 10f;
            _baseBusiness.outputTables = new List<DropTable>
            {
                new DropTable
                {
                    outputs = new List<Output> { new Output { outputAmount = 10, outputMaxAmount = 10 } }
                }
            };
            
            _wrapper.underlyingBusiness = _baseBusiness;
            _wrapper.costScale = 2f;
            _wrapper.productionCostScale = 3f;
            _wrapper.upgradeCostScale = 4f;
            _wrapper.timeToProduceScale = 0.5f;
            _wrapper.outputsScale = 2f;

            _wrapper.SyncData();

            Assert.AreEqual(new BigNumber(20), _wrapper.cost[0].inputAmount, "Cost should be scaled.");
            Assert.AreEqual(new BigNumber(30), _wrapper.productionCost[0].inputAmount, "Production cost should be scaled.");
            Assert.AreEqual(new BigNumber(40), _wrapper.upgradeCost[0].inputAmount, "Upgrade cost should be scaled.");
            Assert.AreEqual(5f, _wrapper.timeToProduce, "Time to produce should be scaled.");
            Assert.AreEqual(new BigNumber(20), _wrapper.outputTables[0].outputs[0].outputAmount, "Output amount should be scaled.");
            Assert.AreEqual(new BigNumber(20), _wrapper.outputTables[0].outputs[0].outputMaxAmount, "Output max amount should be scaled.");
        }

        [Test]
        public void SyncData_AppliesLargeBigNumberScale()
        {
            _baseBusiness.cost = new List<Input> { new Input { inputAmount = 2 } };
            _baseBusiness.productionCost = new List<Input> { new Input { inputAmount = 3 } };
            _baseBusiness.outputTables = new List<DropTable>
            {
                new DropTable
                {
                    outputs = new List<Output> { new Output { outputAmount = 4, outputMaxAmount = 5 } }
                }
            };

            _wrapper.costScale = new BigNumber(1, 12);
            _wrapper.productionCostScale = new BigNumber(2, 9);
            _wrapper.outputsScale = new BigNumber(3, 6);

            _wrapper.SyncData();

            Assert.AreEqual(new BigNumber(2, 12), _wrapper.cost[0].inputAmount, "Cost should preserve BigNumber scale.");
            Assert.AreEqual(new BigNumber(6, 9), _wrapper.productionCost[0].inputAmount, "Production cost should preserve BigNumber scale.");
            Assert.AreEqual(new BigNumber(12, 6), _wrapper.outputTables[0].outputs[0].outputAmount, "Output amount should preserve BigNumber scale.");
            Assert.AreEqual(new BigNumber(15, 6), _wrapper.outputTables[0].outputs[0].outputMaxAmount, "Output max amount should preserve BigNumber scale.");
        }
        private void CreateUpgradeLevelAssets()
        {
            _upgradeLevelTexture = new Texture2D(1, 1);
            _upgradeLevelSprite = Sprite.Create(_upgradeLevelTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            _upgradeLevelModel = new GameObject("Upgrade Level Model");
        }

        private static void AssertUpgradeLevel(
            BusinessUpgradeLevel upgradeLevel,
            BigNumber expectedLevel,
            string expectedBusinessName,
            Sprite expectedImage,
            string expectedIconString,
            GameObject expectedModel,
            string expectedDescription,
            BigNumber expectedMaxAmount,
            BigNumber expectedMaxBuyableAmount,
            string context)
        {
            Assert.AreEqual(expectedLevel, upgradeLevel.level, $"{context} level should remain isolated.");
            Assert.AreEqual(expectedBusinessName, upgradeLevel.businessName, $"{context} businessName should remain isolated.");
            Assert.AreSame(expectedImage, upgradeLevel.image, $"{context} image should remain isolated.");
            Assert.AreEqual(expectedIconString, upgradeLevel.iconString, $"{context} iconString should remain isolated.");
            Assert.AreSame(expectedModel, upgradeLevel.model, $"{context} model should remain isolated.");
            Assert.AreEqual(expectedDescription, upgradeLevel.description, $"{context} description should remain isolated.");
            Assert.AreEqual(expectedMaxAmount, upgradeLevel.maxAmount_, $"{context} maxAmount_ should remain isolated.");
            Assert.AreEqual(expectedMaxBuyableAmount, upgradeLevel.maxBuyableAmount, $"{context} maxBuyableAmount should remain isolated.");
        }
    }
}
