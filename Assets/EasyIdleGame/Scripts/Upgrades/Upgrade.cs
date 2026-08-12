using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "newUpgrade", menuName = "EasyIdleGame/Upgrades/Upgrade", order = 1)]
    public class Upgrade : ScriptableObject, IBuyable, IUnlockable, IMultiplierProvider, ISerializationCallbackReceiver, IMetadataProvider
    {
        [CommentArea("Upgrade", "A purchasable or unlockable modifier. Standard upgrades multiply stats, override upgrades set absolute caps or limits, and override modifiers improve existing override values.", "Create a 2x Production upgrade by adding an UpgradeBlock with type Production and value 2. Create an idle cap upgrade by adding an OverrideUpgradeBlock for idleProductionTimeCap, then assign the asset to an UpgradeDisplayer.")]
        [SerializeField] private string _upgradeComment;

        public IBuyable iBuyable => this;
        public IUnlockable iUnlockable => this;
        public IMultiplierProvider iMultiplierProvider => this;

        [Header("Audio")]
        [Tooltip("Sound played when this upgrade is bought.")]
        public AudioData buySound;
        [Tooltip("Sound played when this upgrade is unlocked.")]
        public AudioData unlockSound;

        // IBuyable
        public List<Input> Cost => cost;
        public BigNumber CostMultiplier => priceMultiplier_;
        public BigNumber MaxBuyableAmount => maxBuyableAmount;
        public bool UseGeometricBoughtAmount => false;
        public AudioData BuySound => buySound;

        // IUnlockable
        public List<Lock> Locks => locks;
        public List<Lock> UpperLocks => null;
        public bool UnlockOnlyOnce => unlockOnlyOnce;
        public AudioData UnlockSound => unlockSound;

        // IMultiplierProvider
        public UpgradeBlock[] Blocks => upgrades.ToArray();

        [System.Obsolete("Use targetBusinessGroups instead.")]
        public string Group => group;
        public List<BusinessGroup> TargetBusinessGroups => targetBusinessGroups;

        [Header("General")]
        [Tooltip("Display metadata shown by upgrade UI. New content should use this instead of the legacy name/icon/description fields.")]
        public Metadata metadata = new Metadata();

        [HideInInspector]
        [Tooltip("[LEGACY] Sprite migrated into metadata.icon.")]
        public Sprite icon;

        [HideInInspector]
        [Tooltip("[LEGACY] Display name migrated into metadata.name.")]
        public string upgradeName;

        [HideInInspector]
        [TextArea(3, 10)]
        [Tooltip("[LEGACY] Description migrated into metadata.description.")]
        public string description;

        [HideInInspector]
        [SerializeField]
        private MetadataLegacySyncState metadataLegacySyncState = new MetadataLegacySyncState();

        [Header("Deprecated")]
        [Tooltip("[DEPRECATED] Legacy string group of businesses affected by this upgrade. Use targetBusinessGroups for new content.")]
        [System.Obsolete("Use targetBusinessGroups instead.")]
        public string group;

        [Tooltip("Business groups affected by this upgrade. Empty means all businesses; multiple groups target any matching business.")]
        public List<BusinessGroup> targetBusinessGroups = new List<BusinessGroup>();

        [Tooltip("Typed groups this upgrade belongs to. Used for shared locks and targeting; leave empty if the upgrade has no specific group.")]
        public List<UpgradeGroup> upgradeGroups = new List<UpgradeGroup>();

        [Header("Upgrade Effects")]
        [CommentArea("Upgrade Modifiers", "Upgrade blocks provide global or context-targeted multipliers. Override upgrades set absolute values such as caps. Override modifiers improve matching override values, optionally targeting a specific source upgrade.", "Use plain upgrade blocks for effects like x2 production or 0.8x purchaseCost. Add filters on an UpgradeBlock when it should only affect a currency, reward source, boost, merge recipe, business level range, or active/offline/manual context.")]
        [SerializeField] private string _upgradeEffectsComment;
        [ConditionalCommentArea("Upgrade setup", "No upgrade effects are configured. This upgrade can still be bought/unlocked, but it will not change any multiplier or override behavior.", "upgrades", ConditionalCommentAreaMode.AllListsEmpty, "overrideUpgrades", "overrideUpgradeModifiers")]
        [SerializeField] private string _upgradeEffectsInfo;
        [Tooltip("Multiplier effects applied while this upgrade is owned. Empty list means the upgrade can be bought/unlocked but changes no multiplier behavior. Add filters on a block for context-aware effects.")]
        public List<UpgradeBlock> upgrades = new List<UpgradeBlock>() { new UpgradeBlock(UpgradeType.speed, 2) };

        [CommentArea("Override Source Values", "Use this list when the upgrade creates an override value by itself, such as setting a maximum capacity, time cap, or other absolute limit. overrideUpgradeLevelMultiplier scales that value for each owned level. Existing override values are still resolved by choosing the highest final value.", "A 'Bigger Wallet' upgrade can create maxCurrencyAmount = 1000 for Coins. If the player owns several wallet upgrades, the system uses the highest modified maxCurrencyAmount.")]
        [SerializeField] private string _overrideUpgradesComment;
        [Tooltip("Absolute override values created by this upgrade, such as caps or limits. Empty list means this upgrade creates no override values.")]
        public List<OverrideUpgradeBlock> overrideUpgrades = new List<OverrideUpgradeBlock>();

        [CommentArea("Override Value Modifiers", "Use this list for upgrades-for-upgrades. These entries do not create an override value on their own; they only improve matching values from overrideUpgrades. Leave targetUpgrade empty to modify all matching overrides, or assign targetUpgrade to modify only that upgrade's override. Final source value resolves as (override value + additive modifiers) * multiplicative modifiers.", "A 'Wallet Efficiency' upgrade can multiply every maxCurrencyAmount override by 1.5, or target only the Bigger Wallet upgrade if targetUpgrade is assigned.")]
        [SerializeField] private string _overrideUpgradeModifiersComment;
        [Tooltip("Modifiers that improve matching override values created by this or other upgrades. Empty list means this upgrade does not modify override values.")]
        public List<OverrideUpgradeModifierBlock> overrideUpgradeModifiers = new List<OverrideUpgradeModifierBlock>();

        [Tooltip("Per-owned-level scaling for this upgrade's override values and override modifier values. Use 1 to keep values constant across owned levels.")]
        public float overrideUpgradeLevelMultiplier = 1.2f;

        [Header("Timed Upgrades")]
        [Min(0)]
        [Tooltip("Delay in seconds before a purchased upgrade becomes active. 0 applies immediately; positive values create pending upgrades controlled by UpgradesManager queueMode.")]
        public float upgradeDurationInSeconds;

        [Header("Buying")] // ------------------------------- BUYING ------------------------------- 
        [ConditionalCommentArea("Upgrade setup", "No cost is defined. This upgrade can be free, but any store or balance logic will treat it as purchasable without spending resources.", "cost", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _costInfo;
        [Tooltip("Inputs paid to buy one upgrade level. Leave empty for a free upgrade.")]
        public List<Input> cost;
        [Tooltip("Maximum owned levels for this upgrade. 0 means it cannot be bought; higher values allow repeated purchases that can stack effects.")]
        public int maxBuyableAmount = 3;
        [Tooltip("Geometric cost multiplier applied after each obtained upgrade level.")]
        public float priceMultiplier_ = 2.25f;

        [Header("Lock")] // ------------------------------- LOCKS -------------------------------
        [ConditionalCommentArea("Upgrade setup", "One or more locks have amount 0 or less. Those locks will be satisfied immediately.", "locks", ConditionalCommentAreaMode.AnyListBigNumberLessThanOrEqual, "inputAmount", 0)]
        [SerializeField] private string _locksInfo;
        [Tooltip("Requirements that must be met before this upgrade appears or can be bought. Empty list means it is available immediately.")]
        public List<Lock> locks;

        [Tooltip("If enabled, this upgrade stays unlocked after its locks are met once, even if the player later spends the required resources.")]
        public bool unlockOnlyOnce;

        //public List<UpgradeBlock> GetUpgradesOfType(UpgradeType type) => upgrades.FindAll(x => x.upgradeType == type);

        public BigNumber GetMaxBuyableAmount(BigNumber currentAmount) => 1;

        public Metadata Metadata => GetMetadata();

        public Metadata GetMetadata()
        {
            EasyIdleGame.Metadata.SyncLegacyFields(
                ref metadata,
                ref metadataLegacySyncState,
                upgradeName,
                icon,
                null,
                null,
                description,
                syncIconString: false,
                syncModel: false);

            upgradeName = metadata.name;
            icon = metadata.icon;
            description = metadata.description;
            metadataLegacySyncState.Capture(metadata, syncIconString: false, syncModel: false);
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

        public OverrideUpgradeBlock GetOverrideUpgradeBlock(OverrideUpgradeType type, Currency currency = null, Business business = null)
        {
            if (overrideUpgrades == null) return null;

            foreach (var ou in overrideUpgrades)
            {
                if (ou != null && ou.Matches(type, currency, business)) return ou;
            }

            return null;
        }

        public BigNumber? GetOverrideMultiplier(OverrideUpgradeType type, Currency currency = null, Business business = null)
        {
            return GetOverrideUpgradeBlock(type, currency, business)?.value;
        }

    }
}
