using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// UI-agnostic text formatting helpers for economy values.
    /// </summary>
    public static class EconomyTextFormatter
    {
        public static string ColorfulText(string text, bool green) => $"<color={(green ? "green" : "red")}>{text}</color>";

        public static string InputsToString(IEnumerable<Input> inputs, bool colorful = true)
        {
            List<string> costs = new();
            if (inputs == null) return string.Empty;

            foreach (Input input in inputs)
            {
                if (input == null) continue;
                if (input.businessInput != null) costs.Add(BusinessInputToString(input, colorful));
                if (input.currencyInput != null) costs.Add(CurrencyInputToString(input, colorful));
            }

            return string.Join(" ", costs);
        }

        public static string LocksToString(IEnumerable<Lock> locks, bool colorful = true)
        {
            List<string> lockStrings = new();
            if (locks == null) return string.Empty;

            foreach (Lock input in locks)
            {
                if (input == null) continue;
                if (input.businessUpgradeLevelLock != null) lockStrings.Add(BusinessUpgradeLevelLockToString(input, colorful));
                if (input.businessLock != null) lockStrings.Add(BusinessLockToString(input, colorful));
                if (input.currencyLock != null) lockStrings.Add(CurrencyLockToString(input, colorful));
                if (input.playerLevelLock) lockStrings.Add(LevelRequirementToString(input, colorful));
            }

            return string.Join(" ", lockStrings);
        }

        public static string UpgradeBlocksToString(IEnumerable<UpgradeBlock> upgrades)
        {
            List<string> upgradeStrings = new();
            if (upgrades == null) return string.Empty;

            foreach (UpgradeBlock upgrade in upgrades)
            {
                if (upgrade == null) continue;
                upgradeStrings.Add(UpgradeBlockToString(upgrade));
            }

            return string.Join(" ", upgradeStrings);
        }

        public static string UpgradeBlockToString(UpgradeBlock upgrade)
        {
            return $"{upgrade.upgradeType}: {upgrade.value:F2}x ";
        }

        public static string BusinessInputToString(Input input, bool colorful = true)
        {
            string text = $"x{input.inputAmount}{input.businessInput.GetIconString()}";
            if (!colorful) return text;

            try
            {
                return ColorfulText(text, input.IsThisInputSufficent_Business());
            }
            catch
            {
                return text;
            }
        }

        public static string CurrencyInputToString(Input input, bool colorful = true)
        {
            string text = $"x{input.inputAmount}{input.currencyInput.GetIconString()}";
            if (!colorful) return text;

            try
            {
                return ColorfulText(text, input.IsThisInputSufficent_Currency());
            }
            catch
            {
                return text;
            }
        }

        public static string BusinessLockToString(Lock lockData, bool colorful = true)
        {
            string text = $"x{lockData.inputAmount}{lockData.businessLock.GetIconString()}";
            if (!colorful) return text;

            try
            {
                return ColorfulText(text, lockData.IsBusinessSufficent());
            }
            catch
            {
                return text;
            }
        }

        public static string BusinessUpgradeLevelLockToString(Lock lockData, bool colorful = true)
        {
            string icon = lockData.businessUpgradeLevelLock.GetIconString();
            BigNumber currentLevel = 0;

            try
            {
                if (BusinessesManager.Instance != null)
                    currentLevel = BusinessesManager.Instance.GetOrAddHolder(lockData.businessUpgradeLevelLock).iUpgradable.Level;
            }
            catch { }

            string text = $"{icon} lvl {currentLevel}/{lockData.inputAmount}";
            if (!colorful) return text;

            try
            {
                return ColorfulText(text, lockData.IsBusinessUpgradeLevelSufficent());
            }
            catch
            {
                return text;
            }
        }

        public static string CurrencyLockToString(Lock lockData, bool colorful = true)
        {
            string text = $"x{lockData.inputAmount}{lockData.currencyLock.GetIconString()}";
            if (!colorful) return text;

            try
            {
                return ColorfulText(text, lockData.IsCurrencySufficent());
            }
            catch
            {
                return text;
            }
        }

        public static string LevelRequirementToString(Lock lockData, bool colorful = true)
        {
            int playerLevel = PlayerStats.Instance ? PlayerStats.Instance.GetPlayerLevel() : 0;
            string text = $"lvl {playerLevel}/{lockData.inputAmount}";
            if (!colorful) return text;

            try
            {
                return ColorfulText(text, lockData.IsLevelSufficent());
            }
            catch
            {
                return text;
            }
        }
    }
}
