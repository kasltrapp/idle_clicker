using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// Interface for SCRIPTABLE OBJECTS that can be upgraded
    /// </summary>
    public interface IUpgradable
    {
        public List<Input> UpgradeCost { get; }

        /// <summary> How much the cost of upgrade is multiplied on each level </summary>
        public float UpgradeCostMultiplier { get; }

        public BigNumber MaxLevel { get; }

        /// <summary> If the item should be upgraded for free whenever possible </summary>
        public bool AutoUpgradeForFree { get; }

        /// <summary> If the first level should be upgraded for free even when the 'AutoUpgradeForFreeIsDisabled' </summary>
        public bool AutoUpgradeToFirstLevelFree { get; }

        public UpgradeLevel[] UpgradeLevels { get; }

        public int UpgradeLevelCopiesCountMultiplier => 2;

        /// <returns> If upgrade costs requirements are met, no other checks </returns>
        public bool CanUpgrade(BigNumber currentAmount)
        {
            return UpgradeCost.IsSufficent(1, currentAmount, UpgradeCostMultiplier, 1) && (currentAmount + 1) <= MaxLevel;
        }

        /// <returns> Pay for the upgrade, no any other actions </returns>
        public void PayUpgrade(BigNumber currentAmount) => UpgradeCost.RemoveInputs(1, currentAmount, UpgradeCostMultiplier, 1);

        public UpgradeLevel GetUpgradeLevel(BigNumber amountLevel, bool exactOnly = false)
        {
            UpgradeLevel level = null;
            BigNumber highestLevel = BigNumber.MinValue;

            foreach (var upgradeLevel in UpgradeLevels)
            {
                bool valid = exactOnly ? upgradeLevel.level == amountLevel : upgradeLevel.level <= amountLevel;

                if (upgradeLevel.level > highestLevel && valid)
                {
                    highestLevel = upgradeLevel.level;
                    level = upgradeLevel;
                }
            }

            return level;
        }

        public BigNumber GetCountToNextLevel(BigNumber currentLevel)
        {
            return Mathb.GeometricSequence_NthTerm(1, UpgradeLevelCopiesCountMultiplier, currentLevel + 1).Round();
        }
    }
}