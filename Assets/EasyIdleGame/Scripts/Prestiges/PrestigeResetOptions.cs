using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyIdleGame
{
    public class PrestigeResetOptions
    {
        public IEnumerable<Manager> additionalExcludedManagers;
        public IEnumerable<Business> additionalExcludedBusinesses;
        public IEnumerable<Boost> additionalExcludedBoosts;
        public IEnumerable<Upgrade> additionalExcludedUpgrades;
        public IEnumerable<Location> additionalExcludedLocations;
        public IEnumerable<Achievement> additionalExcludedAchievements;
        public IEnumerable<Currency> additionalExcludedCurrencies;
        public IEnumerable<ManagerGroup> additionalExcludedManagerGroups;
        public IEnumerable<LocationGroup> additionalExcludedLocationGroups;
        public IEnumerable<BusinessGroup> additionalExcludedBusinessGroups;
        public IEnumerable<BoostGroup> additionalExcludedBoostGroups;
        public IEnumerable<UpgradeGroup> additionalExcludedUpgradeGroups;
        public IEnumerable<AchievementGroup> additionalExcludedAchievementGroups;
        public IEnumerable<CurrencyGroup> additionalExcludedCurrencyGroups;
        public bool notifySaveWorthyStateChanged = true;
    }

    public readonly struct ResetSummary
    {
        public readonly bool managersReset;
        public readonly bool businessesReset;
        public readonly bool activeBoostsCleared;
        public readonly bool boostInventoryReset;
        public readonly bool currenciesReset;
        public readonly bool levelReset;
        public readonly bool upgradesReset;
        public readonly bool locationsReset;
        public readonly bool achievementsReset;
        public readonly IReadOnlyList<Manager> preservedManagers;
        public readonly IReadOnlyList<Business> preservedBusinesses;
        public readonly IReadOnlyList<Boost> preservedBoosts;
        public readonly IReadOnlyList<Upgrade> preservedUpgrades;
        public readonly IReadOnlyList<Location> preservedLocations;
        public readonly IReadOnlyList<Achievement> preservedAchievements;
        public readonly IReadOnlyList<Currency> preservedCurrencies;

        public ResetSummary(
            bool managersReset,
            bool businessesReset,
            bool activeBoostsCleared,
            bool boostInventoryReset,
            bool currenciesReset,
            bool levelReset,
            bool upgradesReset,
            bool locationsReset,
            bool achievementsReset,
            IReadOnlyList<Manager> preservedManagers,
            IReadOnlyList<Business> preservedBusinesses,
            IReadOnlyList<Boost> preservedBoosts,
            IReadOnlyList<Upgrade> preservedUpgrades,
            IReadOnlyList<Location> preservedLocations,
            IReadOnlyList<Achievement> preservedAchievements,
            IReadOnlyList<Currency> preservedCurrencies)
        {
            this.managersReset = managersReset;
            this.businessesReset = businessesReset;
            this.activeBoostsCleared = activeBoostsCleared;
            this.boostInventoryReset = boostInventoryReset;
            this.currenciesReset = currenciesReset;
            this.levelReset = levelReset;
            this.upgradesReset = upgradesReset;
            this.locationsReset = locationsReset;
            this.achievementsReset = achievementsReset;
            this.preservedManagers = preservedManagers;
            this.preservedBusinesses = preservedBusinesses;
            this.preservedBoosts = preservedBoosts;
            this.preservedUpgrades = preservedUpgrades;
            this.preservedLocations = preservedLocations;
            this.preservedAchievements = preservedAchievements;
            this.preservedCurrencies = preservedCurrencies;
        }

        internal static List<T> CopyList<T>(IEnumerable<T> values)
        {
            return values != null ? values.Where(value => value != null).Distinct().ToList() : new List<T>();
        }

        internal static void AddDistinct<T>(List<T> destination, IEnumerable<T> values)
        {
            if (destination == null || values == null) return;

            foreach (T value in values)
            {
                if (value == null || destination.Contains(value)) continue;
                destination.Add(value);
            }
        }

        internal static List<TGroup> CombineGroups<TGroup>(IEnumerable<TGroup> configured, IEnumerable<TGroup> additional)
            where TGroup : class
        {
            List<TGroup> groups = CopyList(configured);
            AddDistinct(groups, additional);
            return groups;
        }

        internal static void AddItemsInGroups<TItem, TGroup>(
            List<TItem> destination,
            IEnumerable<TItem> items,
            Func<TItem, IEnumerable<TGroup>> groupSelector,
            IEnumerable<TGroup> excludedGroups)
            where TItem : class
            where TGroup : class
        {
            if (destination == null || items == null || groupSelector == null || excludedGroups == null) return;

            HashSet<TGroup> groups = new HashSet<TGroup>(excludedGroups.Where(group => group != null));
            if (groups.Count == 0) return;

            foreach (TItem item in items)
            {
                if (item == null || destination.Contains(item)) continue;
                IEnumerable<TGroup> itemGroups = groupSelector(item);
                if (itemGroups != null && itemGroups.Any(groups.Contains))
                    destination.Add(item);
            }
        }
    }
}
