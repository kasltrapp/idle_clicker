using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace EasyIdleGame.Tests
{
    public class LocationsManagerTests
    {
        private GameObject _managerObj;

        [SetUp]
        public void SetUp()
        {
            _managerObj = TestUtilities.SetupManagerObject();
        }

        [TearDown]
        public void TearDown()
        {
            TestUtilities.TearDownManagerObject(_managerObj);
        }

        [Test]
        public void TravelTo_ValidatesUnlockAndPurchaseConditions()
        {
            var freeLocation = ScriptableObject.CreateInstance<Location>();
            freeLocation.cost = new List<Input>();
            freeLocation.locks = new List<Lock>();

            var lockedLocation = ScriptableObject.CreateInstance<Location>();
            lockedLocation.locks = new List<Lock>() { new Lock() { playerLevelLock = true, inputAmount = 100 } };

            var premiumLocation = ScriptableObject.CreateInstance<Location>();
            premiumLocation.locks = new List<Lock>();
            var currency = ScriptableObject.CreateInstance<Currency>();
            currency.maxAmount = -1;
            premiumLocation.cost = new List<Input>() { new Input() { currencyInput = currency, inputAmount = 1000 } };
            CurrencyManager.Instance.GetOrAddHolder(currency).amount = 500; // Not enough

            var purchasableLocation = ScriptableObject.CreateInstance<Location>();
            purchasableLocation.locks = new List<Lock>();
            purchasableLocation.cost = new List<Input>() { new Input() { currencyInput = currency, inputAmount = 100 } };

            Assert.DoesNotThrow(() => LocationsManager.Instance.TryTravelTo(freeLocation), "Should travel to free unlocked location without error");
            Assert.AreEqual(freeLocation, LocationsManager.Instance.ActiveLocation, "ActiveLocation should update to freeLocation");

            LocationsManager.Instance.TryTravelTo(lockedLocation);
            Assert.AreNotEqual(lockedLocation, LocationsManager.Instance.ActiveLocation, "Should not travel to locked location");

            bool canBuy = LocationsManager.Instance.GetOrAddHolder(premiumLocation).iBuyable.CanBuy(1);
            Assert.IsFalse(canBuy, $"CanBuy should return false, but it returned true. PremiumLocation cost: {premiumLocation.cost[0].inputAmount}, currency amount: {CurrencyManager.Instance.GetCurrencyAmount(currency)}");
            
            LocationsManager.Instance.TryTravelTo(premiumLocation);
            Assert.AreNotEqual(premiumLocation, LocationsManager.Instance.ActiveLocation, "Should not travel to premium location without sufficient funds");

            LocationsManager.Instance.TryTravelTo(purchasableLocation);
            Assert.AreEqual(purchasableLocation, LocationsManager.Instance.ActiveLocation, "Should travel to purchasable location when funds are sufficient");
            Assert.AreEqual(new BigNumber(400), CurrencyManager.Instance.GetOrAddHolder(currency).amount, "Should deduct currency for purchasing location");

            Object.DestroyImmediate(freeLocation);
            Object.DestroyImmediate(lockedLocation);
            Object.DestroyImmediate(premiumLocation);
            Object.DestroyImmediate(purchasableLocation);
            Object.DestroyImmediate(currency);
        }

        [Test]
        public void ActiveLocation_ChangeFiresEvent()
        {
            var location = ScriptableObject.CreateInstance<Location>();
            location.locks = new List<Lock>();
            bool eventFired = false;

            LocationsManager.Instance.OnLocationChanged.AddListener((loc) => 
            {
                eventFired = true;
                Assert.AreEqual(location, loc, "Event should pass the new location");
            });

            LocationsManager.Instance.TryTravelTo(location);

            Assert.IsTrue(eventFired, "OnLocationChanged event should fire when traveling to a new location");

            eventFired = false;
            LocationsManager.Instance.TryTravelTo(location);
            Assert.IsFalse(eventFired, "OnLocationChanged event should not fire if traveling to the same location");

            Object.DestroyImmediate(location);
        }
        [Test]
        public void AutoUnlock_FiresEventWhenUnlocked()
        {
            var location = ScriptableObject.CreateInstance<Location>();
            location.locks = new List<Lock>() { new Lock() { playerLevelLock = true, inputAmount = 10 } };

            bool eventFired = false;
            LocationsManager.Instance.OnItemUnlocked.AddListener((loc) =>
            {
                if (loc == location) eventFired = true;
            });

            var iUnlockableManager = (IUnlockableHolderManager<LocationHolder, Location>)LocationsManager.Instance;

            iUnlockableManager.GetOrAddHolder(location);

            iUnlockableManager.CheckAutoUnlocks();
            Assert.IsFalse(eventFired, "Event should not fire when location is still locked.");

            PlayerStats.Instance.level.currentLevel = 10;

            iUnlockableManager.CheckAutoUnlocks();
            Assert.IsTrue(eventFired, "Event should fire when location becomes unlocked.");

            eventFired = false;
            iUnlockableManager.CheckAutoUnlocks();
            Assert.IsFalse(eventFired, "Event should not fire a second time for the same location.");

            Object.DestroyImmediate(location);
        }

        [Test]
        public void TryUnlockLocation_AppliesStarterOutputs_OnFirstUnlock()
        {
            Currency reward = ScriptableObject.CreateInstance<Currency>();
            reward.maxAmount = -1;

            Location location = ScriptableObject.CreateInstance<Location>();
            location.locks = new List<Lock>();
            location.cost = new List<Input>();
            location.starterOutputTables = new List<DropTable>
            {
                new DropTable
                {
                    strategy = DropStrategy.All,
                    outputs = new List<Output>
                    {
                        new Output { currencyOutput = reward, outputAmount = 500, dropChance = 1f }
                    }
                }
            };

            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(reward), "Reward currency should be 0 before unlock");

            bool success = LocationsManager.Instance.TryUnlockLocation(location);

            Assert.IsTrue(success, "TryUnlockLocation should succeed for a free, unlocked location");
            Assert.AreEqual(new BigNumber(500), CurrencyManager.Instance.GetCurrencyAmount(reward), "Starter output should grant 500 currency on first unlock");

            // Attempting to unlock again should fail (already maxed / bought once)
            BigNumber currencyBefore = CurrencyManager.Instance.GetCurrencyAmount(reward);
            bool secondAttempt = LocationsManager.Instance.TryUnlockLocation(location);

            Assert.IsFalse(secondAttempt, "TryUnlockLocation should return false on second attempt (already bought)");
            Assert.AreEqual(currencyBefore, CurrencyManager.Instance.GetCurrencyAmount(reward), "Starter outputs must not be applied again on a second unlock attempt");

            Object.DestroyImmediate(location);
            Object.DestroyImmediate(reward);
        }
    }
}
