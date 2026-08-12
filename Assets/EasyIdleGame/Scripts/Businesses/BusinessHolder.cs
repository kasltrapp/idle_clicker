using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that holds a business at runtime
    /// </summary>
    [System.Serializable]
    public class BusinessHolder : HolderBase<Business>, IUnlockableHolder, ILevelupableHolder, IBuyableHolder, IProducableHolder, IUpgradableHolder
    {
        public IUnlockableHolder iUnlockable => this;
        public ILevelupableHolder iLevelupable => this;
        public IBuyableHolder iBuyable => this;
        public IProducableHolder iProducable => this;
        public IUpgradableHolder iUpgradable => this;

        // IUnlockableHolder
        public IUnlockable UnlockableItem => business;
        public bool AlreadyUnlocked { get => unlockedOnce; set => unlockedOnce = value; }

        // ILevelupableHolder
        public ILevelupable holdedItem => business;
        public Level Level { get => level; set => level = value; }

        // IBuyableHolder
        IBuyable IBuyableHolder.holdedItem => business;
        public BigNumber Amount { get => amount; set => amount = value; }
        public BigNumber TotalAmount => totalAmount;
        public BigNumber BoughtAmount => boughtAmount;
        public BigNumber ActiveCostMultiplier => business.PriceMultiplier;

        // IProducableHolder
        public IProducable ProducableItem => business;

        // IUpgradableHolder
        IUpgradable IUpgradableHolder.holdedItem => base.Item;
        public BigNumber RelativeCount { get => count; set => count = value; }
        BigNumber IUpgradableHolder.Level { get => upgradableLevel; set => upgradableLevel = (float)value.ToDouble(); }
        public float CurrentUpgradeCostMultiplier { get => currentUpgradeCostMultiplier; set => currentUpgradeCostMultiplier = value; }
        public BigNumber TotalCount { get => totalCount; set => totalCount = value; }
        public float LevelUpMultiplier => 2;
        public Action<IUpgradableHolder, SourceType> OnUpgraded { get; set; }
        public Action<IUnlockableHolder> OnUnlocked { get; set; }
        public Action<ILevelupableHolder> OnLeveledUp { get; set; }
        public Action<IBuyableHolder, SourceType> OnBought { get; set; }
        public Action<IBuyableHolder, BigNumber, SourceType> OnAdded { get; set; }
        public Action<IProducableHolder> OnProductionStarted { get; set; }
        Action<IProducableHolder> IProducableHolder.OnProductionFinished { get; set; }

        // count relative to the current level
        public BigNumber count;
        public float upgradableLevel;
        public float currentUpgradeCostMultiplier = 1;
        public BigNumber totalCount;

        public Business business => base.Item;

        // current amount of the businesses
        public BigNumber amount = 0;

        // total amount ever acquired (used for geometric cost scaling; does not decrease on consume/removal)
        public BigNumber totalAmount = 0;

        // amount gained exclusively through buying (TryBuyItems)
        public BigNumber boughtAmount = 0;

        // amount that the production was started with
        public BigNumber ProductionAmount
        {
            get
            {
                if (business.ProductionRoundMode == ProductionRoundMode.SingleRoundSnapshot)
                    return productionAmount_;

                return amount;
            }
            set => productionAmount_ = value;
        }
        private BigNumber productionAmount_ = 0;

        public Level level;

        public List<ProductionRound> activeProductions = new List<ProductionRound>();
        public List<ProductionRound> ActiveProductions => activeProductions;
        public BigNumber CurrentlyProducing
        {
            get
            {
                BigNumber total = 0;
                for (int i = 0; i < activeProductions.Count; i++) total += activeProductions[i].productionAmount;
                return total;
            }
        }

        [Obsolete("Use ActiveProductions instead")]
        public float productionTime
        {
            get => activeProductions.Count > 0 ? (float)activeProductions[0].productionTime : 0;
            set
            {
                if (activeProductions.Count == 0)
                    activeProductions.Add(new ProductionRound() { productionTime = value, productionAmount = ProductionAmount, rolledOutputs = (this as IProducableHolder).GetActualProductionForAmount(ProductionAmount) });
                else
                    activeProductions[0].productionTime = value;
            }
        }

        [Obsolete("Use ActiveProductions.Count > 0 instead")]
        public bool producing
        {
            get => activeProductions.Count > 0;
            set { /* ignored */ }
        }

        public Action<BusinessHolder> OnProductionFinished;

        public bool TryProduceManually(BigNumber instancesToProduce, out ProductionRound round, object context = null)
        {
            round = null;
            if (instancesToProduce <= 0) return false;
            if (business.ProductionRoundMode != ProductionRoundMode.ConcurrentRounds && activeProductions.Count > 0) return false;

            if (business.productionLocks != null)
            {
                for (int i = 0; i < business.productionLocks.Count; i++)
                {
                    if (business.productionLocks[i] != null && !business.productionLocks[i].IsUnlocked()) return false;
                }
            }

            BigNumber availableAmount = amount - CurrentlyProducing;
            if (instancesToProduce > availableAmount) return false;

            instancesToProduce = business.iProducable.GetAffordableProductionAmount(instancesToProduce);
            if (instancesToProduce <= 0) return false;

            business.iProducable.GetProductionCost(instancesToProduce).RemoveInputs(1, 0, 1, 1);

            round = new ProductionRound() { Context = context, productionTime = 0, productionAmount = instancesToProduce, rolledOutputs = (this as IProducableHolder).GetActualProductionForAmount(instancesToProduce, true) };
            activeProductions.Add(round);
            OnProductionStarted?.Invoke(this);
            if (BusinessesManager.Instance) BusinessesManager.Instance.OnHolderProductionStartedEvent?.Invoke(this);
            if (BusinessesManager.Instance) BusinessesManager.Instance.OnProductionRoundStartedEvent?.Invoke(this, round);
            return true;
        }

        public bool TryStartProduction()
        {
            if (business.ProductionRoundMode != ProductionRoundMode.ConcurrentRounds && activeProductions.Count > 0) return false;

            if (business.productionLocks != null)
            {
                for (int i = 0; i < business.productionLocks.Count; i++)
                {
                    if (business.productionLocks[i] != null && !business.productionLocks[i].IsUnlocked()) return false;
                }
            }

            if (business.upperProductionLocks != null)
            {
                for (int i = 0; i < business.upperProductionLocks.Count; i++)
                {
                    if (business.upperProductionLocks[i] != null && !business.upperProductionLocks[i].IsUpperUnlocked()) return false;
                }
            }

            BigNumber amountToProduce = amount - CurrentlyProducing;
            if (amountToProduce <= 0) return false;

            amountToProduce = business.iProducable.GetAffordableProductionAmount(amountToProduce);
            if (amountToProduce <= 0) return false;

            StartProduction(amountToProduce, out _);
            return true;
        }

        public void StartProduction(BigNumber amountToProduce, out ProductionRound round, object context = null)
        {
            OnProductionRoundStarted(amountToProduce);
            round = new ProductionRound() { Context = context, productionTime = 0, productionAmount = amountToProduce, rolledOutputs = (this as IProducableHolder).GetActualProductionForAmount(amountToProduce) };
            activeProductions.Add(round);
            OnProductionStarted?.Invoke(this);
            if (BusinessesManager.Instance) BusinessesManager.Instance.OnHolderProductionStartedEvent?.Invoke(this);
            if (BusinessesManager.Instance) BusinessesManager.Instance.OnProductionRoundStartedEvent?.Invoke(this, round);
        }

        [Obsolete("Use TryStartProduction() or StartProduction(BigNumber) instead.")]
        public void StartProduction()
        {
            TryStartProduction();
        }

        public bool unlockedOnce = false;

        public BusinessHolder(Business business_, BigNumber amount_) : base(business_)
        {
            amount = amount_;
            ProductionAmount = amount;

            (this as ILevelupableHolder).Init();
        }

        public BigNumber GetProductionMultiplier() => business.GetBusinessMultiplierOfType(UpgradeType.production);

        public BigNumber GetSpeedMultiplier() => business.GetBusinessMultiplierOfType(UpgradeType.speed);

        /// <returns> if auto production is possible for this business </returns>
        public bool IsAutoProductionPossible() => IsManagerUnlockedAndAboveAutoProduceThreshold() || BoostsManager.IsAutoProduceOverrideActive(business);

        /// <returns> if business can be autoproduced </returns>
        public bool CanAutoProduce() => IsAutoProductionPossible() && CanProduce();

        /// <returns> if business can start production </returns>
        public bool CanProduce()
        {
            if (business.ProductionRoundMode != ProductionRoundMode.ConcurrentRounds && activeProductions.Count > 0) return false;

            if (business.productionLocks != null)
            {
                for (int i = 0; i < business.productionLocks.Count; i++)
                {
                    if (business.productionLocks[i] != null && !business.productionLocks[i].IsUnlocked()) return false;
                }
            }

            if (business.upperProductionLocks != null)
            {
                for (int i = 0; i < business.upperProductionLocks.Count; i++)
                {
                    if (business.upperProductionLocks[i] != null && !business.upperProductionLocks[i].IsUpperUnlocked()) return false;
                }
            }

            BigNumber amountToProduce = amount - CurrentlyProducing;
            return amountToProduce > 0 && business.iProducable.GetAffordableProductionAmount(amountToProduce) > 0;
        }

        public bool ManagerUnlocked()
        {
            if (!ManagersManager.Instance) return false;
            if (!business.manager) return false;

            if (!ManagersManager.Instance.iUpgradable.IsItemAtLevelOneOrMore(business.manager)) return false;

            return true;
        }

        public bool IsManagerUnlockedAndAboveAutoProduceThreshold()
        {
            if (business.autoProduce) return true;
            if (!ManagerUnlocked()) return false;

            float managerLevel = ManagersManager.Instance.GetOrAddHolder(business.manager).level;
            int threshold = business.manager.autoProduceOnLevel;

            return (threshold == 0 || managerLevel >= threshold);
        }

        public bool IsManagerUnlockedAndAboveAutoCollectThreshold()
        {
            if (!ManagerUnlocked()) return false;

            float managerLevel = ManagersManager.Instance.GetOrAddHolder(business.manager).level;
            int threshold = business.manager.autoCollectOnLevel;

            return (threshold == 0 || managerLevel >= threshold);
        }

        public float GetTimeLeft()
        {
            if (activeProductions.Count == 0) return business.timeToProduce;
            return (float)(business.timeToProduce - activeProductions[0].productionTime);
        }

        public float GetActualTimeLeft()
        {
            return GetTimeLeft() / (float)GetSpeedMultiplier().ToDouble();
        }

        public void HandleUpdate()
        {
            float speedMultiplier = (float)GetSpeedMultiplier().ToDouble();

            bool canProduce = true;
            if (business.productionLocks != null)
            {
                for (int i = 0; i < business.productionLocks.Count; i++)
                {
                    if (business.productionLocks[i] != null && !business.productionLocks[i].IsUnlocked())
                    {
                        canProduce = false;
                        break;
                    }
                }
            }

            if (canProduce && business.upperProductionLocks != null)
            {
                for (int i = 0; i < business.upperProductionLocks.Count; i++)
                {
                    if (business.upperProductionLocks[i] != null && !business.upperProductionLocks[i].IsUpperUnlocked())
                    {
                        canProduce = false;
                        break;
                    }
                }
            }

            if (canProduce)
            {
                for (int i = activeProductions.Count - 1; i >= 0; i--)
                {
                    var round = activeProductions[i];
                    if (round.isPaused) continue;

                    round.productionTime += Time.deltaTime * speedMultiplier;

                    if (round.productionTime >= business.timeToProduce)
                    {
                        activeProductions.RemoveAt(i);
                        OnProductionRoundFinished(round);
                    }
                }
            }

            if (activeProductions.Count == 0)
            {
                ProductionAmount = amount;
            }

            if (CanAutoProduce())
            {
                TryStartProduction();
            }
        }

        public void AddAmount(BigNumber amount_, object context = null, SourceType sourceType = SourceType.Generic)
        {
            if (PlayerStats.Instance)
                PlayerStats.Instance.OnBusinessAmountAdded(business, amount_);

            BigNumber prevAmount = amount;

            BigNumber allowedAddition = BigNumber.Min(amount_, GetRemainingAmountConstrain());

            amount += allowedAddition;
            amount = amount.RoundToWholeNumber();

            BigNumber max = GetMaxAmountConstrain();
            if (amount > max)
                amount = max;

            BigNumber actualAdded = amount - prevAmount;
            totalAmount += actualAdded;
            if (actualAdded > 0)
                OnAdded?.Invoke(this, actualAdded, sourceType);

            if (activeProductions.Count == 0)
                ProductionAmount = amount;
            else if (business.ProductionRoundMode == ProductionRoundMode.SingleRoundLiveAmount && actualAdded > 0)
                RefreshActiveProductionRoundAmount();

            if (actualAdded > 0)
            {
                if (BusinessesManager.Instance)
                {
                    BusinessesManager.Instance.Log($"Added {actualAdded} to {business.GetDisplayName()}. New total: {amount}", LogCategory.Verbose);
                    BusinessesManager.Instance.OnBusinessAmountChanged?.Invoke(business);
                    BusinessesManager.Instance.OnBusinessAmountAddedEvent?.Invoke(this, actualAdded, context);
                    BusinessesManager.Instance.OnBusinessAmountAddedWithSourceEvent?.Invoke(this, actualAdded, context, sourceType);
                    EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BusinessesManager));
                }
            }

            if (actualAdded <= 0) return;

            if (PlayerStats.Instance)
                PlayerStats.Instance.AddXp(business.xpsAddPerPurchaseToPlayer * actualAdded);

            BigNumber xpAmount = actualAdded;
            if (UpgradesManager.Instance != null)
                xpAmount *= UpgradesManager.Instance.GetMultiplierOfType(UpgradeType.businessXp, business.GetEffectiveBusinessGroups(), business.GetEffectiveBusinessGroupName());
            if (BoostsManager.Instance != null)
                xpAmount *= BoostsManager.Instance.GetMultiplierOfType(UpgradeType.businessXp, business.GetEffectiveBusinessGroups(), business.GetEffectiveBusinessGroupName());

            iLevelupable.AddXps(xpAmount);
        }

        public BigNumber GetRemainingAmountConstrain()
        {
            BigNumber remaining = GetMaxAmountConstrain() - amount;

            List<BusinessGroup> effectiveGroups = business.GetEffectiveBusinessGroups();
            if (effectiveGroups != null && BusinessesManager.Instance != null)
            {
                foreach (var group in effectiveGroups)
                {
                    if (group != null && group.capacity >= 0)
                    {
                        BigNumber usedCapacity = BusinessesManager.Instance.GetBusinessGroupUsedCapacity(group);
                        remaining = BigNumber.Min(remaining, group.capacity - usedCapacity);
                    }
                }
            }
            return BigNumber.Max(0, remaining).Floor();
        }

        public BigNumber GetRemainingBuyableAmountConstrain()
        {
            BigNumber remaining = GetRemainingAmountConstrain();
            BigNumber maxBuyable = GetMaxBuyableAmountConstrain();

            if (maxBuyable < BigNumber.MaxValue)
            {
                remaining = BigNumber.Min(remaining, maxBuyable - amount);
            }

            return BigNumber.Max(0, remaining).Floor();
        }

        private void RefreshActiveProductionRoundAmount()
        {
            ProductionRound round = activeProductions[0];
            round.productionAmount = amount;
            round.rolledOutputs = (this as IProducableHolder).GetActualProductionForAmount(round.productionAmount);
        }

        public BigNumber GetMaxAmountConstrain()
        {
            BusinessUpgradeLevel upgLevel = iUpgradable.GetCurrentUpgradeLevel() as BusinessUpgradeLevel;

            BigNumber maxAmount = upgLevel == null ? business.maxAmount_ : upgLevel.maxAmount_;

            BigNumber? upgradeOverride = UpgradesManager.GetOverrideBusinessMaxAmountStatic(business);
            if (upgradeOverride.HasValue && upgradeOverride.Value > 0)
                maxAmount = maxAmount <= 0 ? upgradeOverride.Value : BigNumber.Max(maxAmount, upgradeOverride.Value);

            if (maxAmount <= 0) return BigNumber.MaxValue;
            return maxAmount;
        }

        public BigNumber GetMaxBuyableAmountConstrain()
        {
            BusinessUpgradeLevel upgLevel = iUpgradable.GetCurrentUpgradeLevel() as BusinessUpgradeLevel;

            BigNumber maxAmount = upgLevel == null ? business.MaxBuyableAmount : upgLevel.maxBuyableAmount;

            BigNumber? upgradeOverride = UpgradesManager.GetOverrideBusinessMaxAmountStatic(business);
            if (upgradeOverride.HasValue && upgradeOverride.Value > 0)
                maxAmount = maxAmount <= 0 ? upgradeOverride.Value : BigNumber.Max(maxAmount, upgradeOverride.Value);

            if (maxAmount <= 0) return BigNumber.MaxValue;
            return maxAmount;
        }

        /// <returns> Normalized [0..1] production progress of the oldest active production round, or 0 if idle </returns>
        public float GetProductionProgress()
        {
            if (activeProductions.Count == 0) return 0f;
            return (float)(activeProductions[0].productionTime / business.timeToProduce);
        }

        public void RemoveAmount(BigNumber amount_, object context = null)
        {
            amount -= amount_;
            amount = amount.RoundToWholeNumber();

            if (amount < 0) amount = 0;

            if (BusinessesManager.Instance)
            {
                BusinessesManager.Instance.Log($"Removed {amount_} from {business.GetDisplayName()}. New total: {amount}", LogCategory.Verbose);
                BusinessesManager.Instance.OnBusinessAmountChanged?.Invoke(business);
                BusinessesManager.Instance.OnBusinessAmountRemovedEvent?.Invoke(this, amount_, context);
                EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BusinessesManager));
            }
        }

        public List<Output> unclaimedOutputs = new List<Output>();

        /// <returns> Applies business outputs, with everything taken into account </returns>
        public void OnProductionRoundFinished(ProductionRound round)
        {
            if (unclaimedOutputs == null) unclaimedOutputs = new List<Output>();

            if (round.rolledOutputs != null)
                unclaimedOutputs.AddRange(round.rolledOutputs);
            else
                unclaimedOutputs.AddRange((this as IProducableHolder).GetActualProductionForAmount(round.productionAmount));

            if (business.ConsumeOnProduce)
            {
                RemoveAmount(round.productionAmount, round.Context);
            }

            if (business.autoCollect || IsManagerUnlockedAndAboveAutoCollectThreshold()) ClaimOutputs(round.Context);

            OnProductionFinished?.Invoke(this);
            (this as IProducableHolder).OnProductionFinished?.Invoke(this); // Interface
            if (BusinessesManager.Instance) BusinessesManager.Instance.OnProductionFinishedEvent?.Invoke(this);
            if (BusinessesManager.Instance) BusinessesManager.Instance.OnHolderProductionFinishedEvent?.Invoke(this);
            if (BusinessesManager.Instance) BusinessesManager.Instance.OnProductionRoundFinishedEvent?.Invoke(this, round);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BusinessesManager));
        }

        [Obsolete("Use OnProductionRoundFinished(ProductionRound) instead.")]
        public void OnProductionRoundFinished(BigNumber amountProduced)
        {
            OnProductionRoundFinished(new ProductionRound() { productionAmount = amountProduced, rolledOutputs = (this as IProducableHolder).GetActualProductionForAmount(amountProduced) });
        }

        [Obsolete("Use OnProductionRoundFinished(ProductionRound) instead.")]
        public void OnProductionRoundFinished()
        {
            OnProductionRoundFinished(ProductionAmount);
        }

        public List<Output> ClaimOutputs(object context = null)
        {
            List<Output> data = new List<Output>(unclaimedOutputs);
            unclaimedOutputs.ForEach(x => x.OnProductionRoundFinished_Apply(1, context));
            unclaimedOutputs = new List<Output>();
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BusinessesManager));
            return data;
        }

        public void OnProductionRoundStarted(BigNumber amountToProduce)
        {
            business.iProducable.GetProductionCost(amountToProduce).RemoveInputs(1, 0, 1, 1);
        }

        [Obsolete("Use OnProductionRoundStarted(BigNumber) instead.")]
        public void OnProductionRoundStarted() => OnProductionRoundStarted(ProductionAmount);

        public override string ToString()
        {
            return $"{business.GetDisplayName()}: a:{amount}";
        }

        public bool CanBuy(BigNumber amount)
        {
            return amount > 0
                && amount <= GetRemainingBuyableAmountConstrain()
                && business.iBuyable.CanPay(amount, iBuyable.GetGeometricCurrentAmount())
                && iUnlockable.IsUnlocked();
        }

        public void Pay_BuyEvent(BigNumber amount, SourceType sourceType = SourceType.Purchase)
        {
            business.iBuyable.Pay(amount, iBuyable.GetGeometricCurrentAmount());
            boughtAmount += amount;
            OnBought?.Invoke(this, sourceType);
        }

        #region OBSOLETE METHODS
        /// <returns> Total production time multiplied by all multipliers (Managers, upgrades and all) in seconds </returns>
        [Obsolete("Use iProducable.GetActualTimeToProduce() instead.")]
        public float GetActualTimeToProduce()
        {
            return (float)(BigNumber.Max(business.timeToProduce / GetSpeedMultiplier(), BigNumber.MinPositiveValue)).ToDouble();
        }

        /// <returns> List of outputs with everything taken into account (such as Boosts, Prestiges and amount) </returns>
        [Obsolete("Use iProducable.GetActualProductionPerSecond() instead.")]
        public List<Output> GetActualProductionPerSecond(BigNumber additionalAmount)
        {
            throw new NotImplementedException("Obsolete - " + "Use iProducable.GetOptimisticActualProduction() instead.");
        }

        [Obsolete("Use iProducable.GetOptimisticActualProduction() instead.")]
        public List<Output> GetActualProduction(BigNumber additionalAmount)
        {
            throw new NotImplementedException("Obsolete - " + "Use iProducable.GetOptimisticActualProduction() instead.");
        }

        [Obsolete("Use iProducable.GetProductionCost() instead.")]
        public List<Input> GetProductionCost(BigNumber additionalAmount)
        {
            throw new NotImplementedException("Obsolete - " + "Use iProducable.GetProductionCost() instead.");
        }

        [Obsolete("Use iProducable.GetActualProductionCostPerSecond() instead.")]
        public List<Input> GetActualProductionCostPerSecond(BigNumber additionalAmount)
        {
            throw new NotImplementedException("Obsolete - " + "Use iProducable.GetActualProductionCostPerSecond() instead.");
        }
        #endregion

        public void PauseProductionRound(ProductionRound round)
        {
            if (round != null)
            {
                round.isPaused = true;
            }
        }

        public void PauseAllProductionRounds()
        {
            for (int i = 0; i < activeProductions.Count; i++)
            {
                activeProductions[i].isPaused = true;
            }
        }

        public void UnpauseProductionRound(ProductionRound round)
        {
            if (round != null)
            {
                round.isPaused = false;
            }
        }

        public void UnpauseAllProductionRounds()
        {
            for (int i = 0; i < activeProductions.Count; i++)
            {
                activeProductions[i].isPaused = false;
            }
        }
    }
}
