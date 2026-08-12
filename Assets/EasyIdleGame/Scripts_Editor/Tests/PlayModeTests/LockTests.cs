using NUnit.Framework;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyIdleGame.Tests
{
    public class LockTests
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
        public void Lock_LockAmountType_ValidatesCorrectly()
        {
            // Business test
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 10); // Amount = 10, TotalAmount = 10, BoughtAmount = 0
            
            Lock businessLock = new Lock { businessLock = TestUtilities.TestBusiness, inputAmount = 5 };

            businessLock.amountType = LockAmountType.CurrentAmount;
            Assert.IsTrue(businessLock.IsUnlocked(), "Should be unlocked using CurrentAmount (10 >= 5)");

            businessLock.amountType = LockAmountType.TotalAmount;
            Assert.IsTrue(businessLock.IsUnlocked(), "Should be unlocked using TotalAmount (10 >= 5)");

            businessLock.amountType = LockAmountType.BoughtAmount;
            Assert.IsFalse(businessLock.IsUnlocked(), "Should be locked using BoughtAmount (0 < 5)");
            
            BusinessesManager.Instance.GetOrAddHolder(TestUtilities.TestBusiness).boughtAmount += 5; // Amount = 10, TotalAmount = 10, BoughtAmount = 5
            Assert.IsTrue(businessLock.IsUnlocked(), "Should be unlocked using BoughtAmount (5 >= 5)");

            var oldMaxAmount = TestUtilities.TestBusiness.maxAmount_;
            TestUtilities.TestBusiness.maxAmount_ = 20;
            businessLock.amountType = LockAmountType.MaxAmount;
            Assert.IsTrue(businessLock.IsUnlocked(), "Should be unlocked using MaxAmount (20 >= 5)");
            businessLock.inputAmount = 25;
            Assert.IsFalse(businessLock.IsUnlocked(), "Should be locked using MaxAmount (20 < 25)");
            businessLock.inputAmount = 5;
            TestUtilities.TestBusiness.maxAmount_ = oldMaxAmount;

            // Currency test
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 50); // Total money made = 50, Amount = 50
            CurrencyManager.Instance.RemoveCurrencyAmount(TestUtilities.TestCurrency, 30); // Total money made = 50, Amount = 20

            Lock currencyLock = new Lock { currencyLock = TestUtilities.TestCurrency, inputAmount = 40 };

            currencyLock.amountType = LockAmountType.CurrentAmount;
            Assert.IsFalse(currencyLock.IsUnlocked(), "Should be locked using CurrentAmount (20 < 40)");

            currencyLock.amountType = LockAmountType.TotalAmount;
            Assert.IsTrue(currencyLock.IsUnlocked(), "Should be unlocked using TotalAmount (50 >= 40)");

            var oldMaxCurrencyAmount = TestUtilities.TestCurrency.maxAmount;
            TestUtilities.TestCurrency.maxAmount = 50;
            currencyLock.amountType = LockAmountType.MaxAmount;
            Assert.IsTrue(currencyLock.IsUnlocked(), "Should be unlocked using MaxAmount (50 >= 40)");
            currencyLock.inputAmount = 60;
            Assert.IsFalse(currencyLock.IsUnlocked(), "Should be locked using MaxAmount (50 < 60)");
            TestUtilities.TestCurrency.maxAmount = oldMaxCurrencyAmount;
        }

        [Test]
        public void Lock_BusinessUpgradeLevelLock_ValidatesCorrectly()
        {
            var business = ScriptableObject.CreateInstance<Business>();
            business.businessName = "Upgrade Level Test Business";
            business.upgradeLevels = new[]
            {
                new BusinessUpgradeLevel { level = 2 }
            };

            try
            {
                BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
                Lock upgradeLevelLock = new Lock { businessUpgradeLevelLock = business, inputAmount = 2 };

                Assert.IsFalse(upgradeLevelLock.IsUnlocked(), "Should be locked before the business reaches upgrade level 2.");

                holder.iUpgradable.ForceUpgradeToLevel(2);

                Assert.IsTrue(upgradeLevelLock.IsUnlocked(), "Should unlock when the business reaches upgrade level 2.");

                upgradeLevelLock.inputAmount = 1;
                Assert.IsFalse(upgradeLevelLock.IsUpperUnlocked(), "Upper lock should fail when the business upgrade level is above the threshold.");

                upgradeLevelLock.inputAmount = 2;
                Assert.IsTrue(upgradeLevelLock.IsUpperUnlocked(), "Upper lock should pass when the business upgrade level is at the threshold.");
            }
            finally
            {
                Object.DestroyImmediate(business);
            }
        }

        [Test]
        public void Lock_PrestigeLock_ValidatesCorrectly()
        {
            Lock prestigeLock = new Lock { prestigeLock = true, inputAmount = 2 };
            
            Assert.IsFalse(prestigeLock.IsUnlocked(), "Should be locked, prestige count is 0.");
            
            PrestigeManager.Instance.prestiges.prestigeCount = 1;
            Assert.IsFalse(prestigeLock.IsUnlocked(), "Should be locked, prestige count is 1.");

            PrestigeManager.Instance.prestiges.prestigeCount = 2;
            Assert.IsTrue(prestigeLock.IsUnlocked(), "Should be unlocked, prestige count is 2.");
        }

        [Test]
        public void Lock_LocationLock_ValidatesCorrectly()
        {
            var testLocation = ScriptableObject.CreateInstance<Location>();
            Lock locLock = new Lock { locationLock = testLocation };

            Assert.IsFalse(locLock.IsUnlocked(), "Should be locked, active location is different.");

            LocationsManager.Instance.TryTravelTo(testLocation);

            Assert.IsTrue(locLock.IsUnlocked(), "Should be unlocked, active location is test location.");

            Object.DestroyImmediate(testLocation);
        }

        [Test]
        public void Lock_PlayerRequirements_ThrowWhenPlayerStatsIsMissing()
        {
            CustomStat customStat = ScriptableObject.CreateInstance<CustomStat>();
            Object.DestroyImmediate(PlayerStats.Instance);

            try
            {
                AssertMissingManagerThrows(
                    new Lock { playerLevelLock = true, inputAmount = 1 },
                    nameof(PlayerStats));
                AssertMissingManagerThrows(
                    new Lock { customStatLock = customStat, inputAmount = 1 },
                    nameof(PlayerStats));
            }
            finally
            {
                Object.DestroyImmediate(customStat);
            }
        }

        [Test]
        public void Lock_PrestigeRequirement_ThrowsBeforeOtherRequirementsCanShortCircuit()
        {
            Object.DestroyImmediate(PrestigeManager.Instance);
            Lock lockWithEarlierFailure = new Lock
            {
                businessLock = TestUtilities.TestBusiness,
                inputAmount = 1000,
                prestigeLock = true
            };

            AssertMissingManagerThrows(lockWithEarlierFailure, nameof(PrestigeManager));
        }

        [Test]
        public void Lock_LocationRequirement_ThrowsWhenLocationsManagerIsMissing()
        {
            Location location = ScriptableObject.CreateInstance<Location>();
            Object.DestroyImmediate(LocationsManager.Instance);

            try
            {
                AssertMissingManagerThrows(new Lock { locationLock = location }, nameof(LocationsManager));
            }
            finally
            {
                Object.DestroyImmediate(location);
            }
        }

        [Test]
        public void Lock_WithNoRequirements_DoesNotRequireOptionalManagers()
        {
            Object.DestroyImmediate(PlayerStats.Instance);
            Object.DestroyImmediate(PrestigeManager.Instance);
            Object.DestroyImmediate(LocationsManager.Instance);

            Lock emptyLock = new Lock();

            Assert.IsTrue(emptyLock.IsUnlocked());
            Assert.IsTrue(emptyLock.IsUpperUnlocked());
        }

        [Test]
        public void Lock_UpperLockAmountType_ValidatesCorrectly()
        {
            // Business test
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 10); // Amount = 10, TotalAmount = 10, BoughtAmount = 0
            
            Lock businessLock = new Lock { businessLock = TestUtilities.TestBusiness, inputAmount = 5 }; // max 5 businesses

            businessLock.amountType = LockAmountType.CurrentAmount;
            Assert.IsFalse(businessLock.IsUpperUnlocked(), "Upper lock should be locked using CurrentAmount (10 > 5)");

            businessLock.inputAmount = 15;
            Assert.IsTrue(businessLock.IsUpperUnlocked(), "Upper lock should be unlocked using CurrentAmount (10 <= 15)");

            businessLock.inputAmount = 5;
            businessLock.amountType = LockAmountType.BoughtAmount;
            Assert.IsTrue(businessLock.IsUpperUnlocked(), "Upper lock should be unlocked using BoughtAmount (0 <= 5)");
            
            BusinessesManager.Instance.GetOrAddHolder(TestUtilities.TestBusiness).boughtAmount += 10; 
            Assert.IsFalse(businessLock.IsUpperUnlocked(), "Upper lock should be locked using BoughtAmount (10 > 5)");

            var oldMaxUpperAmount = TestUtilities.TestBusiness.maxAmount_;
            TestUtilities.TestBusiness.maxAmount_ = 20;
            businessLock.inputAmount = 25;
            businessLock.amountType = LockAmountType.MaxAmount;
            Assert.IsTrue(businessLock.IsUpperUnlocked(), "Upper lock should be unlocked using MaxAmount (20 <= 25)");
            businessLock.inputAmount = 15;
            Assert.IsFalse(businessLock.IsUpperUnlocked(), "Upper lock should be locked using MaxAmount (20 > 15)");
            TestUtilities.TestBusiness.maxAmount_ = oldMaxUpperAmount;

            // Level test
            PlayerStats.Instance.AddXp(100); // Level 1
            Lock levelLock = new Lock { playerLevelLock = true, inputAmount = 0 };
            Assert.IsFalse(levelLock.IsUpperUnlocked(), "Upper lock should be locked (Level 1 > 0)");

            levelLock.inputAmount = 1;
            Assert.IsTrue(levelLock.IsUpperUnlocked(), "Upper lock should be unlocked (Level 1 <= 1)");
        }
        [Test]
        public void Lock_BusinessGroupLock_ValidatesCorrectly()
        {
            var testGroup = ScriptableObject.CreateInstance<BusinessGroup>();
            var testBusiness = ScriptableObject.CreateInstance<Business>();
            testBusiness.businessGroups.Add(testGroup);
            
            BusinessesManager.Instance.ForceBuyItemsForFree(testBusiness, 10);
            
            Lock groupLock = new Lock { businessGroupLock = testGroup, inputAmount = 5 };

            groupLock.amountType = LockAmountType.CurrentAmount;
            Assert.IsTrue(groupLock.IsUnlocked(), "Should be unlocked using CurrentAmount (10 >= 5)");

            groupLock.amountType = LockAmountType.TotalAmount;
            Assert.IsTrue(groupLock.IsUnlocked(), "Should be unlocked using TotalAmount (10 >= 5)");

            groupLock.amountType = LockAmountType.BoughtAmount;
            Assert.IsFalse(groupLock.IsUnlocked(), "Should be locked using BoughtAmount (0 < 5)");
            
            BusinessesManager.Instance.GetOrAddHolder(testBusiness).boughtAmount += 5;
            Assert.IsTrue(groupLock.IsUnlocked(), "Should be unlocked using BoughtAmount (5 >= 5)");

            Object.DestroyImmediate(testGroup);
            Object.DestroyImmediate(testBusiness);
        }

        [Test]
        public void Lock_CurrencyGroupLock_ValidatesCorrectly()
        {
            var testGroup = ScriptableObject.CreateInstance<CurrencyGroup>();
            var testCurrency = ScriptableObject.CreateInstance<Currency>();
            testCurrency.currencyGroups.Add(testGroup);

            CurrencyManager.Instance.AddCurrencyAmount(testCurrency, 50);
            CurrencyManager.Instance.RemoveCurrencyAmount(testCurrency, 30); // Total made = 50, amount = 20

            Lock groupLock = new Lock { currencyGroupLock = testGroup, inputAmount = 40 };

            groupLock.amountType = LockAmountType.CurrentAmount;
            Assert.IsFalse(groupLock.IsUnlocked(), "Should be locked using CurrentAmount (20 < 40)");

            groupLock.amountType = LockAmountType.TotalAmount;
            Assert.IsTrue(groupLock.IsUnlocked(), "Should be unlocked using TotalAmount (50 >= 40)");

            groupLock.amountType = LockAmountType.BoughtAmount;
            Assert.IsFalse(groupLock.IsUnlocked(), "Should be locked using BoughtAmount since it acts as CurrentAmount (20 < 40)");

            CurrencyManager.Instance.AddCurrencyAmount(testCurrency, 30); // amount = 50
            Assert.IsTrue(groupLock.IsUnlocked(), "Should be unlocked using BoughtAmount since it acts as CurrentAmount (50 >= 40)");

            Object.DestroyImmediate(testGroup);
            Object.DestroyImmediate(testCurrency);
        }

        [Test]
        public void Lock_UpgradeGroupLock_ValidatesCorrectly()
        {
            var testGroup = ScriptableObject.CreateInstance<UpgradeGroup>();
            var testUpgrade = ScriptableObject.CreateInstance<Upgrade>();
            testUpgrade.upgradeGroups.Add(testGroup);

            UpgradesManager.Instance.ForceBuyItemsForFree(testUpgrade, 10);

            Lock groupLock = new Lock { upgradeGroupLock = testGroup, inputAmount = 15 };

            groupLock.amountType = LockAmountType.CurrentAmount;
            Assert.IsFalse(groupLock.IsUnlocked(), "Should be locked using CurrentAmount (10 < 15)");

            groupLock.amountType = LockAmountType.TotalAmount;
            Assert.IsFalse(groupLock.IsUnlocked(), "Should be locked using TotalAmount (10 < 15)");

            groupLock.amountType = LockAmountType.BoughtAmount;
            Assert.IsFalse(groupLock.IsUnlocked(), "Should be locked using BoughtAmount (0 < 15)");

            UpgradesManager.Instance.ForceBuyItemsForFree(testUpgrade, 5);
            groupLock.amountType = LockAmountType.CurrentAmount;
            Assert.IsTrue(groupLock.IsUnlocked(), "Should be unlocked using CurrentAmount (15 >= 15)");

            groupLock.amountType = LockAmountType.TotalAmount;
            Assert.IsTrue(groupLock.IsUnlocked(), "Should be unlocked using TotalAmount (15 >= 15)");
            
            groupLock.amountType = LockAmountType.BoughtAmount;
            Assert.IsFalse(groupLock.IsUnlocked(), "Should be locked using BoughtAmount (0 < 15)");
            
            UpgradesManager.Instance.GetOrAddHolder(testUpgrade).boughtAmount += 15;
            Assert.IsTrue(groupLock.IsUnlocked(), "Should be unlocked using BoughtAmount (15 >= 15)");

            Object.DestroyImmediate(testGroup);
            Object.DestroyImmediate(testUpgrade);
        }

        private static void AssertMissingManagerThrows(Lock lockRequirement, string managerName)
        {
            InvalidOperationException lowerException = Assert.Throws<InvalidOperationException>(() => lockRequirement.IsUnlocked());
            StringAssert.Contains(managerName, lowerException.Message);

            InvalidOperationException upperException = Assert.Throws<InvalidOperationException>(() => lockRequirement.IsUpperUnlocked());
            StringAssert.Contains(managerName, upperException.Message);
        }
    }
}
