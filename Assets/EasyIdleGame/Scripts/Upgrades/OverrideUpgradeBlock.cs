using System;
using UnityEngine;

namespace EasyIdleGame
{
    [Serializable]
    public class OverrideUpgradeBlock : IComparable<OverrideUpgradeBlock>
    {
        [Tooltip("Absolute value category this block provides, such as a currency cap or business max amount.")]
        public OverrideUpgradeType upgradeType;

        [Tooltip("Override value created by this upgrade before matching override modifiers are applied.")]
        public BigNumber value = 2;

        [Header("Additional data")]
        [Tooltip("Used with maxCurrencyAmount to restrict this override to one currency. Leave null to apply to all matching currencies.")]
        public Currency targetCurrency;

        [Tooltip("Used with maxBusinessAmount to restrict this override to one business. Leave null to apply to all matching businesses.")]
        public Business targetBusiness;

        public OverrideUpgradeBlock(OverrideUpgradeType type_, BigNumber value_)
        {
            upgradeType = type_;
            value = value_;
        }

        public bool Matches(OverrideUpgradeType type, Currency currency = null, Business business = null)
        {
            if (upgradeType != type) return false;
            if (currency != null && targetCurrency != null && targetCurrency != currency) return false;
            if (business != null && targetBusiness != null && targetBusiness != business) return false;
            return true;
        }

        public int CompareTo(OverrideUpgradeBlock other)
        {
            if (upgradeType == other.upgradeType && value == other.value) return 0;
            else return -1;
        }

        public override string ToString()
        {
            return upgradeType + ": " + value;
        }
    }
}
