using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EasyIdleGame.Tests
{
    public class UpgradesManagerTests
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
        public void BuyUpgrade_AppliesMultiplier()
        {
            Assert.IsTrue(UpgradesManager.Instance.GetMultiplierOfType(UpgradeType.speed, "") == 1,
                "Initial speed multiplier should be 1");

            UpgradesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestUpgrade, 1);

            Assert.IsTrue(UpgradesManager.Instance.GetMultiplierOfType(UpgradeType.speed, "") == 2,
                "Speed multiplier should be 2 after buying the upgrade");

            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 1);

            // Test business is producing 1 test currency per second by default
            Assert.IsTrue(BusinessesManager.Instance.GetTotalProductionPerSecond(TestUtilities.TestCurrency, false) == 2,
                $"Test business production should be 2 (1 default * 2 speed multiplier). Actual: {BusinessesManager.Instance.GetTotalProductionPerSecond(TestUtilities.TestCurrency, false)}");
        }
        [UnityTest]
        public System.Collections.IEnumerator BuyTimedUpgrade_ParallelAndSequential_AppliesCorrectly()
        {
            Upgrade timedUpgrade = ScriptableObject.CreateInstance<Upgrade>();
            timedUpgrade.upgradeDurationInSeconds = 1f;
            UpgradesManager.Instance.queueMode = UpgradeQueueMode.Parallel;

            // --- Test Parallel and skip ---
            UpgradesManager.Instance.ForceBuyItemsForFree(timedUpgrade, 1);
            UpgradesManager.Instance.ForceBuyItemsForFree(timedUpgrade, 1);

            UpgradeHolder holder = UpgradesManager.Instance.GetHolder(timedUpgrade);
            Assert.AreEqual((BigNumber)0, holder.amount, "Amount should be 0 immediately after buying a timed upgrade");
            Assert.AreEqual(2, holder.pendingUpgrades.Count, "Should have 2 pending upgrades in parallel mode");

            UpgradesManager.Instance.SkipUpgradeWait(timedUpgrade);
            Assert.AreEqual((BigNumber)2, holder.amount, "Amount should be 2 after skipping wait");
            Assert.AreEqual(0, holder.pendingUpgrades.Count, "Pending upgrades should be cleared after skip");

            // --- Test Sequential and actual wait ---
            UpgradesManager.Instance.queueMode = UpgradeQueueMode.Sequential;
            holder.amount = 0; // reset

            UpgradesManager.Instance.ForceBuyItemsForFree(timedUpgrade, 1);
            UpgradesManager.Instance.ForceBuyItemsForFree(timedUpgrade, 2);

            Assert.AreEqual(2, holder.pendingUpgrades.Count, "Should have 2 pending upgrade batches");

            var pending1 = holder.pendingUpgrades[0];
            var pending2 = holder.pendingUpgrades[1];

            Assert.IsTrue((pending2.upgradeEndTime - pending1.upgradeEndTime).TotalSeconds > 1.9f,
                "Sequential pending upgrades should be offset by the correct duration (1s + 2s offset)");

            // Wait for first upgrade (duration 1s)
            yield return new WaitForSeconds(1.2f);

            Assert.AreEqual((BigNumber)1, holder.amount, "First sequential upgrade should be applied after 1 second wait");
            Assert.AreEqual(1, holder.pendingUpgrades.Count, "One pending upgrade should remain");

            Object.DestroyImmediate(timedUpgrade);
        }

        [Test]
        public void GetOverrideBusinessMaxAmount_ReturnsCorrectValue_AndDoesNotBleedToOtherBusinesses()
        {
            Business targetBusiness = ScriptableObject.CreateInstance<Business>();
            targetBusiness.cost = new System.Collections.Generic.List<Input>();

            Business otherBusiness = ScriptableObject.CreateInstance<Business>();
            otherBusiness.cost = new System.Collections.Generic.List<Input>();

            // No upgrade owned yet: both should return null
            Assert.IsNull(UpgradesManager.Instance.GetOverrideBusinessMaxAmount(targetBusiness),
                "GetOverrideBusinessMaxAmount should return null when no upgrade is owned");
            Assert.IsNull(UpgradesManager.Instance.GetOverrideBusinessMaxAmount(otherBusiness),
                "GetOverrideBusinessMaxAmount should return null for other business too");

            // Create and apply an upgrade scoped to targetBusiness
            Upgrade upgrade = ScriptableObject.CreateInstance<Upgrade>();
            upgrade.overrideUpgrades = new System.Collections.Generic.List<OverrideUpgradeBlock>
            {
                new OverrideUpgradeBlock(OverrideUpgradeType.maxBusinessAmount, 100) { targetBusiness = targetBusiness }
            };
            upgrade.overrideUpgradeLevelMultiplier = 1f;

            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, 1);

            BigNumber? result = UpgradesManager.Instance.GetOverrideBusinessMaxAmount(targetBusiness);
            Assert.IsNotNull(result,
                "GetOverrideBusinessMaxAmount should return a value after the upgrade is applied");
            Assert.AreEqual(new BigNumber(100), result.Value,
                $"GetOverrideBusinessMaxAmount should return 100. Actual: {result.Value}");

            Assert.IsNull(UpgradesManager.Instance.GetOverrideBusinessMaxAmount(otherBusiness),
                "GetOverrideBusinessMaxAmount must return null for a business not targeted by the upgrade");

            Object.DestroyImmediate(targetBusiness);
            Object.DestroyImmediate(otherBusiness);
            Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void OverrideUpgradeModifier_WithTargetUpgrade_MultipliesOnlyThatUpgradeOverride()
        {
            Currency milk = ScriptableObject.CreateInstance<Currency>();

            Upgrade storageUpgrade = CreateCurrencyCapUpgrade(milk, 100, 2f);
            Upgrade otherMilkCapUpgrade = CreateCurrencyCapUpgrade(milk, 600, 1f);
            Upgrade modifierUpgrade = CreateCurrencyCapModifier(
                milk,
                OverrideUpgradeModifierOperation.Multiply,
                2,
                targetUpgrade: storageUpgrade,
                levelMultiplier: 1f);

            UpgradesManager.Instance.ForceBuyItemsForFree(storageUpgrade, 3);
            UpgradesManager.Instance.ForceBuyItemsForFree(otherMilkCapUpgrade, 1);
            UpgradesManager.Instance.ForceBuyItemsForFree(modifierUpgrade, 1);

            BigNumber? highestResult = UpgradesManager.Instance.GetOverrideUpgradeOfType(OverrideUpgradeType.maxCurrencyAmount, milk);
            Assert.IsNotNull(highestResult, "Expected a modified Milk cap override.");
            Assert.AreEqual(new BigNumber(800), highestResult.Value,
                "Only Storage should be multiplied: max(Storage 100 * 2^(3-1) * 2, Other 600) = 800.");

            BigNumber? storageResult = UpgradesManager.Instance.GetOverrideUpgradeOfTypeForUpgrade(storageUpgrade, OverrideUpgradeType.maxCurrencyAmount, milk);
            BigNumber? otherResult = UpgradesManager.Instance.GetOverrideUpgradeOfTypeForUpgrade(otherMilkCapUpgrade, OverrideUpgradeType.maxCurrencyAmount, milk);

            Assert.AreEqual(new BigNumber(800), storageResult.Value, "Targeted modifier should affect Storage.");
            Assert.AreEqual(new BigNumber(600), otherResult.Value, "Targeted modifier should not affect another Milk cap upgrade.");

            DestroyTestObjects(milk, storageUpgrade, otherMilkCapUpgrade, modifierUpgrade);
        }

        [Test]
        public void OverrideUpgradeModifier_WithNoTargetUpgrade_MultipliesAllMatchingOverrides()
        {
            Currency milk = ScriptableObject.CreateInstance<Currency>();

            Upgrade storageUpgrade = CreateCurrencyCapUpgrade(milk, 100, 2f);
            Upgrade otherMilkCapUpgrade = CreateCurrencyCapUpgrade(milk, 600, 1f);
            Upgrade modifierUpgrade = CreateCurrencyCapModifier(
                milk,
                OverrideUpgradeModifierOperation.Multiply,
                2,
                targetUpgrade: null,
                levelMultiplier: 1f);

            UpgradesManager.Instance.ForceBuyItemsForFree(storageUpgrade, 3);
            UpgradesManager.Instance.ForceBuyItemsForFree(otherMilkCapUpgrade, 1);
            UpgradesManager.Instance.ForceBuyItemsForFree(modifierUpgrade, 1);

            BigNumber? highestResult = UpgradesManager.Instance.GetOverrideUpgradeOfType(OverrideUpgradeType.maxCurrencyAmount, milk);
            Assert.IsNotNull(highestResult, "Expected a modified Milk cap override.");
            Assert.AreEqual(new BigNumber(1200), highestResult.Value,
                "Null targetUpgrade should multiply every matching Milk cap source: max(400 * 2, 600 * 2) = 1200.");

            BigNumber? storageResult = UpgradesManager.Instance.GetOverrideUpgradeOfTypeForUpgrade(storageUpgrade, OverrideUpgradeType.maxCurrencyAmount, milk);
            BigNumber? otherResult = UpgradesManager.Instance.GetOverrideUpgradeOfTypeForUpgrade(otherMilkCapUpgrade, OverrideUpgradeType.maxCurrencyAmount, milk);

            Assert.AreEqual(new BigNumber(800), storageResult.Value, "Null-target modifier should affect Storage.");
            Assert.AreEqual(new BigNumber(1200), otherResult.Value, "Null-target modifier should affect the other matching Milk cap source.");

            DestroyTestObjects(milk, storageUpgrade, otherMilkCapUpgrade, modifierUpgrade);
        }

        [Test]
        public void OverrideUpgradeModifiers_AddBeforeMultiplying_AndScaleWithModifierAmount()
        {
            Currency milk = ScriptableObject.CreateInstance<Currency>();

            Upgrade storageUpgrade = CreateCurrencyCapUpgrade(milk, 100, 1f);
            Upgrade additiveModifier = CreateCurrencyCapModifier(
                milk,
                OverrideUpgradeModifierOperation.Add,
                50,
                targetUpgrade: storageUpgrade,
                levelMultiplier: 2f);
            Upgrade multiplierModifier = CreateCurrencyCapModifier(
                milk,
                OverrideUpgradeModifierOperation.Multiply,
                2,
                targetUpgrade: storageUpgrade,
                levelMultiplier: 2f);

            UpgradesManager.Instance.ForceBuyItemsForFree(storageUpgrade, 1);
            UpgradesManager.Instance.ForceBuyItemsForFree(additiveModifier, 2);
            UpgradesManager.Instance.ForceBuyItemsForFree(multiplierModifier, 2);

            BigNumber? result = UpgradesManager.Instance.GetOverrideUpgradeOfTypeForUpgrade(storageUpgrade, OverrideUpgradeType.maxCurrencyAmount, milk);

            Assert.IsNotNull(result, "Expected a modified Storage cap.");
            Assert.AreEqual(new BigNumber(800), result.Value,
                "Modifiers should resolve as (source + additive) * multiplier: (100 + 100) * 4 = 800.");

            DestroyTestObjects(milk, storageUpgrade, additiveModifier, multiplierModifier);
        }

        [Test]
        public void OverrideUpgradeModifier_DoesNotCreateOverrideWithoutMatchingSource()
        {
            Currency milk = ScriptableObject.CreateInstance<Currency>();
            Upgrade modifierUpgrade = CreateCurrencyCapModifier(
                milk,
                OverrideUpgradeModifierOperation.Multiply,
                2,
                targetUpgrade: null,
                levelMultiplier: 1f);

            UpgradesManager.Instance.ForceBuyItemsForFree(modifierUpgrade, 1);

            Assert.IsNull(UpgradesManager.Instance.GetOverrideUpgradeOfType(OverrideUpgradeType.maxCurrencyAmount, milk),
                "A modifier should only modify existing matching override values; it should not create a cap by itself.");

            DestroyTestObjects(milk, modifierUpgrade);
        }

        private static Upgrade CreateCurrencyCapUpgrade(Currency targetCurrency, BigNumber value, float levelMultiplier)
        {
            Upgrade upgrade = ScriptableObject.CreateInstance<Upgrade>();
            upgrade.upgrades = new System.Collections.Generic.List<UpgradeBlock>();
            upgrade.overrideUpgrades = new System.Collections.Generic.List<OverrideUpgradeBlock>
            {
                new OverrideUpgradeBlock(OverrideUpgradeType.maxCurrencyAmount, value) { targetCurrency = targetCurrency }
            };
            upgrade.overrideUpgradeModifiers = new System.Collections.Generic.List<OverrideUpgradeModifierBlock>();
            upgrade.overrideUpgradeLevelMultiplier = levelMultiplier;
            return upgrade;
        }

        private static Upgrade CreateCurrencyCapModifier(
            Currency targetCurrency,
            OverrideUpgradeModifierOperation operation,
            BigNumber value,
            Upgrade targetUpgrade,
            float levelMultiplier)
        {
            Upgrade upgrade = ScriptableObject.CreateInstance<Upgrade>();
            upgrade.upgrades = new System.Collections.Generic.List<UpgradeBlock>();
            upgrade.overrideUpgrades = new System.Collections.Generic.List<OverrideUpgradeBlock>();
            upgrade.overrideUpgradeModifiers = new System.Collections.Generic.List<OverrideUpgradeModifierBlock>
            {
                new OverrideUpgradeModifierBlock(OverrideUpgradeType.maxCurrencyAmount, operation, value)
                {
                    targetUpgrade = targetUpgrade,
                    targetCurrency = targetCurrency
                }
            };
            upgrade.overrideUpgradeLevelMultiplier = levelMultiplier;
            return upgrade;
        }

        private static void DestroyTestObjects(params Object[] objects)
        {
            foreach (Object obj in objects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void OnHolderAdded_FiresWhenHolderIsCreated()
        {
            var upgrade = ScriptableObject.CreateInstance<Upgrade>();
            bool eventFired = false;
            
            UpgradesManager.Instance.OnHolderAdded.AddListener((holder) => 
            {
                if (holder.upgrade == upgrade) eventFired = true;
            });

            // Trigger creation
            UpgradesManager.Instance.GetOrAddHolder(upgrade);

            Assert.IsTrue(eventFired, "OnHolderAdded should fire when a new upgrade holder is created.");

            eventFired = false;
            UpgradesManager.Instance.GetOrAddHolder(upgrade);
            
            Assert.IsFalse(eventFired, "OnHolderAdded should not fire again for an existing holder.");
            DestroyTestObjects(upgrade);
        }

        [Test]
        public void TryGetHolder_FindsHolderCorrectly()
        {
            var upgrade = ScriptableObject.CreateInstance<Upgrade>();
            
            bool foundBefore = UpgradesManager.Instance.TryGetHolder(upgrade, out var holderBefore);
            Assert.IsFalse(foundBefore, "TryGetHolder should return false if holder does not exist yet.");
            Assert.IsNull(holderBefore, "Holder out parameter should be null if not found.");

            // Create it
            UpgradesManager.Instance.GetOrAddHolder(upgrade);

            bool foundAfter = UpgradesManager.Instance.TryGetHolder(upgrade, out var holderAfter);
            Assert.IsTrue(foundAfter, "TryGetHolder should return true if holder exists.");
            Assert.IsNotNull(holderAfter, "Holder out parameter should not be null if found.");
            
            DestroyTestObjects(upgrade);
        }

        [Test]
        public void GetPurchasePreview_ReturnsCorrectData_WithoutMutating()
        {
            var upgrade = ScriptableObject.CreateInstance<Upgrade>();
            var currency = ScriptableObject.CreateInstance<Currency>();
            currency.maxAmount = -1;
            
            upgrade.cost = new System.Collections.Generic.List<Input>
            {
                new Input { currencyInput = currency, inputAmount = 10 }
            };
            upgrade.priceMultiplier_ = 2; // Next level costs 20, then 40, etc.
            upgrade.maxBuyableAmount = 5;

            CurrencyManager.Instance.GetOrAddHolder(currency).amount = 100; // Enough for several levels

            // Preview without holder
            var previewBefore = UpgradesManager.Instance.iBuyable.GetPurchasePreview(upgrade, 1);
            Assert.IsTrue(previewBefore.canAfford, "Should be able to afford level 1.");
            Assert.AreEqual(new BigNumber(10), previewBefore.effectiveCost[0].inputAmount, "Cost of level 1 should be 10.");
            Assert.AreEqual(new BigNumber(0), previewBefore.currentAmount, "Current level should be 0.");
            
            bool found = UpgradesManager.Instance.TryGetHolder(upgrade, out _);
            Assert.IsFalse(found, "GetPurchasePreview should NOT instantiate a holder if it doesn't exist.");

            // Create holder and buy 1 level
            UpgradesManager.Instance.iBuyable.TryBuyItems(upgrade, 1);
            Assert.AreEqual(new BigNumber(90), CurrencyManager.Instance.GetCurrencyAmount(currency), "Currency should be deducted.");

            var previewAfter = UpgradesManager.Instance.iBuyable.GetPurchasePreview(upgrade, 1);
            Assert.IsTrue(previewAfter.canAfford, "Should be able to afford level 2.");
            Assert.AreEqual(new BigNumber(20), previewAfter.effectiveCost[0].inputAmount, "Cost of level 2 should be 20.");
            Assert.AreEqual(new BigNumber(1), previewAfter.currentAmount, "Current level should be 1.");

            DestroyTestObjects(upgrade, currency);
        }
    }
}
