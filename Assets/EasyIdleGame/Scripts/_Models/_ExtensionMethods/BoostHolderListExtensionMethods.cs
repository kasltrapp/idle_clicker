using System.Collections.Generic;

namespace EasyIdleGame
{
    public static class BoostHolderListExtensionMethods
    {
        public static List<UpgradeBlock> GetAllUpgradeBlocks(this List<BoostHolder> holders)
        {
            List<UpgradeBlock> blocks = new List<UpgradeBlock>();
            foreach (var item in holders)
            {
                if (item.boost is not GenericMultiplierBoost gBoost) continue;
                blocks.AddRange(gBoost.upgradeBlocks);
            }
            return blocks;
        }
    }
}
