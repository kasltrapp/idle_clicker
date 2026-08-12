using EasyIdleGame.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    [AddComponentMenu("EasyIdleGame/Managers/Upgrades Manager")]
    public class UpgradesManager : ManagerHolderBase<UpgradesManager, UpgradeHolder, Upgrade>,
        IBuyableHolderManager<UpgradesManager, UpgradeHolder, Upgrade>,
        IUnlockableHolderManager<UpgradeHolder, Upgrade>,
        IMultiplierProviderHolderManager
    {
        public IBuyableHolderManager<UpgradesManager, UpgradeHolder, Upgrade> iBuyable => this;
        public IUnlockableHolderManager<UpgradeHolder, Upgrade> iUnlockable => this;

        public UnityEvent<Upgrade> OnItemUnlocked { get; } = new UnityEvent<Upgrade>();
        public UnityEvent<UpgradeHolder> OnHolderBoughtEvent { get; } = new UnityEvent<UpgradeHolder>();
        public UnityEvent<UpgradeHolder> OnHolderUnlockedEvent { get; } = new UnityEvent<UpgradeHolder>();

        [CommentArea("Upgrades Manager", "Tracks purchased upgrades, pending timed upgrades, upgrade multipliers, and override values used by the economy.", "Place one UpgradesManager in the gameplay scene. Assign Upgrade assets to UpgradeDisplayers, then use queueMode to decide whether timed purchases complete together or one after another.")]
        [SerializeField] private string _upgradesManagerComment;

        [Tooltip("Controls how timed pending upgrades complete when multiple purchases are waiting. Use parallel for simultaneous timers or sequential for one-at-a-time queues.")]
        public UpgradeQueueMode queueMode;

        public List<UpgradeHolder> upgrades => base.holders;

        public UnityEvent<Upgrade> OnItemBought => OnUpgradeBought;

        [Tooltip("Invoked after an upgrade is bought through the normal purchase flow.")]
        public UnityEvent<Upgrade> OnUpgradeBought = new();

        [Tooltip("Invoked when an owned or pending upgrade amount changes.")]
        public UnityEvent<Upgrade> OnUpgradeAmountChanged = new();

        [Tooltip("Invoked when a timed pending upgrade finishes and becomes active.")]
        public UnityEvent<Upgrade> OnUpgradeApplied = new();

        public UnityEvent<Upgrade, BigNumber, SourceType> OnUpgradeAmountAddedEvent = new();

        public void ForceBuyItemsForFree(Upgrade upgrade, BigNumber amount, SourceType sourceType = SourceType.Generic) => GetOrAddHolder(upgrade).ApplyBoughtAmount(amount, sourceType);

        private float _timeSinceLastCheck = 0f;
        private void Update()
        {
            DateTime now = DateTime.Now;
            foreach (var holder in upgrades)
            {
                if (holder.pendingUpgrades.Count > 0)
                {
                    bool anyApplied = false;
                    for (int i = holder.pendingUpgrades.Count - 1; i >= 0; i--)
                    {
                        var pending = holder.pendingUpgrades[i];
                        if (now >= pending.upgradeEndTime)
                        {
                            holder.amount += pending.amount;
                            holder.pendingUpgrades.RemoveAt(i);
                            anyApplied = true;
                        }
                    }

                    if (anyApplied)
                    {
                        OnUpgradeAmountChanged?.Invoke(holder.upgrade);
                        OnUpgradeApplied?.Invoke(holder.upgrade);
                        EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(UpgradesManager));
                    }
                }
            }

            _timeSinceLastCheck += Time.deltaTime;
            if (_timeSinceLastCheck >= 0.2f)
            {
                _timeSinceLastCheck = 0f;
                iUnlockable.CheckAutoUnlocks();
            }
        }

        public void SkipUpgradeWait(Upgrade upgrade)
        {
            var holder = GetHolder(upgrade);
            if (holder != null && holder.pendingUpgrades.Count > 0)
            {
                foreach (var pending in holder.pendingUpgrades)
                {
                    holder.amount += pending.amount;
                }
                holder.pendingUpgrades.Clear();
                OnUpgradeAmountChanged?.Invoke(holder.upgrade);
                OnUpgradeApplied?.Invoke(holder.upgrade);
                EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(UpgradesManager));
            }
        }

        /// <returns> if upgrade is owned by player </returns>
        public bool IsUpgradeMaxed(Upgrade upgrade) => GetHolder(upgrade)?.iBuyable.Maxed ?? false;

        /// <returns>
        /// multiplier of all upgrades of given type and group
        /// </returns>
        [System.Obsolete("Use GetMultiplierOfType(UpgradeType type, List<BusinessGroup> businessGroupList = null, string group = \"\") instead.")]
        public BigNumber GetMultiplierOfType(UpgradeType type, string group)
        {
            BigNumber multiplier = 1;

            foreach (var upgrade in upgrades)
            {
                if (upgrade.amount <= 0) continue;
                multiplier *= upgrade.iMultiplierProvider.GetMultiplierOfType(type, group);
            }

            return multiplier;
        }

        /// <returns>
        /// multiplier of all upgrades of given type and group
        /// </returns>
        [System.Obsolete("Use GetMultiplierOfTypeStatic(UpgradeType type, List<BusinessGroup> businessGroupList = null, string group = \"\") instead.")]
        public static BigNumber GetMultiplierOfTypeStatic(UpgradeType type, string group)
        {
            if (Instance == null) return 1;
            return Instance.GetMultiplierOfType(type, group);
        }

        public static BigNumber GetMultiplierOfTypeStatic(UpgradeType type, System.Collections.Generic.List<BusinessGroup> businessGroupList = null, string group = "")
        {
            if (Instance == null) return 1;
            return Instance.GetMultiplierOfType(type, businessGroupList, group);
        }

        public BigNumber GetMultiplierOfType(UpgradeType type, System.Collections.Generic.List<BusinessGroup> businessGroupList = null, string group = "")
        {
            float multiplier = 1;

            if (!string.IsNullOrEmpty(group))
            {
                Log($"Obsolete string group '{group}' used in GetMultiplierOfType. Use BusinessGroups list instead.", LogCategory.Warning);
            }

            foreach (var upgradeHolder in upgrades)
            {
                if (upgradeHolder.amount <= 0) continue;

                Upgrade upgrade = upgradeHolder.upgrade;

                if (!string.IsNullOrEmpty(upgrade.Group))
                {
                    Log($"Upgrade '{upgrade.GetDisplayName()}' is using obsolete string group '{upgrade.Group}'. Use TargetBusinessGroups instead.", LogCategory.Warning);
                }

                // Call the interface method on the holder directly so that amount scaling
                // is handled correctly by IMultiplierProviderHolder logic.
                BigNumber multiplierToAdd = ((IMultiplierProviderHolder)upgradeHolder).GetMultiplierOfType(type, businessGroupList, group);
                multiplier *= (float)multiplierToAdd.ToDouble();
            }

            return multiplier;
        }

        public override UpgradeHolder GetOrAddHolder(Upgrade upgrade)
        {
            UpgradeHolder upgradeHolder = GetHolder(upgrade);
            if (upgradeHolder == null)
            {
                upgrades.Add(upgradeHolder = new UpgradeHolder(upgrade));
                OnHolderAdded?.Invoke(upgradeHolder);
            }
            return upgradeHolder;
        }

        protected override void NotifyHolderReset(UpgradeHolder holder)
        {
            if (holder?.upgrade == null) return;
            if (holder.amount <= 0 && holder.boughtAmount <= 0 && holder.pendingUpgrades.Count == 0) return;

            OnUpgradeAmountChanged?.Invoke(holder.upgrade);
        }

        public List<UpgradeBlock> GetAllUpgradeBlocks()
        {
            return holders.GetAllUpgradeBlocks();
        }

        public BigNumber? GetOverrideUpgradeOfType(OverrideUpgradeType type, Currency currency = null)
        {
            return GetHighestModifiedOverrideUpgradeOfType(type, currency, null);
        }

        public BigNumber? GetOverrideUpgradeOfTypeForUpgrade(Upgrade upgrade, OverrideUpgradeType type, Currency currency = null, Business business = null)
        {
            if (upgrade == null) return null;

            UpgradeHolder holder = GetHolder(upgrade);
            if (holder == null || holder.amount <= 0) return null;

            OverrideUpgradeBlock block = holder.upgrade.GetOverrideUpgradeBlock(type, currency, business);
            if (block == null) return null;

            BigNumber sourceValue = holder.GetOverrideMultiplier(block);
            return ApplyOverrideModifiers(holder.upgrade, block, sourceValue, type, currency, business);
        }

        private BigNumber? GetHighestModifiedOverrideUpgradeOfType(OverrideUpgradeType type, Currency currency = null, Business business = null)
        {
            BigNumber highestValue = BigNumber.MinValue;

            foreach (var upgradeHolder in upgrades)
            {
                if (upgradeHolder.amount <= 0) continue;

                OverrideUpgradeBlock block = upgradeHolder.upgrade.GetOverrideUpgradeBlock(type, currency, business);
                if (block == null) continue;

                BigNumber q = upgradeHolder.GetOverrideMultiplier(block);
                BigNumber modifiedValue = ApplyOverrideModifiers(upgradeHolder.upgrade, block, q, type, currency, business);

                if (modifiedValue > highestValue)
                {
                    highestValue = modifiedValue;
                }
            }

            return highestValue == BigNumber.MinValue ? (BigNumber?)null : highestValue;
        }

        private BigNumber ApplyOverrideModifiers(Upgrade sourceUpgrade, OverrideUpgradeBlock sourceBlock, BigNumber sourceValue, OverrideUpgradeType type, Currency currency = null, Business business = null)
        {
            BigNumber additiveModifier = 0;
            BigNumber multiplicativeModifier = 1;

            foreach (var modifierHolder in upgrades)
            {
                if (modifierHolder.amount <= 0) continue;
                if (modifierHolder.upgrade.overrideUpgradeModifiers == null) continue;

                foreach (var modifier in modifierHolder.upgrade.overrideUpgradeModifiers)
                {
                    if (modifier == null) continue;
                    if (!modifier.Matches(sourceUpgrade, sourceBlock, type, currency, business)) continue;

                    BigNumber modifierValue = modifierHolder.GetOverrideModifierValue(modifier);

                    if (modifier.operation == OverrideUpgradeModifierOperation.Add)
                        additiveModifier += modifierValue;
                    else if (modifier.operation == OverrideUpgradeModifierOperation.Multiply)
                        multiplicativeModifier *= modifierValue;
                }
            }

            return (sourceValue + additiveModifier) * multiplicativeModifier;
        }

        /// <returns> Highest active max-business-amount override for the given business from all owned upgrades, or null if none active </returns>
        public BigNumber? GetOverrideBusinessMaxAmount(Business business)
        {
            return GetHighestModifiedOverrideUpgradeOfType(OverrideUpgradeType.maxBusinessAmount, null, business);
        }

        public static BigNumber? GetOverrideBusinessMaxAmountStatic(Business business)
        {
            if (Instance == null) return null;
            return Instance.GetOverrideBusinessMaxAmount(business);
        }

        public BigNumber GetUpgradeGroupAmount(UpgradeGroup group, LockAmountType amountType)
        {
            if (group == null) return 0;
            BigNumber sum = 0;
            foreach (var holder in holders)
            {
                if (holder.upgrade.upgradeGroups != null && holder.upgrade.upgradeGroups.Contains(group))
                {
                    sum += amountType switch
                    {
                        LockAmountType.TotalAmount => holder.TotalAmount,
                        LockAmountType.BoughtAmount => holder.BoughtAmount,
                        LockAmountType.MaxAmount => holder.upgrade.maxBuyableAmount,
                        _ => holder.Amount,
                    };
                }
            }
            return sum;
        }
    }
}
