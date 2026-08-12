using System;
using UnityEngine;

namespace EasyIdleGame
{
    [Serializable]
    public class UpgradeBlock : IComparable<UpgradeBlock>
    {
        [Tooltip("Stat this block multiplies, such as production, speed, purchase cost, or player XP.")]
        public UpgradeType upgradeType;

        [Tooltip("Multiplier applied to the selected stat. Use values above 1 to increase, and values below 1 for reductions such as 0.8 purchase cost.")]
        public float value = 2;

        [Tooltip("How value is applied in contextual economy queries. Standard global multipliers use Multiply.")]
        public UpgradeBlockOperation operation = UpgradeBlockOperation.Multiply;

        [Tooltip("Optional source, target, level, and runtime restrictions for this effect. Leave every filter unset for a normal global UpgradeBlock.")]
        public UpgradeBlockFilter filters = new UpgradeBlockFilter();

        public bool IsContextual =>
            operation != UpgradeBlockOperation.Multiply ||
            IsContextualUpgradeType(upgradeType) ||
            (filters != null && filters.HasAnyFilter);

        public UpgradeBlock()
        {
        }

        public UpgradeBlock(UpgradeType type_, float value_)
        {
            upgradeType = type_;
            value = value_;
        }

        public bool Matches(UpgradeEffectContext context)
        {
            if (context == null) return false;
            if (upgradeType != context.UpgradeType) return false;
            return filters == null || filters.Matches(context);
        }

        public UpgradeBlock CloneWithValue(float clonedValue)
        {
            UpgradeBlock clone = (UpgradeBlock)MemberwiseClone();
            clone.value = clonedValue;
            clone.filters = filters != null ? filters.Clone() : new UpgradeBlockFilter();
            return clone;
        }

        public int CompareTo(UpgradeBlock other)
        {
            if (other == null) return -1;
            if (upgradeType == other.upgradeType && value == other.value) return 0;
            return -1;
        }

        public override string ToString()
        {
            return upgradeType + ": " + value;
        }

        private static bool IsContextualUpgradeType(UpgradeType type)
        {
            return type == UpgradeType.duration ||
                   type == UpgradeType.dropChance ||
                   type == UpgradeType.dropWeight ||
                   type == UpgradeType.productionOutputScaling ||
                   type == UpgradeType.cooldown ||
                   type == UpgradeType.productionInputCost ||
                   type == UpgradeType.upgradePurchaseCost ||
                   type == UpgradeType.levelUpCost ||
                   type == UpgradeType.mergeInputCost;
        }
    }
}
