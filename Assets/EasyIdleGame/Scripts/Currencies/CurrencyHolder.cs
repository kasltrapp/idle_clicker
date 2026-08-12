namespace EasyIdleGame
{
    /// <summary>
    /// Class that holds a currency data at runtime
    /// </summary>
    [System.Serializable]
    public class CurrencyHolder : HolderBase<Currency>
    {
        public Currency currency => Item;

        public BigNumber amount;
        public BigNumber totalAmount;

        public BigNumber TotalAmount => totalAmount;

        public CurrencyHolder(Currency cur, BigNumber am) : base(cur)
        {
            amount = am;
            totalAmount = am;
        }

        public override string ToString()
        {
            return $"{currency.GetDisplayName()}: {amount}";
        }

        public void AddAmount(BigNumber amountToAdd, SourceType sourceType = SourceType.Generic)
        {
            if (PlayerStats.Instance)
                PlayerStats.Instance.OnCurrencyAmountAdded(currency, amountToAdd);
            AddTotalAmount(amountToAdd);

            BigNumber allowedAddition = amountToAdd;

            // Enforce Capacity Group limits
            if (currency.currencyGroups != null && CurrencyManager.Instance != null)
            {
                foreach (var group in currency.currencyGroups)
                {
                    if (group != null && group.capacity >= 0)
                    {
                        BigNumber usedCapacity = CurrencyManager.Instance.GetCurrencyGroupUsedCapacity(group);
                        BigNumber remaining = group.capacity - usedCapacity;
                        if (remaining < allowedAddition)
                        {
                            allowedAddition = BigNumber.Max(0, remaining);
                        }
                    }
                }
            }

            amount += allowedAddition;
            amount = amount.Floor();

            BigNumber max = GetMaxCurrencyConstrain(currency);
            if (amount > max)
                amount = max;

            if (CurrencyManager.Instance)
            {
                CurrencyManager.Instance.Log($"Added {amountToAdd} to {currency.GetDisplayName()}. New total: {amount}", LogCategory.Verbose);
                CurrencyManager.Instance.OnCurrencyAmountChanged?.Invoke(currency);
                CurrencyManager.Instance.OnCurrencyAmountAddedEvent?.Invoke(currency, amountToAdd);
                CurrencyManager.Instance.OnCurrencyAmountAddedWithSourceEvent?.Invoke(currency, amountToAdd, sourceType);
                EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(CurrencyManager));
            }
        }

        public void AddTotalAmount(BigNumber amountToAdd)
        {
            totalAmount += amountToAdd;
        }

        public void RemoveAmount(BigNumber amountToRemove)
        {
            amount -= amountToRemove;
            amount = amount.Floor();
            if (amount < 0) amount = 0;

            if (CurrencyManager.Instance)
            {
                CurrencyManager.Instance.Log($"Removed {amountToRemove} from {currency.GetDisplayName()}. New total: {amount}", LogCategory.Verbose);
                CurrencyManager.Instance.OnCurrencyAmountChanged?.Invoke(currency);
                EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(CurrencyManager));
            }
        }

        public BigNumber GetMaxCurrencyConstrain(Currency currency)
        {
            BigNumber? upgradeValue = UpgradesManager.Instance?.GetOverrideUpgradeOfType(OverrideUpgradeType.maxCurrencyAmount, currency: currency);

            if (upgradeValue != null)
            {
                return upgradeValue.Value;
            }

            return currency.maxAmount < 0 ? BigNumber.MaxValue : currency.maxAmount;
        }
    }
}
