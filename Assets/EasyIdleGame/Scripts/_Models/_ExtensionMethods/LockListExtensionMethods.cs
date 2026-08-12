using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that extends Lock menthods for lists
    /// </summary>
    public static class LockListExtensionMethods
    {
        public static bool IsUnlocked(this List<Lock> locks)
        {
            if (locks == null || locks.Count == 0) return true;
            return locks.TrueForAll(lockItem => lockItem.IsUnlocked());
        }

        public static bool IsUpperUnlocked(this List<Lock> locks)
        {
            if (locks == null || locks.Count == 0) return true;
            return locks.TrueForAll(lockItem => lockItem.IsUpperUnlocked());
        }

        public static List<Lock> Multiply(this List<Lock> locks, BigNumber multiplier, bool multiplyLevel)
        {
            List<Lock> newLocks = new List<Lock>();

            foreach (Lock lockItem in locks)
            {
                if (!multiplyLevel && HasLevelLock(lockItem) && HasScalableAmountLock(lockItem))
                {
                    newLocks.Add(new Lock()
                    {
                        playerLevelLock = lockItem.playerLevelLock,
                        prestigeLock = lockItem.prestigeLock,
                        locationLock = lockItem.locationLock,
                        businessUpgradeLevelLock = lockItem.businessUpgradeLevelLock,
                        inputAmount = lockItem.inputAmount
                    }.Multiply(multiplier, multiplyLevel));

                    newLocks.Add(new Lock()
                    {
                        businessLock = lockItem.businessLock,
                        currencyLock = lockItem.currencyLock,
                        customStatLock = lockItem.customStatLock,
                        locationLock = lockItem.locationLock,
                        businessGroupLock = lockItem.businessGroupLock,
                        currencyGroupLock = lockItem.currencyGroupLock,
                        upgradeGroupLock = lockItem.upgradeGroupLock,
                        amountType = lockItem.amountType,
                        inputAmount = lockItem.inputAmount
                    }.Multiply(multiplier, multiplyLevel));

                    continue;
                }

                newLocks.Add(lockItem.Multiply(multiplier, multiplyLevel));
            }

            return newLocks;
        }

        private static bool HasLevelLock(Lock lockItem)
        {
            return lockItem.playerLevelLock || lockItem.prestigeLock || lockItem.businessUpgradeLevelLock != null;
        }

        private static bool HasScalableAmountLock(Lock lockItem)
        {
            return lockItem.businessLock != null
                || lockItem.currencyLock != null
                || lockItem.customStatLock != null
                || lockItem.businessGroupLock != null
                || lockItem.currencyGroupLock != null
                || lockItem.upgradeGroupLock != null;
        }
    }
}
