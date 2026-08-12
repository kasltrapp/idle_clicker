using UnityEngine;

namespace EasyIdleGame
{
    public abstract class ItemGroup<T> : ScriptableObject, IMetadataProvider where T : ScriptableObject
    {
        [Tooltip("Display metadata for custom group UI.")]
        public Metadata metadata = new Metadata();
        public Metadata Metadata => metadata ??= new Metadata();
    }
}
