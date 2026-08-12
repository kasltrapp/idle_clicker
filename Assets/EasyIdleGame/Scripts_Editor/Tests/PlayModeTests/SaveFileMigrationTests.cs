using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EasyIdleGame.Tests
{
    /// <summary>
    /// Covers loading save files that reference scriptable objects (such as active boosts) whose
    /// underlying asset no longer resolves - e.g. because it was renamed or deleted after the save
    /// was written. Regression coverage for the SaveableScriptableObject null-reference crash.
    /// </summary>
    public class SaveFileMigrationTests
    {
        private const string MissingBoostErrorMessage =
            "Scriptable object of type 'Boost' with name 'RenamedOrDeletedBoost' not found while loading save data. It may have been renamed or removed since the save was written.";

        private GameObject _managerObject;
        private GenericMultiplierBoost _resolvableBoost;

#if UNITY_EDITOR
        private List<string> _tempAssetPaths = new List<string>();

        // We must physically create .asset files using AssetDatabase instead of just using
        // ScriptableObject.CreateInstance() because the game's save system (SaveableScriptableObject)
        // relies strictly on Resources.LoadAll<T>("") to deserialize and reconstruct objects by name.
        // Resources.LoadAll cannot find in-memory instances, it only finds actual assets on disk.
        private T CreateTempAsset<T>(string assetName) where T : ScriptableObject
        {
            string testsDir = TestUtilities.FindScriptDirectory(nameof(SaveFileMigrationTests));
            string dir = $"{testsDir}/Resources";
            if (!UnityEditor.AssetDatabase.IsValidFolder(dir))
            {
                UnityEditor.AssetDatabase.CreateFolder(testsDir, "Resources");
            }
            string path = $"{dir}/{assetName}.asset";

            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = assetName;

            UnityEditor.AssetDatabase.CreateAsset(asset, path);
            _tempAssetPaths.Add(path);

            return asset;
        }
#endif

        [SetUp]
        public void SetUp()
        {
            // These tests intentionally trigger the "asset could not be resolved" path, which would
            // otherwise pop a blocking Editor dialog and stall the test run.
            SaveLoadDiagnostics.SuppressLoadErrorDialog = true;

#if UNITY_EDITOR
            _resolvableBoost = CreateTempAsset<GenericMultiplierBoost>("TempMigrationTestBoost");
            UnityEditor.AssetDatabase.SaveAssets();
#endif
            _managerObject = TestUtilities.SetupManagerObject();
        }

        [TearDown]
        public void TearDown()
        {
            SaveLoadDiagnostics.SuppressLoadErrorDialog = false;

            TestUtilities.TearDownManagerObject(_managerObject);

#if UNITY_EDITOR
            foreach (var path in _tempAssetPaths)
            {
                UnityEditor.AssetDatabase.DeleteAsset(path);
            }
            _tempAssetPaths.Clear();
#endif
        }

        [Test]
        public void SaveableScriptableObject_Constructor_DoesNotThrow_WhenValueIsNull()
        {
            SaveableScriptableObject<Boost> saveable = null;

            Assert.DoesNotThrow(() => saveable = new SaveableScriptableObject<Boost>(null),
                "Constructing with a null value (e.g. an asset that failed to resolve) must not throw.");
            Assert.IsNull(saveable.Value, "Value should stay null.");
            Assert.IsNull(saveable.Id, "Id should stay null when no value was given.");
        }

        [Test]
        public void LoadScriptableObject_ReturnsNull_WhenAssetCannotBeFoundByName()
        {
            SaveableScriptableObject<Boost> saveable = new SaveableScriptableObject<Boost>(_resolvableBoost)
            {
                Id = "RenamedOrDeletedBoost"
            };

            LogAssert.Expect(LogType.Error, MissingBoostErrorMessage);

            Boost resolved = _resolvableBoost;
            Assert.DoesNotThrow(() => resolved = saveable.LoadScriptableObject(),
                "Resolving a missing asset by name should not throw.");
            Assert.IsNull(resolved, "A name that matches no on-disk asset should resolve to null.");
        }

        [Test]
        public void Load_DropsActiveBoost_WhenBoostAssetNoLongerResolves()
        {
            ActiveBoostHolder unresolvableHolder = new ActiveBoostHolder(_resolvableBoost, 42);
            unresolvableHolder.boostSaveable.Id = "RenamedOrDeletedBoost";

            SaveFile oldSave = new SaveFile
            {
                activeBoosts = new[] { unresolvableHolder }
            };

            LogAssert.Expect(LogType.Error, MissingBoostErrorMessage);

            Assert.DoesNotThrow(() => oldSave.Load(),
                "Loading a save that references a boost asset which no longer exists must not crash.");

            Assert.AreEqual(0, BoostsManager.Instance.activeBoosts.Count,
                "An active boost that can't be resolved should be dropped rather than kept with a null boost reference.");
        }

        [Test]
        public void Load_KeepsResolvableActiveBoosts_WhenASiblingEntryIsUnresolvable()
        {
            ActiveBoostHolder resolvableHolder = new ActiveBoostHolder(_resolvableBoost, 42);

            ActiveBoostHolder unresolvableHolder = new ActiveBoostHolder(_resolvableBoost, 7);
            unresolvableHolder.boostSaveable.Id = "RenamedOrDeletedBoost";

            SaveFile oldSave = new SaveFile
            {
                activeBoosts = new[] { resolvableHolder, unresolvableHolder }
            };

            LogAssert.Expect(LogType.Error, MissingBoostErrorMessage);

            oldSave.Load();

            Assert.AreEqual(1, BoostsManager.Instance.activeBoosts.Count,
                "The resolvable active boost should still load even when a sibling entry can't be resolved.");
            Assert.AreEqual(_resolvableBoost, BoostsManager.Instance.activeBoosts[0].boost);
            Assert.AreEqual(42, BoostsManager.Instance.activeBoosts[0].timeLeft);
        }
    }
}
