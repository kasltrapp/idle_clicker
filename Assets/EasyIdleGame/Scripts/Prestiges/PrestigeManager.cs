using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that handles prestiging
    /// </summary>
    [AddComponentMenu("EasyIdleGame/Managers/Prestige Manager")]
    public class PrestigeManager : ManagerBase<PrestigeManager>, ISerializationCallbackReceiver
    {
        [Header("Settings")]
        [CommentArea("Prestige Flow", "Prestige checks requirements, resets selected systems, grants rewards, and increases the persistent prestige upgrade multiplier.", "Require player level 50 in prestigeRequirements, add a production UpgradeBlock to prestigeUpgrades, add premium currency to exludedCurrencies, then wire PrestigeDisplayer.Prestige() to a UI button.")]
        [SerializeField] private string _prestigeFlowComment;
        [ConditionalCommentArea("Prestige setup", "No prestige requirements are configured. Prestige can be available immediately unless max prestige count or other runtime state prevents it.", "prestigeRequirements", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _prestigeRequirementsInfo;
        [ConditionalCommentArea("Prestige setup", "One or more prestige requirements have amount 0 or less. Those requirements will be satisfied immediately.", "prestigeRequirements", ConditionalCommentAreaMode.AnyListBigNumberLessThanOrEqual, "inputAmount", 0)]
        [SerializeField] private string _prestigeRequirementAmountsInfo;
        [Tooltip("Requirements that must be met before the player can prestige. Empty list allows prestige immediately unless maxPrestigeCount or runtime state prevents it.")]
        public List<Lock> prestigeRequirements;

        [Tooltip("Multiplier applied to prestigeRequirements after each prestige. Player level and prestige-count requirements are not scaled unless multiplyLevelOnPrestige is enabled.")]
        [SerializeField] private float prestigeCostMultiplier = 2;

        [Tooltip("If enabled, player level and prestige-count requirements are also multiplied when prestige requirements scale.")]
        [SerializeField] private bool multiplyLevelOnPrestige = false;

        [Tooltip("Maximum number of prestiges the player can perform. 0 means infinite.")]
        public int maxPrestigeCount = 0;

        [Tooltip("Multiplier applied to prestigeUpgrades after each prestige. Higher values make persistent prestige bonuses grow faster.")]
        public float prestigeUpgradesMultiplier = 2;

        [ConditionalCommentArea("Prestige setup", "No prestige upgrades are configured. Prestiging can still reset and reward, but it will not add persistent upgrade multipliers.", "prestigeUpgrades", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _prestigeUpgradesInfo;
        [Tooltip("Persistent upgrade multipliers granted by prestige count. Empty list means prestige grants no ongoing multiplier bonus.")]
        public List<UpgradeBlock> prestigeUpgrades = new()
        {
            new UpgradeBlock(UpgradeType.production, 1),
            new UpgradeBlock(UpgradeType.speed, 1),
        };

        [ConditionalCommentArea("Prestige setup", "No prestige rewards are configured. Prestiging will not grant reward outputs.", "prestigeRewardTables", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _prestigeRewardTablesInfo;
        [Tooltip("Drop tables granted immediately after a successful prestige. Empty list means prestige grants no immediate rewards.")]
        public List<DropTable> prestigeRewardTables = new List<DropTable>();

        [HideInInspector]
        [UnityEngine.Serialization.FormerlySerializedAs("prestigeRewards")]
        public List<Output> _legacyPrestigeRewards;

        [Tooltip("If enabled, immediate prestige rewards are multiplied by the new prestige count.")]
        public bool scaleRewardsWithPrestigeCount = true;

        [Header("Reset settings")]
        [CommentArea("Reset Configuration", "When prestiging, the game state is wiped based on these flags. Disabling a flag preserves that system's progress. Use the excluded lists to protect specific items, like premium currencies or permanent boosts, from being reset.", "For a standard prestige reset, enable resets for businesses, managers, boosts, upgrades, and level. Add Gems to exludedCurrencies so premium currency survives the reset.")]
        [SerializeField] private string _resetSettingsComment;

        [Header("Managers")]
        [Tooltip("If enabled, manager holders are reset during prestige, except entries in excludedManagers.")]
        public bool resetManagers = true;

        [Tooltip("If enabled, prestige clears the Unlock Only Once state cached by manager holders.")]
        public bool resetManagerUnlockState = true;

        [Tooltip("Managers that survive prestige when resetManagers is enabled. Leave empty for a full manager reset.")]
        public List<Manager> excludedManagers = new();

        [Tooltip("Manager groups whose members survive prestige when resetManagers is enabled.")]
        public List<ManagerGroup> excludedManagerGroups = new();

        [Header("Businesses")]
        [Tooltip("If enabled, business holders are reset during prestige, except entries in excludedBusinesses.")]
        public bool resetBusinesses = true;

        [Tooltip("If enabled, prestige clears the Unlock Only Once state cached by business holders.")]
        public bool resetBusinessUnlockState = true;

        [Tooltip("Businesses that survive prestige when resetBusinesses is enabled. Leave empty for a full business reset.")]
        public List<Business> excludedBusinesses = new();

        [Tooltip("Business groups whose members survive prestige when resetBusinesses is enabled.")]
        public List<BusinessGroup> excludedBusinessGroups = new();

        [Header("Boosts")]
        [Tooltip("If enabled, active boost timers are cleared during prestige.")]
        public bool resetActiveBoosts = true;

        [Tooltip("If enabled, boost inventory holders are reset during prestige, except entries in excludedBoosts.")]
        public bool resetBoostsInInventory = true;

        [Tooltip("If enabled, prestige clears the Unlock Only Once state cached by boost holders.")]
        public bool resetBoostUnlockState = true;

        [Tooltip("Boosts that survive prestige when resetBoostsInInventory is enabled. Leave empty for a full boost inventory reset.")]
        public List<Boost> excludedBoosts = new();

        [Tooltip("Boost groups whose members survive prestige when resetBoostsInInventory is enabled.")]
        public List<BoostGroup> excludedBoostGroups = new();

        [Header("Player Level")]
        [Tooltip("If enabled, player level and XP are reset to default during prestige.")]
        public bool resetLevel = true;

        [Header("Upgrades")]
        [Tooltip("If enabled, purchased upgrades are reset during prestige, except entries in excludedUpgrades.")]
        public bool resetUpgrades = true;

        [Tooltip("If enabled, prestige clears the Unlock Only Once state cached by upgrade holders.")]
        public bool resetUpgradeUnlockState = true;

        [Tooltip("Upgrades that survive prestige when resetUpgrades is enabled. Leave empty for a full upgrade reset.")]
        public List<Upgrade> excludedUpgrades = new();

        [Tooltip("Upgrade groups whose members survive prestige when resetUpgrades is enabled.")]
        public List<UpgradeGroup> excludedUpgradeGroups = new();

        [Header("Locations")]
        [Tooltip("If enabled, unlocked locations/worlds are reset during prestige, except entries in excludedLocations.")]
        public bool resetLocations = true;

        [Tooltip("If enabled, prestige clears the Unlock Only Once state cached by location holders.")]
        public bool resetLocationUnlockState = true;

        [Tooltip("Locations that survive prestige when resetLocations is enabled. Leave empty for a full location reset.")]
        public List<Location> excludedLocations = new();

        [Tooltip("Location groups whose members survive prestige when resetLocations is enabled.")]
        public List<LocationGroup> excludedLocationGroups = new();

        [Header("Achievements")]
        [Tooltip("If enabled, achievement unlock and claim progress is reset during prestige, except entries in excludedAchievements.")]
        public bool resetAchievements = true;

        [Tooltip("If enabled, prestige clears the Unlock Only Once state cached by achievement holders.")]
        public bool resetAchievementUnlockState = true;

        [Tooltip("Achievements that survive prestige when resetAchievements is enabled. Leave empty for a full achievement reset.")]
        public List<Achievement> excludedAchievements = new();

        [Tooltip("Achievement groups whose members survive prestige when resetAchievements is enabled.")]
        public List<AchievementGroup> excludedAchievementGroups = new();

        [Header("Currencies")]
        [Tooltip("If enabled, currency balances are reset during prestige, except entries in exludedCurrencies.")]
        public bool resetCurrencies = true;

        [Tooltip("Currencies that survive prestige when resetCurrencies is enabled. Leave empty for a full currency reset.")]
        public Currency[] exludedCurrencies = new Currency[0];

        [Tooltip("Currency groups whose members survive prestige when resetCurrencies is enabled.")]
        public List<CurrencyGroup> excludedCurrencyGroups = new();

        [Header("Shop Items")]
        [Tooltip("If enabled, prestige clears the Unlock Only Once state cached by shop item holders. Shop ownership is not reset.")]
        public bool resetShopItemUnlockState = true;

        [Header("Auto")]
        [Tooltip("Runtime prestige count and scaling state. Usually managed by save/load; edit only for debugging or migration.")]
        public PrestigeData prestiges = new PrestigeData();

        /// <summary> Current prestige requirements </summary>
        public List<Lock> currentPrestigeRequirements = new();

        [Header("Audio")]
        [Tooltip("Sound played when prestiging.")]
        public AudioData prestigeSound;

        [Header("Events")]
        [Tooltip("Invoked after a successful prestige reset, reward grant, and prestige counter increment.")]
        public UnityEvent OnPrestige = new();

        public static float GetPrestigeMultiplierOfType(UpgradeType type)
        {
            if (!Instance) return 1;
            return Instance.prestigeUpgrades.Multiply(Instance.prestiges.currentPrestigeUpgradesMultiplier).GetMultiplierOfType(type);
        }

        public List<UpgradeBlock> GetPrestigeMultipliers()
        {
            return prestigeUpgrades.Multiply(Instance.prestiges.currentPrestigeUpgradesMultiplier);
        }

        public List<Output> GetExpectedPrestigeRewards()
        {
            BigNumber multiplier = scaleRewardsWithPrestigeCount ? prestiges.prestigeCount + 1 : 1;
            return RewardCalculator.GetAverageOutputs(prestigeRewardTables, multiplier, RewardCalculator.CreateContext(SourceType.Reset));
        }

        [ContextMenu("Prestige Game")]
        public void PrestigeGameContextMenu()
        {
            TryPrestigeGame(out _);
        }

        public ResetSummary ResetProgress(PrestigeResetOptions options = null)
        {
            options ??= new PrestigeResetOptions();

            List<Manager> preservedManagers = ResetSummary.CopyList(excludedManagers);
            List<Business> preservedBusinesses = ResetSummary.CopyList(excludedBusinesses);
            List<Boost> preservedBoosts = ResetSummary.CopyList(excludedBoosts);
            List<Upgrade> preservedUpgrades = ResetSummary.CopyList(excludedUpgrades);
            List<Location> preservedLocations = ResetSummary.CopyList(excludedLocations);
            List<Achievement> preservedAchievements = ResetSummary.CopyList(excludedAchievements);
            List<Currency> preservedCurrencies = exludedCurrencies != null
                ? exludedCurrencies.Where(currency => currency != null).Distinct().ToList()
                : new List<Currency>();

            ResetSummary.AddDistinct(preservedManagers, options.additionalExcludedManagers);
            ResetSummary.AddDistinct(preservedBusinesses, options.additionalExcludedBusinesses);
            ResetSummary.AddDistinct(preservedBoosts, options.additionalExcludedBoosts);
            ResetSummary.AddDistinct(preservedUpgrades, options.additionalExcludedUpgrades);
            ResetSummary.AddDistinct(preservedLocations, options.additionalExcludedLocations);
            ResetSummary.AddDistinct(preservedAchievements, options.additionalExcludedAchievements);
            ResetSummary.AddDistinct(preservedCurrencies, options.additionalExcludedCurrencies);

            ResetSummary.AddItemsInGroups(
                preservedManagers,
                ManagersManager.Instance ? ManagersManager.Instance.Holders.Select(holder => holder.Item) : null,
                manager => manager.managerGroups,
                ResetSummary.CombineGroups(excludedManagerGroups, options.additionalExcludedManagerGroups));
            ResetSummary.AddItemsInGroups(
                preservedLocations,
                LocationsManager.Instance ? LocationsManager.Instance.Holders.Select(holder => holder.Item) : null,
                location => location.locationGroups,
                ResetSummary.CombineGroups(excludedLocationGroups, options.additionalExcludedLocationGroups));
            ResetSummary.AddItemsInGroups(
                preservedBusinesses,
                BusinessesManager.Instance ? BusinessesManager.Instance.Holders.Select(holder => holder.Item) : null,
                business => business.GetEffectiveBusinessGroups(),
                ResetSummary.CombineGroups(excludedBusinessGroups, options.additionalExcludedBusinessGroups));
            ResetSummary.AddItemsInGroups(
                preservedBoosts,
                BoostsManager.Instance ? BoostsManager.Instance.Holders.Select(holder => holder.Item) : null,
                boost => boost.boostGroups,
                ResetSummary.CombineGroups(excludedBoostGroups, options.additionalExcludedBoostGroups));
            ResetSummary.AddItemsInGroups(
                preservedUpgrades,
                UpgradesManager.Instance ? UpgradesManager.Instance.Holders.Select(holder => holder.Item) : null,
                upgrade => upgrade.upgradeGroups,
                ResetSummary.CombineGroups(excludedUpgradeGroups, options.additionalExcludedUpgradeGroups));
            ResetSummary.AddItemsInGroups(
                preservedAchievements,
                AchievementsManager.Instance ? AchievementsManager.Instance.Holders.Select(holder => holder.Item) : null,
                achievement => achievement.achievementGroups,
                ResetSummary.CombineGroups(excludedAchievementGroups, options.additionalExcludedAchievementGroups));
            ResetSummary.AddItemsInGroups(
                preservedCurrencies,
                CurrencyManager.Instance ? CurrencyManager.Instance.Holders.Select(holder => holder.Item) : null,
                currency => currency.currencyGroups,
                ResetSummary.CombineGroups(excludedCurrencyGroups, options.additionalExcludedCurrencyGroups));

            if (resetManagers)
                ManagersManager.TryResetManagerHolders(preservedManagers);

            if (resetBusinesses)
                BusinessesManager.TryResetManagerHolders(preservedBusinesses);

            if (resetActiveBoosts && BoostsManager.Instance)
                BoostsManager.Instance.ClearActiveBoosts();

            if (resetBoostsInInventory)
                BoostsManager.TryResetManagerHolders(preservedBoosts);

            if (resetCurrencies)
                CurrencyManager.TryResetManagerHolders(preservedCurrencies);

            if (resetLevel && PlayerStats.Instance)
                PlayerStats.Instance.ResetLevel();

            if (resetUpgrades)
                UpgradesManager.TryResetManagerHolders(preservedUpgrades);

            if (resetLocations)
                LocationsManager.TryResetManagerHolders(preservedLocations);

            if (resetAchievements)
                AchievementsManager.TryResetManagerHolders(preservedAchievements);

            if (resetManagerUnlockState)
                ManagersManager.TryResetUnlockedState();
            if (resetBusinessUnlockState)
                BusinessesManager.TryResetUnlockedState();
            if (resetBoostUnlockState)
                BoostsManager.TryResetUnlockedState();
            if (resetUpgradeUnlockState)
                UpgradesManager.TryResetUnlockedState();
            if (resetLocationUnlockState)
                LocationsManager.TryResetUnlockedState();
            if (resetAchievementUnlockState)
                AchievementsManager.TryResetUnlockedState();
            if (resetShopItemUnlockState)
                ShopManager.TryResetUnlockedState();

            if (options.notifySaveWorthyStateChanged)
                EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(PrestigeManager));

            return new ResetSummary(
                resetManagers,
                resetBusinesses,
                resetActiveBoosts,
                resetBoostsInInventory,
                resetCurrencies,
                resetLevel,
                resetUpgrades,
                resetLocations,
                resetAchievements,
                preservedManagers,
                preservedBusinesses,
                preservedBoosts,
                preservedUpgrades,
                preservedLocations,
                preservedAchievements,
                preservedCurrencies);
        }

        public bool TryPrestigeGame(out List<Output> outputs)
        {
            return TryPrestigeGame(out outputs, out _, null);
        }

        public bool TryPrestigeGame(out List<Output> outputs, PrestigeResetOptions resetOptions)
        {
            return TryPrestigeGame(out outputs, out _, resetOptions);
        }

        public bool TryPrestigeGame(out List<Output> outputs, out ResetSummary resetSummary, PrestigeResetOptions resetOptions = null)
        {
            outputs = null;
            resetSummary = default;
            if (!CanPrestige()) return false;

            resetOptions ??= new PrestigeResetOptions();
            bool notifyAfterReset = resetOptions.notifySaveWorthyStateChanged;
            resetOptions.notifySaveWorthyStateChanged = false;
            try
            {
                resetSummary = ResetProgress(resetOptions);
            }
            finally
            {
                resetOptions.notifySaveWorthyStateChanged = notifyAfterReset;
            }

            prestiges.prestigeCount++;

            prestiges.currentCostMultiplier *= prestigeCostMultiplier;
            prestiges.currentPrestigeUpgradesMultiplier *= prestigeUpgradesMultiplier;

            currentPrestigeRequirements = prestigeRequirements.Multiply(prestiges.currentCostMultiplier, multiplyLevelOnPrestige);

            BigNumber rewardMultiplier = scaleRewardsWithPrestigeCount ? prestiges.prestigeCount : 1;
            outputs = RewardCalculator.ApplyOutputs(prestigeRewardTables, rewardMultiplier, RewardCalculator.CreateContext(SourceType.Reset));

            if (LocationsManager.Instance != null)
            {
                LocationsManager.Instance.RestoreStarterOutputs();
            }

            OnPrestige.Invoke();
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(PrestigeManager));

            Log($"Prestiged game! New prestige count: {prestiges.prestigeCount}", LogCategory.Verbose);

            return true;
        }

        public bool CanPrestige()
        {
            if (maxPrestigeCount > 0 && prestiges.prestigeCount >= maxPrestigeCount)
                return false;

            if (currentPrestigeRequirements.Count == 0)
                currentPrestigeRequirements = prestigeRequirements.Multiply(prestiges.currentCostMultiplier, multiplyLevelOnPrestige);

            return currentPrestigeRequirements.IsUnlocked();
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (_legacyPrestigeRewards != null && _legacyPrestigeRewards.Count > 0)
            {
                if (prestigeRewardTables == null) prestigeRewardTables = new List<DropTable>();

                DropTable table = new DropTable();
                table.strategy = DropStrategy.All;
                table.outputs = new List<Output>(_legacyPrestigeRewards);

                prestigeRewardTables.Add(table);
                _legacyPrestigeRewards.Clear();
            }
        }
    }
}
