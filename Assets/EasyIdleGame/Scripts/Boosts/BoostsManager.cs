using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that manages boosts
    /// </summary>
    [AddComponentMenu("EasyIdleGame/Managers/Boosts Manager")]
    public class BoostsManager : ManagerHolderBase<BoostsManager, BoostHolder, Boost>,
        IBuyableHolderManager<BoostsManager, BoostHolder, Boost>,
        IUnlockableHolderManager<BoostHolder, Boost>,
        IMultiplierProviderHolderManager
    {
        public IBuyableHolderManager<BoostsManager, BoostHolder, Boost> iBuyable => this;
        public IUnlockableHolderManager<BoostHolder, Boost> iUnlockable => this;

        [Header("Auto")]
        [CommentArea("Boost Runtime State", "Tracks currently active timed boosts and boost inventory holders. This component also applies active boost timers and raises boost-related events.", "Place one BoostsManager in the gameplay scene. Use BoostDisplayer to buy/use boost assets, and use ActiveBoostDisplayer to show active multiplier timers.")]
        [SerializeField] private string _boostRuntimeComment;
        [Tooltip("Runtime list of timed boosts currently active in the game. Usually managed automatically and saved; edit only for debugging or migration.")]
        public List<ActiveBoostHolder> activeBoosts = new();
        public List<BoostHolder> boostsInInventory => base.holders;

        public UnityEvent<Boost> OnItemBought => OnBoostBought;
        public UnityEvent<Boost> OnItemUnlocked { get; } = new UnityEvent<Boost>();
        public UnityEvent<BoostHolder> OnHolderBoughtEvent { get; } = new UnityEvent<BoostHolder>();
        public UnityEvent<BoostHolder> OnHolderUnlockedEvent { get; } = new UnityEvent<BoostHolder>();

        [Header("Events")]
        [Tooltip("Invoked when a boost inventory amount changes from buying, using, rewards, or resets.")]
        public UnityEvent<Boost> OnBoostAmountChanged = new UnityEvent<Boost>();
        public UnityEvent<Boost, BigNumber, SourceType> OnBoostAmountAddedEvent = new UnityEvent<Boost, BigNumber, SourceType>();

        [Tooltip("Invoked after a boost is bought through the normal purchase flow.")]
        public UnityEvent<Boost> OnBoostBought = new();

        [Tooltip("Invoked after a boost is consumed from inventory and its effect is activated.")]
        public UnityEvent<Boost> OnBoostUsed = new();

        [Tooltip("Invoked when active boosts list changes from additions, expirations, or clears.")]
        public UnityEvent OnActiveBoostsChanged = new();

        [Tooltip("Invoked when a TimeSkipBoost is used. OnBoostUsed is invoked for the boost as well.")]
        public UnityEvent<ProfitData> OnTimeSkip = new();

        /// <returns> Return amount of boosts in inventory </returns>
        public BigNumber GetBoostAmountInInventory(Boost boost) => GetOrAddHolder(boost).amount;

        private float _timeSinceLastCheck = 0f;
        private void Update()
        {
            OnTimePassed(Time.deltaTime);

            _timeSinceLastCheck += Time.deltaTime;
            if (_timeSinceLastCheck >= 0.2f)
            {
                _timeSinceLastCheck = 0f;
                iUnlockable.CheckAutoUnlocks();
            }
        }

        public void OnTimePassed(float time)
        {
            bool removedAny = false;
            for (int i = 0; i < activeBoosts.Count; i++)
            {
                activeBoosts[i].timeLeft -= time;

                if (activeBoosts[i].timeLeft <= 0)
                {
                    activeBoosts.RemoveAt(i);
                    i--;
                    removedAny = true;
                }
            }

            if (removedAny)
            {
                OnActiveBoostsChanged?.Invoke();
            }
        }

        public static bool IsAutoProduceOverrideActive(Business business)
        {
            if (Instance == null) return false;

            for (int i = 0; i < Instance.activeBoosts.Count; i++)
            {
                if (Instance.activeBoosts[i].boost is AutoProduceBoost b)
                {
                    if (business.HasGroupMatch(b.targetBusinessGroups, b.group)) return true;
                }
            }
            return false;
        }

        /// <summary> If there is a 'boost' in inventory, it will be used </summary>
        /// <return> if boost was used </return>
        public bool TryUseBoost(Boost boostToAdd)
        {
            if (GetBoostAmountInInventory(boostToAdd) <= 0)
            {
                Log($"TryUseBoost failed: {boostToAdd.GetDisplayName()} - no boost in inventory", LogCategory.Warning);
                return false;
            }

            GetHolder(boostToAdd).amount--;
            OnBoostAmountChanged?.Invoke(boostToAdd);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BoostsManager));

            ActiveBoostHolder addedBoost = boostToAdd.Activate();
            if (addedBoost != null)
                addedBoost.timeLeft *= UpgradeEffectResolver.GetBoostDurationMultiplier(boostToAdd).ToDouble();
            OnBoostUsed.Invoke(boostToAdd);
            Log($"TryUseBoost succeeded: {boostToAdd.GetDisplayName()}");

            if (addedBoost == null) return true;

            if (boostToAdd is not IMergableBoost)
            {
                activeBoosts.Add(addedBoost);
                OnActiveBoostsChanged?.Invoke();
                return true;
            }

            for (int i = 0; i < activeBoosts.Count; i++)
            {
                if (activeBoosts[i].boost is not IMergableBoost mergable) continue;

                if (mergable.TryMergeBoost(activeBoosts[i], addedBoost))
                {
                    OnActiveBoostsChanged?.Invoke();
                    return true;
                }
            }

            activeBoosts.Add(addedBoost);
            OnActiveBoostsChanged?.Invoke();
            return true;
        }

        public ActiveBoostHolder SetActiveBoost(Boost boost, float duration)
        {
            ActiveBoostHolder addedBoost = boost.Activate();
            if (addedBoost == null) return null;

            addedBoost.timeLeft = duration;

            if (boost is IMergableBoost)
            {
                for (int i = 0; i < activeBoosts.Count; i++)
                {
                    if (activeBoosts[i].boost is not IMergableBoost mergable) continue;

                    if (mergable.TryMergeBoost(activeBoosts[i], addedBoost))
                    {
                        OnActiveBoostsChanged?.Invoke();
                        EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BoostsManager));
                        return activeBoosts[i];
                    }
                }
            }

            activeBoosts.Add(addedBoost);
            OnActiveBoostsChanged?.Invoke();
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BoostsManager));
            return addedBoost;
        }

        public bool RemoveActiveBoost(Boost boost)
        {
            int index = activeBoosts.FindIndex(b => b.boost == boost);
            if (index >= 0)
            {
                activeBoosts.RemoveAt(index);
                OnActiveBoostsChanged?.Invoke();
                EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BoostsManager));
                return true;
            }
            return false;
        }

        public bool IsItemUnlocked(Boost boost) => GetOrAddHolder(boost).iUnlockable.IsUnlocked();

        /// <summary>
        /// Adds 'amount' of 'boost' to inventory without any checks
        /// </summary>
        public void AddBoosts(Boost boost, BigNumber amount, SourceType sourceType = SourceType.Generic) => ForceBuyItemsForFree(boost, amount, sourceType);

        public void ForceBuyItemsForFree(Boost boost, BigNumber amount, SourceType sourceType = SourceType.Generic) => GetOrAddHolder(boost).AddAmount(amount, sourceType);

        /// <returns> Returns all boosts of type 'T' in list </returns>
        public List<ActiveBoostHolder> GetBoostsOfType<T>() where T : Boost
        {
            return activeBoosts.FindAll(b => b.boost is T);
        }

        public List<ActiveBoostHolder> GetBoostsOfType(Boost type)
        {
            return activeBoosts.FindAll(b => b.boost.GetType() == type.GetType());
        }

        /// <returns> Returns minimal duration of all boosts of type 'T' </returns>
        public float GetMinDurationOfBoostOfType<T>() where T : Boost
        {
            try
            {
                return (float)activeBoosts.Where((b) => b.boost is T).Min((b) => b.timeLeft);
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }

        public float GetMinDurationOfBoostOfType(UpgradeType type)
        {
            try
            {
                return (float)activeBoosts.Where((b) => b.boost is GenericMultiplierBoost gBoost && gBoost.upgradeBlocks.Any(b => b.upgradeType == type)).Min((b) => b.timeLeft);
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }

        /// <returns> Returns minimal duration of active boosts of 'type' that target 'group', matching the same group rules as <see cref="GetMultiplierOfType(UpgradeType, string)"/> </returns>
        public float GetMinDurationOfBoostOfType(UpgradeType type, string group)
        {
            try
            {
                return (float)activeBoosts
                    .Where((b) => b.boost is GenericMultiplierBoost gBoost
                        && gBoost.iMultiplierProvider.HasUpgradeOfType(type)
                        && Business.HasGroupMatch(null, group, gBoost.TargetBusinessGroups, gBoost.Group))
                    .Min((b) => b.timeLeft);
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }

        public float GetMultiplierOfType<T>(string group) where T : IMultipliableBoost
        {
            float multiplier = 1;

            for (int i = 0; i < activeBoosts.Count; i++)
            {
                if (activeBoosts[i].boost is not T mBoost) continue;

                if (mBoost.Group != group && mBoost.Group != "") continue;

                multiplier *= mBoost.Multiplier;
            }

            return multiplier;
        }

        [System.Obsolete("Use GetMultiplierOfTypeStatic(UpgradeType type, List<BusinessGroup> businessGroupList = null, string group = \"\") instead.")]
        public static BigNumber GetMultiplierOfTypeStatic(UpgradeType type, string group)
        {
            if (Instance == null) return 1;
            return Instance.GetMultiplierOfType(type, group);
        }

        [System.Obsolete("Use GetMultiplierOfType(UpgradeType type, List<BusinessGroup> businessGroupList = null, string group = \"\") instead.")]
        public BigNumber GetMultiplierOfType(UpgradeType type, string group)
        {
            float multiplier = 1;

            for (int i = 0; i < activeBoosts.Count; i++)
            {
                if (activeBoosts[i].boost is not GenericMultiplierBoost mBoost) continue;

                BigNumber boostMultiplier = mBoost.iMultiplierProvider.GetMultiplierOfType(type, group);
                if (boostMultiplier != 1)
                    boostMultiplier = UpgradeEffectResolver.ScaleMultiplierValue(boostMultiplier, UpgradeEffectResolver.GetBoostEffectMultiplier(mBoost, type));

                multiplier *= (float)boostMultiplier.ToDouble();
            }

            return multiplier;
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

            for (int i = 0; i < activeBoosts.Count; i++)
            {
                if (activeBoosts[i].boost is not GenericMultiplierBoost mBoost) continue;

                if (!string.IsNullOrEmpty(mBoost.Group))
                {
                    Log($"Boost '{mBoost.GetDisplayName()}' is using obsolete string group '{mBoost.Group}'. Use TargetBusinessGroups instead.", LogCategory.Warning);
                }

                BigNumber boostMultiplier = mBoost.iMultiplierProvider.GetMultiplierOfType(type, businessGroupList, group);
                if (boostMultiplier != 1)
                    boostMultiplier = UpgradeEffectResolver.ScaleMultiplierValue(boostMultiplier, UpgradeEffectResolver.GetBoostEffectMultiplier(mBoost, type, businessGroupList, group));

                multiplier *= (float)boostMultiplier.ToDouble();
            }

            return multiplier;
        }

        public override BoostHolder GetOrAddHolder(Boost boost)
        {
            BoostHolder holder = GetHolder(boost);
            if (holder == null)
            {
                boostsInInventory.Add(holder = new BoostHolder(boost));
                OnHolderAdded?.Invoke(holder);
            }
            return holder;
        }

        protected override void NotifyHolderReset(BoostHolder holder)
        {
            if (holder?.boost == null) return;
            if (holder.amount <= 0 && holder.totalAmount <= 0 && holder.boughtAmount <= 0) return;

            OnBoostAmountChanged?.Invoke(holder.boost);
        }

        [ContextMenu("Clear Active Boosts")]
        public void ClearActiveBoosts()
        {
            if (activeBoosts.Count == 0) return;
            Log("Clearing active boosts");
            activeBoosts.Clear();
            OnActiveBoostsChanged?.Invoke();
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BoostsManager));
        }

        public List<UpgradeBlock> GetAllUpgradeBlocks()
        {
            return holders.GetAllUpgradeBlocks();
        }
    }
}
