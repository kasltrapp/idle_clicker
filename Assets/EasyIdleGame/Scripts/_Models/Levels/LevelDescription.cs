using UnityEngine;

namespace EasyIdleGame
{
    [System.Serializable]
    public class LevelDescription
    {
        [Tooltip("First level where this description becomes active. Lower entries are replaced by later entries as the player reaches higher levels.")]
        public int level;

        [Tooltip("Description or title shown while this level range is active. Leave empty if the UI should show no label for this range.")]
        public string name;
    }
}
