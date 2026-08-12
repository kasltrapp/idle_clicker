using NUnit.Framework;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class OffersManagerTests
    {
        private GameObject _managerObject;

        private ShopItem _testShopItem1;
        private ShopItem _testShopItem2;

        private T CreateTempAsset<T>(string assetName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = assetName;
            return asset;
        }

        [SetUp]
        public void SetUp()
        {
            _testShopItem1 = CreateTempAsset<ShopItem>("TempOfferShopItem1");
            _testShopItem2 = CreateTempAsset<ShopItem>("TempOfferShopItem2");

            _managerObject = TestUtilities.SetupManagerObject();
        }

        [TearDown]
        public void TearDown()
        {
            TestUtilities.TearDownManagerObject(_managerObject);

            Object.DestroyImmediate(_testShopItem1);
            Object.DestroyImmediate(_testShopItem2);
        }

        [Test]
        public void TimePasses_OfferBecomesAvailableAndEventIsFired()
        {
            OffersManager manager = OffersManager.Instance;
            manager.possibleOffers.Add(new OfferPoolItem { item = _testShopItem1, weight = 1, durationSeconds = 10f });
            manager.minSecondsBetweenOffers = 5f;
            manager.maxSecondsBetweenOffers = 5f;

            bool eventFired = false;
            manager.OnOfferBecameAvailable.AddListener((offer) => eventFired = true);

            manager.OnTimePassed(5.1f);

            Assert.IsNotNull(manager.ActiveOffer, "An offer should have become active after time passed.");
            Assert.AreEqual(_testShopItem1, manager.ActiveOffer.item, "The active offer should be the test shop item.");
            Assert.IsTrue(eventFired, "OnOfferBecameAvailable event should have fired.");
        }

        [Test]
        public void ActiveOffer_ExpiresAfterDuration_AndEventIsFired()
        {
            OffersManager manager = OffersManager.Instance;
            manager.possibleOffers.Add(new OfferPoolItem { item = _testShopItem1, weight = 1, durationSeconds = 10f });
            manager.minSecondsBetweenOffers = 5f;
            manager.maxSecondsBetweenOffers = 5f;

            manager.OnTimePassed(5.1f);
            Assert.IsNotNull(manager.ActiveOffer, "Offer should be active.");

            bool eventFired = false;
            manager.OnOfferExpired.AddListener((offer) => eventFired = true);

            manager.OnTimePassed(10.1f);

            Assert.IsNull(manager.ActiveOffer, "Offer should be null after expiring.");
            Assert.IsTrue(eventFired, "OnOfferExpired event should have fired.");
        }

        [Test]
        public void TryConsumeCurrentOffer_ClearsActiveOffer_AndSchedulesNext()
        {
            OffersManager manager = OffersManager.Instance;
            manager.possibleOffers.Add(new OfferPoolItem { item = _testShopItem1, weight = 1, durationSeconds = 10f });
            manager.minSecondsBetweenOffers = 5f;
            manager.maxSecondsBetweenOffers = 5f;

            manager.OnTimePassed(5.1f);
            Assert.IsTrue(manager.ActiveOffer != null, "Offer should be active.");
            
            manager.TryConsumeCurrentOffer();
            
            Assert.IsNull(manager.ActiveOffer, "ActiveOffer should be cleared after consumption.");
            
            // Fast forward to ensure next offer is scheduled
            manager.OnTimePassed(5.1f);
            Assert.IsNotNull(manager.ActiveOffer, "A new offer should be scheduled and active after consuming the previous one.");
        }

        [Test]
        public void ShowRandomOffer_WithLockedItem_SelectsOnlyUnlockedItem()
        {
            OffersManager manager = OffersManager.Instance;
            
            // Add a lock to shopItem1 so it's not unlocked
            _testShopItem1.locks = new System.Collections.Generic.List<Lock>();
            var lockObj = new Lock();
            lockObj.playerLevelLock = true;
            lockObj.inputAmount = 9999;
            _testShopItem1.locks.Add(lockObj);

            // Add both items to the pool. _testShopItem2 is unlocked by default.
            manager.possibleOffers.Add(new OfferPoolItem { item = _testShopItem1, weight = 1, durationSeconds = 10f });
            manager.possibleOffers.Add(new OfferPoolItem { item = _testShopItem2, weight = 1, durationSeconds = 10f });
            
            manager.minSecondsBetweenOffers = 5f;
            manager.maxSecondsBetweenOffers = 5f;

            manager.OnTimePassed(5.1f);
            
            Assert.IsNotNull(manager.ActiveOffer, "An offer should be shown because item 2 is unlocked.");
            Assert.AreEqual(_testShopItem2, manager.ActiveOffer.item, "The active offer should be item 2, completely ignoring the locked item 1.");
        }

        [Test]
        public void ShowRandomOffer_WithFailedUpperLock_DoesNotActivateOffer()
        {
            OffersManager manager = OffersManager.Instance;

            PlayerStats.Instance.AddXp(100); // Level 1, above the upper-lock limit of 0.
            var upperLock = new Lock
            {
                playerLevelLock = true,
                inputAmount = 0
            };
            _testShopItem1.upperLocks = new System.Collections.Generic.List<Lock>
            {
                upperLock
            };

            Assert.IsFalse(upperLock.IsUpperUnlocked(), "The test requires an upper lock that is not passing.");

            manager.possibleOffers.Add(new OfferPoolItem
            {
                item = _testShopItem1,
                weight = 1,
                durationSeconds = 10f
            });

            bool eventFired = false;
            manager.OnOfferBecameAvailable.AddListener((offer) => eventFired = true);

            manager.TriggerOffer();

            Assert.IsNull(manager.ActiveOffer, "An offer with a failed upper lock must not become active.");
            Assert.IsFalse(manager.IsOfferActive, "The manager must remain without an active offer when all upper locks fail.");
            Assert.IsFalse(eventFired, "OnOfferBecameAvailable must not fire for an offer with a failed upper lock.");
        }
    }
}
