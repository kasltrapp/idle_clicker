using UnityEngine;

namespace EasyIdleGame.Tests
{
    public static class TestUtilities
    {
        public static Business TestBusiness => Resources.Load<Business>("__testBusiness");
        public static Business TestBusinessWithPriceMultiplier => Resources.Load<Business>("__testBusinessWith1.8PriceMult");

        public static Currency TestCurrency => Resources.Load<Currency>("__testCurrency");

        public static GenericMultiplierBoost TestBoost1 => Resources.Load<GenericMultiplierBoost>("__test0.65PriceBoost");
        public static GenericMultiplierBoost TestBoost2 => Resources.Load<GenericMultiplierBoost>("__test0.33PriceBoost");

        public static Manager TestManager => Resources.Load<Manager>("__testManager");
        public static Business TestBusinessWithManager => Resources.Load<Business>("__testBusinessWithManager");

        public static Upgrade TestUpgrade => Resources.Load<Upgrade>("__testUpgrade");

        // Profit specific
        public static Currency TestCurrencyProfit => Resources.Load<Currency>("idleProfitCalculation/__i_testCurrency");
        public static Currency TestCurrency2Profit => Resources.Load<Currency>("idleProfitCalculation/__i_testCurrency2");
        public static Business TestBusinessAutoProduce => Resources.Load<Business>("idleProfitCalculation/__i_testBusinessAutoProduction");
        public static Manager TestManagerProfit => Resources.Load<Manager>("idleProfitCalculation/__i_testManager");
        public static Business TestBusinessManagerProfit => Resources.Load<Business>("idleProfitCalculation/__i_testBusinessManager");
        public static Business TestBusinessOutput => Resources.Load<Business>("idleProfitCalculation/__i_testBusinessOutput");
        public static Business TestBusinessProductionCost => Resources.Load<Business>("idleProfitCalculation/__i_testBusinessProductionCost");
        public static Business TestBusinessStockpile => Resources.Load<Business>("idleProfitCalculation/__i_testBusinessStockpile");
        public static Boost TestProductionBoost => Resources.Load<Boost>("idleProfitCalculation/__i_testProductionBoost");
        public static Boost TestSpeedBoost => Resources.Load<Boost>("idleProfitCalculation/__i_testSpeedBoost");
        public static Currency TestCurrencyMaxAmount => Resources.Load<Currency>("idleProfitCalculation/__i_testCurrencyMaxAmount");
        public static Business TestBusinessMaxAmountCurOutput => Resources.Load<Business>("idleProfitCalculation/__i_testBusinessMaxAmountCurOutput");
        public static Business TestBusinessMaxAmountCurBusOutput => Resources.Load<Business>("idleProfitCalculation/__i_testBusinessMaxAmountCurBusinessOutput");
        public static Upgrade TestProductionUpgrade => Resources.Load<Upgrade>("idleProfitCalculation/__i_testProductionUpgrade");

        public static GameObject SetupManagerObject()
        {
            GameObject emptyObj = new GameObject("TestManagersEmptyObject");

            emptyObj.AddComponent<BusinessesManager>();
            emptyObj.AddComponent<CurrencyManager>();
            emptyObj.AddComponent<BoostsManager>();
            emptyObj.AddComponent<ManagersManager>();
            emptyObj.AddComponent<UpgradesManager>();
            emptyObj.AddComponent<DailyRewardsManager>();
            emptyObj.AddComponent<PrestigeManager>();
            emptyObj.AddComponent<LocationsManager>();
            emptyObj.AddComponent<OffersManager>();
            emptyObj.AddComponent<ShopManager>();
            emptyObj.AddComponent<AchievementsManager>();

            emptyObj.AddComponent<SaveAndLoad>();
            SaveAndLoad.Instance.savePath = "testSaveFile+" + System.Guid.NewGuid().ToString();

            PlayerStats stats = emptyObj.AddComponent<PlayerStats>();
            stats.firstLevelXp = 100;
            stats.levelIncrementMultiplier = 2;
            stats.Start();

            return emptyObj;
        }

        public static void TearDownManagerObject(GameObject managerObject)
        {
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
            if (SaveAndLoad.Instance != null)
            {
                SaveAndLoad.Instance.IrreversiblyDeleteSave();
            }
        }

#if UNITY_EDITOR
        // Easy Idle Game can be embedded at different locations under Assets (e.g. directly under
        // Assets/EasyIdleGame or nested under Assets/x_Packages/EasyIdleGame), so test setup code
        // must not hardcode a fixed path to this folder. Resolve it from the calling script's own
        // asset location instead.
        public static string FindScriptDirectory(string scriptClassName)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"{scriptClassName} t:Script");
            foreach (var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == scriptClassName)
                {
                    return System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
                }
            }
            throw new System.IO.FileNotFoundException($"Could not locate script file for '{scriptClassName}' to resolve its containing folder.");
        }
#endif
    }
}
