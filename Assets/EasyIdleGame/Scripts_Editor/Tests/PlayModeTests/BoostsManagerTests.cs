using NUnit.Framework;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class BoostsManagerTests
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
        public void UseBoost()
        {
            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 1);
            Assert.IsTrue(BoostsManager.Instance.activeBoosts.Count == 0, "Active boosts count should be 0 before usage");
            Assert.IsTrue(BoostsManager.Instance.GetBoostAmountInInventory(TestUtilities.TestBoost1) == 1, "Boost amount in inventory should be 1 before usage");

            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);
            Assert.IsTrue(BoostsManager.Instance.activeBoosts.Count == 1, "Active boosts count should be 1 after usage");
            Assert.IsTrue(BoostsManager.Instance.GetBoostAmountInInventory(TestUtilities.TestBoost1) == 0, "Boost amount in inventory should be 0 after usage");
        }

        [Test]
        public void BoostMerging()
        {
            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);

            Assert.IsTrue(BoostsManager.Instance.activeBoosts[0].timeLeft == TestUtilities.TestBoost1.duration.TotalSeconds, "Time left should equal boost duration after first use");

            BoostsManager.Instance.ForceBuyItemsForFree(TestUtilities.TestBoost1, 1);
            BoostsManager.Instance.TryUseBoost(TestUtilities.TestBoost1);

            Assert.IsTrue(BoostsManager.Instance.activeBoosts[0].timeLeft == TestUtilities.TestBoost1.duration.TotalSeconds * 2,
                $"Incorrect duration after merging boosts - Expected: {TestUtilities.TestBoost1.duration.TotalSeconds * 2}, Actual: {BoostsManager.Instance.activeBoosts[0].timeLeft}");
        }

        [Test]
        public void SetActiveBoost_AddsAndMergesBoostCorrectly_TriggersEvent()
        {
            bool eventTriggered = false;
            BoostsManager.Instance.OnActiveBoostsChanged.AddListener(() => eventTriggered = true);

            // Add new active boost programmatically
            BoostsManager.Instance.SetActiveBoost(TestUtilities.TestBoost1, 10f);

            Assert.IsTrue(eventTriggered, "OnActiveBoostsChanged should be triggered when a new boost is set");
            Assert.AreEqual(1, BoostsManager.Instance.activeBoosts.Count, "Should have 1 active boost");
            Assert.AreEqual(10f, BoostsManager.Instance.activeBoosts[0].timeLeft, "Time left should be exactly 10s");

            eventTriggered = false;

            // Merge boost
            BoostsManager.Instance.SetActiveBoost(TestUtilities.TestBoost1, 15f);

            Assert.IsTrue(eventTriggered, "OnActiveBoostsChanged should be triggered when boost is merged");
            Assert.AreEqual(1, BoostsManager.Instance.activeBoosts.Count, "Should still have 1 active boost due to merging");
            Assert.AreEqual(25f, BoostsManager.Instance.activeBoosts[0].timeLeft, "Time left should be merged to 25s");
        }

        [Test]
        public void RemoveActiveBoost_RemovesBoost_TriggersEvent()
        {
            BoostsManager.Instance.SetActiveBoost(TestUtilities.TestBoost1, 10f);
            Assert.AreEqual(1, BoostsManager.Instance.activeBoosts.Count, "Setup: Should have 1 active boost");

            bool eventTriggered = false;
            BoostsManager.Instance.OnActiveBoostsChanged.AddListener(() => eventTriggered = true);

            bool removed = BoostsManager.Instance.RemoveActiveBoost(TestUtilities.TestBoost1);

            Assert.IsTrue(removed, "RemoveActiveBoost should return true if boost was removed");
            Assert.IsTrue(eventTriggered, "OnActiveBoostsChanged should be triggered when boost is removed");
            Assert.AreEqual(0, BoostsManager.Instance.activeBoosts.Count, "Should have 0 active boosts after removal");

            eventTriggered = false;
            bool removedAgain = BoostsManager.Instance.RemoveActiveBoost(TestUtilities.TestBoost1);
            Assert.IsFalse(removedAgain, "RemoveActiveBoost should return false if boost was not present");
            Assert.IsFalse(eventTriggered, "OnActiveBoostsChanged should not be triggered if nothing was removed");
        }
    }
}
