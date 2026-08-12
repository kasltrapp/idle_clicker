using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    [AddComponentMenu("EasyIdleGame/Managers/Shop Manager")]
    public class ShopManager : ManagerHolderBase<ShopManager, ShopItemHolder, ShopItem>,
        IBuyableHolderManager<ShopManager, ShopItemHolder, ShopItem>,
        IUnlockableHolderManager<ShopItemHolder, ShopItem>
    {
        public IBuyableHolderManager<ShopManager, ShopItemHolder, ShopItem> iBuyable => this;
        public IUnlockableHolderManager<ShopItemHolder, ShopItem> iUnlockable => this;

        public UnityEvent<ShopItem> OnItemBought { get; } = new UnityEvent<ShopItem>();
        public UnityEvent<ShopItem> OnShopItemAmountChanged = new UnityEvent<ShopItem>();
        public UnityEvent<ShopItem, BigNumber, SourceType> OnShopItemAmountAddedEvent = new UnityEvent<ShopItem, BigNumber, SourceType>();
        public UnityEvent<ShopItem> OnItemUnlocked { get; } = new UnityEvent<ShopItem>();
        public UnityEvent<ShopItemHolder> OnHolderBoughtEvent { get; } = new UnityEvent<ShopItemHolder>();
        public UnityEvent<ShopItemHolder> OnHolderUnlockedEvent { get; } = new UnityEvent<ShopItemHolder>();

        [CommentArea("Shop Manager", "Tracks shop item ownership, claims rewards, and delegates real-money or ad purchases through events.", "Place one ShopManager in the gameplay scene. For IAP, subscribe to OnAttemptRealMoneyPurchase and call CompleteExternalPurchase after the platform confirms the transaction.")]
        [SerializeField] private string _shopManagerComment;

        [Tooltip("Invoked after rewards are granted from a shop item purchase or external completion. Arguments are item, reward source, and granted outputs.")]
        public UnityEvent<ShopItem, SourceType, List<Output>> OnShopItemRewardsGranted = new UnityEvent<ShopItem, SourceType, List<Output>>();

        [Tooltip("Invoked when a user attempts to buy a shop item with real money. Subscribe to start platform IAP, then call CompleteExternalPurchase after confirmation.")]
        public UnityEvent<ShopItem> OnAttemptRealMoneyPurchase = new UnityEvent<ShopItem>();

        [Tooltip("Invoked when a user attempts to buy a shop item via ad. Subscribe to show a rewarded video, then call CompleteExternalPurchase after reward confirmation.")]
        public UnityEvent<ShopItem> OnAttemptAdPurchase = new UnityEvent<ShopItem>();

        private float _timeSinceLastCheck = 0f;
        private void Update()
        {
            _timeSinceLastCheck += Time.deltaTime;
            if (_timeSinceLastCheck >= 0.2f)
            {
                _timeSinceLastCheck = 0f;
                iUnlockable.CheckAutoUnlocks();
            }
        }

        public override ShopItemHolder GetOrAddHolder(ShopItem item)
        {
            if (item == null) throw new Exception("ShopItem is null");

            ShopItemHolder h = GetHolder(item);
            if (h == null)
            {
                holders.Add(h = new ShopItemHolder(item));
                OnHolderAdded?.Invoke(h);
            }
            return h;
        }

        public void ForceBuyItemsForFree(ShopItem item, BigNumber amount, SourceType sourceType = SourceType.Purchase)
        {
            CompleteExternalPurchase(item, amount, sourceType);
        }

        public List<Output> CompleteExternalPurchase(ShopItem item, SourceType sourceType = SourceType.Purchase)
            => CompleteExternalPurchase(item, 1, sourceType);

        public List<Output> CompleteExternalPurchase(ShopItem item, BigNumber amount, SourceType sourceType = SourceType.Purchase)
        {
            List<Output> outputs = new List<Output>();
            ShopItemHolder holder = GetOrAddHolder(item);

            if (holder.iBuyable.Maxed)
            {
                Log($"Cannot complete external purchase for {item.GetDisplayName()} from {sourceType} - max purchase limit reached.", LogCategory.Warning);
                return outputs;
            }

            BigNumber grantAmount = BigNumber.Min(amount.Floor(), item.GetMaxBuyableAmount(holder.Amount));
            if (grantAmount <= 0)
            {
                Log($"Cannot complete external purchase for {item.GetDisplayName()} from {sourceType} - requested amount {amount} must be a positive whole number.", LogCategory.Warning);
                return outputs;
            }

            holder.AddAmount(grantAmount, sourceType);

            int buyCount = (int)grantAmount.ToDouble();
            for (int i = 0; i < buyCount; i++)
            {
                outputs.AddRange(holder.ClaimRewards(sourceType));
            }

            outputs = outputs.Squash();
            OnShopItemRewardsGranted.Invoke(item, sourceType, outputs);
            OnItemBought.Invoke(item);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(ShopManager));

            return outputs;
        }

        public bool TryConsumeItem(ShopItem item, BigNumber amount, out List<Output> outputs, SourceType sourceType = SourceType.Purchase)
        {
            return GetOrAddHolder(item).TryConsumeAmount(amount, out outputs, sourceType);
        }

        public void AttemptPurchaseWithRealMoney(ShopItem item)
        {
            if (string.IsNullOrEmpty(item.productId))
            {
                Log($"ShopItem {item.GetDisplayName()} does not have a productId assigned for real money purchase.", LogCategory.Warning);
                return;
            }

            ShopItemHolder holder = GetOrAddHolder(item);
            if (holder.iBuyable.Maxed)
            {
                Log($"Cannot purchase {item.GetDisplayName()} - max purchase limit reached.");
                return;
            }

            Log($"Attempting real money purchase for {item.GetDisplayName()} (ProductID: {item.productId}). Delegating to IAP system.");
            OnAttemptRealMoneyPurchase?.Invoke(item);
        }

        public void AttemptPurchaseWithAd(ShopItem item)
        {
            if (!item.buyWithAd)
            {
                Log($"ShopItem {item.GetDisplayName()} cannot be bought with an ad.", LogCategory.Warning);
                return;
            }

            ShopItemHolder holder = GetOrAddHolder(item);
            if (holder.iBuyable.Maxed)
            {
                Log($"Cannot purchase {item.GetDisplayName()} via ad - max purchase limit reached.");
                return;
            }

            Log($"Attempting ad purchase for {item.GetDisplayName()}. Delegating to Ad system.");
            OnAttemptAdPurchase?.Invoke(item);
        }
        public void AttemptPurchase(ShopItem item)
        {
            if (item.buyWithAd)
            {
                AttemptPurchaseWithAd(item);
            }
            else if (!string.IsNullOrEmpty(item.productId))
            {
                AttemptPurchaseWithRealMoney(item);
            }
            else if (item.inGameCost != null && item.inGameCost.Count > 0)
            {
                if (iBuyable.CanBuyItems(item, 1))
                {
                    iBuyable.TryBuyItems(item, 1, SourceType.Purchase);
                }
            }
            else
            {
                iBuyable.TryBuyItems(item, 1, SourceType.Purchase);
            }
        }
    }
}
