using System.Collections.Generic;

namespace EasyIdleGame.UI
{
    public static class UpgradeHolderListExtensionMethods
    {
        public static List<UpgradeBlock> MergeUpgradeBlocks(this List<UpgradeHolder> upgrades)
        {
            return GetAllUpgradeBlocks(upgrades).Squash();
        }

        public static List<UpgradeBlock> GetAllUpgradeBlocks(this List<UpgradeHolder> upgrades)
        {
            List<UpgradeBlock> allBlocks = new List<UpgradeBlock>();

            foreach (UpgradeHolder upgradeHolder in upgrades)
            {
                if (upgradeHolder.amount == 0) continue;

                foreach (UpgradeBlock b in upgradeHolder.upgrade.upgrades)
                {
                    if (b == null || b.IsContextual) continue;
                    allBlocks.Add(new UpgradeBlock(b.upgradeType, Mathb.GeometricSequence_NthTerm_Float(b.value, b.value, upgradeHolder.amount.ToInt())));
                }
            }

            return allBlocks;
        }
    }
}
