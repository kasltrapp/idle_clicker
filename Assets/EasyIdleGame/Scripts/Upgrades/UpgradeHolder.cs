using System;
using System.Collections.Generic;

namespace EasyIdleGame
{
    [System.Serializable]
    public class UpgradeHolder : HolderBase<Upgrade>, IUnlockableHolder, IBuyableHolder, IMultiplierProviderHolder
    {
        public IUnlockableHolder iUnlockable => this;
        public IBuyableHolder iBuyable => this;
        public IMultiplierProviderHolder iMultiplierProvider => this;

        public Action<IUnlockableHolder> OnUnlocked { get; set; }
        public Action<IBuyableHolder, SourceType> OnBought { get; set; }
        public Action<IBuyableHolder, BigNumber, SourceType> OnAdded { get; set; }

        // IUnlockableHolder
        public IUnlockable UnlockableItem => upgrade;
        public bool AlreadyUnlocked { get => unlockedOnce; set => unlockedOnce = value; }

        // IBuyableHolder
        public IBuyable holdedItem => Item;
        public BigNumber Amount { get => amount; set => amount = value; }
        public BigNumber TotalAmount => amount;
        public BigNumber BoughtAmount => boughtAmount;
        public BigNumber ActiveCostMultiplier => UpgradeEffectResolver.GetUpgradePurchaseCostMultiplier(upgrade);

        // IMultiplierProviderHolder
        IMultiplierProvider IMultiplierProviderHolder.holdedItem => upgrade;

        public Upgrade upgrade => Item;

        public bool unlockedOnce = false;

        public UpgradeHolder(Upgrade upgrade) : base(upgrade) { }

        public BigNumber amount;
        public BigNumber boughtAmount;
        public List<PendingUpgrade> pendingUpgrades = new List<PendingUpgrade>();
        //public BigNumber currentCostMultiplier = 1;

        public bool CanBuy(BigNumber amount)
        {
            return upgrade.Cost.IsSufficent(amount, iBuyable.GetGeometricCurrentAmount(), upgrade.CostMultiplier, ActiveCostMultiplier) && iUnlockable.IsUnlocked() && !iBuyable.Maxed;
        }

        // Custom buying amount for upgrades is not implemented yet
        public void Pay_BuyEvent(BigNumber amount, SourceType sourceType = SourceType.Purchase)
        {
            upgrade.Cost.RemoveInputs(amount, iBuyable.GetGeometricCurrentAmount(), upgrade.CostMultiplier, ActiveCostMultiplier);
            boughtAmount += amount;
            OnBought?.Invoke(this, sourceType);

            if (UpgradesManager.Instance)
                UpgradesManager.Instance.Log($"Bought {amount} of {upgrade.GetDisplayName()}. New bought amount: {boughtAmount}", LogCategory.Verbose);
        }

        public void ApplyBoughtAmount(BigNumber boughtAmount, SourceType sourceType = SourceType.Generic)
        {
            OnAdded?.Invoke(this, boughtAmount, sourceType);
            if (upgrade.upgradeDurationInSeconds > 0)
            {
                if (UpgradesManager.Instance.queueMode == UpgradeQueueMode.Parallel)
                {
                    DateTime newEndTime = DateTime.Now.AddSeconds(upgrade.upgradeDurationInSeconds);
                    pendingUpgrades.Add(new PendingUpgrade(boughtAmount, newEndTime));
                }
                else // Sequential
                {
                    DateTime baseTime = DateTime.Now;
                    if (pendingUpgrades.Count > 0)
                    {
                        DateTime lastEndTime = pendingUpgrades[pendingUpgrades.Count - 1].upgradeEndTime;
                        if (lastEndTime > baseTime)
                        {
                            baseTime = lastEndTime;
                        }
                    }
                    DateTime newEndTime = baseTime.AddSeconds(upgrade.upgradeDurationInSeconds * boughtAmount.ToDouble());
                    pendingUpgrades.Add(new PendingUpgrade(boughtAmount, newEndTime));
                }

                if (UpgradesManager.Instance)
                    UpgradesManager.Instance.Log($"Added {boughtAmount} of {upgrade.GetDisplayName()} to pending upgrades.", LogCategory.Verbose);

                EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(UpgradesManager));
                UpgradesManager.Instance?.OnUpgradeAmountAddedEvent?.Invoke(upgrade, boughtAmount, sourceType);
            }
            else
            {
                amount += boughtAmount;

                if (UpgradesManager.Instance)
                {
                    UpgradesManager.Instance.Log($"Added {boughtAmount} to {upgrade.GetDisplayName()}. New total amount: {amount}", LogCategory.Verbose);
                    UpgradesManager.Instance.OnUpgradeAmountChanged?.Invoke(upgrade);
                    UpgradesManager.Instance.OnUpgradeAmountAddedEvent?.Invoke(upgrade, boughtAmount, sourceType);
                    EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(UpgradesManager));
                }
            }
        }

        public BigNumber? GetOverrideMultiplier(OverrideUpgradeType type, Currency currency = null, Business business = null)
        {
            OverrideUpgradeBlock block = upgrade.GetOverrideUpgradeBlock(type, currency, business);
            if (block == null) return null;

            return GetOverrideMultiplier(block);
        }

        public BigNumber GetOverrideMultiplier(OverrideUpgradeBlock block)
        {
            return Mathb.GeometricSequence_NthTerm(block.value, upgrade.overrideUpgradeLevelMultiplier, Amount);
        }

        public BigNumber GetOverrideModifierValue(OverrideUpgradeModifierBlock block)
        {
            return Mathb.GeometricSequence_NthTerm(block.value, upgrade.overrideUpgradeLevelMultiplier, Amount);
        }

    }
}
