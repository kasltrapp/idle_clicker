using System;
using UnityEngine;

namespace EasyIdleGame
{
    [Serializable]
    public class OverrideUpgradeModifierBlock : IComparable<OverrideUpgradeModifierBlock>
    {
        [Tooltip("Source upgrade filter for this modifier. Assign an upgrade to modify only that upgrade's override values; leave null to modify every matching override value.")]
        public Upgrade targetUpgrade;

        [Tooltip("Override value category this modifier changes. Must match the source override type to apply.")]
        public OverrideUpgradeType targetOverrideType;

        [Tooltip("How this modifier changes the matched override value: add a flat amount or multiply the final value.")]
        public OverrideUpgradeModifierOperation operation = OverrideUpgradeModifierOperation.Multiply;

        [Tooltip("Amount added or multiplier applied to the matched override value, depending on operation.")]
        public BigNumber value = 2;

        [Header("Additional data")]
        [Tooltip("Used with maxCurrencyAmount to restrict this modifier to one currency. Leave null to apply to all matching currencies.")]
        public Currency targetCurrency;

        [Tooltip("Used with maxBusinessAmount to restrict this modifier to one business. Leave null to apply to all matching businesses.")]
        public Business targetBusiness;

        public OverrideUpgradeModifierBlock(OverrideUpgradeType type_, OverrideUpgradeModifierOperation operation_, BigNumber value_)
        {
            targetOverrideType = type_;
            operation = operation_;
            value = value_;
        }

        public bool Matches(Upgrade sourceUpgrade, OverrideUpgradeBlock sourceBlock, OverrideUpgradeType overrideType, Currency currency = null, Business business = null)
        {
            if (targetOverrideType != overrideType) return false;
            if (targetUpgrade != null && targetUpgrade != sourceUpgrade) return false;

            Currency effectiveCurrency = currency ?? sourceBlock?.targetCurrency;
            if (targetCurrency != null && targetCurrency != effectiveCurrency) return false;

            Business effectiveBusiness = business ?? sourceBlock?.targetBusiness;
            if (targetBusiness != null && targetBusiness != effectiveBusiness) return false;

            return true;
        }

        public int CompareTo(OverrideUpgradeModifierBlock other)
        {
            if (other == null) return -1;
            if (targetUpgrade == other.targetUpgrade &&
                targetOverrideType == other.targetOverrideType &&
                operation == other.operation &&
                value == other.value &&
                targetCurrency == other.targetCurrency &&
                targetBusiness == other.targetBusiness)
            {
                return 0;
            }

            return -1;
        }

        public override string ToString()
        {
            return $"{operation} {targetOverrideType}: {value}";
        }
    }
}
