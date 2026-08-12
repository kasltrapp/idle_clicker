using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Scriptable object that stores manager data
    /// </summary>
    [CreateAssetMenu(fileName = "newManager", menuName = "EasyIdleGame/Managers/Manager", order = 1)]
    public class Manager : ScriptableObject, IBuyable, IUpgradable, IUnlockable, ISerializationCallbackReceiver, IMetadataProvider
    {
        [CommentArea("Manager", "Managers automate business production and provide both passive and active boosts to specific businesses. They can be bought, leveled up to increase their bonuses, and activated for temporary powerful effects.", "Create a Farm Manager, assign it to one or more Business assets, set autoProduceOnLevel to 0, add a passive speed UpgradeBlock, then show it with a ManagerDisplayer.")]
        [SerializeField] private string _managerComment;

        public IBuyable iBuyable => this;
        public IUpgradable iUpgradable => this;
        public IUnlockable iUnlockable => this;

        [Header("Audio")]
        [Tooltip("Sound played when this manager is bought.")]
        public AudioData buySound;
        [Tooltip("Sound played when this manager is unlocked.")]
        public AudioData unlockSound;

        // IBuyable
        public List<Input> Cost => cost;
        public BigNumber CostMultiplier => costMultiplier;
        public BigNumber MaxBuyableAmount => maxBuyableAmount;
        public bool UseGeometricBoughtAmount => useGeometricBoughtAmount;
        public AudioData BuySound => buySound;

        // IUpgradable
        public List<Input> UpgradeCost => upgradeCost;
        public float UpgradeCostMultiplier => upgradeCostMultiplier;
        public bool AutoUpgradeForFree => autoUpgradeForFree;
        public bool AutoUpgradeToFirstLevelFree => autoUpgradeToFirstLevelFree;
        public BigNumber MaxLevel => BigNumber.MaxValue;
        public UpgradeLevel[] UpgradeLevels => new UpgradeLevel[0];

        // IUnlockable
        public List<Lock> Locks => locks;
        public List<Lock> UpperLocks => null;
        public bool UnlockOnlyOnce => unlockOnlyOnce;
        public AudioData UnlockSound => unlockSound;

        [Header("General")]
        [Tooltip("Display metadata shown by manager UI. New content should use this instead of the legacy name/icon/description fields.")]
        public Metadata metadata = new Metadata();

        [HideInInspector]
        [Tooltip("[LEGACY] Display name migrated into metadata.name.")]
        public string managerName;

        [HideInInspector]
        [TextArea(3, 10)]
        [Tooltip("[LEGACY] Description migrated into metadata.description.")]
        public string description;

        [HideInInspector]
        [Tooltip("[LEGACY] Sprite migrated into metadata.icon.")]
        public Sprite image;

        [HideInInspector]
        [Tooltip("[LEGACY] Compact text migrated into metadata.iconString.")]
        public string iconString;

        [Tooltip("Typed groups this manager belongs to. Used for targeting upgrades and boosts; leave empty if the manager has no specific group.")]
        public List<ManagerGroup> managerGroups = new List<ManagerGroup>();

        [HideInInspector]
        [SerializeField]
        private MetadataLegacySyncState metadataLegacySyncState = new MetadataLegacySyncState();

        [Header("Buy")] // ------------------------------- BUY ------------------------------- 
        [ConditionalCommentArea("Manager setup", "No cost is defined. This manager can be free, but any store or balance logic will treat it as purchasable without spending resources.", "cost", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _costInfo;
        [Tooltip("Inputs paid to buy one manager copy. Leave empty for a free manager.")]
        public List<Input> cost;
        [Tooltip("Geometric cost multiplier applied after each obtained copy. If useGeometricBoughtAmount is enabled, only bought copies affect this scaling.")]
        public float costMultiplier = 1;

        [Tooltip("Maximum manager copies the player can own. -1 means unlimited; this limits copy count, not upgrade level.")]
        public int maxBuyableAmount = -1;

        [Tooltip("When enabled, the geometric cost formula uses only the amount bought via TryBuyItems, " +
            "ignoring free grants from outputs or rewards. Useful so awarded managers do not raise the purchase price.")]
        public bool useGeometricBoughtAmount = false;

        [Header("Lock")] // ------------------------------- LOCKS ------------------------------- 
        [ConditionalCommentArea("Manager setup", "One or more locks have amount 0 or less. Those locks will be satisfied immediately.", "locks", ConditionalCommentAreaMode.AnyListBigNumberLessThanOrEqual, "inputAmount", 0)]
        [SerializeField] private string _locksInfo;
        [Tooltip("Requirements that must be met before this manager can be shown or bought. Empty list means it is available immediately.")]
        public List<Lock> locks;
        [Tooltip("If enabled, this manager stays unlocked after its locks are met once, even if the player later spends the required resources.")]
        public bool unlockOnlyOnce = true;

        [Header("Upgrades")] // ------------------------------- UPGRADES ------------------------------- 
        [Tooltip("If enabled, the manager upgrades automatically for free when enough copies are owned.")]
        public bool autoUpgradeForFree = true;

        [Tooltip("If enabled, the first manager upgrade level is granted without charging upgradeCost. Later manual upgrades still use upgradeCost unless autoUpgradeForFree handles them.")]
        public bool autoUpgradeToFirstLevelFree = true;

        [Tooltip("Inputs paid when manually upgrading this manager. Leave empty for free manual upgrades.")]
        public List<Input> upgradeCost;
        [Min(0.01f)]
        [Tooltip("Geometric multiplier applied to upgradeCost after each manager upgrade level.")]
        public int upgradeCostMultiplier = 2;

        [Header("Passive boost")] // ------------------------------- PASSIVE BOOST ------------------------------- 
        [ConditionalCommentArea("Manager setup", "No passive boosts are configured. This manager can still automate production, but it will not provide passive multiplier effects.", "passiveBoosts", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _passiveBoostsInfo;
        [Tooltip("Multiplier blocks applied whenever the manager is owned. Empty list means the manager can still automate but provides no passive stat bonus.")]
        public List<UpgradeBlock> passiveBoosts = new List<UpgradeBlock>() { new UpgradeBlock(UpgradeType.speed, 2) };

        [Tooltip("The manager level at which auto-production is enabled for its assigned businesses. " +
            "0 = auto-produce as soon as the manager is bought (level >= 1). " +
            "Any positive value = auto-produce only once the manager reaches that level.")]
        [Min(0)]
        public int autoProduceOnLevel = 0;

        [Tooltip("Manager level required for auto-collection on assigned businesses. 0 means as soon as the manager is bought.")]
        [Min(0)]
        public int autoCollectOnLevel = 0;

        [Min(0.01f)]
        [Tooltip("Per-level scaling applied to passive boost blocks. Active boosts are not affected by this multiplier.")]
        public float businessSpeedMultiplier = 2;

        [Header("Active boost")] // ------------------------------- ACTIVE BOOST ------------------------------- 
        [CommentArea("Active Boosts", "Managers can be activated to grant temporary boosts. 'activeDuration' specifies how long the boost lasts, while 'cooldownDuration' is the wait time before it can be activated again.", "For a temporary production burst, set activeDuration to 5 minutes, cooldownDuration to 30 minutes, and add an activeUpgrade block like production x5.")]
        [SerializeField] private string _activeBoostComment;
        [Tooltip("How long activeUpgrade effects last after activation. Zero duration makes the active effect expire immediately.")]
        public Duration activeDuration;

        [Tooltip("Cooldown required before this manager can be activated again.")]
        public Duration cooldownDuration = new Duration(0, 0, 5, 0);

        [ConditionalCommentArea("Manager setup", "No active boosts are configured. Activating this manager can still consume cooldown/duration logic, but it will not apply active multiplier effects.", "activeUpgrade", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _activeUpgradeInfo;
        [Tooltip("Multiplier blocks applied only while this manager's active boost is running. Empty list means activation only uses duration/cooldown state.")]
        public List<UpgradeBlock> activeUpgrade;

        public float GetActiveMultiplierOfType(UpgradeType type) => activeUpgrade.GetMultiplierOfType(type);

        public BigNumber GetMaxBuyableAmount(BigNumber currentAmount) => 1;

        public Metadata Metadata => GetMetadata();

        public string GetIconString()
        {
            return GetMetadata().GetIconString();
        }

        public Metadata GetMetadata()
        {
            EasyIdleGame.Metadata.SyncLegacyFields(ref metadata, ref metadataLegacySyncState, managerName, image, iconString, null, description, syncModel: false);
            managerName = metadata.name;
            image = metadata.icon;
            iconString = metadata.iconString;
            description = metadata.description;
            metadataLegacySyncState.Capture(metadata, syncModel: false);
            return metadata;
        }

        public void OnBeforeSerialize()
        {
            GetMetadata();
        }

        public void OnAfterDeserialize()
        {
            GetMetadata();
        }
    }
}
