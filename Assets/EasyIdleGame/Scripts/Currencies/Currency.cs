using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Scriptable object that stores currency data
    /// </summary>
    [CreateAssetMenu(fileName = "Currency", menuName = "EasyIdleGame/Currencies/Currency", order = 1)]
    public class Currency : ScriptableObject, ISerializationCallbackReceiver, IMetadataProvider
    {
        [CommentArea("Currency", "Represents a distinct resource type used in the game economy. Currencies act as inputs (costs) or outputs (rewards) across businesses, upgrades, shops, daily rewards, and other systems.", "Create a Coin currency, use it as the currencyInput in a Business cost, add it as the currencyOutput in a DropTable, then assign the same asset to a CurrencyDisplayer to show the player's balance.")]
        [SerializeField] private string _currencyComment;

        [Header("General")]
        [Tooltip("Display metadata shown by currency UI. New content should use this instead of the legacy name/icon/description fields.")]
        public Metadata metadata = new Metadata();

        [HideInInspector]
        [Tooltip("[LEGACY] Sprite migrated into metadata.icon.")]
        public Sprite image;

        [HideInInspector]
        [Tooltip("[LEGACY] Display name migrated into metadata.name.")]
        public string currencyName;

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

        [Tooltip("Typed groups this currency belongs to. Used for shared capacities; leave empty if the currency has no shared cap group.")]
        public System.Collections.Generic.List<CurrencyGroup> currencyGroups = new System.Collections.Generic.List<CurrencyGroup>();

        [Tooltip("Maximum amount the player can hold for this currency. -1 means infinite; currency groups and override upgrades can further affect effective capacity.")]
        public BigNumber maxAmount = -1;

        [Header("Audio")]
        [Tooltip("Sound played when this currency is collected.")]
        public AudioData collectSound;

        /// <returns> if player has enough currency </returns>
        public bool HasEnough(BigNumber amount) => CurrencyManager.Instance.CanBuy(this, amount);

        public Metadata Metadata => GetMetadata();

        public string GetIconString()
        {
            return GetMetadata().GetIconString();
        }

        public Metadata GetMetadata()
        {
            EasyIdleGame.Metadata.SyncLegacyFields(ref metadata, ref metadataLegacySyncState, currencyName, image, iconString, null, description, syncModel: false);
            currencyName = metadata.name;
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
