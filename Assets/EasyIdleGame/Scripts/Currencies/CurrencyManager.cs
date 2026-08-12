using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    [AddComponentMenu("EasyIdleGame/Managers/Currencies Manager")]
    public class CurrencyManager : ManagerHolderBase<CurrencyManager, CurrencyHolder, Currency>
    {
        public CurrencyManager() { _necessary = true; }

        [CommentArea("Currencies Manager", "Tracks runtime currency balances and shared currency group capacity. Other systems use this manager when they add rewards, pay costs, or check whether the player can buy something.", "Place one CurrencyManager in the gameplay scene. Add CurrencyDisplayers to show balances, and use Output currencyOutput entries to award currency from businesses, rewards, or shop items.")]
        [SerializeField] private string _currencyManagerComment;

        [Tooltip("Invoked when a currency balance changes from rewards, costs, resets, or direct manager calls.")]
        public UnityEvent<Currency> OnCurrencyAmountChanged = new UnityEvent<Currency>();

        public UnityEvent<Currency, BigNumber> OnCurrencyAmountAddedEvent = new UnityEvent<Currency, BigNumber>();
        public UnityEvent<Currency, BigNumber, SourceType> OnCurrencyAmountAddedWithSourceEvent = new UnityEvent<Currency, BigNumber, SourceType>();

        public void AddCurrencyAmount(Currency currency, BigNumber amount, SourceType sourceType = SourceType.Generic) => GetOrAddHolder(currency).AddAmount(amount, sourceType);

        public BigNumber GetMaxCurrencyConstrain(Currency currency) => GetOrAddHolder(currency).GetMaxCurrencyConstrain(currency);

        public void RemoveCurrencyAmount(Currency currency, BigNumber amount)
        {
            CurrencyHolder h = GetHolder(currency);
            if (h == null)
            {
                Log($"RemoveCurrencyAmount failed: {currency.GetDisplayName()} does not exist yet", LogCategory.Critical);
                return;
            }
            h.RemoveAmount(amount);
        }

        /// <summary> if player has enough currency </summary>
        public bool CanBuy(Currency currency, BigNumber amount)
        {
            return GetCurrencyAmount(currency) >= amount;
        }

        /// <returns> amount of currency player owns </returns>
        public BigNumber GetCurrencyAmount(Currency currency) => GetOrAddHolder(currency).amount;

        public BigNumber GetTotalCurrencyAmount(Currency currency) => GetOrAddHolder(currency).TotalAmount;

        public void AddTotalCurrencyAmount(Currency currency, BigNumber amount)
        {
            if (currency == null) return;
            GetOrAddHolder(currency).AddTotalAmount(amount);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(CurrencyManager));
        }

        public void SeedTotalCurrencyAmount(Currency currency, BigNumber amount)
        {
            if (currency == null) return;
            CurrencyHolder holder = GetOrAddHolder(currency);
            if (holder.totalAmount < amount) holder.totalAmount = amount;
        }

        public override CurrencyHolder GetOrAddHolder(Currency holder)
        {
            CurrencyHolder h = GetHolder(holder);
            if (h == null)
            {
                holders.Add(h = new CurrencyHolder(holder, 0));
                OnHolderAdded?.Invoke(h);
            }
            return h;
        }

        protected override void NotifyHolderReset(CurrencyHolder holder)
        {
            if (holder?.currency == null || holder.amount <= 0) return;
            OnCurrencyAmountChanged?.Invoke(holder.currency);
        }

        public BigNumber GetCurrencyGroupUsedCapacity(CurrencyGroup group)
        {
            if (group == null) return 0;
            BigNumber sum = 0;
            foreach (var holder in holders)
            {
                if (holder.currency.currencyGroups != null && holder.currency.currencyGroups.Contains(group))
                {
                    sum += holder.amount;
                }
            }
            return sum;
        }

        public BigNumber GetCurrencyGroupAmount(CurrencyGroup group, LockAmountType amountType)
        {
            if (group == null) return 0;
            BigNumber sum = 0;
            foreach (var holder in holders)
            {
                if (holder.currency.currencyGroups != null && holder.currency.currencyGroups.Contains(group))
                {
                    sum += amountType switch
                    {
                        LockAmountType.TotalAmount => holder.TotalAmount,
                        LockAmountType.BoughtAmount => holder.amount,
                        LockAmountType.MaxAmount => holder.GetMaxCurrencyConstrain(holder.currency),
                        _ => holder.amount,
                    };
                }
            }
            return sum;
        }
    }
}
