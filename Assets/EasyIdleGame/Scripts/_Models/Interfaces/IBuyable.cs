using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// Interface for SCRIPTABLE OBJECTS that can be bought
    /// </summary>
    public interface IBuyable
    {
        /// <summary> Initial cost of the item - this will not reflect any multipliers </summary>
        public List<Input> Cost { get; }

        /// <summary> Cost quotient </summary>
        public BigNumber CostMultiplier { get; }

        /// <summary> -1 means unlimited </summary>
        public BigNumber MaxBuyableAmount { get; }

        /// <summary>
        /// When true, geometric cost calculations use the holder's BoughtAmount (items gained exclusively
        /// through purchase) instead of TotalAmount (all acquisitions including free grants).
        /// Useful for consumables or items that can be awarded for free, so free grants do not inflate the purchase price.
        /// </summary>
        public bool UseGeometricBoughtAmount { get; }

        public AudioData BuySound { get; }

        /// <returns> If you can pay for the item, no any other checks (such as if is unlocked) </returns>
        public bool CanPay(BigNumber amountToBuy, BigNumber currentAmount) => Cost.IsSufficent(amountToBuy, currentAmount, CostMultiplier, 1);

        /// <returns> Pay for the item, no any other actions (such as xps handling) </returns>
        public void Pay(BigNumber amount, BigNumber currentAmount) => Cost.RemoveInputs(amount, currentAmount, CostMultiplier, 1);

        /// <returns> If 'amount' exceeds max buyable amount </returns>
        public bool ExceedsBuyableAmount(BigNumber amount) => MaxBuyableAmount >= 0 && MaxBuyableAmount <= amount;

        /// <returns> Max amount that can be currently bought - including all (local & global) multipliers but exlucing constraints such as MaxAmount or MaxBuyableAmount </returns>
        public BigNumber GetMaxBuyableAmount(BigNumber currentAmount);

        /// <returns> Real cost based on current amount and multiplier </returns>
        public List<Input> GetRealCostToInputs(BigNumber amountToBuy, BigNumber currentAmount, BigNumber priceMultiplier)
        {
            return Cost.GetRealCostToInputs(CostMultiplier, amountToBuy, currentAmount, priceMultiplier);
        }
    }
}
