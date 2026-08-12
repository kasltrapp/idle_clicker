using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace EasyIdleGame.Tests
{
    public class ShopManagerTests
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
        public void ShopItem_PurchaseValidation()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            var currency = ScriptableObject.CreateInstance<Currency>();
            currency.maxAmount = -1;
            
            shopItem.inGameCost = new List<Input>() { new Input() { currencyInput = currency, inputAmount = 100 } };
            shopItem.maxPurchases = 0; // Infinite
            
            var holder = new ShopItemHolder(shopItem);
            holder.AlreadyUnlocked = true;

            Assert.IsFalse(holder.CanBuy(1), "Cannot buy if currency is 0");

            CurrencyManager.Instance.GetOrAddHolder(currency).amount = 250;
            Assert.IsTrue(holder.CanBuy(2), "Can buy 2 if currency is sufficient");
            Assert.IsFalse(holder.CanBuy(3), "Cannot buy 3 if currency is insufficient");

            // Cap purchases
            shopItem.maxPurchases = 2;
            holder.AddAmount(2); // Manually max out
            Assert.IsFalse(holder.CanBuy(1), "Cannot buy if max purchases limit reached");

            Object.DestroyImmediate(shopItem);
            Object.DestroyImmediate(currency);
        }

        [Test]
        public void GetPurchasePreview_WithoutHolder_ReturnsShopDataWithoutMutatingState()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            var currency = ScriptableObject.CreateInstance<Currency>();
            currency.maxAmount = -1;
            shopItem.inGameCost = new List<Input>
            {
                new Input { currencyInput = currency, inputAmount = 25 }
            };
            shopItem.maxPurchases = 4;
            CurrencyManager.Instance.GetOrAddHolder(currency).amount = 100;

            var preview = ShopManager.Instance.iBuyable.GetPurchasePreview(shopItem, 3);

            Assert.IsTrue(preview.canAfford, "Three items should be affordable with 100 currency.");
            Assert.AreEqual(new BigNumber(75), preview.effectiveCost[0].inputAmount, "Preview should include the total cost for the requested amount.");
            Assert.AreEqual(new BigNumber(4), preview.maxAffordableAmount, "A new item should report its complete purchase allowance.");
            Assert.AreEqual(new BigNumber(0), preview.currentAmount, "A shop item without a holder should have no current amount.");
            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(currency), "Previewing must not deduct currency.");
            Assert.IsFalse(ShopManager.Instance.TryGetHolder(shopItem, out _), "Previewing must not create a shop item holder.");

            Object.DestroyImmediate(shopItem);
            Object.DestroyImmediate(currency);
        }

        [Test]
        public void GetPurchasePreview_WithHolder_UsesCurrentShopStateWithoutMutatingIt()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            var currency = ScriptableObject.CreateInstance<Currency>();
            currency.maxAmount = -1;
            shopItem.inGameCost = new List<Input>
            {
                new Input { currencyInput = currency, inputAmount = 40 }
            };
            shopItem.maxPurchases = 3;
            CurrencyManager.Instance.GetOrAddHolder(currency).amount = 80;

            ShopItemHolder holder = ShopManager.Instance.GetOrAddHolder(shopItem);
            holder.AlreadyUnlocked = true;
            holder.AddAmount(2);

            var preview = ShopManager.Instance.iBuyable.GetPurchasePreview(shopItem, 1);

            Assert.IsTrue(preview.canAfford, "The final allowed purchase should be affordable.");
            Assert.AreEqual(new BigNumber(40), preview.effectiveCost[0].inputAmount, "Preview should return the effective in-game cost.");
            Assert.AreEqual(new BigNumber(1), preview.maxAffordableAmount, "Only one purchase should remain before the limit.");
            Assert.AreEqual(new BigNumber(2), preview.currentAmount, "Preview should reflect the holder's current amount.");
            Assert.AreEqual(new BigNumber(2), holder.Amount, "Previewing must not change the held amount.");
            Assert.AreEqual(new BigNumber(80), CurrencyManager.Instance.GetCurrencyAmount(currency), "Previewing must not deduct currency.");

            Object.DestroyImmediate(shopItem);
            Object.DestroyImmediate(currency);
        }

        [Test]
        public void GetPurchasePreview_WhenShopCostIsUnaffordable_ReturnsFalseWithoutMutating()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            var currency = ScriptableObject.CreateInstance<Currency>();
            currency.maxAmount = -1;
            shopItem.inGameCost = new List<Input>
            {
                new Input { currencyInput = currency, inputAmount = 60 }
            };
            CurrencyManager.Instance.GetOrAddHolder(currency).amount = 100;

            var preview = ShopManager.Instance.iBuyable.GetPurchasePreview(shopItem, 2);

            Assert.IsFalse(preview.canAfford, "Two items should not be affordable with only 100 currency.");
            Assert.AreEqual(new BigNumber(120), preview.effectiveCost[0].inputAmount);
            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(currency), "An unaffordable preview must not deduct currency.");
            Assert.IsFalse(ShopManager.Instance.TryGetHolder(shopItem, out _), "An unaffordable preview must not create a holder.");

            Object.DestroyImmediate(shopItem);
            Object.DestroyImmediate(currency);
        }

        [Test]
        public void AttemptPurchase_ValidatesLimitsAndDelegates()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            shopItem.buyWithAd = true;
            shopItem.productId = "premium.pack.1";
            shopItem.maxPurchases = 1;

            var holder = ShopManager.Instance.GetOrAddHolder(shopItem);
            
            // These should log correctly instead of throwing
            Assert.DoesNotThrow(() => ShopManager.Instance.AttemptPurchaseWithRealMoney(shopItem), "Attempting purchase should not throw");
            Assert.DoesNotThrow(() => ShopManager.Instance.AttemptPurchaseWithAd(shopItem), "Attempting purchase with ad should not throw");

            holder.AddAmount(1); // Max out

            // Trying to purchase when maxed should just early return with a log
            Assert.DoesNotThrow(() => ShopManager.Instance.AttemptPurchaseWithRealMoney(shopItem), "Attempting purchase when maxed should not throw");
            Assert.DoesNotThrow(() => ShopManager.Instance.AttemptPurchaseWithAd(shopItem), "Attempting purchase with ad when maxed should not throw");

            Object.DestroyImmediate(shopItem);
        }

        [Test]
        public void ForceBuyItemsForFree_GrantsRewardsCorrectly()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            var rewardCurrency = ScriptableObject.CreateInstance<Currency>();
            rewardCurrency.maxAmount = -1;
            shopItem.rewardTables = new List<DropTable>() { new DropTable() { outputs = new List<Output>() { new Output() { currencyOutput = rewardCurrency, outputAmount = 500 } } } };

            bool boughtEventFired = false;
            ShopManager.Instance.OnItemBought.AddListener((item) => { if (item == shopItem) boughtEventFired = true; });

            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetOrAddHolder(rewardCurrency).amount, "Initial reward currency should be 0");

            ShopManager.Instance.ForceBuyItemsForFree(shopItem, 3); // buy 3 times

            Assert.AreEqual(new BigNumber(1500), CurrencyManager.Instance.GetOrAddHolder(rewardCurrency).amount, "Should grant 500 * 3 = 1500 reward currency");
            Assert.IsTrue(boughtEventFired, "OnItemBought event should be fired");
            Assert.AreEqual(new BigNumber(3), ShopManager.Instance.GetOrAddHolder(shopItem).Amount, "Holder amount should increase by 3");

            Object.DestroyImmediate(shopItem);
            Object.DestroyImmediate(rewardCurrency);
        }

        [Test]
        public void AttemptPurchase_WithAd_DelegatesToAdSystem()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            shopItem.buyWithAd = true;
            
            bool adFired = false;
            ShopManager.Instance.OnAttemptAdPurchase.AddListener((item) => { if (item == shopItem) adFired = true; });

            ShopManager.Instance.AttemptPurchase(shopItem);

            Assert.IsTrue(adFired, "OnAttemptAdPurchase should fire for ad-enabled item");
            Object.DestroyImmediate(shopItem);
        }

        [Test]
        public void AttemptPurchase_WithRealMoney_DelegatesToIAPSystem()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            shopItem.productId = "premium.pack.1";
            
            bool iapFired = false;
            ShopManager.Instance.OnAttemptRealMoneyPurchase.AddListener((item) => { if (item == shopItem) iapFired = true; });

            ShopManager.Instance.AttemptPurchase(shopItem);

            Assert.IsTrue(iapFired, "OnAttemptRealMoneyPurchase should fire for IAP item");
            Object.DestroyImmediate(shopItem);
        }

        [Test]
        public void AttemptPurchase_WithInGameCost_DeductsCostAndGrantsItem()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            var currency = ScriptableObject.CreateInstance<Currency>();
            currency.maxAmount = -1;
            
            shopItem.inGameCost = new List<Input>() { new Input() { currencyInput = currency, inputAmount = 100 } };
            CurrencyManager.Instance.GetOrAddHolder(currency).amount = 200;
            
            ShopManager.Instance.AttemptPurchase(shopItem);

            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(currency), "Cost should be deducted");
            Assert.AreEqual(new BigNumber(1), ShopManager.Instance.GetHolder(shopItem).Amount, "Item should be granted");

            Object.DestroyImmediate(shopItem);
            Object.DestroyImmediate(currency);
        }

        [Test]
        public void AttemptPurchase_FreeItem_GrantsItem()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            ShopManager.Instance.AttemptPurchase(shopItem);
            Assert.AreEqual(new BigNumber(1), ShopManager.Instance.GetHolder(shopItem).Amount, "Free item should be granted directly");
            Object.DestroyImmediate(shopItem);
        }

        [Test]
        public void AttemptPurchase_MultipleCostsConfigured_PrioritizesAd()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            shopItem.buyWithAd = true;
            shopItem.productId = "premium.pack.1";
            var currency = ScriptableObject.CreateInstance<Currency>();
            shopItem.inGameCost = new List<Input>() { new Input() { currencyInput = currency, inputAmount = 100 } };
            CurrencyManager.Instance.GetOrAddHolder(currency).amount = 200;
            
            bool adFired = false;
            bool iapFired = false;
            ShopManager.Instance.OnAttemptAdPurchase.AddListener((item) => adFired = true);
            ShopManager.Instance.OnAttemptRealMoneyPurchase.AddListener((item) => iapFired = true);

            ShopManager.Instance.AttemptPurchase(shopItem);

            Assert.IsTrue(adFired, "Ad should be prioritized");
            Assert.IsFalse(iapFired, "IAP should not fire if Ad is present");
            Assert.AreEqual(new BigNumber(200), CurrencyManager.Instance.GetCurrencyAmount(currency), "InGameCost should not be deducted");
            Assert.AreEqual(new BigNumber(0), ShopManager.Instance.GetOrAddHolder(shopItem).Amount, "Item should not be granted directly");

            Object.DestroyImmediate(shopItem);
            Object.DestroyImmediate(currency);
        }

        [Test]
        public void AttemptPurchase_RealMoneyAndInGameConfigured_PrioritizesRealMoney()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            shopItem.productId = "premium.pack.1";
            var currency = ScriptableObject.CreateInstance<Currency>();
            shopItem.inGameCost = new List<Input>() { new Input() { currencyInput = currency, inputAmount = 100 } };
            CurrencyManager.Instance.GetOrAddHolder(currency).amount = 200;
            
            bool iapFired = false;
            ShopManager.Instance.OnAttemptRealMoneyPurchase.AddListener((item) => iapFired = true);

            ShopManager.Instance.AttemptPurchase(shopItem);

            Assert.IsTrue(iapFired, "IAP should be prioritized over InGameCost");
            Assert.AreEqual(new BigNumber(200), CurrencyManager.Instance.GetCurrencyAmount(currency), "InGameCost should not be deducted");
            Assert.AreEqual(new BigNumber(0), ShopManager.Instance.GetOrAddHolder(shopItem).Amount, "Item should not be granted directly");

            Object.DestroyImmediate(shopItem);
            Object.DestroyImmediate(currency);
        }

        [Test]
        public void TryConsumeItem_ReturnsAccurateProfitData()
        {
            var shopItem = ScriptableObject.CreateInstance<ShopItem>();
            var rewardCurrency = ScriptableObject.CreateInstance<Currency>();
            rewardCurrency.maxAmount = -1;
            var rewardBusiness = ScriptableObject.CreateInstance<Business>();
            
            shopItem.rewardTables = new List<DropTable>() 
            { 
                new DropTable() 
                { 
                    strategy = DropStrategy.All,
                    outputs = new List<Output>() 
                    { 
                        new Output() { currencyOutput = rewardCurrency, outputAmount = 150, dropChance = 1f },
                        new Output() { businessOutput = rewardBusiness, outputAmount = 2, dropChance = 1f }
                    } 
                } 
            };

            // Setup
            ShopItemHolder holder = ShopManager.Instance.GetOrAddHolder(shopItem);
            holder.amount = 1; // Give the player 1 item manually to bypass ForceBuyItemsForFree's instant rewards
            
            // Initial checks
            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetOrAddHolder(rewardCurrency).amount, "Initial currency is 0");
            Assert.AreEqual(new BigNumber(0), BusinessesManager.Instance.GetOrAddHolder(rewardBusiness).amount, "Initial business is 0");
            Assert.AreEqual(new BigNumber(1), holder.Amount, "Player has 1 shop item");

            // Consume
            bool success = ShopManager.Instance.TryConsumeItem(shopItem, 1, out List<Output> outputs);

            // Assert List<Output> is not null
            Assert.IsTrue(success, "Consume should be successful");
            Assert.IsNotNull(outputs, "Consuming should return List<Output>");

            // Assert List<Output> values
            bool hasCurrency = outputs.Exists(x => x.currencyOutput == rewardCurrency && x.outputAmount == 150);
            Assert.IsTrue(hasCurrency, "List<Output> contains currency");
            bool hasBusiness = outputs.Exists(x => x.businessOutput == rewardBusiness && x.outputAmount == 2);
            Assert.IsTrue(hasBusiness, "List<Output> contains business");

            // Assert actual managers received the values
            Assert.AreEqual(new BigNumber(150), CurrencyManager.Instance.GetCurrencyAmount(rewardCurrency), "Currency manager received 150");
            Assert.AreEqual(new BigNumber(2), BusinessesManager.Instance.GetHolder(rewardBusiness).amount, "Business manager received 2");
            Assert.AreEqual(new BigNumber(0), holder.Amount, "Shop item was consumed");

            Object.DestroyImmediate(shopItem);
            Object.DestroyImmediate(rewardCurrency);
            Object.DestroyImmediate(rewardBusiness);
        }
    }
}
