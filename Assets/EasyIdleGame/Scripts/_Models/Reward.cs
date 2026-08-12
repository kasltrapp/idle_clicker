using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "Reward", menuName = "EasyIdleGame/Rewards/Reward", order = 0)]
    public class Reward : ScriptableObject, ISerializationCallbackReceiver
    {
        [CommentArea("Reward", "Defines a collection of drop tables granted to the player at once. Rewards can grant currencies, businesses, boosts, managers, or any mix of those outputs.", "Create a Reward asset for a daily login prize, assign one or more DropTables, then reference that Reward from DailyRewardsManager or a LevelRewardHolder.")]
        [SerializeField] private string _rewardComment;

        [ConditionalCommentArea("Reward setup", "No reward tables are configured. Applying this reward will not grant anything.", "rewardTables", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _rewardTablesInfo;
        [Tooltip("Drop tables applied when this reward is granted. Empty rewards are valid but grant nothing; use multiple tables when a reward needs separate guaranteed and weighted pools.")]
        public List<DropTable> rewardTables = new List<DropTable>();

        [Header("Audio")]
        [Tooltip("Sound played when this reward asset is claimed, such as from daily rewards.")]
        public AudioData claimSound;

        [HideInInspector]
        [UnityEngine.Serialization.FormerlySerializedAs("rewards")]
        public List<Output> _legacyRewards;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (_legacyRewards != null && _legacyRewards.Count > 0)
            {
                if (rewardTables == null) rewardTables = new List<DropTable>();

                DropTable table = new DropTable();
                table.strategy = DropStrategy.All;
                table.outputs = new List<Output>(_legacyRewards);

                rewardTables.Add(table);
                _legacyRewards.Clear();
            }
        }
    }
}
