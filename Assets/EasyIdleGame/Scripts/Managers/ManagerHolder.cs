using System;
using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that holds a manager and its data at runtime
    /// </summary>
    [Serializable]
    public class ManagerHolder : HolderBase<Manager>, IUpgradableHolder, IBuyableHolder, IUnlockableHolder
    {
        public IUnlockableHolder iUnlockable => this;
        public IUpgradableHolder iUpgradable => this;
        public IBuyableHolder iBuyable => this;

        // IUpgradableHolder
        public IUpgradable holdedItem => base.Item;
        public BigNumber RelativeCount { get => count; set => count = value; }
        public BigNumber Level { get => level; set => level = (float)value.ToDouble(); }
        public float CurrentUpgradeCostMultiplier { get => currentUpgradeCostMultiplier; set => currentUpgradeCostMultiplier = value; }
        public BigNumber TotalCount { get => totalCount; set => totalCount = value; }
        public float LevelUpMultiplier => 2;
        public Action<IUpgradableHolder, SourceType> OnUpgraded { get; set; }
        public Action<IUnlockableHolder> OnUnlocked { get; set; }
        public Action<IBuyableHolder, SourceType> OnBought { get; set; }
        public Action<IBuyableHolder, BigNumber, SourceType> OnAdded { get; set; }

        // IBuyableHolder
        IBuyable IBuyableHolder.holdedItem => base.Item;
        public BigNumber Amount { get => totalCount; set => throw new Exception("You can't set manager amount through here"); }
        public BigNumber TotalAmount => totalCount;
        public BigNumber BoughtAmount => boughtAmount;
        public BigNumber ActiveCostMultiplier => UpgradesManager.GetMultiplierOfTypeStatic(UpgradeType.purchaseCost) * UpgradeEffectResolver.GetPurchaseCostMultiplier(manager);

        // IUnlockableHolder
        public IUnlockable UnlockableItem => Item;
        public bool AlreadyUnlocked { get => wasUnlockedOnce; set => wasUnlockedOnce = value; }

        public ManagerHolder(Manager manager_) : base(manager_) { }

        public Manager manager => Item;

        public BigNumber totalCount;

        // amount gained exclusively through buying (TryBuyItems)
        public BigNumber boughtAmount;

        // count relative to the current level
        public BigNumber count;
        public float level;
        public float currentUpgradeCostMultiplier = 1;

        public List<UpgradeBlock> CurrentPassiveUpgrades
        {
            get
            {
                float multiplier = manager.businessSpeedMultiplier * (level - 1);
                if (multiplier <= 0)
                    multiplier = 1;

                return manager.passiveBoosts.Multiply(multiplier);
            }
        }

        public float GetMultiplierOfType(UpgradeType type) => CurrentPassiveUpgrades.GetMultiplierOfType(type);

        public bool wasUnlockedOnce = false;

        public bool IsUnlocked()
        {
            if (manager.unlockOnlyOnce && wasUnlockedOnce) return true;

            if (manager.iUnlockable.IsCurrentlyUnlocked())
                return wasUnlockedOnce = true;

            return false;
        }

        public void AddManagerCount(BigNumber amount, SourceType sourceType = SourceType.Generic)
        {
            iUpgradable.AddCount(amount, sourceType);
            OnAdded?.Invoke(this, amount, sourceType);

            if (ManagersManager.Instance)
            {
                ManagersManager.Instance.Log($"Added {amount} to manager {manager.GetDisplayName()}. New total count: {totalCount}", LogCategory.Verbose);
                ManagersManager.Instance.OnManagerAmountChanged?.Invoke(manager);
                ManagersManager.Instance.OnManagerAmountAddedEvent?.Invoke(manager, amount, sourceType);
                EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(ManagersManager));
            }
        }

        // experimental
        public bool IsActive => activeTimeLeft > 0;
        public bool IsOnCooldown => cooldownTimeLeft > 0;

        public double activeTimeLeft;
        public double cooldownTimeLeft;

        public bool TryActivate()
        {
            if (!CanActivate()) return false;

            activeTimeLeft = UpgradeEffectResolver.GetManagerActiveDurationSeconds(this);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(ManagersManager));
            return true;
        }

        public bool CanActivate() => cooldownTimeLeft <= 0 && level > 0;

        public void OnTimePassed(float seconds)
        {
            if (activeTimeLeft <= 0)
            {
                cooldownTimeLeft -= seconds;
                if (cooldownTimeLeft <= 0) cooldownTimeLeft = 0;

                return;
            }

            cooldownTimeLeft = UpgradeEffectResolver.GetManagerCooldownDurationSeconds(this);
            activeTimeLeft -= seconds;
        }

        public override string ToString()
        {
            return $"{manager.GetDisplayName()}: {count}";
        }

        public bool CanBuy(BigNumber amount)
        {
            return manager.Cost.IsSufficent(amount, iBuyable.GetGeometricCurrentAmount(), manager.CostMultiplier, ActiveCostMultiplier) && iUnlockable.IsUnlocked() && !iBuyable.Maxed;
        }

        public void Pay_BuyEvent(BigNumber amount, SourceType sourceType = SourceType.Purchase)
        {
            manager.Cost.RemoveInputs(amount, iBuyable.GetGeometricCurrentAmount(), manager.CostMultiplier, ActiveCostMultiplier);
            boughtAmount += amount;
            OnBought?.Invoke(this, sourceType);

            if (ManagersManager.Instance)
                ManagersManager.Instance.Log($"Bought {amount} of manager {manager.GetDisplayName()}. New bought amount: {boughtAmount}", LogCategory.Verbose);
        }
    }
}
