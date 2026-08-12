using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class BusinessManagerTests
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
        public void MaxBuyableAmount()
        {
            Assert.IsTrue(TestUtilities.TestBusiness.GetMaxBuyableAmount(0) == 0,
                "Max amount with 0 currency should be 0");
            Assert.IsTrue(TestUtilities.TestBusinessWithPriceMultiplier.GetMaxBuyableAmount(1) == 0,
                "Max amount with 0 currency should be 0 for business with price multiplier");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 1000);
            Assert.IsTrue(TestUtilities.TestBusiness.GetMaxBuyableAmount(0) == 1000,
                $"Max amount with 1000 currency should be 1000. Actual: {TestUtilities.TestBusiness.GetMaxBuyableAmount(0)}");
            Assert.IsTrue(TestUtilities.TestBusinessWithPriceMultiplier.GetMaxBuyableAmount(0) == 11,
                $"Max amount with 1000 currency and price multiplier should be 11. Actual: {TestUtilities.TestBusinessWithPriceMultiplier.GetMaxBuyableAmount(0)}");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 12_000_000);
            Assert.IsTrue(TestUtilities.TestBusiness.GetMaxBuyableAmount(0).Floor() == 12_001_000,
                $"Max amount with 12_001_000 currency should be 12_001_000. Actual: {TestUtilities.TestBusiness.GetMaxBuyableAmount(0).Floor()}");
            Assert.IsTrue(TestUtilities.TestBusinessWithPriceMultiplier.GetMaxBuyableAmount(0).Floor() == 27,
                $"Max amount with 12_001_000 currency and price multiplier should be 27. Actual: {TestUtilities.TestBusinessWithPriceMultiplier.GetMaxBuyableAmount(0).Floor()}");
        }

        [Test]
        public void MaxBuyableAmount_WithSaleBoosts()
        {
            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 10);

            Assert.IsTrue(TestUtilities.TestBusiness.GetMaxBuyableAmount(0) == 15,
                $"Max amount with 10 currency and 45% sale. Actual: {TestUtilities.TestBusiness.GetMaxBuyableAmount(0)}");
            Assert.IsTrue(TestUtilities.TestBusinessWithPriceMultiplier.GetMaxBuyableAmount(0) == 4,
                $"Max amount with 10 currency, price multiplier and 45% sale. Actual: {TestUtilities.TestBusinessWithPriceMultiplier.GetMaxBuyableAmount(0)}");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 990);

            Assert.IsTrue(TestUtilities.TestBusiness.GetMaxBuyableAmount(0) == 1538,
                $"Max amount with 1000 currency and 45% sale. Actual: {TestUtilities.TestBusiness.GetMaxBuyableAmount(0)}");
            Assert.IsTrue(TestUtilities.TestBusinessWithPriceMultiplier.GetMaxBuyableAmount(0) == 12,
                $"Max amount with 1000 currency, price multiplier and 45% sale. Actual: {TestUtilities.TestBusinessWithPriceMultiplier.GetMaxBuyableAmount(0)}");

            BoostsManager.Instance.activeBoosts.Clear();

            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost2, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost2);

            Assert.IsTrue(TestUtilities.TestBusiness.GetMaxBuyableAmount(0) == 3_030,
                $"Max amount with 1000 currency and 67% sale. Actual: {TestUtilities.TestBusiness.GetMaxBuyableAmount(0).Floor()}");
        }

        [Test]
        public void CanPay()
        {
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 1000);

            Assert.IsTrue(TestUtilities.TestBusiness.iBuyable.CanPay(1, 0), "Should be able to buy 1 item with 1000 currency");
            Assert.IsTrue(TestUtilities.TestBusiness.iBuyable.CanPay(1000, 0), "Should be able to buy 1000 items with 1000 currency");

            Assert.IsTrue(TestUtilities.TestBusinessWithPriceMultiplier.iBuyable.CanPay(1, 0), "Should be able to buy 1 item with price multiplier");
            Assert.IsTrue(TestUtilities.TestBusinessWithPriceMultiplier.iBuyable.CanPay(11, 0), "Should be able to buy 11 items with price multiplier");
            Assert.IsFalse(TestUtilities.TestBusinessWithPriceMultiplier.iBuyable.CanPay(12, 0), "Should not be able to buy 12 items with 1000 currency and multiplier");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 123_455_789);
            Assert.IsTrue(TestUtilities.TestBusiness.iBuyable.CanPay(123_456_789, 0), "Should be able to buy 123_456_789 items with 123_456_789 currency");

            CurrencyManager.Instance.holders.Clear();
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, new BigNumber(1.23, 78));
            Assert.IsTrue(TestUtilities.TestBusiness.iBuyable.CanPay(new BigNumber(1.23, 78), 0), "Should be able to buy 1.23e78 items with 1.23e78 currency");
        }

        [Test]
        public void CanPay_WithSaleBoosts()
        {
            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);

            Assert.IsFalse(TestUtilities.TestBusiness.iBuyable.CanPay(1, 0), "Cannot buy with 0 currency and 45% sale");
            Assert.IsFalse(TestUtilities.TestBusinessWithPriceMultiplier.iBuyable.CanPay(1, 0), "Cannot buy with 0 currency, multiplier and 45% sale");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 10);

            Assert.IsTrue(TestUtilities.TestBusiness.iBuyable.CanPay(15, 0), "Can buy 15 with 10 currency and 45% sale");
            Assert.IsTrue(TestUtilities.TestBusinessWithPriceMultiplier.iBuyable.CanPay(4, 0), "Can buy 4 with 10 currency, multiplier and 45% sale");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 990);

            Assert.IsTrue(TestUtilities.TestBusiness.iBuyable.CanPay(1538, 0), "Can buy 1538 with 1000 currency and 45% sale");
            Assert.IsFalse(TestUtilities.TestBusiness.iBuyable.CanPay(1538 + 1, 0), "Cannot buy 1539 with 1000 currency and 45% sale");

            Assert.IsTrue(TestUtilities.TestBusinessWithPriceMultiplier.iBuyable.CanPay(12, 0), "Can buy 12 with 1000 currency, multiplier and 45% sale");
            Assert.IsFalse(TestUtilities.TestBusinessWithPriceMultiplier.iBuyable.CanPay(12 + 1, 0), "Cannot buy 13 with 1000 currency, multiplier and 45% sale");

            BoostsManager.Instance.activeBoosts.Clear();
            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost2, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost2);

            Assert.IsTrue(TestUtilities.TestBusiness.iBuyable.CanPay(3_030, 0), "Can buy 3030 with 1000 currency and 67% sale");
            Assert.IsFalse(TestUtilities.TestBusiness.iBuyable.CanPay(3_030 + 1, 0), "Cannot buy 3031 with 1000 currency and 67% sale");
        }

        [Test]
        public void MaxAmount_CanBuy_Tests()
        {
            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, 10);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, 10);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, 1_000);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, 1_000);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, 1_000_000);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, 1_000_000);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, new BigNumber(2.25, 8));
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(2.25, 8));

            // non precise tests
            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, new BigNumber(1.23, 123_456_789), false);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(1.23, 123_456_789), false);
        }

        [Test]
        public void MaxAmount_CanBuy_Tests_WithSaleBoost()
        {
            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, 10);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, 10);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, 1_000);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, 1_000);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, 1_000_000);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, 1_000_000);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, new BigNumber(2.2512, 4));
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(2.2512, 4));

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, new BigNumber(2.25, 8));
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(2.25, 8));

            // non precise tests
            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, new BigNumber(1.23, 123_456), false);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(1.23, 123_456), false);
        }

        [Test]
        public void MaxAmount_CanBuy_Tests_WithSaleBoost_NonZeroStartAmount()
        {
            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, new BigNumber(2.25, 5), 1);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(2.25, 5), 1);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, new BigNumber(2.25, 8), 10);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(2.25, 5), 10);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, new BigNumber(2.25, 500), 62, false);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(2.25, 500), 62, false);

            MaxAmount_CanBuy_Test(TestUtilities.TestBusiness, new BigNumber(2.25, 123_456), 1262, false);
            MaxAmount_CanBuy_Test(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(2.25, 123_456), 1262, false);
        }

        private void MaxAmount_CanBuy_Test(Business business, BigNumber currency, bool precise = true)
            => MaxAmount_CanBuy_Test(business, currency, 0, precise);

        private void MaxAmount_CanBuy_Test(Business business, BigNumber currency, BigNumber startBusinessAmount, bool precise = true)
        {
            CurrencyManager.Instance.holders.Clear(); // clear all currencies
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, currency);

            BigNumber maxAmount = business.GetMaxBuyableAmount(startBusinessAmount);

            Assert.IsTrue(business.iBuyable.CanPay(maxAmount, startBusinessAmount),
                $"MaxAmount with {currency} CANNOT be bought ({business.name}) - calculated amount = {maxAmount}");
            if (precise)
                Assert.IsFalse(business.iBuyable.CanPay(maxAmount + 1, startBusinessAmount),
                    $"MaxAmount + 1 with {currency} CAN be bought ({business.name}) - calculated amount = {maxAmount}");
        }

        [Test]
        public void BuyMaxAmountLoop_Simple()
        {
            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);
            BuyMaxLoopTest(TestUtilities.TestBusiness, new BigNumber(3, 15));

            TestUtilities.TearDownManagerObject(_managerObject);
            _managerObject = TestUtilities.SetupManagerObject();

            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 5);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);
            BuyMaxLoopTest(TestUtilities.TestBusiness, new BigNumber(3.32483423, 123));

            TestUtilities.TearDownManagerObject(_managerObject);
            _managerObject = TestUtilities.SetupManagerObject();

            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, new BigNumber(3, 15));
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);
            BuyMaxLoopTest(TestUtilities.TestBusiness, new BigNumber(3.32483423, 123));
        }

        [Test]
        public void BuyMaxAmountLoop()
        {
            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);
            BuyMaxLoopTest(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(3, 15));

            TestUtilities.TearDownManagerObject(_managerObject);
            _managerObject = TestUtilities.SetupManagerObject();

            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 12);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);
            BuyMaxLoopTest(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(3.32483423, 15));

            TestUtilities.TearDownManagerObject(_managerObject);
            _managerObject = TestUtilities.SetupManagerObject();

            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, new BigNumber(3, 3));
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);
            BuyMaxLoopTest(TestUtilities.TestBusinessWithPriceMultiplier, new BigNumber(4.6414899, 123));
        }

        [Test]
        public void BusinessLevelup()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 1);
            BusinessHolder businessHolder = BusinessesManager.Instance.GetHolder(TestUtilities.TestBusiness);

            Assert.IsTrue(businessHolder.level.currentLevel == 0, "Business should be at level 0 after buying 1 item");

            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 19);
            Assert.IsTrue(businessHolder.level.currentLevel == 1, "Business should be at level 1 after buying 20 total items");
            Assert.IsTrue(businessHolder.level.TargetXp == 40, $"Business target xp should be 40 for lvl 1 -> 2. Actual: {businessHolder.level.TargetXp}");
        }

        [Test]
        public void CanMergeBusinesses_ValidatesInputsCorrectly()
        {
            BusinessMergeRecipe recipe = ScriptableObject.CreateInstance<BusinessMergeRecipe>();
            recipe.inputs = new List<Input> { new Input { businessInput = TestUtilities.TestBusiness, inputAmount = 3 } };
            recipe.outputTables = new List<DropTable> { new DropTable { strategy = DropStrategy.All, outputs = new List<Output> { new Output { businessOutput = TestUtilities.TestBusinessWithPriceMultiplier, outputAmount = 1 } } } };

            Assert.IsFalse(BusinessesManager.Instance.CanMergeBusinesses(recipe), "Should not merge without enough inputs.");

            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 5);

            Assert.IsTrue(BusinessesManager.Instance.CanMergeBusinesses(recipe), "Should merge when having enough inputs.");

            Object.DestroyImmediate(recipe);
        }

        [Test]
        public void TryMergeBusinesses_RemovesInputsAndAddsOutputs()
        {
            BusinessMergeRecipe recipe = ScriptableObject.CreateInstance<BusinessMergeRecipe>();
            recipe.inputs = new List<Input> { new Input { businessInput = TestUtilities.TestBusiness, inputAmount = 3 } };
            recipe.outputTables = new List<DropTable> { new DropTable { strategy = DropStrategy.All, outputs = new List<Output> { new Output { businessOutput = TestUtilities.TestBusinessWithPriceMultiplier, outputAmount = 1 } } } };

            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 5);
            Assert.AreEqual(new BigNumber(0), BusinessesManager.Instance.GetHolder(TestUtilities.TestBusinessWithPriceMultiplier)?.amount ?? 0, "Initial output should be 0.");

            bool success = BusinessesManager.Instance.TryMergeBusinesses(recipe, out List<Output> result);

            Assert.IsTrue(success, "Merge should succeed.");
            Assert.IsNotNull(result, "Merge should return List<Output>.");
            Assert.AreEqual(new BigNumber(2), BusinessesManager.Instance.GetHolder(TestUtilities.TestBusiness).amount, "Inputs should be removed.");
            Assert.AreEqual(new BigNumber(1), BusinessesManager.Instance.GetHolder(TestUtilities.TestBusinessWithPriceMultiplier).amount, "Outputs should be added.");

            Object.DestroyImmediate(recipe);
        }

        [Test]
        public void BusinessWrapper_RuntimeHolderState_IsolatedBetweenSourceAndWrappers()
        {
            Business source = ScriptableObject.CreateInstance<Business>();
            BusinessWrapper wrapper1 = ScriptableObject.CreateInstance<BusinessWrapper>();
            BusinessWrapper wrapper2 = ScriptableObject.CreateInstance<BusinessWrapper>();

            try
            {
                source.cost = new List<Input>();
                source.maxAmount_ = -1;
                source.maxBuyableAmount = -1;

                wrapper1.underlyingBusiness = source;
                wrapper2.underlyingBusiness = source;
                wrapper1.SyncData();
                wrapper2.SyncData();

                BusinessHolder sourceHolder = BusinessesManager.Instance.GetOrAddHolder(source);
                BusinessHolder wrapper1Holder = BusinessesManager.Instance.GetOrAddHolder(wrapper1);
                BusinessHolder wrapper2Holder = BusinessesManager.Instance.GetOrAddHolder(wrapper2);

                Assert.AreNotSame(sourceHolder, wrapper1Holder, "Source and wrapper 1 should have separate runtime holders.");
                Assert.AreNotSame(sourceHolder, wrapper2Holder, "Source and wrapper 2 should have separate runtime holders.");
                Assert.AreNotSame(wrapper1Holder, wrapper2Holder, "Wrappers should have separate runtime holders.");
                AssertBusinessAmounts(sourceHolder, wrapper1Holder, wrapper2Holder, 0, 0, 0, "Initial state");

                BusinessesManager.Instance.ForceBuyItemsForFree(source, 5);
                AssertBusinessAmounts(sourceHolder, wrapper1Holder, wrapper2Holder, 5, 0, 0, "Mutating source should not affect wrappers");

                BusinessesManager.Instance.ForceBuyItemsForFree(wrapper1, 7);
                AssertBusinessAmounts(sourceHolder, wrapper1Holder, wrapper2Holder, 5, 7, 0, "Mutating wrapper 1 should not affect source or wrapper 2");

                BusinessesManager.Instance.RemoveBusiness(wrapper1, 2);
                AssertBusinessAmounts(sourceHolder, wrapper1Holder, wrapper2Holder, 5, 5, 0, "Removing wrapper 1 amount should remain isolated");

                BusinessesManager.Instance.ForceBuyItemsForFree(wrapper2, 11);
                AssertBusinessAmounts(sourceHolder, wrapper1Holder, wrapper2Holder, 5, 5, 11, "Mutating wrapper 2 should not affect source or wrapper 1");

                BusinessesManager.Instance.RemoveBusiness(source, 3);
                AssertBusinessAmounts(sourceHolder, wrapper1Holder, wrapper2Holder, 2, 5, 11, "Removing source amount should not affect wrappers");
            }
            finally
            {
                Object.DestroyImmediate(wrapper2);
                Object.DestroyImmediate(wrapper1);
                Object.DestroyImmediate(source);
            }
        }

        private static void AssertBusinessAmounts(
            BusinessHolder sourceHolder,
            BusinessHolder wrapper1Holder,
            BusinessHolder wrapper2Holder,
            BigNumber expectedSource,
            BigNumber expectedWrapper1,
            BigNumber expectedWrapper2,
            string context)
        {
            Assert.AreEqual(expectedSource, sourceHolder.Amount, $"{context}: source amount");
            Assert.AreEqual(expectedWrapper1, wrapper1Holder.Amount, $"{context}: wrapper 1 amount");
            Assert.AreEqual(expectedWrapper2, wrapper2Holder.Amount, $"{context}: wrapper 2 amount");
        }

        private void BuyMaxLoopTest(Business business, BigNumber currencyAmount, bool matchBusinessCount = true)
        {
            string testName = $"BuyMaxLoopTest: {business.name} with {currencyAmount}";

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, currencyAmount);
            BigNumber maxAmount = business.GetMaxBuyableAmount(BusinessesManager.Instance.iBuyable.GetItemAmount(TestUtilities.TestBusiness));

            bool success = BusinessesManager.Instance.iBuyable.TryBuyItems(business, maxAmount);
            Assert.IsTrue(success, $"Failed to buy max amount ({maxAmount}) of {business.name} with {currencyAmount} -- {testName}");

            if (matchBusinessCount)
                Assert.IsTrue(BusinessesManager.Instance.iBuyable.GetItemAmount(business) == maxAmount,
                    $"Bought business count does not match {maxAmount}. Actual: {BusinessesManager.Instance.iBuyable.GetItemAmount(business)} -- {testName}");

            maxAmount = business.GetMaxBuyableAmount(BusinessesManager.Instance.iBuyable.GetItemAmount(business));
            Assert.IsTrue(maxAmount == 0, $"There are still some businesses that can be bought {maxAmount}. Currency left: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency)} -- {testName}");
        }
        [Test]
        public void TotalAmount_TracksAllAcquisitions()
        {
            // starts at 0
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(TestUtilities.TestBusiness);
            Assert.AreEqual(new BigNumber(0), holder.TotalAmount, "TotalAmount should start at 0.");
            Assert.AreEqual(new BigNumber(0), holder.Amount, "Amount should start at 0.");

            // buying 5: both increase
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 5);
            Assert.AreEqual(new BigNumber(5), holder.TotalAmount, "TotalAmount should be 5 after buying 5.");
            Assert.AreEqual(new BigNumber(5), holder.Amount, "Amount should be 5 after buying 5.");

            // removing 3: Amount drops but TotalAmount must not
            BusinessesManager.Instance.RemoveBusiness(TestUtilities.TestBusiness, 3);
            Assert.AreEqual(new BigNumber(5), holder.TotalAmount, "TotalAmount must not decrease after removal.");
            Assert.AreEqual(new BigNumber(2), holder.Amount, "Amount should be 2 after removing 3.");

            // buying 10 more: TotalAmount accumulates from 5, not from 2
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 10);
            Assert.AreEqual(new BigNumber(15), holder.TotalAmount, "TotalAmount should be 15 after buying 10 more (cumulative from 5).");
            Assert.AreEqual(new BigNumber(12), holder.Amount, "Amount should be 12 (2 remaining + 10 bought).");
        }

        [Test]
        public void TotalAmount_RespectsMaxCapOnAmount_ButNotOnTotal()
        {
            // TestBusinessWithPriceMultiplier has a maxAmount configured — verify TotalAmount still grows
            // to test the cap behavior: buy well beyond current amount, Amount should cap but TotalAmount should reflect what was actually added
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(TestUtilities.TestBusiness);

            BigNumber cap = holder.GetMaxAmountConstrain();
            if (cap == BigNumber.MaxValue)
            {
                // no cap on this business — just verify TotalAmount == Amount when not removing
                BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 100);
                Assert.AreEqual(holder.Amount, holder.TotalAmount, "Without a cap and no removals, TotalAmount should equal Amount.");
                return;
            }

            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, cap + 100);
            Assert.AreEqual(cap, holder.Amount, "Amount should be capped.");
            Assert.AreEqual(cap, holder.TotalAmount, "TotalAmount should only count what was actually added (up to cap).");
        }

        [Test]
        public void BusinessGroupCapacity_ClampsTargetAndBlocksOversizedPurchase()
        {
            BusinessGroup group = ScriptableObject.CreateInstance<BusinessGroup>();
            Business business = CreateOneCreditBusiness("Grouped Test Business");

            try
            {
                group.capacity = 4;
                group.metadata.name = "Shared Test Capacity";

                business.businessGroups = new List<BusinessGroup> { group };

                CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 100);
                BusinessesManager.Instance.buyAmounts = new[] { new BuyAmount(5, BuyAmount.Type.digit) };

                BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);

                Assert.AreEqual(new BigNumber(4), BusinessesManager.Instance.GetTargetBusinessBuyAmount(business),
                    "The buy target should clamp to the remaining shared group capacity.");
                Assert.IsFalse(BusinessesManager.Instance.iBuyable.TryBuyItems(business, 5),
                    "A direct purchase larger than the remaining shared group capacity must fail.");
                Assert.AreEqual(new BigNumber(0), holder.amount, "Oversized purchase must not add any owned copies.");
                Assert.AreEqual(new BigNumber(0), holder.BoughtAmount, "Oversized purchase must not increment BoughtAmount.");
                Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency),
                    "Oversized purchase must not spend currency.");

                Assert.IsTrue(BusinessesManager.Instance.iBuyable.TryBuyItems(business, 4),
                    "A purchase that exactly fits the shared group capacity should succeed.");
                Assert.AreEqual(new BigNumber(4), holder.amount, "Owned copies should fill the shared group capacity.");
                Assert.AreEqual(new BigNumber(4), holder.BoughtAmount, "BoughtAmount should match the successful purchase.");
                Assert.AreEqual(new BigNumber(96), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency),
                    "Only the successful purchase should spend currency.");
                Assert.AreEqual(new BigNumber(0), BusinessesManager.Instance.GetTargetBusinessBuyAmount(business),
                    "No buy target should be available once the shared group capacity is full.");
                Assert.IsFalse(BusinessesManager.Instance.iBuyable.TryBuyItems(business, 1),
                    "Purchases should be blocked after the shared group capacity is full.");
                Assert.AreEqual(new BigNumber(4), holder.BoughtAmount, "Blocked purchases after the cap is full must not change BoughtAmount.");
            }
            finally
            {
                Object.DestroyImmediate(business);
                Object.DestroyImmediate(group);
            }
        }

        [Test]
        public void BusinessMaxAmount_ClampsTargetAndBlocksOversizedPurchase()
        {
            Business business = CreateOneCreditBusiness("Max Amount Test Business");

            try
            {
                business.maxAmount_ = 4;

                CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 100);
                BusinessesManager.Instance.buyAmounts = new[] { new BuyAmount(5, BuyAmount.Type.digit) };

                BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);

                Assert.AreEqual(new BigNumber(4), BusinessesManager.Instance.GetTargetBusinessBuyAmount(business),
                    "The buy target should clamp to the remaining owned max amount.");
                Assert.IsFalse(BusinessesManager.Instance.iBuyable.TryBuyItems(business, 5),
                    "A direct purchase larger than the owned max amount must fail.");
                Assert.AreEqual(new BigNumber(0), holder.amount, "Oversized purchase must not add any owned copies.");
                Assert.AreEqual(new BigNumber(0), holder.BoughtAmount, "Oversized purchase must not increment BoughtAmount.");
                Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency),
                    "Oversized purchase must not spend currency.");

                Assert.IsTrue(BusinessesManager.Instance.iBuyable.TryBuyItems(business, 4),
                    "A purchase that exactly fits the owned max amount should succeed.");
                Assert.AreEqual(new BigNumber(4), holder.amount, "Owned copies should fill the owned max amount.");
                Assert.AreEqual(new BigNumber(4), holder.BoughtAmount, "BoughtAmount should match the successful purchase.");
                Assert.AreEqual(new BigNumber(0), BusinessesManager.Instance.GetTargetBusinessBuyAmount(business),
                    "No buy target should be available once the owned max amount is full.");
                Assert.IsFalse(BusinessesManager.Instance.iBuyable.TryBuyItems(business, 1),
                    "Purchases should be blocked after the owned max amount is full.");
            }
            finally
            {
                Object.DestroyImmediate(business);
            }
        }

        [Test]
        public void BusinessMaxBuyableAmount_ClampsTargetAndBlocksOversizedPurchase()
        {
            Business business = CreateOneCreditBusiness("Max Buyable Test Business");

            try
            {
                business.maxAmount_ = 10;
                business.maxBuyableAmount = 3;

                CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 100);
                BusinessesManager.Instance.buyAmounts = new[] { new BuyAmount(5, BuyAmount.Type.digit) };

                BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);

                Assert.AreEqual(new BigNumber(3), BusinessesManager.Instance.GetTargetBusinessBuyAmount(business),
                    "The buy target should clamp to the remaining direct buy cap.");
                Assert.IsFalse(BusinessesManager.Instance.iBuyable.TryBuyItems(business, 5),
                    "A direct purchase larger than the direct buy cap must fail.");
                Assert.AreEqual(new BigNumber(0), holder.amount, "Oversized purchase must not add any owned copies.");
                Assert.AreEqual(new BigNumber(0), holder.BoughtAmount, "Oversized purchase must not increment BoughtAmount.");
                Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency),
                    "Oversized purchase must not spend currency.");

                Assert.IsTrue(BusinessesManager.Instance.iBuyable.TryBuyItems(business, 3),
                    "A purchase that exactly fits the direct buy cap should succeed.");
                Assert.AreEqual(new BigNumber(3), holder.amount, "Owned copies should fill the direct buy cap.");
                Assert.AreEqual(new BigNumber(3), holder.BoughtAmount, "BoughtAmount should match the successful purchase.");
                Assert.AreEqual(new BigNumber(0), BusinessesManager.Instance.GetTargetBusinessBuyAmount(business),
                    "No buy target should be available once the direct buy cap is full.");
                Assert.IsFalse(BusinessesManager.Instance.iBuyable.TryBuyItems(business, 1),
                    "Purchases should be blocked after the direct buy cap is full.");
            }
            finally
            {
                Object.DestroyImmediate(business);
            }
        }

        private static Business CreateOneCreditBusiness(string businessName)
        {
            Business business = ScriptableObject.CreateInstance<Business>();
            business.businessName = businessName;
            business.businessGroups = new List<BusinessGroup>();
            business.maxAmount_ = -1;
            business.maxBuyableAmount = -1;
            business.costMultiplier = 1;
            business.cost = new List<Input>
            {
                new Input { currencyInput = TestUtilities.TestCurrency, inputAmount = 1 }
            };
            business.locks = new List<Lock>();
            business.upperLocks = new List<Lock>();
            business.firstLevelCopies = 100;
            business.incrementMultiplier = 2;
            return business;
        }

        [Test]
        public void MergeRecipe_CurrencyInput_ConsumesCorrectly()
        {
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 100);

            BusinessMergeRecipe recipe = ScriptableObject.CreateInstance<BusinessMergeRecipe>();
            recipe.inputs = new List<Input> { new Input { currencyInput = TestUtilities.TestCurrency, inputAmount = 50 } };
            recipe.outputTables = new List<DropTable>();

            Assert.IsTrue(BusinessesManager.Instance.CanMergeBusinesses(recipe), "Should be able to merge with 100 currency (needs 50).");

            BusinessesManager.Instance.TryMergeBusinesses(recipe, out _);
            Assert.AreEqual(new BigNumber(50), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "50 currency should have been consumed.");

            // now below cost — should fail
            CurrencyManager.Instance.RemoveCurrencyAmount(TestUtilities.TestCurrency, 49);
            Assert.IsFalse(BusinessesManager.Instance.CanMergeBusinesses(recipe), "Should not merge with only 1 currency (needs 50).");

            Object.DestroyImmediate(recipe);
        }

        [Test]
        public void MergeRecipe_DropTableOutput_AppliesCorrectly()
        {
            BusinessMergeRecipe recipe = ScriptableObject.CreateInstance<BusinessMergeRecipe>();
            recipe.inputs = new List<Input>();

            // DropTable with guaranteed (dropChance=1) currency output
            recipe.outputTables = new List<DropTable>
            {
                new DropTable
                {
                    strategy = DropStrategy.All,
                    outputs = new List<Output>
                    {
                        new Output { currencyOutput = TestUtilities.TestCurrency, outputAmount = 200, dropChance = 1f }
                    }
                }
            };

            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Currency should start at 0.");

            bool success = BusinessesManager.Instance.TryMergeBusinesses(recipe, out List<Output> result);

            Assert.IsTrue(success, "Merge should succeed.");
            Assert.IsNotNull(result, "Merge should return List<Output>.");
            Assert.AreEqual(new BigNumber(200), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "DropTable output should grant 200 currency.");

            Object.DestroyImmediate(recipe);
        }

        [Test]
        public void MergeRecipe_MixedInputsAndOutputs()
        {
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 50);
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 3);

            BusinessMergeRecipe recipe = ScriptableObject.CreateInstance<BusinessMergeRecipe>();
            recipe.inputs = new List<Input>
            {
                new Input { currencyInput = TestUtilities.TestCurrency, inputAmount = 50 },
                new Input { businessInput = TestUtilities.TestBusiness, inputAmount = 3 }
            };
            recipe.outputTables = new List<DropTable>
            {
                new DropTable
                {
                    strategy = DropStrategy.All,
                    outputs = new List<Output>
                    {
                        new Output { businessOutput = TestUtilities.TestBusinessWithPriceMultiplier, outputAmount = 1, dropChance = 1f }
                    }
                }
            };

            Assert.IsTrue(BusinessesManager.Instance.CanMergeBusinesses(recipe), "Should be able to merge with sufficient inputs.");

            bool success = BusinessesManager.Instance.TryMergeBusinesses(recipe, out List<Output> result);

            Assert.IsTrue(success, "Merge should succeed.");
            Assert.IsNotNull(result, "Merge should return List<Output>.");
            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Currency should be fully consumed.");
            Assert.AreEqual(new BigNumber(0), BusinessesManager.Instance.GetHolder(TestUtilities.TestBusiness).amount, "Business inputs should be fully consumed.");
            Assert.AreEqual(new BigNumber(1), BusinessesManager.Instance.GetOrAddHolder(TestUtilities.TestBusinessWithPriceMultiplier).amount, "Output business should be granted.");

            Object.DestroyImmediate(recipe);
        }
        [Test]
        public void BoughtAmount_TracksOnlyPurchases()
        {
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(TestUtilities.TestBusiness);

            // starts at 0
            Assert.AreEqual(new BigNumber(0), holder.BoughtAmount, "BoughtAmount should start at 0");

            // ForceBuyItemsForFree must NOT increment BoughtAmount
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 10);
            Assert.AreEqual(new BigNumber(0), holder.BoughtAmount, "ForceBuyItemsForFree must not increment BoughtAmount");
            Assert.AreEqual(new BigNumber(10), holder.TotalAmount, "TotalAmount should be 10 after free grant");

            // TryBuyItems must increment BoughtAmount but not if it fails
            bool failedBuy = BusinessesManager.Instance.iBuyable.TryBuyItems(TestUtilities.TestBusiness, 5);
            Assert.IsFalse(failedBuy, "TryBuyItems should fail without sufficient currency");
            Assert.AreEqual(new BigNumber(0), holder.BoughtAmount, "BoughtAmount must not change after a failed buy");

            // fund the purchase and succeed
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 1_000_000);
            bool success = BusinessesManager.Instance.iBuyable.TryBuyItems(TestUtilities.TestBusiness, 5);
            Assert.IsTrue(success, "TryBuyItems should succeed with sufficient currency");
            Assert.AreEqual(new BigNumber(5), holder.BoughtAmount, "BoughtAmount should be 5 after buying 5");
            Assert.AreEqual(new BigNumber(15), holder.TotalAmount, "TotalAmount should be 15 (10 free + 5 bought)");

            // removal must not touch BoughtAmount
            BusinessesManager.Instance.RemoveBusiness(TestUtilities.TestBusiness, 3);
            Assert.AreEqual(new BigNumber(5), holder.BoughtAmount, "BoughtAmount must not decrease after removal");

            // second paid purchase accumulates
            bool success2 = BusinessesManager.Instance.iBuyable.TryBuyItems(TestUtilities.TestBusiness, 7);
            Assert.IsTrue(success2, "Second TryBuyItems should succeed");
            Assert.AreEqual(new BigNumber(12), holder.BoughtAmount, "BoughtAmount should be 12 after second purchase");
        }

        [Test]
        public void UseGeometricBoughtAmount_CostScalesOnBoughtNotTotal()
        {
            // Create a fresh business that uses geometric bought amount
            Business business = ScriptableObject.CreateInstance<Business>();
            business.cost = new System.Collections.Generic.List<Input>
            {
                new Input { currencyInput = TestUtilities.TestCurrency, inputAmount = 1 }
            };
            business.costMultiplier = 2f; // doubles each purchase
            business.maxBuyableAmount = -1;
            business.useGeometricBoughtAmount = true;

            // Grant 9 for free — TotalAmount becomes 9, BoughtAmount stays 0
            BusinessesManager.Instance.ForceBuyItemsForFree(business, 9);
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);

            Assert.AreEqual(new BigNumber(0), holder.BoughtAmount, "BoughtAmount should be 0 after free grants");
            Assert.AreEqual(new BigNumber(9), holder.TotalAmount, "TotalAmount should be 9 after free grants");

            // Formula: GetCurrentPrice(q, currentAmount) = NthTerm(1, q, currentAmount+1) = q^currentAmount
            // With UseGeometricBoughtAmount=true, cost is based on BoughtAmount (0): q^0 = 1
            // With UseGeometricBoughtAmount=false, cost is based on TotalAmount (9): q^9 = 512
            BigNumber costWithBoughtAmount = holder.iBuyable.GetRealCostToInputs(1)[0].inputAmount;
            Assert.AreEqual(new BigNumber(1), costWithBoughtAmount,
                $"Cost should be 1 (based on BoughtAmount=0, q^0=1), not 512 (based on TotalAmount=9, q^9=512). Actual: {costWithBoughtAmount}");

            // Fund exactly 1 for the bought-amount-based price and verify we can buy
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 1);
            bool success = BusinessesManager.Instance.iBuyable.TryBuyItems(business, 1);
            Assert.IsTrue(success, "Should be able to buy with cost based on BoughtAmount=0");
            Assert.AreEqual(new BigNumber(1), holder.BoughtAmount, "BoughtAmount should be 1 after first purchase");

            // Next price is now based on BoughtAmount=1: q^1 = 2
            BigNumber costAfterOnePurchase = holder.iBuyable.GetRealCostToInputs(1)[0].inputAmount;
            Assert.AreEqual(new BigNumber(2), costAfterOnePurchase,
                $"Cost should be 2 (based on BoughtAmount=1, q^1=2). Actual: {costAfterOnePurchase}");

            Object.DestroyImmediate(business);
        }

        [Test]
        public void Business_IgnoredMultipliers_IgnoresSelectedMultipliers()
        {
            Business business = ScriptableObject.CreateInstance<Business>();
            business.businessGroup = "TestGroup";
            business.costMultiplier = 1;
            
            // Create a production boost
            GenericMultiplierBoost mockBoost = ScriptableObject.CreateInstance<GenericMultiplierBoost>();
            mockBoost.group = "TestGroup";
            mockBoost.upgradeBlocks = new UpgradeBlock[] { new UpgradeBlock(UpgradeType.production, 5) };
            mockBoost.duration = new Duration(0, 0, 60, 0);
            
            // Apply the boost
            BoostsManager.Instance.ForceBuyItemsForFree(mockBoost, 1);
            BoostsManager.Instance.TryUseBoost(mockBoost); 
            
            // Apply prestige multiplier
            PrestigeManager.Instance.prestiges.currentPrestigeUpgradesMultiplier = 10; 
            
            // Check without ignore
            business.ignoredMultipliers = MultiplierSource.None;
            BigNumber standardMultiplier = business.GetBusinessMultiplierOfType(UpgradeType.production);
            Assert.AreEqual(new BigNumber(50), standardMultiplier, "Multiplier should be 50 (5 from boost * 10 from prestige).");

            // Check ignore prestige
            business.ignoredMultipliers = MultiplierSource.Prestige;
            BigNumber noPrestigeMultiplier = business.GetBusinessMultiplierOfType(UpgradeType.production);
            Assert.AreEqual(new BigNumber(5), noPrestigeMultiplier, "Multiplier should be 5 when Prestige is ignored.");

            // Check ignore boosts
            business.ignoredMultipliers = MultiplierSource.Boosts;
            BigNumber noBoostsMultiplier = business.GetBusinessMultiplierOfType(UpgradeType.production);
            Assert.AreEqual(new BigNumber(10), noBoostsMultiplier, "Multiplier should be 10 when Boosts are ignored.");

            // Check ignore both
            business.ignoredMultipliers = MultiplierSource.Prestige | MultiplierSource.Boosts;
            BigNumber ignoredBothMultiplier = business.GetBusinessMultiplierOfType(UpgradeType.production);
            Assert.AreEqual(new BigNumber(1), ignoredBothMultiplier, "Multiplier should be 1 when both Prestige and Boosts are ignored.");

            Object.DestroyImmediate(business);
            Object.DestroyImmediate(mockBoost);
        }

        [Test]
        public void Business_BoostApplication_WithTargetBusinessGroups()
        {
            var businessGroup = ScriptableObject.CreateInstance<BusinessGroup>();
            businessGroup.name = "TestBusinessGroupSO";

            Business business = ScriptableObject.CreateInstance<Business>();
            business.businessGroups = new List<BusinessGroup> { businessGroup };
            business.costMultiplier = 1;
            
            // Create a production boost with target group
            GenericMultiplierBoost mockBoost = ScriptableObject.CreateInstance<GenericMultiplierBoost>();
            mockBoost.targetBusinessGroups = new List<BusinessGroup> { businessGroup };
            mockBoost.upgradeBlocks = new UpgradeBlock[] { new UpgradeBlock(UpgradeType.production, 5) };
            mockBoost.duration = new Duration(0, 0, 60, 0);
            
            // Apply the boost
            BoostsManager.Instance.ForceBuyItemsForFree(mockBoost, 1);
            BoostsManager.Instance.TryUseBoost(mockBoost); 
            
            business.ignoredMultipliers = MultiplierSource.None;
            BigNumber multiplier = business.GetBusinessMultiplierOfType(UpgradeType.production);
            Assert.AreEqual(new BigNumber(5), multiplier, "Multiplier should be 5 because the boost applies to the specific BusinessGroup.");

            // Verify a business without the group does not get the boost
            Business otherBusiness = ScriptableObject.CreateInstance<Business>();
            otherBusiness.costMultiplier = 1;
            
            BigNumber otherMultiplier = otherBusiness.GetBusinessMultiplierOfType(UpgradeType.production);
            Assert.AreEqual(new BigNumber(1), otherMultiplier, "Multiplier should be 1 for a business not in the target group.");

            Object.DestroyImmediate(businessGroup);
            Object.DestroyImmediate(business);
            Object.DestroyImmediate(otherBusiness);
            Object.DestroyImmediate(mockBoost);
        }

        [Test]
        public void GetProductionProgress_ReturnsCorrectNormalizedProgress()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBusiness, 1);
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(TestUtilities.TestBusiness);

            Assert.AreEqual(0f, holder.GetProductionProgress(), "Progress should be 0 when idle");

            holder.StartProduction(1, out _);
            Assert.AreEqual(0f, holder.GetProductionProgress(), 0.001f, "Progress should be ~0 immediately after starting");

            // Manually advance the first round to half-way
            holder.activeProductions[0].productionTime = holder.business.timeToProduce * 0.5f;
            Assert.AreEqual(0.5f, holder.GetProductionProgress(), 0.001f, "Progress should be ~0.5 at the halfway point");

            // Finish the round
            holder.activeProductions[0].productionTime = holder.business.timeToProduce;
            Assert.AreEqual(1f, holder.GetProductionProgress(), 0.001f, "Progress should be 1 when production time equals timeToProduce");
        }

        [Test]
        public void MaxAmountConstrain_UpgradeOverride_TakesHighestValue()
        {
            Business business = ScriptableObject.CreateInstance<Business>();
            business.maxAmount_ = 10;
            business.maxBuyableAmount = new BigNumber(5);
            business.cost = new System.Collections.Generic.List<Input>
            {
                new Input { currencyInput = TestUtilities.TestCurrency, inputAmount = 1 }
            };

            Upgrade upgrade = ScriptableObject.CreateInstance<Upgrade>();
            upgrade.overrideUpgrades = new System.Collections.Generic.List<OverrideUpgradeBlock>
            {
                new OverrideUpgradeBlock(OverrideUpgradeType.maxBusinessAmount, 20) { targetBusiness = business }
            };
            upgrade.overrideUpgradeLevelMultiplier = 1f;

            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);

            // Without upgrade: base values
            Assert.AreEqual(new BigNumber(10), holder.GetMaxAmountConstrain(),
                "GetMaxAmountConstrain should be 10 (base maxAmount_) without any upgrade");
            Assert.AreEqual(new BigNumber(5), holder.GetMaxBuyableAmountConstrain(),
                "GetMaxBuyableAmountConstrain should be 5 (base maxBuyableAmount) without any upgrade");

            // Apply upgrade — override value (20) is higher than base (10 / 5) → takes the upgrade
            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, 1);

            Assert.AreEqual(new BigNumber(20), holder.GetMaxAmountConstrain(),
                "GetMaxAmountConstrain should be 20 (upgrade override) because 20 > 10");
            Assert.AreEqual(new BigNumber(20), holder.GetMaxBuyableAmountConstrain(),
                "GetMaxBuyableAmountConstrain should be 20 (upgrade override) because 20 > 5");

            Object.DestroyImmediate(business);
            Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void MaxAmountConstrain_UpgradeOverride_DoesNotApplyToOtherBusiness()
        {
            Business targetBusiness = ScriptableObject.CreateInstance<Business>();
            targetBusiness.maxAmount_ = 5;
            targetBusiness.cost = new System.Collections.Generic.List<Input>();

            Business otherBusiness = ScriptableObject.CreateInstance<Business>();
            otherBusiness.maxAmount_ = 3;
            otherBusiness.cost = new System.Collections.Generic.List<Input>();

            // Upgrade scoped only to targetBusiness
            Upgrade upgrade = ScriptableObject.CreateInstance<Upgrade>();
            upgrade.overrideUpgrades = new System.Collections.Generic.List<OverrideUpgradeBlock>
            {
                new OverrideUpgradeBlock(OverrideUpgradeType.maxBusinessAmount, 50) { targetBusiness = targetBusiness }
            };
            upgrade.overrideUpgradeLevelMultiplier = 1f;

            UpgradesManager.Instance.ForceBuyItemsForFree(upgrade, 1);

            BusinessHolder targetHolder = BusinessesManager.Instance.GetOrAddHolder(targetBusiness);
            BusinessHolder otherHolder = BusinessesManager.Instance.GetOrAddHolder(otherBusiness);

            Assert.AreEqual(new BigNumber(50), targetHolder.GetMaxAmountConstrain(),
                "Upgrade override should apply to the target business");
            Assert.AreEqual(new BigNumber(3), otherHolder.GetMaxAmountConstrain(),
                "Upgrade override must not apply to a different business");

            Object.DestroyImmediate(targetBusiness);
            Object.DestroyImmediate(otherBusiness);
            Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void SharedCapacity_EnforcesGroupLimit()
        {
            BusinessGroup group = ScriptableObject.CreateInstance<BusinessGroup>();
            group.capacity = 100;

            Business b1 = ScriptableObject.CreateInstance<Business>();
            b1.businessGroups = new List<BusinessGroup> { group };

            Business b2 = ScriptableObject.CreateInstance<Business>();
            b2.businessGroups = new List<BusinessGroup> { group };

            BusinessesManager.Instance.ForceBuyItemsForFree(b1, 60);
            Assert.AreEqual(new BigNumber(60), BusinessesManager.Instance.iBuyable.GetItemAmount(b1), "b1 should have 60");

            BusinessesManager.Instance.ForceBuyItemsForFree(b2, 60);
            Assert.AreEqual(new BigNumber(40), BusinessesManager.Instance.iBuyable.GetItemAmount(b2), "b2 should be capped to remaining 40 capacity");

            Object.DestroyImmediate(group);
            Object.DestroyImmediate(b1);
            Object.DestroyImmediate(b2);
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator HandleUpdate_ProductionLockFails_ProductionPausesAndResumesWhenUnlocked()
        {
            var business = ScriptableObject.CreateInstance<Business>();
            business.timeToProduce = 0.5f;
            business.autoProduce = false;
            business.autoCollect = false;
            
            var requiredBusiness = ScriptableObject.CreateInstance<Business>();

            var lockObj = new Lock() { businessLock = requiredBusiness, inputAmount = 1, amountType = LockAmountType.CurrentAmount };
            business.productionLocks = new List<Lock>() { lockObj };

            var holder = BusinessesManager.Instance.GetOrAddHolder(business);
            holder.Amount = 10;
            
            var requiredHolder = BusinessesManager.Instance.GetOrAddHolder(requiredBusiness);
            requiredHolder.Amount = 0;

            // Try start production manually (should fail)
            Assert.IsFalse(holder.TryStartProduction(), "Should not start production while locked.");

            // Unlock
            requiredHolder.Amount = 1;
            Assert.IsTrue(holder.TryStartProduction(), "Should start production when unlocked.");
            
            // Wait half way
            yield return new UnityEngine.WaitForSeconds(0.2f);
            
            // Lock again
            requiredHolder.Amount = 0;
            float progressBefore = holder.GetProductionProgress();
            
            yield return new UnityEngine.WaitForSeconds(0.2f);
            float progressAfter = holder.GetProductionProgress();
            
            Assert.AreEqual(progressBefore, progressAfter, 0.01f, "Production should pause while locked.");
            
            // Unlock again
            requiredHolder.Amount = 1;
            yield return new UnityEngine.WaitForSeconds(0.4f); // enough to finish
            
            Assert.AreEqual(0, holder.ActiveProductions.Count, "Production should finish after resuming.");
            
            // Now test if autoProduce properly starts production once unlocked
            business.autoProduce = true;
            requiredHolder.Amount = 0; // Lock it again
            yield return new UnityEngine.WaitForSeconds(0.1f);
            Assert.AreEqual(0, holder.ActiveProductions.Count, "Auto production should not start while locked.");
            
            requiredHolder.Amount = 1; // Unlock it
            yield return null; // Wait one frame for HandleUpdate to catch it
            Assert.AreEqual(1, holder.ActiveProductions.Count, "Auto production should start automatically when unlocked.");
            
            Object.DestroyImmediate(business);
            Object.DestroyImmediate(requiredBusiness);
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator HandleUpdate_UpperProductionLocks_PausesWhenOverThreshold()
        {
            var business = ScriptableObject.CreateInstance<Business>();
            business.timeToProduce = 0.5f;
            business.autoProduce = false;
            business.autoCollect = false;
            
            var thresholdBusiness = ScriptableObject.CreateInstance<Business>();

            var lockObj = new Lock() { businessLock = thresholdBusiness, inputAmount = 5, amountType = LockAmountType.CurrentAmount };
            business.upperProductionLocks = new List<Lock>() { lockObj };

            var holder = BusinessesManager.Instance.GetOrAddHolder(business);
            holder.Amount = 10;
            
            var thresholdHolder = BusinessesManager.Instance.GetOrAddHolder(thresholdBusiness);
            thresholdHolder.Amount = 2; // Under threshold, so it's unlocked

            Assert.IsTrue(holder.TryStartProduction(), "Should start production when under upper threshold.");
            
            yield return new UnityEngine.WaitForSeconds(0.2f);
            
            thresholdHolder.Amount = 10; // Over threshold, lock it
            float progressBefore = holder.GetProductionProgress();
            
            yield return new UnityEngine.WaitForSeconds(0.2f);
            float progressAfter = holder.GetProductionProgress();
            
            Assert.AreEqual(progressBefore, progressAfter, 0.01f, "Production should pause while over upper threshold.");
            
            thresholdHolder.Amount = 2; // Under threshold again
            yield return new UnityEngine.WaitForSeconds(0.4f); // enough to finish
            
            Assert.AreEqual(0, holder.ActiveProductions.Count, "Production should finish after falling back under threshold.");
            
            Object.DestroyImmediate(business);
            Object.DestroyImmediate(thresholdBusiness);
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator ProductionRound_PauseAndUnpause_AffectsProductionProgress()
        {
            var business = ScriptableObject.CreateInstance<Business>();
            business.timeToProduce = 0.5f;
            business.autoProduce = false;
            business.autoCollect = false;

            var holder = BusinessesManager.Instance.GetOrAddHolder(business);
            holder.Amount = 10;

            Assert.IsTrue(holder.TryStartProduction(), "Should start production.");
            
            yield return new UnityEngine.WaitForSeconds(0.1f);
            
            holder.PauseAllProductionRounds();
            
            float progressBefore = holder.GetProductionProgress();
            
            yield return new UnityEngine.WaitForSeconds(0.2f);
            float progressAfter = holder.GetProductionProgress();
            
            Assert.AreEqual(progressBefore, progressAfter, 0.01f, "Production should pause while explicitly paused.");
            
            holder.UnpauseAllProductionRounds();
            yield return new UnityEngine.WaitForSeconds(0.5f); // enough to finish
            
            Assert.AreEqual(0, holder.ActiveProductions.Count, "Production should finish after unpausing.");
            
            Object.DestroyImmediate(business);
        }

        [Test]
        public void BusinessWrapper_EffectiveMetadataModelAndGroups_FallBackToUnderlyingBusiness()
        {
            BusinessGroup group = ScriptableObject.CreateInstance<BusinessGroup>();
            Business source = ScriptableObject.CreateInstance<Business>();
            BusinessWrapper wrapper = ScriptableObject.CreateInstance<BusinessWrapper>();
            GameObject model = new GameObject("EffectiveModel");

            source.metadata = new Metadata
            {
                name = "Source Business",
                iconString = "SB",
                model = model,
                description = "Source description"
            };
            source.businessGroups = new List<BusinessGroup> { group };
            wrapper.underlyingBusiness = source;

            Metadata metadata = wrapper.GetEffectiveMetadata(null);

            Assert.AreEqual("Source Business", metadata.name, "Wrapper should fall back to underlying metadata name.");
            Assert.AreEqual("SB", metadata.iconString, "Wrapper should fall back to underlying icon string.");
            Assert.AreSame(model, wrapper.GetEffectiveModel(null), "Wrapper should fall back to underlying model.");
            CollectionAssert.Contains(wrapper.GetEffectiveBusinessGroups(), group, "Wrapper should fall back to underlying business groups.");

            Object.DestroyImmediate(model);
            Object.DestroyImmediate(group);
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(wrapper);
        }

        [Test]
        public void SharedCapacity_UsesWrapperEffectiveBusinessGroups()
        {
            BusinessGroup group = ScriptableObject.CreateInstance<BusinessGroup>();
            group.capacity = 5;

            Business source = ScriptableObject.CreateInstance<Business>();
            source.businessGroups = new List<BusinessGroup> { group };
            source.maxAmount_ = -1;

            BusinessWrapper wrapper = ScriptableObject.CreateInstance<BusinessWrapper>();
            wrapper.underlyingBusiness = source;
            wrapper.maxAmount_ = -1;

            BusinessesManager.Instance.ForceBuyItemsForFree(source, 3);
            BusinessesManager.Instance.ForceBuyItemsForFree(wrapper, 5);

            Assert.AreEqual(new BigNumber(3), BusinessesManager.Instance.GetOrAddHolder(source).amount, "Source should keep its initial amount.");
            Assert.AreEqual(new BigNumber(2), BusinessesManager.Instance.GetOrAddHolder(wrapper).amount, "Wrapper should be capped by the inherited group capacity.");
            Assert.AreEqual(new BigNumber(5), BusinessesManager.Instance.GetBusinessGroupUsedCapacity(group), "Group capacity should include source and wrapper amounts.");

            Object.DestroyImmediate(group);
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(wrapper);
        }

        [Test]
        public void BusinessUpgrade_OnBusinessUpgradeChanged_FiresFromUpgradableHolder()
        {
            Business business = ScriptableObject.CreateInstance<Business>();
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            int businessEventCount = 0;
            int holderEventCount = 0;
            Business upgradedBusiness = null;
            BusinessHolder upgradedHolder = null;

            BusinessesManager.Instance.OnBusinessUpgradeChanged.AddListener(b =>
            {
                businessEventCount++;
                upgradedBusiness = b;
            });
            BusinessesManager.Instance.OnHolderUpgradedEvent.AddListener(h =>
            {
                holderEventCount++;
                upgradedHolder = h;
            });

            holder.iUpgradable.ForceUpgradeToLevel(1);

            Assert.AreEqual(1, businessEventCount, "Business upgrade event should fire once.");
            Assert.AreEqual(1, holderEventCount, "Holder upgrade event should fire once.");
            Assert.AreSame(business, upgradedBusiness, "Business event should pass the upgraded business.");
            Assert.AreSame(holder, upgradedHolder, "Holder event should pass the upgraded holder.");

            Object.DestroyImmediate(business);
        }
    }
}
