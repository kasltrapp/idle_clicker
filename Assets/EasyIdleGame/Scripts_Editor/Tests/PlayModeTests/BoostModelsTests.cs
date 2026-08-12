using NUnit.Framework;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class BoostModelsTests
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
        public void AutoProduceBoost_ActivateAndMerge_BehavesCorrectly()
        {
            var boost1 = ScriptableObject.CreateInstance<AutoProduceBoost>();
            boost1.group = "testGroup";
            boost1.duration = new Duration(0, 0, 1, 0); // 60 seconds

            var active1 = boost1.Activate();
            Assert.AreEqual(60f, active1.timeLeft, "Activation should set correct time left.");

            var boost2 = ScriptableObject.CreateInstance<AutoProduceBoost>();
            boost2.group = "testGroup";
            boost2.duration = new Duration(0, 0, 2, 0); // 120 seconds

            var active2 = boost2.Activate();

            Assert.IsTrue(boost1.CanMergeBoost(active2), "Should be mergable with same group.");

            boost1.MergeBoost(active1, active2);
            Assert.AreEqual(180f, active1.timeLeft, "Merging should sum the timeLeft.");

            Object.DestroyImmediate(boost1);
            Object.DestroyImmediate(boost2);
        }

        [Test]
        public void GenericMultiplierBoost_ActivateAndMerge_BehavesCorrectly()
        {
            var block = new UpgradeBlock(UpgradeType.production, 2);
            var boost1 = ScriptableObject.CreateInstance<GenericMultiplierBoost>();
            boost1.group = "testGroup";
            boost1.upgradeBlocks = new[] { block };
            boost1.duration = new Duration(0, 0, 1, 0); // 60 seconds

            var active1 = boost1.Activate();

            var boost2 = ScriptableObject.CreateInstance<GenericMultiplierBoost>();
            boost2.group = "testGroup";
            boost2.upgradeBlocks = new[] { block };
            boost2.duration = new Duration(0, 0, 0, 30); // 30 seconds

            var active2 = boost2.Activate();

            Assert.IsTrue(boost1.CanMergeBoost(active2), "Should be mergable with same blocks and group.");

            boost1.MergeBoost(active1, active2);
            Assert.AreEqual(90f, active1.timeLeft, "Merging should sum the timeLeft.");

            var boostDiff = ScriptableObject.CreateInstance<GenericMultiplierBoost>();
            boostDiff.group = "testGroup";
            boostDiff.upgradeBlocks = new[] { new UpgradeBlock(UpgradeType.speed, 2) };
            boostDiff.duration = new Duration(0, 0, 0, 30); // 30 seconds

            var activeDiff = boostDiff.Activate();
            Assert.IsFalse(boost1.CanMergeBoost(activeDiff), "Should not merge with different upgrade blocks.");

            Object.DestroyImmediate(boost1);
            Object.DestroyImmediate(boost2);
            Object.DestroyImmediate(boostDiff);
        }

        [Test]
        public void AutoProduceBoost_CanMerge_WithTargetBusinessGroups()
        {
            var group1 = ScriptableObject.CreateInstance<BusinessGroup>(); group1.name = "group1";
            var group2 = ScriptableObject.CreateInstance<BusinessGroup>(); group2.name = "group2";

            var boost1 = ScriptableObject.CreateInstance<AutoProduceBoost>();
            boost1.targetBusinessGroups = new System.Collections.Generic.List<BusinessGroup> { group1, group2 };
            boost1.duration = new Duration(0, 0, 1, 0);

            var boost2 = ScriptableObject.CreateInstance<AutoProduceBoost>();
            boost2.targetBusinessGroups = new System.Collections.Generic.List<BusinessGroup> { group1, group2 };
            boost2.duration = new Duration(0, 0, 2, 0);

            var boostDiff = ScriptableObject.CreateInstance<AutoProduceBoost>();
            boostDiff.targetBusinessGroups = new System.Collections.Generic.List<BusinessGroup> { group1 };
            boostDiff.duration = new Duration(0, 0, 2, 0);

            var active1 = boost1.Activate();
            var active2 = boost2.Activate();
            var activeDiff = boostDiff.Activate();

            Assert.IsTrue(boost1.CanMergeBoost(active2), "Should be mergable with exact same targetBusinessGroups.");
            Assert.IsFalse(boost1.CanMergeBoost(activeDiff), "Should not merge with different targetBusinessGroups.");

            Object.DestroyImmediate(group1);
            Object.DestroyImmediate(group2);
            Object.DestroyImmediate(boost1);
            Object.DestroyImmediate(boost2);
            Object.DestroyImmediate(boostDiff);
        }
    }
}
