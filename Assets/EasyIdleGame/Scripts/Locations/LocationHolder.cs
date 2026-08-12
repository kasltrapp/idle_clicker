using System;

namespace EasyIdleGame
{
    [System.Serializable]
    public class LocationHolder : HolderBase<Location>, IBuyableHolder, IUnlockableHolder
    {
        public Location location => base.Item;

        public Action<IUnlockableHolder> OnUnlocked { get; set; }
        public Action<IBuyableHolder, SourceType> OnBought { get; set; }
        public Action<IBuyableHolder, BigNumber, SourceType> OnAdded { get; set; }

        // IBuyableHolder implementation
        public IBuyable holdedItem => location;

        public BigNumber amount;
        public BigNumber Amount { get => amount; private set => amount = value; }
        public BigNumber TotalAmount => Amount;
        public BigNumber BoughtAmount => boughtAmount;
        public BigNumber ActiveCostMultiplier => 1;

        public BigNumber boughtAmount;

        // IUnlockableHolder implementation
        public IUnlockable UnlockableItem => location;

        public bool alreadyUnlocked;
        public bool AlreadyUnlocked { get => alreadyUnlocked; set => alreadyUnlocked = value; }

        public IUnlockableHolder iUnlockable => this;
        public IBuyableHolder iBuyable => this;

        public LocationHolder(Location item) : base(item)
        {
            Amount = 0;
            AlreadyUnlocked = false;
        }

        public bool CanBuy(BigNumber amount)
        {
            if (!iUnlockable.IsUnlocked()) return false;
            if (iBuyable.Maxed) return false;
            return holdedItem.CanPay(amount, iBuyable.GetGeometricCurrentAmount());
        }

        public void Pay_BuyEvent(BigNumber amount, SourceType sourceType = SourceType.Purchase)
        {
            holdedItem.Pay(amount, iBuyable.GetGeometricCurrentAmount());
            boughtAmount += amount;
            OnBought?.Invoke(this, sourceType);

            if (LocationsManager.Instance)
                LocationsManager.Instance.Log($"Bought {amount} of {location.GetDisplayName()}. New bought amount: {boughtAmount}", LogCategory.Verbose);
        }

        public void AddAmount(BigNumber amountToAdd, SourceType sourceType = SourceType.Generic)
        {
            Amount += amountToAdd;
            OnAdded?.Invoke(this, amountToAdd, sourceType);

            if (LocationsManager.Instance)
            {
                LocationsManager.Instance.Log($"Added {amountToAdd} to {location.GetDisplayName()}. New total: {Amount}", LogCategory.Verbose);
                LocationsManager.Instance.OnLocationAmountChanged?.Invoke(location);
            }
        }
    }
}
