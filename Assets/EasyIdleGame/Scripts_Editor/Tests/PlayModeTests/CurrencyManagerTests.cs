using NUnit.Framework;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class CurrencyManagerTests
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
        public void AddCurrency()
        {
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 1000);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency) == 1000,
                $"Incorrect currency amount after adding 1000. Expected: 1000, Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency).ToStringDebug()}");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 12_345_678);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency).Floor() == 12_346_678,
                $"Incorrect currency amount after adding 12_345_678. Expected: 12_346_678, Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency).ToStringDebug()}");
        }

        [Test]
        public void RemoveCurrency()
        {
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 1000);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency) == 1000,
                $"Failed setup amount (1000). Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency).ToStringDebug()}");

            CurrencyManager.Instance.RemoveCurrencyAmount(TestUtilities.TestCurrency, 1000);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency) == 0,
                $"Incorrect currency amount after removing 1000. Expected: 0, Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency).ToStringDebug()}");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 12_345_678);
            CurrencyManager.Instance.RemoveCurrencyAmount(TestUtilities.TestCurrency, 12_345_678);
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency) == 0,
                $"Incorrect currency amount after removing 12_345_678. Expected: 0, Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency).ToStringDebug()}");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, new BigNumber(2, 42));
            CurrencyManager.Instance.RemoveCurrencyAmount(TestUtilities.TestCurrency, new BigNumber(1.998, 42));
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency) == new BigNumber(2, 39),
                $"Incorrect currency amount after removing large numbers. Expected: 2e39, Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency).ToStringDebug()}");

            CurrencyManager.Instance.RemoveCurrencyAmount(TestUtilities.TestCurrency, new BigNumber(2, 39));
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency) == 0,
                $"Incorrect currency amount after removing exact remainder. Expected: 0, Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency).ToStringDebug()}");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, new BigNumber(9.6692796, 41));
            CurrencyManager.Instance.RemoveCurrencyAmount(TestUtilities.TestCurrency, new BigNumber(6.9689829, 41));
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency) == new BigNumber(2.7002967, 41),
                $"Incorrect currency amount after removing precise big numbers. Expected: 2.7002967e41, Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency).ToStringDebug()}");
        }

        [Test]
        public void TotalCurrencyAmount_TracksAllAdditionsIndependentlyFromCurrentAmount()
        {
            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 50);
            CurrencyManager.Instance.RemoveCurrencyAmount(TestUtilities.TestCurrency, 30);

            Assert.AreEqual(new BigNumber(20), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Current amount should decrease after removal.");
            Assert.AreEqual(new BigNumber(50), CurrencyManager.Instance.GetTotalCurrencyAmount(TestUtilities.TestCurrency), "Total amount should keep lifetime additions.");
        }

        [Test]
        public void SharedCapacity_EnforcesGroupLimit()
        {
            CurrencyGroup group = ScriptableObject.CreateInstance<CurrencyGroup>();
            group.capacity = 100;

            Currency c1 = ScriptableObject.CreateInstance<Currency>();
            c1.currencyGroups = new System.Collections.Generic.List<CurrencyGroup> { group };

            Currency c2 = ScriptableObject.CreateInstance<Currency>();
            c2.currencyGroups = new System.Collections.Generic.List<CurrencyGroup> { group };

            CurrencyManager.Instance.AddCurrencyAmount(c1, 60);
            Assert.AreEqual(new BigNumber(60), CurrencyManager.Instance.GetCurrencyAmount(c1), "c1 should have 60");

            CurrencyManager.Instance.AddCurrencyAmount(c2, 60);
            Assert.AreEqual(new BigNumber(40), CurrencyManager.Instance.GetCurrencyAmount(c2), "c2 should be capped to remaining 40 capacity");

            Object.DestroyImmediate(group);
            Object.DestroyImmediate(c1);
            Object.DestroyImmediate(c2);
        }
    }
}
