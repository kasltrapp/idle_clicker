using System;
using System.Collections.Generic;

namespace EasyIdleGame
{
    [System.Serializable]
    public class ShopItemHolder : HolderBase<ShopItem>, IBuyableHolder, IUnlockableHolder
    {
        public ShopItem shopItem => base.Item;

        public Action<IUnlockableHolder> OnUnlocked { get; set; }
        public Action<IBuyableHolder, SourceType> OnBought { get; set; }
        public Action<IBuyableHolder, BigNumber, SourceType> OnAdded { get; set; }

        // IBuyableHolder implementation
        public IBuyable holdedItem => shopItem;

        public BigNumber amount;
        public BigNumber Amount { get => amount; set => amount = value; }
        public BigNumber TotalAmount => Amount;
        public BigNumber BoughtAmount => boughtAmount;
        public BigNumber ActiveCostMultiplier => 1;

        public BigNumber boughtAmount;

        // IUnlockableHolder implementation
        public IUnlockable UnlockableItem => shopItem;

        public bool alreadyUnlocked;
        public bool AlreadyUnlocked { get => alreadyUnlocked; set => alreadyUnlocked = value; }

        public IUnlockableHolder iUnlockable => this;
        public IBuyableHolder iBuyable => this;

        public ShopItemHolder(ShopItem item) : base(item)
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

            if (ShopManager.Instance)
                ShopManager.Instance.Log($"Bought {amount} of {shopItem.GetDisplayName()}. New bought amount: {boughtAmount}", LogCategory.Verbose);
        }

        public void AddAmount(BigNumber amountToAdd, SourceType sourceType = SourceType.Purchase)
        {
            Amount += amountToAdd;
            OnAdded?.Invoke(this, amountToAdd, sourceType);

            if (ShopManager.Instance)
            {
                ShopManager.Instance.Log($"Added {amountToAdd} to {shopItem.GetDisplayName()}. New total: {Amount}", LogCategory.Verbose);
                ShopManager.Instance.OnShopItemAmountChanged?.Invoke(shopItem);
                ShopManager.Instance.OnShopItemAmountAddedEvent?.Invoke(shopItem, amountToAdd, sourceType);
            }
        }

        public List<Output> ClaimRewards(SourceType sourceType = SourceType.Purchase)
        {
            return RewardCalculator.ApplyOutputs(shopItem.rewardTables, 1, RewardCalculator.CreateContext(sourceType));
        }

        public bool TryConsumeAmount(BigNumber amountToConsume, out List<Output> outputs, SourceType sourceType = SourceType.Purchase)
        {
            if (amountToConsume <= 0)
            {
                outputs = null;
                return false;
            }

            if (Amount >= amountToConsume)
            {
                Amount -= amountToConsume;
                if (ShopManager.Instance)
                {
                    ShopManager.Instance.Log($"Consumed {amountToConsume} of {shopItem.GetDisplayName()}. New total: {Amount}", LogCategory.Verbose);
                    ShopManager.Instance.OnShopItemAmountChanged?.Invoke(shopItem);
                }
                outputs = RewardCalculator.ApplyOutputs(shopItem.rewardTables, amountToConsume, RewardCalculator.CreateContext(sourceType));
                EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(ShopManager));
                return true;
            }
            outputs = null;
            return false;
        }
    }
}
