using UnityEngine;

namespace EasyIdleGame
{
    [System.Serializable]
    public class BusinessUpgradeLevel : UpgradeLevel, ISerializationCallbackReceiver, IMetadataProvider
    {
        [Header("Overrides")]
        [CommentArea("Level Overrides", "When this upgrade level is reached, these fields optionally override the base business data. This allows for visual progression, new sprites/models, renamed tiers, or expanded limits.", "For a restaurant business, level 1 can show a food cart, level 5 can override the name/icon to Cafe, and level 10 can raise maxAmount for the next growth tier.")]
        [SerializeField] private string _overridesComment;

        [Tooltip("Metadata overrides shown after this upgrade level is reached. Leave fields empty to keep the base business values.")]
        public Metadata metadata = new Metadata();

        [HideInInspector]
        [Tooltip("[LEGACY] Name override migrated into metadata.name.")]
        public string businessName;

        [HideInInspector]
        [Tooltip("[LEGACY] Sprite override migrated into metadata.icon.")]
        public Sprite image;

        [HideInInspector]
        [Tooltip("[LEGACY] Compact override migrated into metadata.iconString.")]
        public string iconString;

        [HideInInspector]
        [Tooltip("[LEGACY] Model override migrated into metadata.model.")]
        public GameObject model;

        [HideInInspector]
        [TextArea(3, 10)]
        [Tooltip("[LEGACY] Description override migrated into metadata.description.")]
        public string description;

        [HideInInspector]
        [SerializeField]
        private MetadataLegacySyncState metadataLegacySyncState = new MetadataLegacySyncState();

        [Tooltip("Owned amount cap while this level is active. -1 means infinite; upgrade overrides and group capacity can still affect the effective cap.")]
        public BigNumber maxAmount_ = -1;

        [Tooltip("Purchase cap while this level is active. -1 means unlimited purchases, still capped by maxAmount_, group capacity, and overrides.")]
        public BigNumber maxBuyableAmount = new BigNumber(-1);

        public Metadata Metadata => GetMetadata();

        public Metadata GetMetadata()
        {
            EasyIdleGame.Metadata.SyncLegacyFields(ref metadata, ref metadataLegacySyncState, businessName, image, iconString, model, description);
            businessName = metadata.name;
            image = metadata.icon;
            iconString = metadata.iconString;
            model = metadata.model;
            description = metadata.description;
            metadataLegacySyncState.Capture(metadata);
            return metadata;
        }

        public string GetIconString()
        {
            return GetMetadata().GetIconString();
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
