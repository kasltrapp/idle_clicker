using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EasyIdleGame.Demos
{
    public abstract partial class DemoSceneController : MonoBehaviour
    {
        protected enum BusinessPanelMode
        {
            Minimal,
            Inventory,
            Production,
            ProductionCost,
            BoostTarget,
            Automation,
            Progression,
            Offline
        }

        [SerializeField] protected string resourcePrefix;
        [SerializeField] protected string demoTitle;
        [SerializeField] protected PanelSettings sharedPanelSettings;
        [SerializeField] private DemoSceneReadme readme;

        private readonly List<string> saveEventSources = new List<string>();
        private readonly List<Action> liveUiRefreshers = new List<Action>();
        private int saveEventCount;

        private UIDocument uiDocument;
        protected VisualElement dashboard;
        protected Label statusText;
        protected Label titleText;
        protected PanelSettings runtimePanelSettings;
        private PanelTextSettings runtimeTextSettings;
        private Font runtimeFont;

        protected string LastActionResult = "Ready";
        protected string LastShopRewardResult = "None";
        protected string LastBoostResult = "None";
        protected string LastUpgradeResult = "None";
        protected string LastLocationResult = "None";
        protected string LastProfitResult = "None";
        protected string LastManagerResult = "None";
        protected string LastPlayerStatsResult = "None";
        protected string LastDailyRewardResult = "None";
        protected string LastAchievementResult = "None";
        protected string LastPrestigeResult = "None";
        protected string LastOfferResult = "None";
        protected string LastTimeSkipResult = "None";
        protected string LastSaveEventSource = "None";
        protected List<Output> LastOutputs = new List<Output>();
        protected ProfitData LastCalculatedProfit;
        protected ProfitApplicationSummary LastProfitSummary;
        protected int PlayerXpGrantAmount = 120;
        protected int OfflineMinutes = 30;

        private int lastRenderedDailyDay = -1;

        protected virtual bool RefreshWhenDailyRewardDayChanges => false;

        protected void Awake()
        {
            EnsureManagers();
            EnsureUi();
        }

        protected void Start()
        {
            ConfigureManagers();
            RegisterEvents();
            ResetDemoState();
        }

        protected void Update()
        {
            RefreshLiveUi();

            if (RefreshWhenDailyRewardDayChanges && DailyRewardsManager.Instance && lastRenderedDailyDay >= 0)
            {
                int currentDay = DailyRewardsManager.Instance.GetCurrentDay();
                if (currentDay != lastRenderedDailyDay)
                {
                    RefreshDashboard();
                }
            }
        }

        protected void RefreshLiveUi()
        {
            for (int i = 0; i < liveUiRefreshers.Count; i++)
            {
                liveUiRefreshers[i]?.Invoke();
            }
        }

        protected void OnDestroy()
        {
            EconomyChangedEvents.OnSaveWorthyStateChanged.RemoveListener(OnSaveWorthyStateChanged);

            if (ShopManager.Instance)
            {
                ShopManager.Instance.OnShopItemRewardsGranted.RemoveListener(OnShopRewardsGranted);
            }

            if (BoostsManager.Instance)
            {
                BoostsManager.Instance.OnBoostUsed.RemoveListener(OnBoostUsed);
                BoostsManager.Instance.OnTimeSkip.RemoveListener(OnTimeSkip);
            }

            if (UpgradesManager.Instance)
            {
                UpgradesManager.Instance.OnUpgradeAmountChanged.RemoveListener(OnUpgradeChanged);
                UpgradesManager.Instance.OnUpgradeApplied.RemoveListener(OnUpgradeChanged);
            }

            if (LocationsManager.Instance)
            {
                LocationsManager.Instance.OnLocationChanged.RemoveListener(OnLocationChanged);
            }

            ProfitCalculator.OnProfitApplied.RemoveListener(OnProfitApplied);

            if (DailyRewardsManager.Instance)
            {
                DailyRewardsManager.Instance.OnDailyRewardClaimed.RemoveListener(OnDailyRewardClaimed);
            }

            if (AchievementsManager.Instance)
            {
                AchievementsManager.Instance.OnAchievementClaimed.RemoveListener(OnAchievementClaimed);
                AchievementsManager.Instance.OnAchievementChanged.RemoveListener(OnAchievementChanged);
            }

            if (PlayerStats.Instance)
            {
                PlayerStats.Instance.OnXpAdded.RemoveListener(OnXpAdded);
                PlayerStats.Instance.OnLevelUpEvent.RemoveListener(OnPlayerLevelUp);
            }

            if (PrestigeManager.Instance)
            {
                PrestigeManager.Instance.OnPrestige.RemoveListener(OnPrestige);
            }

            if (ManagersManager.Instance)
            {
                ManagersManager.Instance.OnManagerAmountChanged.RemoveListener(OnManagerChanged);
                ManagersManager.Instance.OnManagerBought.RemoveListener(OnManagerChanged);
            }

            if (OffersManager.Instance)
            {
                OffersManager.Instance.OnOfferBecameAvailable.RemoveListener(OnOfferBecameAvailable);
                OffersManager.Instance.OnOfferExpired.RemoveListener(OnOfferExpired);
            }

            if (runtimePanelSettings != null)
            {
                Destroy(runtimePanelSettings);
            }

            if (runtimeTextSettings != null)
            {
                Destroy(runtimeTextSettings);
            }
        }

        public void ResetDemoState()
        {
            if (BusinessesManager.Instance) BusinessesManager.Instance.holders.Clear();
            if (CurrencyManager.Instance) CurrencyManager.Instance.holders.Clear();
            if (BoostsManager.Instance)
            {
                BoostsManager.Instance.holders.Clear();
                BoostsManager.Instance.activeBoosts.Clear();
            }
            if (ManagersManager.Instance) ManagersManager.Instance.holders.Clear();
            if (UpgradesManager.Instance) UpgradesManager.Instance.holders.Clear();
            if (LocationsManager.Instance) LocationsManager.Instance.holders.Clear();
            if (ShopManager.Instance) ShopManager.Instance.holders.Clear();
            if (AchievementsManager.Instance) AchievementsManager.Instance.holders.Clear();
            if (DailyRewardsManager.Instance)
            {
                DailyRewardsManager.Instance.dailyRewards.Clear();
                DailyRewardsManager.Instance.eachDayReward.Clear();
                DailyRewardsManager.Instance.claimedDays.Clear();
                DailyRewardsManager.Instance.combineExactDayRewards = true;
                DailyRewardsManager.Instance.scaleEachDayReward = false;
                DailyRewardsManager.Instance.dayLengthHours = 24f;
                DailyRewardsManager.Instance.initDate = DateTime.Now;
            }
            if (PrestigeManager.Instance)
            {
                PrestigeManager.Instance.prestigeRequirements.Clear();
                PrestigeManager.Instance.currentPrestigeRequirements.Clear();
                PrestigeManager.Instance.prestigeRewardTables.Clear();
                PrestigeManager.Instance.prestiges = new PrestigeData();
                PrestigeManager.Instance.prestigeUpgrades = new List<UpgradeBlock>
                {
                    new UpgradeBlock(UpgradeType.production, 1),
                    new UpgradeBlock(UpgradeType.speed, 1)
                };
                PrestigeManager.Instance.resetBusinesses = true;
                PrestigeManager.Instance.resetCurrencies = true;
                PrestigeManager.Instance.resetBoostsInInventory = true;
                PrestigeManager.Instance.resetActiveBoosts = true;
                PrestigeManager.Instance.resetManagers = true;
                PrestigeManager.Instance.resetUpgrades = true;
                PrestigeManager.Instance.resetLocations = true;
                PrestigeManager.Instance.resetLevel = true;
            }
            if (OffersManager.Instance)
            {
                OffersManager.Instance.possibleOffers.Clear();
                OffersManager.Instance.minSecondsBetweenOffers = 999f;
                OffersManager.Instance.maxSecondsBetweenOffers = 999f;
                OffersManager.Instance.TryConsumeCurrentOffer();
            }

            if (PlayerStats.Instance)
            {
                PlayerStats.Instance.totalMoneyMade.Clear();
                PlayerStats.Instance.totalBusinessesObtained.Clear();
                PlayerStats.Instance.totalBoostsObtained.Clear();
                PlayerStats.Instance.customStats.Clear();
                PlayerStats.Instance.ResetLevel();
            }

            LastSaveEventSource = "None";
            saveEventSources.Clear();
            saveEventCount = 0;
            LastOutputs.Clear();
            LastCalculatedProfit = null;
            LastProfitSummary = null;
            LastActionResult = "Ready";
            LastShopRewardResult = "None";
            LastBoostResult = "None";
            LastUpgradeResult = "None";
            LastLocationResult = "None";
            LastProfitResult = "None";
            LastManagerResult = "None";
            LastPlayerStatsResult = "None";
            LastDailyRewardResult = "None";
            LastAchievementResult = "None";
            LastPrestigeResult = "None";
            LastOfferResult = "None";
            LastTimeSkipResult = "None";
            lastRenderedDailyDay = -1;

            BusinessesManager.RefreshMergeRecipeCache();
            SeedDemoState();
            Log($"Reset and seeded {demoTitle}.");
            RefreshDashboard();
        }

        public string CaptureVerificationSnapshotForMcp()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                return "No UIDocument root is available.";
            }

            RefreshLiveUi();

            string outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "TestResults", "McpDemoSceneRuntimeScreenshots"));
            Directory.CreateDirectory(outputDirectory);

            string sceneName = gameObject.scene.name;
            string screenshotPath = Path.Combine(outputDirectory, sceneName + "_screen_capture_latest.png");
            string summaryPath = Path.Combine(outputDirectory, sceneName + "_ui_summary_latest.txt");

            if (File.Exists(screenshotPath))
            {
                File.Delete(screenshotPath);
            }

            WriteFallbackSnapshotPng(screenshotPath);

            string summary = BuildVerificationSummary(screenshotPath, summaryPath);
            File.WriteAllText(summaryPath, summary);
            Log($"Wrote MCP verification snapshot for {sceneName}.");
            return summary;
        }

        public string VerifyProductionActionRefreshForMcp()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                return "No UIDocument root is available.";
            }

            RefreshDashboard();
            RefreshLiveUi();

            string businessName = GetFirstStartableBusinessResourceName();
            if (string.IsNullOrEmpty(businessName))
            {
                List<string> reports = LoadAllPrefixed<Business>()
                    .Select(business =>
                    {
                        string name = GetResourceNameWithoutPrefix(business);
                        return $"{name}: {DescribeStartAllProductionAvailability(name)}";
                    })
                    .ToList();
                return reports.Count == 0
                    ? $"No Business resources matched prefix '{resourcePrefix}_'."
                    : "No startable business was available in this scene. " + string.Join(" | ", reports);
            }

            int enabledBeforeStart = CountEnabledStartAllButtons();
            StartAllOwnedBusinessCopies(businessName);
            RefreshLiveUi();
            int enabledAfterStart = CountEnabledStartAllButtons();

            FinishBusinessProductions(businessName);
            RefreshLiveUi();
            int enabledAfterFinish = CountEnabledStartAllButtons();

            bool passed = enabledBeforeStart > 0
                && enabledAfterStart < enabledBeforeStart
                && enabledAfterFinish >= enabledBeforeStart;

            return $"{(passed ? "PASS" : "FAIL")}: {businessName} Start all buttons enabled before={enabledBeforeStart}, after start={enabledAfterStart}, after finish={enabledAfterFinish}.";
        }

        private string GetFirstStartableBusinessResourceName()
        {
            foreach (Business business in LoadAllPrefixed<Business>())
            {
                string businessName = GetResourceNameWithoutPrefix(business);
                if (CanStartAnyBusinessProduction(businessName))
                {
                    return businessName;
                }
            }

            return null;
        }

        protected string GetResourceNameWithoutPrefix(UnityEngine.Object asset)
        {
            if (asset == null) return null;

            string resourceName = GetResourceNameWithPrefix(asset);
            return StripResourcePrefixAndCategory(resourceName);
        }

        private string GetResourceNameWithPrefix(UnityEngine.Object asset)
        {
            if (asset == null) return null;

            string resourceName = asset.name;
#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrWhiteSpace(assetPath))
            {
                resourceName = Path.GetFileNameWithoutExtension(assetPath);
            }
#endif
            return resourceName;
        }

        private int CountEnabledStartAllButtons()
        {
            int enabledButtons = 0;
            uiDocument.rootVisualElement.Query<Button>().ForEach(button =>
            {
                if (button.text == "Start all" && button.enabledSelf && IsElementVisibleForSummary(button))
                {
                    enabledButtons++;
                }
            });
            return enabledButtons;
        }

        private static void WriteFallbackSnapshotPng(string screenshotPath)
        {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color background = FallbackSnapshotBackgroundColor;
            Color accent = FallbackSnapshotAccentColor;
            Color[] pixels = new Color[16 * 16];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    bool line = x == 0 || y == 0 || x == 15 || y == 15 || x == y;
                    pixels[y * 16 + x] = line ? accent : background;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(screenshotPath, texture.EncodeToPNG());
            Destroy(texture);
        }

        protected abstract void SeedDemoState();

        protected void RefreshDashboard()
        {
            if (dashboard == null) return;

            dashboard.Clear();
            liveUiRefreshers.Clear();
            AddOverview();
            AddCurrencyTotalsPanel();
            BuildSceneDashboard();

            UpdateStatusText();
        }

        protected abstract void BuildSceneDashboard();
    }
}

