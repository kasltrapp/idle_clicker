using UnityEngine;

namespace EasyIdleGame
{
    public interface IMetadataProvider
    {
        public Metadata Metadata { get; }
    }

    /// <summary>
    /// This class holds metadata information for an item, such as its name, icon, model, and description.
    /// was added in v1.7.0 and therefore is not used in older models
    /// </summary>
    [System.Serializable]
    public class Metadata
    {
        [Tooltip("Display name shown in UI for this item. Leave empty only if the owning asset provides its own name.")]
        public string name;

        [Tooltip("Sprite shown by metadata-aware displayers. Leave null if UI should rely on iconString, model, or text-only layout.")]
        public Sprite icon;

        [Tooltip("Short text fallback used when an icon sprite is not available or not enough, for example '$' or 'XP'. Leave empty to fall back to name.")]
        public string iconString;

        [Tooltip("Optional 3D model or visual prefab used by custom UI or gameplay views. Leave null for sprite/text-only presentation.")]
        public GameObject model;

        [TextArea(3, 10)]
        [Tooltip("Longer UI description shown by detail panels and metadata displayers. Leave empty when no detail text is needed.")]
        public string description;

        public string GetIconString()
        {
            if (!string.IsNullOrEmpty(iconString))
            {
                return iconString;
            }

            return $" {name}";
        }

        public static void SyncLegacyFields(
            ref Metadata metadata,
            ref MetadataLegacySyncState syncState,
            string legacyName,
            Sprite legacyIcon,
            string legacyIconString,
            GameObject legacyModel,
            string legacyDescription,
            bool syncIconString = true,
            bool syncModel = true,
            bool syncDescription = true)
        {
            if (metadata == null) metadata = new Metadata();
            if (syncState == null) syncState = new MetadataLegacySyncState();

            if (syncState.HasLegacyChanged(
                    legacyName,
                    legacyIcon,
                    legacyIconString,
                    legacyModel,
                    legacyDescription,
                    syncIconString,
                    syncModel,
                    syncDescription))
            {
                metadata.name = legacyName;
                metadata.icon = legacyIcon;
                if (syncIconString) metadata.iconString = legacyIconString;
                if (syncModel) metadata.model = legacyModel;
                if (syncDescription) metadata.description = legacyDescription;
            }
        }
    }

    [System.Serializable]
    public class MetadataLegacySyncState
    {
        [SerializeField] private string name;
        [SerializeField] private Sprite icon;
        [SerializeField] private string iconString;
        [SerializeField] private GameObject model;
        [SerializeField] private string description;

        public bool HasLegacyChanged(
            string legacyName,
            Sprite legacyIcon,
            string legacyIconString,
            GameObject legacyModel,
            string legacyDescription,
            bool syncIconString,
            bool syncModel,
            bool syncDescription)
        {
            return !StringValuesEqual(name, legacyName)
                || icon != legacyIcon
                || (syncIconString && !StringValuesEqual(iconString, legacyIconString))
                || (syncModel && model != legacyModel)
                || (syncDescription && !StringValuesEqual(description, legacyDescription));
        }

        public void Capture(Metadata metadata, bool syncIconString = true, bool syncModel = true, bool syncDescription = true)
        {
            if (metadata == null) metadata = new Metadata();

            name = metadata.name;
            icon = metadata.icon;
            if (syncIconString) iconString = metadata.iconString;
            if (syncModel) model = metadata.model;
            if (syncDescription) description = metadata.description;
        }

        private static bool StringValuesEqual(string left, string right)
        {
            if (string.IsNullOrEmpty(left) && string.IsNullOrEmpty(right)) return true;
            return left == right;
        }
    }

    public static class MetadataExtensions
    {
        public static string GetDisplayName(this ScriptableObject item)
        {
            if (item is IMetadataProvider metadataProvider && !string.IsNullOrEmpty(metadataProvider.Metadata?.name))
                return metadataProvider.Metadata.name;

            return item ? item.name : "";
        }
    }
}
