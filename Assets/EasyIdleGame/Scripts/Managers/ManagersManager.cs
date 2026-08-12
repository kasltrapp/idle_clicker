using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that manages all managers
    /// </summary>
    [AddComponentMenu("EasyIdleGame/Managers/Managers Manager")]
    public class ManagersManager : ManagerHolderBase<ManagersManager, ManagerHolder, Manager>,
        IBuyableHolderManager<ManagersManager, ManagerHolder, Manager>,
        IUpgradableHolderManager<ManagerHolder, Manager>,
        IUnlockableHolderManager<ManagerHolder, Manager>
    {
        public IBuyableHolderManager<ManagersManager, ManagerHolder, Manager> iBuyable => this;
        public IUpgradableHolderManager<ManagerHolder, Manager> iUpgradable => this;
        public IUnlockableHolderManager<ManagerHolder, Manager> iUnlockable => this;

        public UnityEvent<Manager> OnItemUnlocked { get; } = new UnityEvent<Manager>();
        public UnityEvent<ManagerHolder> OnHolderBoughtEvent { get; } = new UnityEvent<ManagerHolder>();
        public UnityEvent<ManagerHolder> OnHolderUnlockedEvent { get; } = new UnityEvent<ManagerHolder>();
        public UnityEvent<ManagerHolder> OnHolderUpgradedEvent { get; } = new UnityEvent<ManagerHolder>();

        [Tooltip("Invoked after a manager is bought through the normal purchase flow.")]
        public UnityEvent<Manager> OnManagerBought = new();

        [Tooltip("Invoked when manager amount, upgrade level, active state, or cooldown-relevant state changes.")]
        public UnityEvent<Manager> OnManagerAmountChanged = new();
        public UnityEvent<Manager, BigNumber, SourceType> OnManagerAmountAddedEvent = new();


        public UnityEvent<Manager> OnItemBought => OnManagerBought;

        public void ForceBuyItemsForFree(Manager manager, BigNumber amount, SourceType sourceType = SourceType.Generic) => GetOrAddHolder(manager).AddManagerCount(amount, sourceType);

        public override ManagerHolder GetOrAddHolder(Manager manager)
        {
            ManagerHolder h = GetHolder(manager);
            if (h == null)
            {
                holders.Add(h = new ManagerHolder(manager));
                OnHolderAdded?.Invoke(h);
            }
            return h;
        }

        protected override void NotifyHolderReset(ManagerHolder holder)
        {
            if (holder?.manager == null) return;
            if (holder.totalCount <= 0 && holder.count <= 0 && holder.boughtAmount <= 0 && holder.level <= 0 && holder.activeTimeLeft <= 0 && holder.cooldownTimeLeft <= 0) return;

            OnManagerAmountChanged?.Invoke(holder.manager);
        }

        public void AddManagers(Manager manager, BigNumber amount, SourceType sourceType = SourceType.Generic) => ForceBuyItemsForFree(manager, amount, sourceType);

        public static float GetPassiveMultiplierOfType(UpgradeType type, Business business)
        {
            if (Instance == null) return 1;

            ManagerHolder holder = Instance.GetHolder(business.manager);

            if (holder == null) return 1;

            BigNumber multiplier = holder.GetMultiplierOfType(type);
            if (multiplier != 1)
                multiplier = UpgradeEffectResolver.ScaleMultiplierValue(multiplier, UpgradeEffectResolver.GetManagerEffectMultiplier(holder, type, business));

            return (float)multiplier.ToDouble();
        }

        // experimental
        private float _timeSinceLastCheck = 0f;
        public void Update()
        {
            OnTimePassed(Time.deltaTime);

            _timeSinceLastCheck += Time.deltaTime;
            if (_timeSinceLastCheck >= 0.2f)
            {
                _timeSinceLastCheck = 0f;
                iUnlockable.CheckAutoUnlocks();
            }
        }

        public void OnTimePassed(float seconds)
        {
            foreach (ManagerHolder holder in holders)
                holder.OnTimePassed(seconds);
        }

        public static float GetActiveMultiplierOfType(UpgradeType type, Business business)
        {
            if (Instance == null) return 1;

            ManagerHolder holder = Instance.GetHolder(business.manager);

            if (holder == null || holder.activeTimeLeft <= 0) return 1;

            BigNumber multiplier = holder.manager.GetActiveMultiplierOfType(type);
            if (multiplier != 1)
                multiplier = UpgradeEffectResolver.ScaleMultiplierValue(multiplier, UpgradeEffectResolver.GetManagerEffectMultiplier(holder, type, business));

            return (float)multiplier.ToDouble();
        }

        public static Business[] GetBusinessForManager(Manager manager)
        {
            return BusinessesManager.GetAllScriptableObjects_Static().Where(b => b.manager == manager).ToArray();
        }

        public bool TryActivateManager(Manager manager)
        {
            bool result = GetOrAddHolder(manager).TryActivate();
            if (!result)
                Log($"TryActivateManager failed: {manager.GetDisplayName()} - on cooldown or not owned", LogCategory.Warning);
            else
                Log($"TryActivateManager succeeded: {manager.GetDisplayName()}");
            return result;
        }
    }
}
