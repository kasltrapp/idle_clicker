using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class PrestigeManagerTests
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
        public void PrestigeManager_CanPrestige_ValidatesRequirements()
        {
            PrestigeManager manager = PrestigeManager.Instance;
            manager.prestigeRequirements = new List<Lock>
            {
                new Lock { currencyLock = TestUtilities.TestCurrency, inputAmount = 100 }
            };

            Assert.IsFalse(manager.CanPrestige(), "Should not be able to prestige without enough currency.");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 150);

            Assert.IsTrue(manager.CanPrestige(), "Should be able to prestige with enough currency.");
        }

        [Test]
        public void PrestigeManager_PrestigeGame_ResetsGameAndAppliesRewards()
        {
            PrestigeManager manager = PrestigeManager.Instance;
            manager.prestigeRequirements = new List<Lock>(); // No requirements for test

            var reward = ScriptableObject.CreateInstance<Reward>();
            var testLocation = ScriptableObject.CreateInstance<Location>();
            manager.prestigeRewardTables = new List<DropTable> { new DropTable {
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output { currencyOutput = TestUtilities.TestCurrency, outputAmount = 10 } }
            }};
            manager.scaleRewardsWithPrestigeCount = false;

            // Setup a game state that should be reset
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 5);
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 500);
            PlayerStats.Instance.AddXp(2000);
            BoostsManager.Instance.AddBoosts(TestUtilities.TestProductionBoost, 3);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestProductionBoost);
            UpgradesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestProductionUpgrade, 2);
            LocationsManager.Instance.GetOrAddHolder(testLocation).AddAmount(1);

            Assert.AreEqual(new BigNumber(5), BusinessesManager.Instance.GetOrAddHolder(TestUtilities.TestBusiness).amount, "Initial business amount is 5.");
            Assert.AreEqual(new BigNumber(500), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Initial currency is 500.");
            Assert.IsTrue(PlayerStats.Instance.level.currentLevel > 0, "Initial player level is > 0.");
            Assert.AreEqual(new BigNumber(2), BoostsManager.Instance.GetBoostAmountInInventory(TestUtilities.TestProductionBoost), "Boosts in inventory is 2.");
            Assert.AreEqual(1, BoostsManager.Instance.activeBoosts.Count, "1 active boost.");
            Assert.AreEqual(new BigNumber(2), UpgradesManager.Instance.GetOrAddHolder(TestUtilities.TestProductionUpgrade).amount, "Upgrades amount is 2.");
            Assert.AreEqual(new BigNumber(1), LocationsManager.Instance.GetOrAddHolder(testLocation).amount, "Locations amount is 1.");

            manager.resetBusinesses = true;
            manager.resetCurrencies = true;
            manager.resetLevel = true;
            manager.resetBoostsInInventory = true;
            manager.resetActiveBoosts = true;
            manager.resetUpgrades = true;
            manager.resetLocations = true;

            bool success = PrestigeManager.Instance.TryPrestigeGame(out List<Output> result);

            Assert.IsTrue(success, "Prestige should be successful.");
            Assert.IsNotNull(result, "Prestige should return List<Output>.");
            Assert.AreEqual(new BigNumber(0), BusinessesManager.Instance.GetOrAddHolder(TestUtilities.TestBusiness).amount, "Businesses should be reset.");
            Assert.AreEqual(new BigNumber(10), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Prestige reward should be granted, other currencies removed.");
            Assert.AreEqual(0, PlayerStats.Instance.level.currentLevel, "Player level should be reset to 0.");
            Assert.AreEqual(new BigNumber(0), BoostsManager.Instance.GetBoostAmountInInventory(TestUtilities.TestProductionBoost), "Boosts in inventory should be reset.");
            Assert.AreEqual(0, BoostsManager.Instance.activeBoosts.Count, "Active boosts should be reset.");
            Assert.AreEqual(new BigNumber(0), UpgradesManager.Instance.GetOrAddHolder(TestUtilities.TestProductionUpgrade).amount, "Upgrades should be reset.");
            Assert.AreEqual(new BigNumber(0), LocationsManager.Instance.GetOrAddHolder(testLocation).amount, "Locations should be reset.");

            Assert.AreEqual(1, manager.prestiges.prestigeCount, "Prestige count should increment.");

            Object.DestroyImmediate(reward);
            Object.DestroyImmediate(testLocation);
        }
        [Test]
        public void PrestigeManager_PrestigeGame_RaisesAmountChangedEventsForResetAmounts()
        {
            PrestigeManager manager = PrestigeManager.Instance;
            manager.prestigeRequirements = new List<Lock>();
            manager.prestigeRewardTables = new List<DropTable>();
            manager.resetBusinesses = true;
            manager.resetCurrencies = true;
            manager.resetLevel = false;
            manager.resetBoostsInInventory = true;
            manager.resetActiveBoosts = false;
            manager.resetManagers = true;
            manager.resetUpgrades = true;
            manager.resetLocations = true;

            Location testLocation = ScriptableObject.CreateInstance<Location>();

            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 5);
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 500);
            BoostsManager.Instance.AddBoosts(TestUtilities.TestProductionBoost, 3);
            ManagersManager.Instance.ForceBuyItemsForFree(TestUtilities.TestManager, 1);
            UpgradesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestProductionUpgrade, 2);
            LocationsManager.Instance.GetOrAddHolder(testLocation).AddAmount(1);

            bool businessResetEvent = false;
            bool currencyResetEvent = false;
            bool boostResetEvent = false;
            bool managerResetEvent = false;
            bool upgradeResetEvent = false;
            bool locationResetEvent = false;

            BusinessesManager.Instance.OnBusinessAmountChanged.AddListener(business =>
            {
                if (business == TestUtilities.TestBusiness && BusinessesManager.Instance.GetOrAddHolder(business).amount == 0)
                    businessResetEvent = true;
            });
            CurrencyManager.Instance.OnCurrencyAmountChanged.AddListener(currency =>
            {
                if (currency == TestUtilities.TestCurrency && CurrencyManager.Instance.GetCurrencyAmount(currency) == 0)
                    currencyResetEvent = true;
            });
            BoostsManager.Instance.OnBoostAmountChanged.AddListener(boost =>
            {
                if (boost == TestUtilities.TestProductionBoost && BoostsManager.Instance.GetBoostAmountInInventory(boost) == 0)
                    boostResetEvent = true;
            });
            ManagersManager.Instance.OnManagerAmountChanged.AddListener(managerItem =>
            {
                if (managerItem == TestUtilities.TestManager && ManagersManager.Instance.GetOrAddHolder(managerItem).Amount == 0)
                    managerResetEvent = true;
            });
            UpgradesManager.Instance.OnUpgradeAmountChanged.AddListener(upgrade =>
            {
                if (upgrade == TestUtilities.TestProductionUpgrade && UpgradesManager.Instance.GetOrAddHolder(upgrade).amount == 0)
                    upgradeResetEvent = true;
            });
            LocationsManager.Instance.OnLocationAmountChanged.AddListener(location =>
            {
                if (location == testLocation && LocationsManager.Instance.GetOrAddHolder(location).amount == 0)
                    locationResetEvent = true;
            });

            Assert.IsTrue(manager.TryPrestigeGame(out _), "Prestige should be successful.");

            Assert.IsTrue(businessResetEvent, "Business reset should raise OnBusinessAmountChanged after the amount is reset.");
            Assert.IsTrue(currencyResetEvent, "Currency reset should raise OnCurrencyAmountChanged after the amount is reset.");
            Assert.IsTrue(boostResetEvent, "Boost inventory reset should raise OnBoostAmountChanged after the amount is reset.");
            Assert.IsTrue(managerResetEvent, "Manager reset should raise OnManagerAmountChanged after the amount is reset.");
            Assert.IsTrue(upgradeResetEvent, "Upgrade reset should raise OnUpgradeAmountChanged after the amount is reset.");
            Assert.IsTrue(locationResetEvent, "Location reset should raise OnLocationAmountChanged after the amount is reset.");

            Object.DestroyImmediate(testLocation);
        }
        [Test]
        public void PrestigeManager_MaxPrestigeCount_PreventsPrestige()
        {
            PrestigeManager manager = PrestigeManager.Instance;
            manager.prestigeRequirements = new List<Lock>(); // No requirements
            manager.maxPrestigeCount = 2;

            Assert.IsTrue(manager.CanPrestige(), "Should be able to prestige initially.");
            manager.TryPrestigeGame(out _); // 1st prestige
            Assert.IsTrue(manager.CanPrestige(), "Should be able to prestige 2nd time.");
            manager.TryPrestigeGame(out _); // 2nd prestige

            Assert.IsFalse(manager.CanPrestige(), "Should not be able to prestige exceeding maxPrestigeCount.");
            Assert.IsFalse(manager.TryPrestigeGame(out _), "TryPrestigeGame should return false when max count reached.");
            Assert.AreEqual(2, manager.prestiges.prestigeCount, "Prestige count should not exceed max.");
        }

        [Test]
        public void PrestigeManager_PrestigeGame_RewardsDefaultLocationBusinessesAgain()
        {
            var defaultLocation = ScriptableObject.CreateInstance<Location>();
            defaultLocation.locks = new List<Lock>();
            defaultLocation.cost = new List<Input>();

            var testBusiness = ScriptableObject.CreateInstance<Business>();

            defaultLocation.starterOutputTables = new List<DropTable>
            {
                new DropTable
                {
                    strategy = DropStrategy.All,
                    outputs = new List<Output>
                    {
                        new Output { businessOutput = testBusiness, outputAmount = 10, dropChance = 1f }
                    }
                }
            };

            LocationsManager.Instance.defaultActiveLocation = defaultLocation;
            PrestigeManager.Instance.prestigeRequirements = new List<Lock>();

            // To simulate Start, we need to call Awake on LocationsManager to clear holders, but test sets up already.
            // Let's call a reflection or simulate Start behavior directly:
            LocationsManager.Instance.TryTravelTo(defaultLocation);
            // Simulate Start registering to PrestigeManager manually because TestUtilities already instantiated it
            PrestigeManager.Instance.OnPrestige.AddListener(() =>
            {
                // In play mode, Start is called once. The component is already active.
                // Our LocationsManager code adds OnPrestige via Start().
                // However, since TestUtilities uses AddComponent, Start() might run at the end of the frame.
                // To be safe, we can manually ensure the OnPrestige from LocationsManager is registered if it wasn't.
                // Wait, it is already added by AddComponent? Unity calls Awake immediately, Start before first Update.
            });
            // Let's actually use reflection to invoke Start to ensure we're matching the lifecycle,
            // or just assume it's attached (since LocationsManager has no other listeners, we could just do it manually if it fails, but let's test it first)
            var startMethod = typeof(LocationsManager).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startMethod != null) startMethod.Invoke(LocationsManager.Instance, null);

            Assert.AreEqual(new BigNumber(10), BusinessesManager.Instance.GetOrAddHolder(testBusiness).amount, "Starter business should be awarded at the beginning.");
            Assert.AreEqual(defaultLocation, LocationsManager.Instance.ActiveLocation, "ActiveLocation should be defaultLocation.");

            PrestigeManager.Instance.resetBusinesses = true;
            PrestigeManager.Instance.resetLocations = true;

            // Prestige
            bool success = PrestigeManager.Instance.TryPrestigeGame(out _);
            Assert.IsTrue(success, "Prestige should succeed.");

            // After prestige, the default location should be unlocked again, and starter outputs awarded again.
            Assert.AreEqual(new BigNumber(10), BusinessesManager.Instance.GetOrAddHolder(testBusiness).amount, "Starter business should be rewarded again after prestige.");
            Assert.AreEqual(defaultLocation, LocationsManager.Instance.ActiveLocation, "ActiveLocation should be restored to defaultLocation after prestige.");

            Object.DestroyImmediate(defaultLocation);
            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void PrestigeManager_ResetProgress_PreservesAdditionalExcludedUpgradesAndCurrencies()
        {
            PrestigeManager manager = PrestigeManager.Instance;
            manager.resetUpgrades = true;
            manager.resetCurrencies = true;
            manager.resetBusinesses = false;
            manager.resetManagers = false;
            manager.resetBoostsInInventory = false;
            manager.resetActiveBoosts = false;
            manager.resetLevel = false;
            manager.resetLocations = false;
            manager.excludedUpgrades = new List<Upgrade>();
            manager.exludedCurrencies = new Currency[0];

            Upgrade keptUpgrade = ScriptableObject.CreateInstance<Upgrade>();
            Upgrade groupedUpgrade = ScriptableObject.CreateInstance<Upgrade>();
            Upgrade resetUpgrade = ScriptableObject.CreateInstance<Upgrade>();
            UpgradeGroup keptUpgradeGroup = ScriptableObject.CreateInstance<UpgradeGroup>();
            Currency keptCurrency = ScriptableObject.CreateInstance<Currency>();
            Currency resetCurrency = ScriptableObject.CreateInstance<Currency>();

            groupedUpgrade.upgradeGroups = new List<UpgradeGroup> { keptUpgradeGroup };

            UpgradesManager.Instance.ForceBuyItemsForFree(keptUpgrade, 1);
            UpgradesManager.Instance.ForceBuyItemsForFree(groupedUpgrade, 1);
            UpgradesManager.Instance.ForceBuyItemsForFree(resetUpgrade, 1);
            CurrencyManager.Instance.AddCurrencyAmount(keptCurrency, 25);
            CurrencyManager.Instance.AddCurrencyAmount(resetCurrency, 50);

            ResetSummary summary = manager.ResetProgress(new PrestigeResetOptions
            {
                additionalExcludedUpgrades = new[] { keptUpgrade },
                additionalExcludedUpgradeGroups = new[] { keptUpgradeGroup },
                additionalExcludedCurrencies = new[] { keptCurrency }
            });

            Assert.AreEqual(new BigNumber(1), UpgradesManager.Instance.GetOrAddHolder(keptUpgrade).amount, "Additional excluded upgrade should survive reset.");
            Assert.AreEqual(new BigNumber(1), UpgradesManager.Instance.GetOrAddHolder(groupedUpgrade).amount, "Every member of an excluded upgrade group should survive reset.");
            Assert.AreEqual(new BigNumber(0), UpgradesManager.Instance.GetOrAddHolder(resetUpgrade).amount, "Non-excluded upgrade should reset.");
            Assert.AreEqual(new BigNumber(25), CurrencyManager.Instance.GetCurrencyAmount(keptCurrency), "Additional excluded currency should survive reset.");
            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(resetCurrency), "Non-excluded currency should reset.");
            CollectionAssert.Contains((System.Collections.ICollection)summary.preservedUpgrades, keptUpgrade);
            CollectionAssert.Contains((System.Collections.ICollection)summary.preservedUpgrades, groupedUpgrade);
            CollectionAssert.Contains((System.Collections.ICollection)summary.preservedCurrencies, keptCurrency);

            Object.DestroyImmediate(keptUpgrade);
            Object.DestroyImmediate(groupedUpgrade);
            Object.DestroyImmediate(resetUpgrade);
            Object.DestroyImmediate(keptUpgradeGroup);
            Object.DestroyImmediate(keptCurrency);
            Object.DestroyImmediate(resetCurrency);
        }

        [Test]
        public void PrestigeManager_ResetProgress_AchievementFlagAndExclusionsControlReset()
        {
            PrestigeManager manager = PrestigeManager.Instance;
            manager.resetBusinesses = false;
            manager.resetManagers = false;
            manager.resetBoostsInInventory = false;
            manager.resetActiveBoosts = false;
            manager.resetCurrencies = false;
            manager.resetLevel = false;
            manager.resetUpgrades = false;
            manager.resetLocations = false;
            manager.resetAchievements = true;

            Achievement keptAchievement = ScriptableObject.CreateInstance<Achievement>();
            Achievement resetAchievement = ScriptableObject.CreateInstance<Achievement>();
            AchievementHolder keptHolder = AchievementsManager.Instance.GetOrAddHolder(keptAchievement);
            AchievementHolder resetHolder = AchievementsManager.Instance.GetOrAddHolder(resetAchievement);
            keptHolder.isUnlocked = true;
            keptHolder.claimedAmount = 1;
            keptHolder.isClaimed = true;
            resetHolder.isUnlocked = true;
            resetHolder.claimedAmount = 1;
            resetHolder.isClaimed = true;

            ResetSummary summary = manager.ResetProgress(new PrestigeResetOptions
            {
                additionalExcludedAchievements = new[] { keptAchievement }
            });

            Assert.AreSame(keptHolder, AchievementsManager.Instance.GetHolder(keptAchievement), "Excluded achievement progress should survive prestige.");
            Assert.IsNull(AchievementsManager.Instance.GetHolder(resetAchievement), "Non-excluded achievement progress should reset.");
            Assert.IsTrue(summary.achievementsReset);
            CollectionAssert.Contains((System.Collections.ICollection)summary.preservedAchievements, keptAchievement);

            manager.resetAchievements = false;
            AchievementHolder persistentHolder = AchievementsManager.Instance.GetOrAddHolder(resetAchievement);
            persistentHolder.isUnlocked = true;
            manager.ResetProgress();
            Assert.AreSame(persistentHolder, AchievementsManager.Instance.GetHolder(resetAchievement), "Achievement progress should survive when resetAchievements is disabled.");

            Object.DestroyImmediate(keptAchievement);
            Object.DestroyImmediate(resetAchievement);
        }

        [Test]
        public void PrestigeManager_ResetProgress_PerSystemUnlockStateFlagsAreIndependent()
        {
            PrestigeManager manager = PrestigeManager.Instance;
            manager.resetBusinesses = true;
            manager.resetManagers = false;
            manager.resetBoostsInInventory = false;
            manager.resetActiveBoosts = false;
            manager.resetCurrencies = false;
            manager.resetLevel = false;
            manager.resetUpgrades = false;
            manager.resetLocations = false;

            Business preservedBusiness = ScriptableObject.CreateInstance<Business>();
            manager.excludedBusinesses = new List<Business> { preservedBusiness };

            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(preservedBusiness);
            ShopItem shopItem = ScriptableObject.CreateInstance<ShopItem>();
            ShopItemHolder shopHolder = ShopManager.Instance.GetOrAddHolder(shopItem);
            holder.AlreadyUnlocked = true;
            shopHolder.AlreadyUnlocked = true;

            manager.resetBusinessUnlockState = false;
            manager.resetShopItemUnlockState = true;
            manager.ResetProgress();

            Assert.IsTrue(BusinessesManager.Instance.GetOrAddHolder(preservedBusiness).AlreadyUnlocked, "Business unlock state should survive when its reset option is disabled.");
            Assert.IsFalse(ShopManager.Instance.GetOrAddHolder(shopItem).AlreadyUnlocked, "Shop unlock state should reset independently when its option is enabled.");

            holder.AlreadyUnlocked = true;
            shopHolder.AlreadyUnlocked = true;
            manager.resetBusinessUnlockState = true;
            manager.resetShopItemUnlockState = false;
            manager.ResetProgress();

            Assert.IsFalse(BusinessesManager.Instance.GetOrAddHolder(preservedBusiness).AlreadyUnlocked, "Business unlock state should reset when its option is enabled.");
            Assert.IsTrue(ShopManager.Instance.GetOrAddHolder(shopItem).AlreadyUnlocked, "Shop unlock state should survive independently when its option is disabled.");

            Object.DestroyImmediate(preservedBusiness);
            Object.DestroyImmediate(shopItem);
        }

        [Test]
        public void TryPrestigeGame_RestoresStarterOutputsForPreservedLocations()
        {
            var preservedLocation = ScriptableObject.CreateInstance<Location>();
            preservedLocation.locks = new System.Collections.Generic.List<Lock>();
            preservedLocation.cost = new System.Collections.Generic.List<Input>();
            
            var currency = ScriptableObject.CreateInstance<Currency>();
            currency.maxAmount = -1;
            
            preservedLocation.starterOutputTables = new System.Collections.Generic.List<DropTable>
            {
                new DropTable
                {
                    strategy = DropStrategy.All,
                    outputs = new System.Collections.Generic.List<Output>
                    {
                        new Output { currencyOutput = currency, outputAmount = 100, dropChance = 1f }
                    }
                }
            };

            LocationsManager.Instance.TryUnlockLocation(preservedLocation);

            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(currency), "Initial unlock should give 100 currency");

            // Exclude location from prestige resets
            PrestigeManager.Instance.prestigeRequirements = new System.Collections.Generic.List<Lock>();
            PrestigeManager.Instance.excludedLocations.Add(preservedLocation);
            PrestigeManager.Instance.resetLocations = true;
            PrestigeManager.Instance.resetCurrencies = true;

            bool prestigeSuccess = PrestigeManager.Instance.TryPrestigeGame(out _);
            Assert.IsTrue(prestigeSuccess, "Prestige should succeed.");

            // Because the location was preserved, its starter outputs should have been reapplied!
            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(currency), "Starter outputs should be restored for preserved locations after prestige");
            Assert.IsTrue(LocationsManager.Instance.GetOrAddHolder(preservedLocation).iUnlockable.IsUnlocked(), "Preserved location should remain unlocked");

            Object.DestroyImmediate(preservedLocation);
            Object.DestroyImmediate(currency);
        }
    }
}
