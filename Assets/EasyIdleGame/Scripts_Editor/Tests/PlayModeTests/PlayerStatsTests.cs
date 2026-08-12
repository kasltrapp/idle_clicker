using NUnit.Framework;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class PlayerStatsTests
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
        public void AddXp_IncreasesLevel()
        {
            Assert.AreEqual(0, PlayerStats.Instance.level.currentLevel, "Initial player level should be 0");

            PlayerStats.Instance.AddXp(200);

            Assert.AreEqual(1, PlayerStats.Instance.level.currentLevel, "Player level should be 1 after adding 200 XP");
        }

        [Test]
        public void TrackTotalMoneyMade_AddsCorrectly()
        {
            Assert.AreEqual(new BigNumber(0), PlayerStats.Instance.GetTotalMoneyMade(TestUtilities.TestCurrency), "Initial should be 0.");

            PlayerStats.Instance.OnCurrencyAmountAdded(TestUtilities.TestCurrency, 500);
            Assert.AreEqual(new BigNumber(500), PlayerStats.Instance.GetTotalMoneyMade(TestUtilities.TestCurrency), "Should track added amount.");

            PlayerStats.Instance.OnCurrencyAmountAdded(TestUtilities.TestCurrency, 1500);
            Assert.AreEqual(new BigNumber(2000), PlayerStats.Instance.GetTotalMoneyMade(TestUtilities.TestCurrency), "Should accumulate added amounts.");
        }

        [Test]
        public void TrackTotalMoneyMade_DoesNotMutateCurrencyHolderTotal()
        {
            PlayerStats.Instance.OnCurrencyAmountAdded(TestUtilities.TestCurrency, 500);

            Assert.AreEqual(new BigNumber(500), PlayerStats.Instance.GetTotalMoneyMade(TestUtilities.TestCurrency), "Player stats total should use its own legacy storage.");
            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetTotalCurrencyAmount(TestUtilities.TestCurrency), "Currency holder total should stay independent from direct player stats updates.");
        }

        [Test]
        public void TrackTotalBusinessesObtained_AddsCorrectly()
        {
            Assert.AreEqual(new BigNumber(0), PlayerStats.Instance.GetTotalBusinessesObtained(TestUtilities.TestBusiness), "Initial should be 0.");

            PlayerStats.Instance.OnBusinessAmountAdded(TestUtilities.TestBusiness, 5);
            Assert.AreEqual(new BigNumber(5), PlayerStats.Instance.GetTotalBusinessesObtained(TestUtilities.TestBusiness), "Should track added amount.");

            PlayerStats.Instance.OnBusinessAmountAdded(TestUtilities.TestBusiness, 15);
            Assert.AreEqual(new BigNumber(20), PlayerStats.Instance.GetTotalBusinessesObtained(TestUtilities.TestBusiness), "Should accumulate added amounts.");
        }

        [Test]
        public void TrackTotalBoostsObtained_AddsCorrectly()
        {
            Assert.AreEqual(new BigNumber(0), PlayerStats.Instance.GetTotalBoostsObtained(TestUtilities.TestBoost1), "Initial should be 0.");

            PlayerStats.Instance.OnBoostAmountAdded(TestUtilities.TestBoost1, 2);
            Assert.AreEqual(new BigNumber(2), PlayerStats.Instance.GetTotalBoostsObtained(TestUtilities.TestBoost1), "Should track added amount.");
        }

        [Test]
        public void Achievement_IsUnlocked_ValidatesTotalTracking()
        {
            Achievement achievement = ScriptableObject.CreateInstance<Achievement>();
            achievement.locks = new System.Collections.Generic.List<Lock>
            {
                new Lock { currencyLock = TestUtilities.TestCurrency, inputAmount = 1000, amountType = LockAmountType.TotalAmount } // Total currency requirement
            };

            Assert.IsFalse(achievement.IsCurrentlyUnlocked(), "Achievement should be locked initially.");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 1000);

            Assert.IsTrue(achievement.IsCurrentlyUnlocked(), "Achievement should be unlocked when total money meets requirement.");

            Object.DestroyImmediate(achievement);
        }
    }
}
