using System;
using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// Interface for HOLDER OBJECTS that can be purchased, holder scriptable objects have to implement IBuyable
    /// </summary>
    public interface IBuyableHolder
    {
        public Action<IBuyableHolder, SourceType> OnBought { get; set; }

        /// <summary>Invoked whenever an amount is added, including purchases and free grants.</summary>
        public Action<IBuyableHolder, BigNumber, SourceType> OnAdded { get; set; }

        /// <summary> The item that is being held </summary>
        public IBuyable holdedItem { get; }

        /// <summary> Current amount of the item (e.g. items currently in inventory) </summary>
        public BigNumber Amount { get; }

        /// <summary>
        /// Total amount ever acquired (used as currentAmount in geometric cost formulas).
        /// For items that cannot be consumed/sold this equals Amount.
        /// For consumable items (e.g. boosts) this prevents cost from resetting when items are used.
        /// </summary>
        public BigNumber TotalAmount { get; }

        /// <summary>
        /// Amount gained exclusively through buying (TryBuyItems). Never incremented by free grants or outputs.
        /// Used as the currentAmount for geometric cost formulas when UseGeometricBoughtAmount is enabled.
        /// </summary>
        public BigNumber BoughtAmount { get; }

        /// <summary> sales and similar stuff </summary>
        public BigNumber ActiveCostMultiplier { get; }

        /// <summary> If the item is maxed out </summary>
        public bool Maxed => holdedItem.ExceedsBuyableAmount(TotalAmount);

        /// <summary>
        /// Returns the amount used as currentAmount in geometric cost formulas:
        /// BoughtAmount when UseGeometricBoughtAmount is enabled, otherwise TotalAmount.
        /// </summary>
        public BigNumber GetGeometricCurrentAmount() =>
            holdedItem.UseGeometricBoughtAmount ? BoughtAmount : TotalAmount;

        /// <summary> If you can buy the item - all necessary checks (cost, level, isMaxed, ...) </summary>
        public bool CanBuy(BigNumber amount);

        /// <summary> Pay for the the item </summary>
        public void Pay(BigNumber amount) => holdedItem.Pay(amount, GetGeometricCurrentAmount());

        /// <summary> Pays for the item and increments BoughtAmount. Called only by TryBuyItems. </summary>
        public void Pay_BuyEvent(BigNumber amount, SourceType sourceType = SourceType.Purchase);

        /// <returns> Real cost </returns>
        public List<Input> GetRealCostToInputs(BigNumber amountToBuy)
        {
            return holdedItem.GetRealCostToInputs(amountToBuy, GetGeometricCurrentAmount(), ActiveCostMultiplier);
        }
    }
}
