using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    public interface IBuyableHolderManager<T, E, V> : ILogger
    where T : ManagerHolderBase<T, E, V>
    where E : HolderBase<V>, IBuyableHolder
    where V : ScriptableObject, IBuyable
    {
        /// <summary>
        /// Event that is called when an item is bought - BOUGHT, not added - ForceBuyItemsForFree doesn't call this
        /// </summary>
        public UnityEvent<V> OnItemBought { get; }
        public UnityEvent<E> OnHolderBoughtEvent { get; }

        /// <returns> If items can be bought - checks everything (cost, locks, ...) </returns>
        public bool CanBuyItems(V item, BigNumber amount)
        {
            return GetOrAddHolder(item).CanBuy(amount);
        }

        /// <summary>
        /// If items can be bought - it will buy it
        /// </summary>
        /// <returns> If items were bought </returns>
        public bool TryBuyItems(V item, BigNumber amount, SourceType sourceType = SourceType.Purchase)
        {
            if (!CanBuyItems(item, amount))
            {
                Log($"TryBuyItems failed: {item.GetDisplayName()} x{amount} - cannot afford or locked", LogCategory.Warning);
                return false;
            }

            E holder = GetOrAddHolder(item);
            holder.Pay_BuyEvent(amount, sourceType);
            ForceBuyItemsForFree(item, amount, sourceType);
            OnItemBought.Invoke(item);
            OnHolderBoughtEvent?.Invoke(holder);
            Log($"TryBuyItems succeeded: {item.GetDisplayName()} x{amount}");

            return true;
        }

        public void ForceBuyItemsForFree(V item, BigNumber amount, SourceType sourceType = SourceType.Generic);

        /// <summary>
        /// Returns the Amount of the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public BigNumber GetItemAmount(V item)
        {
            return GetOrAddHolder(item).Amount;
        }

        public bool IsItemMaxed(V item)
        {
            return GetOrAddHolder(item).Maxed;
        }

        public PurchasePreview GetPurchasePreview(V item, BigNumber amount)
        {
            if (this is ManagerHolderBase<T, E, V> managerBase && managerBase.TryGetHolder(item, out E holder))
            {
                return new PurchasePreview(
                    holder.CanBuy(amount),
                    holder.GetRealCostToInputs(amount),
                    item.GetMaxBuyableAmount(holder.GetGeometricCurrentAmount()),
                    holder.Amount
                );
            }
            else
            {
                bool isUnlocked = true;
                if (item is IUnlockable unlockable) isUnlocked = unlockable.Locks.IsUnlocked();

                bool canAfford = isUnlocked && !item.ExceedsBuyableAmount(amount) && item.CanPay(amount, 0);

                return new PurchasePreview(
                    canAfford,
                    item.GetRealCostToInputs(amount, 0, 1),
                    item.GetMaxBuyableAmount(0),
                    0
                );
            }
        }

        // experimental QoL functions
        public V[] GetAllItemsByCost(Currency currencyCost = null, Business businessCost = null)
        {
            return GetAllScriptableObjects().Where(b =>
            {
                return b.Cost.Any(c =>
                    (!currencyCost || c.currencyInput == currencyCost) &&
                    (!businessCost || c.businessInput == businessCost)
                );
            }).ToArray();
        }

        // required functions
        public E GetOrAddHolder(V v);
        public V[] GetAllScriptableObjects();
    }
}
