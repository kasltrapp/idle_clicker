using System;
using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// Interface for HOLDER OBJECTS that can be upgraded, holder scriptable objects have to implement IUpgradable
    /// </summary>
    public interface IUpgradableHolder
    {
        public Action<IUpgradableHolder, SourceType> OnUpgraded { get; set; }

        public IUpgradable holdedItem { get; }

        /// <summary> Total count of the item </summary>
        public BigNumber TotalCount { get; set; }

        /// <summary> Count relative to the current level </summary>
        public BigNumber RelativeCount { get; set; }

        /// <summary> Count needed to upgrade to the next level </summary>
        public BigNumber GetCountToNextLevel() => holdedItem.GetCountToNextLevel(Level);

        /// <summary> Current level of the item </summary>
        public BigNumber Level { get; set; }

        //public BigNumber GetCurrentUpgradeCostMultiplier() => GetUpgradeCostMultiplier(Level);

        //public BigNumber GetUpgradeCostMultiplier(BigNumber level) => new BigNumber(holdedItem.UpgradeCostMultiplier).Pow(level);

        public void AddCount(BigNumber amount, SourceType sourceType = SourceType.Generic)
        {
            TotalCount += amount;
            RelativeCount += amount;

            while (HasEnoughToUpgrade())
            {
                if (holdedItem.AutoUpgradeToFirstLevelFree && Level == 0)
                {
                    ForceUpgradeToNextLevel(sourceType);
                    continue;
                }

                if (holdedItem.AutoUpgradeForFree) ForceUpgradeToNextLevel(sourceType);
                else break;
            }
        }

        /// <returns> If count is sufficent, no other checks </returns>
        public bool HasEnoughToUpgrade() => RelativeCount >= GetCountToNextLevel();

        public bool CanUpgrade()
        {
            return HasEnoughToUpgrade() && CanPayUpgradeCost(Level) && (Level + 1) <= holdedItem.MaxLevel;
        }

        public bool CanUpgradeToLevel(BigNumber level)
        {
            return CanPayUpgradeCost(level) && (level + 1) <= holdedItem.MaxLevel;
        }

        public bool CanPayUpgradeCost(BigNumber currentAmount)
        {
            List<Input> cost = GetUpgradeCost(currentAmount);
            if (cost == null || cost.Count == 0) return true;

            foreach (Input input in cost)
            {
                if (input != null && !input.IsThisInputSufficent()) return false;
            }

            return true;
        }

        public List<Input> GetUpgradeCost(BigNumber currentAmount)
        {
            BigNumber costMultiplier = UpgradeEffectResolver.GetLevelUpCostMultiplier(this);
            return holdedItem.UpgradeCost.GetRealCostToInputs(holdedItem.UpgradeCostMultiplier, 1, currentAmount, costMultiplier);
        }

        public void ForceUpgradeToNextLevel(SourceType sourceType = SourceType.Generic)
        {
            RelativeCount -= GetCountToNextLevel();
            Level++;
            OnUpgraded?.Invoke(this, sourceType);
            if (this is BusinessHolder businessHolder)
            {
                BusinessesManager businessesManager = UnityEngine.Object.FindObjectOfType<BusinessesManager>();
                if (businessesManager != null) businessesManager.NotifyBusinessUpgradeChanged(businessHolder);
            }
        }

        public void ForceUpgradeToLevel(BigNumber level, SourceType sourceType = SourceType.Generic)
        {
            if (level < Level) return;
            RelativeCount -= 0;
            Level = level;
            OnUpgraded?.Invoke(this, sourceType);
            if (this is BusinessHolder businessHolder)
            {
                BusinessesManager businessesManager = UnityEngine.Object.FindObjectOfType<BusinessesManager>();
                if (businessesManager != null) businessesManager.NotifyBusinessUpgradeChanged(businessHolder);
            }
        }

        public bool TryUpgradeToLevel(BigNumber level, SourceType sourceType = SourceType.Purchase)
        {
            BigNumber currentLevel = level - 1;

            if (!CanUpgradeToLevel(currentLevel)) return false;

            GetUpgradeCost(currentLevel).RemoveInputs(1, 0, 1, 1);
            ForceUpgradeToLevel(level, sourceType);

            return true;
        }

        public bool TryUpgrade(SourceType sourceType = SourceType.Purchase)
        {
            if (!CanUpgrade()) return false;

            GetUpgradeCost(Level).RemoveInputs(1, 0, 1, 1);
            ForceUpgradeToNextLevel(sourceType);

            return true;
        }

        public UpgradeLevel GetCurrentUpgradeLevel()
        {
            return holdedItem.GetUpgradeLevel(Level);
        }
    }
}
