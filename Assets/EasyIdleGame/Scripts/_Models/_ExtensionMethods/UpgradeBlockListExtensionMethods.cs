using System.Collections.Generic;

namespace EasyIdleGame
{
    public static class UpgradeBlockListExtensionMethods
    {
        public static List<UpgradeBlock> Squash(this List<UpgradeBlock> blocks_)
        {
            List<UpgradeBlock> mergedBlocks = new List<UpgradeBlock>();

            foreach (UpgradeBlock b in blocks_)
            {
                if (b == null) continue;

                if (b.IsContextual)
                {
                    mergedBlocks.Add(b.CloneWithValue(b.value));
                    continue;
                }

                UpgradeBlock foundBlock = mergedBlocks.Find(x => !x.IsContextual && x.upgradeType == b.upgradeType);

                if (foundBlock == null)
                {
                    mergedBlocks.Add(foundBlock = new UpgradeBlock(b.upgradeType, b.value));
                    continue;
                }

                foundBlock.value *= b.value;
            }

            return mergedBlocks;
        }

        public static float GetMultiplierOfType(this List<UpgradeBlock> upgradeBlocks, UpgradeType type)
        {
            float multiplier = 1;

            foreach (var upgrade in upgradeBlocks)
            {
                if (upgrade == null || upgrade.upgradeType != type || upgrade.IsContextual) continue;
                multiplier *= upgrade.value;
            }

            return multiplier;
        }

        public static List<UpgradeBlock> Multiply(this List<UpgradeBlock> upgradeBlocks, float multiplier)
        {
            List<UpgradeBlock> copy = new List<UpgradeBlock>();

            foreach (var upgrade in upgradeBlocks)
            {
                if (upgrade == null) continue;
                copy.Add(upgrade.CloneWithValue(upgrade.value * multiplier));
            }

            return copy;
        }
    }
}
