using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "New Custom Stat", menuName = "EasyIdleGame/Player Stats/Custom Stat")]
    public class CustomStat : ScriptableObject, IMetadataProvider
    {
        [CommentArea("Custom Stat", "Defines a bespoke player statistic that can be incremented and tracked. Custom stats can be used as requirements for achievements, unlock locks, or custom progression UI.", "Create a 'Quests Completed' stat, call PlayerStats.Instance.IncrementStat(stat, 1) when a quest ends, then use the same stat in an Achievement lock.")]
        [SerializeField] private string _customStatComment;

        [Tooltip("Display metadata for custom stat UI. Fill this when achievements or stat screens need a name, icon, or description.")]
        public Metadata metadata = new Metadata();
        public Metadata Metadata => metadata ??= new Metadata();
    }
}
