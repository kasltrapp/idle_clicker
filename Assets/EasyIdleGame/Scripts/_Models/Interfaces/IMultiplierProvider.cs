namespace EasyIdleGame
{
    /// <summary>
    /// Interface for SCRIPTABLE OBJECTS that provide multipliers
    /// </summary>
    public interface IMultiplierProvider
    {
        public UpgradeBlock[] Blocks { get; }

        [System.Obsolete("Use TargetBusinessGroups instead.")]
        public string Group { get => ""; }

        /// <summary> Business groups this upgrade applies to. Null/empty means 'all of them' </summary>
        public System.Collections.Generic.List<BusinessGroup> TargetBusinessGroups { get => null; }

        /// <returns> Total multiplier of the 'type' and 'group' </returns>
        [System.Obsolete("Use GetMultiplierOfType(UpgradeType type, List<BusinessGroup> businessGroupList = null, string group = \"\") instead.")]
        public float GetMultiplierOfType(UpgradeType type, string group)
        {
            return GetMultiplierOfType(type, null, group);
        }

        /// <returns> Total multiplier of the 'type' for the specified groups </returns>
        public float GetMultiplierOfType(UpgradeType type, System.Collections.Generic.List<BusinessGroup> businessGroupList = null, string group = "")
        {
            if (Business.HasGroupMatch(businessGroupList, group, TargetBusinessGroups, Group))
                return GetMultiplierOfType_NoGroup(type);

            return 1;
        }

        /// <returns> Total non-contextual multiplier of the 'type' but completely IGNORES group </returns>
        public float GetMultiplierOfType_NoGroup(UpgradeType type)
        {
            float multiplier = 1;

            for (int i = 0; i < Blocks.Length; i++)
            {
                if (Blocks[i].upgradeType != type) continue;
                if (Blocks[i].IsContextual) continue;
                multiplier *= Blocks[i].value;
            }

            return multiplier;
        }

        /// <returns> Whether there item has a non-contextual upgrade of type </returns>
        public bool HasUpgradeOfType(UpgradeType type)
        {
            for (int i = 0; i < Blocks.Length; i++)
            {
                if (Blocks[i].upgradeType == type && !Blocks[i].IsContextual) return true;
            }
            return false;
        }
    }

}
