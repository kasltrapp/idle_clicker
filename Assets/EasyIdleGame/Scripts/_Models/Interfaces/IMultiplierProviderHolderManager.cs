using System.Collections.Generic;

namespace EasyIdleGame
{
    public interface IMultiplierProviderHolderManager
    {
        /// <returns> Multiplier of 'type'</returns>
        public BigNumber GetMultiplierOfType(UpgradeType type) => GetMultiplierOfType(type, "");

        /// <returns> Multiplier of 'type' and 'group' (if group == "", it will be ignored)</returns>
        public BigNumber GetMultiplierOfType(UpgradeType type, string group);

        /// <returns> All upgrade blocks </returns>
        public List<UpgradeBlock> GetAllUpgradeBlocks();

        /// <returns> Squashed all upgrade blocks </returns>
        public List<UpgradeBlock> GetAllUpgradeBlocks_Merged() => GetAllUpgradeBlocks().Squash();
    }
}
