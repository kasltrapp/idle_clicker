using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// ScriptableObject that stores boost data
    /// </summary>
    public abstract class Boost : ScriptableObject, IBuyable, IUnlockable, ISerializationCallbackReceiver, IMetadataProvider
    {
        public IBuyable iBuyable => this;
        public IUnlockable iUnlockable => this;

        [Header("Audio")]
        [Tooltip("Sound played when this boost is bought.")]
        public AudioData buySound;
        [Tooltip("Sound played when this boost is unlocked.")]
        public AudioData unlockSound;
        [Tooltip("Sound played when this boost is activated.")]
        public AudioData activateSound;

        // IBuyable
        public List<Input> Cost => cost;
        public BigNumber CostMultiplier => costMultiplier;
        // I don't see how this could be useful for boosts as this would limit the total buy count, not amount in the inventory
        public BigNumber MaxBuyableAmount => -1;
        public bool UseGeometricBoughtAmount => useGeometricBoughtAmount;
        public AudioData BuySound => buySound;

        // IUnlockable
        public List<Lock> Locks => locks;
        public List<Lock> UpperLocks => null;
        public bool UnlockOnlyOnce => unlockOnlyOnce;
        public AudioData UnlockSound => unlockSound;

        [Space(16)]
        [Header("General")]
        [Tooltip("Display metadata shown by boost UI. New content should use this instead of the legacy name/icon/description fields.")]
        public Metadata metadata = new Metadata();

        [Tooltip("Typed groups this boost belongs to. Used for shared locks and targeting; leave empty if the boost has no specific group.")]
        public List<BoostGroup> boostGroups = new List<BoostGroup>();

        [HideInInspector]
        [Tooltip("[LEGACY] Display name migrated into metadata.name.")]
        public string boostName;

        [HideInInspector]
        [Tooltip("[LEGACY] Sprite migrated into metadata.icon.")]
        public Sprite icon;

        [HideInInspector]
        [Tooltip("[LEGACY] Compact text migrated into metadata.iconString.")]
        public string iconString;

        [HideInInspector]
        [TextArea(3, 10)]
        [Tooltip("[LEGACY] Description migrated into metadata.description.")]
        public string description;

        [HideInInspector]
        [SerializeField]
        private MetadataLegacySyncState metadataLegacySyncState = new MetadataLegacySyncState();

        [Header("Buy")]
        [ConditionalCommentArea("Boost setup", "No cost is defined. This boost can be free, but any store or balance logic will treat it as purchasable without spending resources.", "cost", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _costInfo;
        [Tooltip("Inputs paid to buy one boost. Leave empty for a free boost.")]
        public List<Input> cost;
        [Tooltip("Geometric cost multiplier applied after each obtained boost. If useGeometricBoughtAmount is enabled, only bought boosts affect this scaling.")]
        public float costMultiplier = 1;
        [Tooltip("When enabled, the geometric cost formula uses only the amount bought via TryBuyItems, " +
            "ignoring boosts awarded freely (e.g. from outputs or reward tables). " +
            "Useful so free awards do not raise the purchase price.")]
        public bool useGeometricBoughtAmount = false;

        [Header("Lock")]
        [CommentArea("Unlock Conditions", "List of requirements that must be met before this boost becomes available for purchase. If 'unlockOnlyOnce' is enabled, the boost stays unlocked even if the requirements are later unfulfilled.", "Use locks to reveal a boost after the player reaches a location, owns a certain business amount, or has prestiged once.")]
        [SerializeField] private string _boostLockComment;
        [ConditionalCommentArea("Boost setup", "One or more locks have amount 0 or less. Those locks will be satisfied immediately.", "locks", ConditionalCommentAreaMode.AnyListBigNumberLessThanOrEqual, "inputAmount", 0)]
        [SerializeField] private string _locksInfo;
        [Tooltip("Requirements that must be met before this boost appears or can be bought. Empty list means it is available immediately.")]
        public List<Lock> locks;

        [Tooltip("If enabled, this boost stays unlocked after its locks are met once, even if the player later spends the required resources.")]
        public bool unlockOnlyOnce = true;

        public abstract ActiveBoostHolder Activate();

        public BigNumber GetMaxBuyableAmount(BigNumber currentAmount) => 1;

        public Metadata Metadata => GetMetadata();

        public string GetIconString()
        {
            return GetMetadata().GetIconString();
        }

        public Metadata GetMetadata()
        {
            EasyIdleGame.Metadata.SyncLegacyFields(ref metadata, ref metadataLegacySyncState, boostName, icon, iconString, null, description, syncModel: false);
            boostName = metadata.name;
            icon = metadata.icon;
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
