using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// Represents a read-only preview of a purchase operation for UI rendering, 
    /// avoiding actual runtime state mutation or holder creation.
    /// </summary>
    public struct PurchasePreview
    {
        public bool canAfford;
        public List<Input> effectiveCost;
        public BigNumber maxAffordableAmount;
        public BigNumber currentAmount;

        public PurchasePreview(bool canAfford, List<Input> effectiveCost, BigNumber maxAffordableAmount, BigNumber currentAmount)
        {
            this.canAfford = canAfford;
            this.effectiveCost = effectiveCost;
            this.maxAffordableAmount = maxAffordableAmount;
            this.currentAmount = currentAmount;
        }
    }
}
