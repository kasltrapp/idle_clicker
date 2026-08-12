using System;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that holds boost in inventory
    /// </summary>
    [Serializable]
    public class BoostHolder : HolderBase<Boost>, IUnlockableHolder, IBuyableHolder
    {
        public IUnlockableHolder iUnlockable => this;
        public IBuyableHolder iBuyable => this;

        public Action<IUnlockableHolder> OnUnlocked { get; set; }
        public Action<IBuyableHolder, SourceType> OnBought { get; set; }
        public Action<IBuyableHolder, BigNumber, SourceType> OnAdded { get; set; }

        // IUnlockableHolder
        public IUnlockable UnlockableItem => Item;
        public bool AlreadyUnlocked { get => alreadyUnlocked; set => alreadyUnlocked = value; }

        //IBuyableHolder
        public IBuyable holdedItem => Item;
        public BigNumber Amount { get => amount; set => throw new Exception("You cannot set amount through here"); }
        public BigNumber TotalAmount { get => totalAmount; }
        public BigNumber BoughtAmount => boughtAmount;
        public BigNumber ActiveCostMultiplier => UpgradesManager.GetMultiplierOfTypeStatic(UpgradeType.purchaseCost) * UpgradeEffectResolver.GetPurchaseCostMultiplier(boost);

        public Boost boost
        {
            get => Item;
            set => Item = value;
        }

        public bool alreadyUnlocked;

        public BigNumber amount;
        public BigNumber totalAmount;
        public BigNumber boughtAmount;

        public BoostHolder(Boost item) : base(item) { }

        public override string ToString()
        {
            return $"{boost.GetDisplayName()}: {amount}";
        }

        public bool CanBuy(BigNumber amount)
        {
            return boost.Cost.IsSufficent(amount, iBuyable.GetGeometricCurrentAmount(), boost.CostMultiplier, ActiveCostMultiplier) && iUnlockable.IsUnlocked();
        }

        public void Pay_BuyEvent(BigNumber amount, SourceType sourceType = SourceType.Purchase)
        {
            boost.Cost.RemoveInputs(amount, iBuyable.GetGeometricCurrentAmount(), boost.CostMultiplier, ActiveCostMultiplier);
            boughtAmount += amount;
            OnBought?.Invoke(this, sourceType);

            if (BoostsManager.Instance)
                BoostsManager.Instance.Log($"Bought {amount} of {boost.GetDisplayName()}. New bought amount: {boughtAmount}", LogCategory.Verbose);
        }

        public void AddAmount(BigNumber amount, SourceType sourceType = SourceType.Generic)
        {
            this.amount += amount;
            this.totalAmount += amount;
            OnAdded?.Invoke(this, amount, sourceType);

            if (PlayerStats.Instance)
                PlayerStats.Instance.OnBoostAmountAdded(boost, amount);

            if (BoostsManager.Instance)
            {
                BoostsManager.Instance.Log($"Added {amount} to {boost.GetDisplayName()}. New total: {totalAmount}", LogCategory.Verbose);
                BoostsManager.Instance.OnBoostAmountChanged?.Invoke(boost);
                BoostsManager.Instance.OnBoostAmountAddedEvent?.Invoke(boost, amount, sourceType);
                EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BoostsManager));
            }
        }
    }
}
