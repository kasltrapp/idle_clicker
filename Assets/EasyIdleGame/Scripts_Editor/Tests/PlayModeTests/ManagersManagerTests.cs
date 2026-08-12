using NUnit.Framework;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class ManagersManagerTests
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
        public void PassiveAndActiveMultiplier()
        {
            Assert.IsTrue(ManagersManager.GetPassiveMultiplierOfType(UpgradeType.speed, TestUtilities.TestBusinessWithManager) == 1,
                "Initial passive speed multiplier should be 1");

            ManagersManager.Instance.AddManagers(TestUtilities.TestManager, 1);
            Assert.IsTrue(ManagersManager.Instance.GetHolder(TestUtilities.TestManager).level == 1,
                "Manager should be at level 1 after adding it");
            Assert.IsTrue(ManagersManager.GetPassiveMultiplierOfType(UpgradeType.speed, TestUtilities.TestBusinessWithManager) == 2,
                $"Manager should provide a passive multiplier of 2 after being added. Actual: {ManagersManager.GetPassiveMultiplierOfType(UpgradeType.speed, TestUtilities.TestBusinessWithManager)}");

            ManagersManager.Instance.TryActivateManager(TestUtilities.TestManager);
            Assert.IsTrue(ManagersManager.GetActiveMultiplierOfType(UpgradeType.production, TestUtilities.TestBusinessWithManager) == 2,
                "Manager should provide an active multiplier of 2 after being activated");
        }

        [Test]
        public void UpgradingManager()
        {
            ManagersManager.Instance.GetOrAddHolder(TestUtilities.TestManager);
            Assert.IsTrue(ManagersManager.Instance.GetHolder(TestUtilities.TestManager).Level == 0,
                "Manager level should be zero initially");
            Assert.IsTrue(ManagersManager.Instance.GetHolder(TestUtilities.TestManager).iUpgradable.GetCountToNextLevel() == 1,
                $"Count to next level should be 1. Actual: {ManagersManager.Instance.GetHolder(TestUtilities.TestManager).iUpgradable.GetCountToNextLevel().ToStringDebug()}");

            ManagersManager.Instance.ForceBuyItemsForFree(TestUtilities.TestManager, 9); // lvl 1: 1 copy, lvl 2: 2 copies, lvl 3: 4 copies
            Assert.IsTrue(ManagersManager.Instance.GetHolder(TestUtilities.TestManager).Level == 1,
                "Manager should automatically upgrade to first level for free");
            Assert.IsTrue(ManagersManager.Instance.GetHolder(TestUtilities.TestManager).iUpgradable.GetCountToNextLevel() == 2,
                $"Count to next level should be 2. Actual: {ManagersManager.Instance.GetHolder(TestUtilities.TestManager).iUpgradable.GetCountToNextLevel().ToStringDebug()}");

            Assert.IsFalse(ManagersManager.Instance.GetHolder(TestUtilities.TestManager).iUpgradable.TryUpgrade(),
                "Upgrade should not be possible due to insufficient currency count");

            CurrencyManager.Instance.AddCurrencyAmount(TestUtilities.TestCurrency, 15); // lvl 1: 1, lvl 2: 3, lvl 3: 9
            ManagersManager.Instance.GetHolder(TestUtilities.TestManager).iUpgradable.TryUpgrade();

            Assert.IsTrue(ManagersManager.Instance.GetHolder(TestUtilities.TestManager).Level == 2,
                $"Manager level should be 2. Actual: {ManagersManager.Instance.GetHolder(TestUtilities.TestManager).Level}");
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency) == 12,
                $"Currency amount should be 12. Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency)}");

            ManagersManager.Instance.GetHolder(TestUtilities.TestManager).iUpgradable.TryUpgrade();
            Assert.IsTrue(ManagersManager.Instance.GetHolder(TestUtilities.TestManager).Level == 3,
                $"Manager level should be 3. Actual: {ManagersManager.Instance.GetHolder(TestUtilities.TestManager).Level}");
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency) == 3,
                $"Currency amount should be 3. Actual: {CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency)}");
        }
    }
}