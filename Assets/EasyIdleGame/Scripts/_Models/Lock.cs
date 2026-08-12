using System;
using UnityEngine;

namespace EasyIdleGame
{
    public enum LockAmountType
    {
        CurrentAmount,
        TotalAmount,
        BoughtAmount,
        MaxAmount
    }

    /// <summary>
    /// Class that stores lock data
    /// </summary>
    [System.Serializable]
    public class Lock
    {
        [Tooltip("Business requirement for this lock. Leave null if this lock should not check business ownership; when assigned, inputAmount is compared using amountType.")]
        public Business businessLock;

        [Tooltip("Business upgrade level requirement for this lock. Leave null if this lock should not check a business upgrade level; when assigned, inputAmount is compared with that business holder's current upgrade level.")]
        public Business businessUpgradeLevelLock;

        [Tooltip("Currency requirement for this lock. Leave null if this lock should not check a currency balance; when assigned, inputAmount is compared using amountType.")]
        public Currency currencyLock;

        [Tooltip("Player level requirement for this lock. Enable to require PlayerStats level >= inputAmount; leave false when level should not affect the unlock.")]
        public bool playerLevelLock;

        [Tooltip("Custom stat requirement for this lock. Leave null if this lock should not check custom stat progress; when assigned, the stat must be >= inputAmount.")]
        public CustomStat customStatLock;

        [Tooltip("Prestige requirement for this lock. Enable to require prestige count >= inputAmount; leave false when prestige should not affect the unlock.")]
        public bool prestigeLock;

        [Tooltip("Location requirement for this lock. Leave null if any location is valid; when assigned, the active location must be this exact asset.")]
        public Location locationLock;

        [Tooltip("Business group requirement for this lock. Leave null if this lock should not check a business group; when assigned, the sum of amounts of all businesses in the group must meet inputAmount.")]
        public BusinessGroup businessGroupLock;

        [Tooltip("Currency group requirement for this lock. Leave null if this lock should not check a currency group; when assigned, the sum of amounts of all currencies in the group must meet inputAmount.")]
        public CurrencyGroup currencyGroupLock;

        [Tooltip("Upgrade group requirement for this lock. Leave null if this lock should not check an upgrade group; when assigned, the sum of amounts of all upgrades in the group must meet inputAmount.")]
        public UpgradeGroup upgradeGroupLock;

        [Tooltip("Amount source used by business and currency requirements. CurrentAmount checks the current balance, TotalAmount checks lifetime totals where available, and BoughtAmount checks purchased count for businesses.")]
        public LockAmountType amountType = LockAmountType.CurrentAmount;

        [Tooltip("Threshold required by the enabled requirements. Business, business upgrade level, currency, player level, custom stat, and prestige checks use this amount; location checks ignore it.")]
        public BigNumber inputAmount = 1;

        private static T RequireManager<T>(T manager, string requirementName) where T : UnityEngine.Object
        {
            if (manager == null)
            {
                throw new InvalidOperationException(
                    $"Cannot evaluate the '{requirementName}' lock requirement because {typeof(T).Name} is missing from the active scene.");
            }

            return manager;
        }

        private void ValidateRequiredManagers()
        {
            if (businessLock != null)
                RequireManager(BusinessesManager.Instance, nameof(businessLock));
            if (businessUpgradeLevelLock != null)
                RequireManager(BusinessesManager.Instance, nameof(businessUpgradeLevelLock));
            if (currencyLock != null)
                RequireManager(CurrencyManager.Instance, nameof(currencyLock));
            if (playerLevelLock)
                RequireManager(PlayerStats.Instance, nameof(playerLevelLock));
            if (customStatLock != null)
                RequireManager(PlayerStats.Instance, nameof(customStatLock));
            if (prestigeLock)
                RequireManager(PrestigeManager.Instance, nameof(prestigeLock));
            if (locationLock != null)
                RequireManager(LocationsManager.Instance, nameof(locationLock));
            if (businessGroupLock != null)
                RequireManager(BusinessesManager.Instance, nameof(businessGroupLock));
            if (currencyGroupLock != null)
                RequireManager(CurrencyManager.Instance, nameof(currencyGroupLock));
            if (upgradeGroupLock != null)
                RequireManager(UpgradesManager.Instance, nameof(upgradeGroupLock));
        }

        public bool IsBusinessSufficent()
        {
            var manager = RequireManager(BusinessesManager.Instance, nameof(businessLock));
            var holder = manager.GetOrAddHolder(businessLock);
            return amountType switch
            {
                LockAmountType.TotalAmount => holder.TotalAmount >= inputAmount,
                LockAmountType.BoughtAmount => holder.BoughtAmount >= inputAmount,
                LockAmountType.MaxAmount => holder.GetMaxAmountConstrain() >= inputAmount,
                _ => holder.Amount >= inputAmount,
            };
        }

        public bool IsBusinessUpgradeLevelSufficent()
        {
            if (businessUpgradeLevelLock == null) return false;
            var manager = RequireManager(BusinessesManager.Instance, nameof(businessUpgradeLevelLock));
            return manager.GetOrAddHolder(businessUpgradeLevelLock).iUpgradable.Level >= inputAmount;
        }

        public bool IsCurrencySufficent()
        {
            var manager = RequireManager(CurrencyManager.Instance, nameof(currencyLock));
            var holder = manager.GetOrAddHolder(currencyLock);
            return amountType switch
            {
                LockAmountType.TotalAmount => holder.TotalAmount >= inputAmount,
                LockAmountType.BoughtAmount => holder.amount >= inputAmount, // Currencies don't track bought amount independently
                LockAmountType.MaxAmount => holder.GetMaxCurrencyConstrain(currencyLock) >= inputAmount,
                _ => holder.amount >= inputAmount,
            };
        }

        public bool IsLevelSufficent() => RequireManager(PlayerStats.Instance, nameof(playerLevelLock)).GetPlayerLevel() >= inputAmount;
        public bool IsCustomStatSufficent() => customStatLock == null || RequireManager(PlayerStats.Instance, nameof(customStatLock)).GetStat(customStatLock) >= inputAmount;
        public bool IsPrestigeSufficent() => RequireManager(PrestigeManager.Instance, nameof(prestigeLock)).prestiges.prestigeCount >= inputAmount;
        public bool IsLocationSufficent() => RequireManager(LocationsManager.Instance, nameof(locationLock)).ActiveLocation == locationLock;

        public bool IsBusinessGroupSufficent()
        {
            var manager = RequireManager(BusinessesManager.Instance, nameof(businessGroupLock));
            return manager.GetBusinessGroupAmount(businessGroupLock, amountType) >= inputAmount;
        }

        public bool IsCurrencyGroupSufficent()
        {
            var manager = RequireManager(CurrencyManager.Instance, nameof(currencyGroupLock));
            return manager.GetCurrencyGroupAmount(currencyGroupLock, amountType) >= inputAmount;
        }

        public bool IsUpgradeGroupSufficent()
        {
            var manager = RequireManager(UpgradesManager.Instance, nameof(upgradeGroupLock));
            return manager.GetUpgradeGroupAmount(upgradeGroupLock, amountType) >= inputAmount;
        }

        public bool IsUnlocked()
        {
            ValidateRequiredManagers();

            return (businessLock == null || IsBusinessSufficent()) &&
                (businessUpgradeLevelLock == null || IsBusinessUpgradeLevelSufficent()) &&
                (currencyLock == null || IsCurrencySufficent()) &&
                (!playerLevelLock || IsLevelSufficent()) &&
                (customStatLock == null || IsCustomStatSufficent()) &&
                (!prestigeLock || IsPrestigeSufficent()) &&
                (locationLock == null || IsLocationSufficent()) &&
                (businessGroupLock == null || IsBusinessGroupSufficent()) &&
                (currencyGroupLock == null || IsCurrencyGroupSufficent()) &&
                (upgradeGroupLock == null || IsUpgradeGroupSufficent());
        }

        public bool IsUpperBusinessSufficent()
        {
            var manager = RequireManager(BusinessesManager.Instance, nameof(businessLock));
            var holder = manager.GetOrAddHolder(businessLock);
            return amountType switch
            {
                LockAmountType.TotalAmount => holder.TotalAmount <= inputAmount,
                LockAmountType.BoughtAmount => holder.BoughtAmount <= inputAmount,
                LockAmountType.MaxAmount => holder.GetMaxAmountConstrain() <= inputAmount,
                _ => holder.Amount <= inputAmount,
            };
        }

        public bool IsUpperBusinessUpgradeLevelSufficent()
        {
            if (businessUpgradeLevelLock == null) return false;
            var manager = RequireManager(BusinessesManager.Instance, nameof(businessUpgradeLevelLock));
            return manager.GetOrAddHolder(businessUpgradeLevelLock).iUpgradable.Level <= inputAmount;
        }

        public bool IsUpperCurrencySufficent()
        {
            var manager = RequireManager(CurrencyManager.Instance, nameof(currencyLock));
            var holder = manager.GetOrAddHolder(currencyLock);
            return amountType switch
            {
                LockAmountType.TotalAmount => holder.TotalAmount <= inputAmount,
                LockAmountType.BoughtAmount => holder.amount <= inputAmount,
                LockAmountType.MaxAmount => holder.GetMaxCurrencyConstrain(currencyLock) <= inputAmount,
                _ => holder.amount <= inputAmount,
            };
        }

        public bool IsUpperLevelSufficent() => RequireManager(PlayerStats.Instance, nameof(playerLevelLock)).GetPlayerLevel() <= inputAmount;
        public bool IsUpperCustomStatSufficent() => customStatLock == null || RequireManager(PlayerStats.Instance, nameof(customStatLock)).GetStat(customStatLock) <= inputAmount;
        public bool IsUpperPrestigeSufficent() => RequireManager(PrestigeManager.Instance, nameof(prestigeLock)).prestiges.prestigeCount <= inputAmount;

        public bool IsUpperBusinessGroupSufficent()
        {
            var manager = RequireManager(BusinessesManager.Instance, nameof(businessGroupLock));
            return manager.GetBusinessGroupAmount(businessGroupLock, amountType) <= inputAmount;
        }

        public bool IsUpperCurrencyGroupSufficent()
        {
            var manager = RequireManager(CurrencyManager.Instance, nameof(currencyGroupLock));
            return manager.GetCurrencyGroupAmount(currencyGroupLock, amountType) <= inputAmount;
        }

        public bool IsUpperUpgradeGroupSufficent()
        {
            var manager = RequireManager(UpgradesManager.Instance, nameof(upgradeGroupLock));
            return manager.GetUpgradeGroupAmount(upgradeGroupLock, amountType) <= inputAmount;
        }

        public bool IsUpperUnlocked()
        {
            ValidateRequiredManagers();

            return (businessLock == null || IsUpperBusinessSufficent()) &&
                (businessUpgradeLevelLock == null || IsUpperBusinessUpgradeLevelSufficent()) &&
                (currencyLock == null || IsUpperCurrencySufficent()) &&
                (!playerLevelLock || IsUpperLevelSufficent()) &&
                (customStatLock == null || IsUpperCustomStatSufficent()) &&
                (!prestigeLock || IsUpperPrestigeSufficent()) &&
                (locationLock == null || !IsLocationSufficent()) &&
                (businessGroupLock == null || IsUpperBusinessGroupSufficent()) &&
                (currencyGroupLock == null || IsUpperCurrencyGroupSufficent()) &&
                (upgradeGroupLock == null || IsUpperUpgradeGroupSufficent());
        }

        public Lock Multiply(BigNumber multiplier, bool multiplyLevel)
        {
            BigNumber newInputA = inputAmount * multiplier;

            if (!multiplyLevel && (playerLevelLock || prestigeLock || businessUpgradeLevelLock != null))
                newInputA = inputAmount;

            return new()
            {
                businessLock = businessLock,
                businessUpgradeLevelLock = businessUpgradeLevelLock,
                currencyLock = currencyLock,
                playerLevelLock = playerLevelLock,
                customStatLock = customStatLock,
                prestigeLock = prestigeLock,
                locationLock = locationLock,
                businessGroupLock = businessGroupLock,
                currencyGroupLock = currencyGroupLock,
                upgradeGroupLock = upgradeGroupLock,
                amountType = amountType,
                inputAmount = newInputA
            };
        }

        public override string ToString()
        {
            string str = "";

            if (businessLock != null)
                str += $"Business: {businessLock.GetDisplayName()} x{inputAmount} ({amountType})\n";

            if (businessUpgradeLevelLock != null)
                str += $"Business upgrade level: {businessUpgradeLevelLock.GetDisplayName()} level {inputAmount}\n";

            if (currencyLock != null)
                str += $"Currency: {currencyLock.GetIconString()} x{inputAmount} ({amountType})\n";

            if (playerLevelLock)
                str += $"Player level: {inputAmount}\n";

            if (customStatLock != null)
                str += $"Stat '{customStatLock.GetDisplayName()}': {inputAmount}\n";

            if (prestigeLock)
                str += $"Prestige: {inputAmount}\n";

            if (locationLock != null)
                str += $"Location: {locationLock.GetDisplayName()}\n";

            if (businessGroupLock != null)
                str += $"Business Group: {businessGroupLock.name} x{inputAmount} ({amountType})\n";

            if (currencyGroupLock != null)
                str += $"Currency Group: {currencyGroupLock.name} x{inputAmount} ({amountType})\n";

            if (upgradeGroupLock != null)
                str += $"Upgrade Group: {upgradeGroupLock.name} x{inputAmount} ({amountType})\n";

            return str;
        }
    }
}
