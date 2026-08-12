using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class LegacySerializationTests
    {
        private Currency _currency;
        private Texture2D _texture;
        private Sprite _sprite;
        private GameObject _model;

        [SetUp]
        public void SetUp()
        {
            _currency = ScriptableObject.CreateInstance<Currency>();
            _currency.metadata.name = "Migration Test Currency";

            _texture = new Texture2D(1, 1);
            _sprite = Sprite.Create(_texture, new Rect(0, 0, 1, 1), Vector2.zero);
            _model = new GameObject("Metadata Model");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_model);
            Object.DestroyImmediate(_sprite);
            Object.DestroyImmediate(_texture);
            Object.DestroyImmediate(_currency);
        }

        [Test]
        public void Business_OnAfterDeserialize_MigratesLegacyOutputsToDropTable()
        {
            Business business = ScriptableObject.CreateInstance<Business>();
            Output legacyOutput = CreateCurrencyOutput(7);

            business.outputTables = null;
            business.outputs = new List<Output> { legacyOutput };

            business.OnAfterDeserialize();

            AssertLegacyOutputsMigrated(business.outputTables, legacyOutput);
            Assert.IsNotNull(business.outputs, "Legacy outputs list should still exist for compatibility.");
            Assert.AreEqual(0, business.outputs.Count, "Legacy outputs should be cleared after migration to avoid duplicate rewards.");

            Object.DestroyImmediate(business);
        }

        [Test]
        public void Reward_OnAfterDeserialize_MigratesLegacyRewardsToDropTable()
        {
            Reward reward = ScriptableObject.CreateInstance<Reward>();
            Output legacyReward = CreateCurrencyOutput(11);

            reward.rewardTables = null;
            reward._legacyRewards = new List<Output> { legacyReward };

            reward.OnAfterDeserialize();

            AssertLegacyOutputsMigrated(reward.rewardTables, legacyReward);
            Assert.IsNotNull(reward._legacyRewards, "Legacy rewards list should still exist for compatibility.");
            Assert.AreEqual(0, reward._legacyRewards.Count, "Legacy rewards should be cleared after migration to avoid duplicate rewards.");

            Object.DestroyImmediate(reward);
        }

        [Test]
        public void Achievement_OnAfterDeserialize_MigratesLegacyRewardsToDropTable()
        {
            Achievement achievement = ScriptableObject.CreateInstance<Achievement>();
            Output legacyReward = CreateCurrencyOutput(13);

            achievement.rewardTables = null;
            achievement._legacyRewards = new List<Output> { legacyReward };

            achievement.OnAfterDeserialize();

            AssertLegacyOutputsMigrated(achievement.rewardTables, legacyReward);
            Assert.IsNotNull(achievement._legacyRewards, "Legacy rewards list should still exist for compatibility.");
            Assert.AreEqual(0, achievement._legacyRewards.Count, "Legacy rewards should be cleared after migration to avoid duplicate rewards.");

            Object.DestroyImmediate(achievement);
        }

        [Test]
        public void BusinessMetadata_MigratesLegacyFieldsAndKeepsLegacyReadable()
        {
            Business business = ScriptableObject.CreateInstance<Business>();
            business.businessName = "Legacy Business";
            business.image = _sprite;
            business.iconString = "LB";
            business.model = _model;
            business.description = "Legacy description";

            Metadata metadata = business.Metadata;

            Assert.AreEqual("Legacy Business", metadata.name);
            Assert.AreSame(_sprite, metadata.icon);
            Assert.AreEqual("LB", metadata.iconString);
            Assert.AreSame(_model, metadata.model);
            Assert.AreEqual("Legacy description", metadata.description);
            Assert.AreEqual("LB", business.GetIconString());

            business.metadata.name = "Metadata Business";
            business.metadata.iconString = "MB";
            business.metadata.description = "Metadata description";
            business.GetMetadata();

            Assert.AreEqual("Metadata Business", business.businessName);
            Assert.AreEqual("MB", business.iconString);
            Assert.AreEqual("Metadata description", business.description);

            Object.DestroyImmediate(business);
        }

        [Test]
        public void BusinessMetadata_LegacyWritesAfterSyncStillWin()
        {
            Business business = ScriptableObject.CreateInstance<Business>();
            business.metadata.name = "Metadata Business";
            business.GetMetadata();

            business.businessName = "Legacy Override";

            Assert.AreEqual("Legacy Override", business.Metadata.name);
            Assert.AreEqual("Legacy Override", business.businessName);

            Object.DestroyImmediate(business);
        }

        [Test]
        public void BusinessUpgradeLevelMetadata_MigratesLegacyOverrides()
        {
            BusinessUpgradeLevel upgradeLevel = new BusinessUpgradeLevel
            {
                businessName = "Legacy Level",
                image = _sprite,
                iconString = "LL",
                model = _model,
                description = "Legacy level description"
            };

            Metadata metadata = upgradeLevel.Metadata;

            Assert.AreEqual("Legacy Level", metadata.name);
            Assert.AreSame(_sprite, metadata.icon);
            Assert.AreEqual("LL", metadata.iconString);
            Assert.AreSame(_model, metadata.model);
            Assert.AreEqual("Legacy level description", metadata.description);

            upgradeLevel.metadata.name = "Metadata Level";
            upgradeLevel.metadata.description = "Metadata level description";
            upgradeLevel.GetMetadata();

            Assert.AreEqual("Metadata Level", upgradeLevel.businessName);
            Assert.AreEqual("Metadata level description", upgradeLevel.description);
        }

        [Test]
        public void CurrencyManagerBoostAndUpgradeMetadata_MigrateLegacyFields()
        {
            Currency currency = ScriptableObject.CreateInstance<Currency>();
            currency.currencyName = "Legacy Currency";
            currency.image = _sprite;
            currency.iconString = "LC";
            currency.description = "Legacy currency description";

            AssertLegacyMetadata(currency.Metadata, "Legacy Currency", _sprite, "LC", "Legacy currency description");
            Assert.AreEqual("LC", currency.GetIconString());

            Manager manager = ScriptableObject.CreateInstance<Manager>();
            manager.managerName = "Legacy Manager";
            manager.image = _sprite;
            manager.iconString = "LM";
            manager.description = "Legacy manager description";

            AssertLegacyMetadata(manager.Metadata, "Legacy Manager", _sprite, "LM", "Legacy manager description");
            Assert.AreEqual("LM", manager.GetIconString());

            GenericMultiplierBoost boost = ScriptableObject.CreateInstance<GenericMultiplierBoost>();
            boost.boostName = "Legacy Boost";
            boost.icon = _sprite;
            boost.iconString = "LB";
            boost.description = "Legacy boost description";

            AssertLegacyMetadata(boost.Metadata, "Legacy Boost", _sprite, "LB", "Legacy boost description");
            Assert.AreEqual("LB", boost.GetIconString());

            Upgrade upgrade = ScriptableObject.CreateInstance<Upgrade>();
            upgrade.upgradeName = "Legacy Upgrade";
            upgrade.icon = _sprite;
            upgrade.description = "Legacy upgrade description";

            Assert.AreEqual("Legacy Upgrade", upgrade.Metadata.name);
            Assert.AreSame(_sprite, upgrade.Metadata.icon);
            Assert.AreEqual("Legacy upgrade description", upgrade.Metadata.description);

            Object.DestroyImmediate(currency);
            Object.DestroyImmediate(manager);
            Object.DestroyImmediate(boost);
            Object.DestroyImmediate(upgrade);
        }

        private Output CreateCurrencyOutput(BigNumber amount)
        {
            return new Output
            {
                currencyOutput = _currency,
                outputAmount = amount,
                outputMaxAmount = amount
            };
        }

        private static void AssertLegacyOutputsMigrated(List<DropTable> tables, Output expectedOutput)
        {
            Assert.IsNotNull(tables, "Target drop table list should be created when legacy outputs exist.");
            Assert.AreEqual(1, tables.Count, "Legacy outputs should be wrapped in one compatibility drop table.");
            Assert.AreEqual(DropStrategy.All, tables[0].strategy, "Compatibility table should grant every legacy output.");
            Assert.IsNotNull(tables[0].outputs, "Compatibility table outputs should not be null.");
            Assert.AreEqual(1, tables[0].outputs.Count, "Compatibility table should contain the legacy output.");
            Assert.AreSame(expectedOutput, tables[0].outputs[0], "Migration should preserve the original output data.");
        }

        private static void AssertLegacyMetadata(Metadata metadata, string expectedName, Sprite expectedIcon, string expectedIconString, string expectedDescription)
        {
            Assert.AreEqual(expectedName, metadata.name);
            Assert.AreSame(expectedIcon, metadata.icon);
            Assert.AreEqual(expectedIconString, metadata.iconString);
            Assert.AreEqual(expectedDescription, metadata.description);
        }
    }
}
