using System;

namespace EasyIdleGame
{
    [Serializable]
    public class PendingUpgrade
    {
        public BigNumber amount;
        public long _upgradeEndTimecode;
        public DateTime upgradeEndTime
        {
            get => DateTime.FromFileTime(_upgradeEndTimecode);
            set => _upgradeEndTimecode = value.ToFileTime();
        }

        public PendingUpgrade(BigNumber amount, DateTime upgradeEndTime)
        {
            this.amount = amount;
            this.upgradeEndTime = upgradeEndTime;
        }
    }
}
