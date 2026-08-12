using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace EasyIdleGame.Tests
{
    public class SaveAndLoadSystemTests
    {
        private GameObject _managerObject;

        private Business _testBusiness;
        private Currency _testCurrency;
        private Upgrade _testUpgrade;
        private GenericMultiplierBoost _testBoost;
        private Manager _testManager;
        private Location _testLocation;
        private ShopItem _testShopItem;
        private Achievement _testAchievement;

#if UNITY_EDITOR
        private List<string> _tempAssetPaths = new List<string>();

        // We must physically create .asset files using AssetDatabase instead of just using 
        // ScriptableObject.CreateInstance() because the game's save system (SaveableScriptableObject)
        // relies strictly on Resources.LoadAll<T>("") to deserialize and reconstruct objects by name. 
        // Resources.LoadAll cannot find in-memory instances, it only finds actual assets on disk.
        private T CreateTempAsset<T>(string assetName) where T : ScriptableObject
        {
            string testsDir = TestUtilities.FindScriptDirectory(nameof(SaveAndLoadSystemTests));
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
#if UNITY_EDITOR
            _testBusiness = CreateTempAsset<Business>("TempSaveTestBusiness");
            _testCurrency = CreateTempAsset<Currency>("TempSaveTestCurrency");
            _testUpgrade = CreateTempAsset<Upgrade>("TempSaveTestUpgrade");
            _testBoost = CreateTempAsset<GenericMultiplierBoost>("TempSaveTestBoost");
            _testManager = CreateTempAsset<Manager>("TempSaveTestManager");
            _testLocation = CreateTempAsset<Location>("TempSaveTestLocation");
            _testShopItem = CreateTempAsset<ShopItem>("TempSaveTestShopItem");
            _testAchievement = CreateTempAsset<Achievement>("TempSaveTestAchievement");

            UnityEditor.AssetDatabase.SaveAssets();
#endif
            _managerObject = TestUtilities.SetupManagerObject();
        }

        [TearDown]
        public void TearDown()
        {
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
        public void SaveAndLoad_RestoresCorrectAmounts()
        {
            BusinessesManager.Instance.ForceBuyItemsForFree(_testBusiness, 1);
            CurrencyManager.Instance.AddCurrencyAmount(_testCurrency, 500);
            PlayerStats.Instance.AddXp(200);

            UpgradesManager.Instance.GetOrAddHolder(_testUpgrade).amount = 1;
            BoostsManager.Instance.AddBoosts(_testBoost, 1);
            ManagersManager.Instance.GetOrAddHolder(_testManager).AddManagerCount(1);
            LocationsManager.Instance.GetOrAddHolder(_testLocation).AddAmount(1);
            ShopManager.Instance.GetOrAddHolder(_testShopItem).AddAmount(1);
            AchievementsManager.Instance.GetOrAddHolder(_testAchievement).isClaimed = true;

            DailyRewardsManager.Instance.initDate = new System.DateTime(2020, 1, 1);
            DailyRewardsManager.Instance.claimedDays.Add(1);

            SaveAndLoad.Instance.Save();

            BusinessesManager.Instance.ForceBuyItemsForFree(_testBusiness, 1);
            CurrencyManager.Instance.AddCurrencyAmount(_testCurrency, 100);
            PlayerStats.Instance.AddXp(100);

            UpgradesManager.Instance.holders.Clear();
            BoostsManager.Instance.holders.Clear();
            ManagersManager.Instance.holders.Clear();
            LocationsManager.Instance.holders.Clear();
            ShopManager.Instance.holders.Clear();
            AchievementsManager.Instance.holders.Clear();
            DailyRewardsManager.Instance.initDate = System.DateTime.Now;
            DailyRewardsManager.Instance.claimedDays.Clear();

            SaveAndLoad.Instance.Load();

            Assert.IsTrue(BusinessesManager.Instance.GetHolder(_testBusiness).amount == 1, "The business amount should be restored to 1 after loading the save file");
            Assert.IsTrue(CurrencyManager.Instance.GetCurrencyAmount(_testCurrency) == 500, "Currency should be restored to 500.");
            Assert.AreEqual(new BigNumber(500), CurrencyManager.Instance.GetTotalCurrencyAmount(_testCurrency), "Currency total amount should be restored to the saved holder total.");

            Assert.AreEqual(1, PlayerStats.Instance.level.currentLevel, "Player should be level 1.");
            Assert.AreEqual(new BigNumber(101), PlayerStats.Instance.level.currentXp, $"XP should be restored to 101, but are {PlayerStats.Instance.level.currentXp}");

            Assert.IsTrue(UpgradesManager.Instance.GetHolder(_testUpgrade).amount == 1, "Upgrade amount should be restored to 1");
            Assert.IsTrue(BoostsManager.Instance.GetHolder(_testBoost).amount == 1, "Boost amount should be restored to 1");
            Assert.IsTrue(ManagersManager.Instance.GetHolder(_testManager).TotalCount == 1, "Manager amount should be restored to 1");
            Assert.IsTrue(LocationsManager.Instance.GetHolder(_testLocation).Amount == 1, "Location amount should be restored to 1");
            Assert.IsTrue(ShopManager.Instance.GetHolder(_testShopItem).Amount == 1, "ShopItem amount should be restored to 1");
            Assert.IsTrue(AchievementsManager.Instance.GetHolder(_testAchievement).isClaimed, "Achievement should be claimed");

            Assert.AreEqual(new System.DateTime(2020, 1, 1), DailyRewardsManager.Instance.initDate, "DailyRewards initDate should be restored");
            Assert.IsTrue(DailyRewardsManager.Instance.claimedDays.Contains(1), "DailyRewards claimedDays should be restored");
        }

        [Test]
        public void Load_SeedsCurrencyTotalAmountFromCurrentAmount_WhenOldSaveHasNoCurrencyTotal()
        {
            SaveFile oldSave = new SaveFile
            {
                currencies = new[]
                {
                    new CurrencyHolder(_testCurrency, 250)
                    {
                        totalAmount = 0
                    }
                }
            };

            oldSave.Load();

            CurrencyHolder loadedHolder = CurrencyManager.Instance.GetHolder(_testCurrency);
            Assert.NotNull(loadedHolder, "Currency holder should load from the old-save-shaped data.");
            Assert.AreEqual(new BigNumber(250), loadedHolder.amount, "Current amount should load unchanged.");
            Assert.AreEqual(new BigNumber(250), loadedHolder.TotalAmount, "Missing currency total should seed from current amount for old saves.");
        }
    }
}
