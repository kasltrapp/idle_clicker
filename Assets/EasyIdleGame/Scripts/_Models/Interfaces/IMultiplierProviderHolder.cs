namespace EasyIdleGame
{
    public interface IMultiplierProviderHolder
    {
        public IMultiplierProvider holdedItem { get; }

        // inherited
        public BigNumber Amount { get; }

        /// <returns> Multiplier transformed with all runtime values, such as amount </returns>
        [System.Obsolete("Use GetMultiplierOfType(UpgradeType type, List<BusinessGroup> businessGroupList = null, string group = \"\") instead.")]
        public BigNumber GetMultiplierOfType(UpgradeType type, string group)
        {
            BigNumber q = holdedItem.GetMultiplierOfType(type, group);

            return Mathb.GeometricSequence_NthTerm(q, q, Amount);
        }

        /// <returns> Multiplier transformed with all runtime values, such as amount </returns>
        public BigNumber GetMultiplierOfType(UpgradeType type, System.Collections.Generic.List<BusinessGroup> businessGroupList = null, string group = "")
        {
            BigNumber q = holdedItem.GetMultiplierOfType(type, businessGroupList, group);

            return Mathb.GeometricSequence_NthTerm(q, q, Amount);
        }
    }
}