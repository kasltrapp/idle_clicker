using UnityEngine;

namespace EasyIdleGame
{
    [System.Serializable]
    public class LevelRewardHolder
    {
        [Tooltip("Level rule for this reward. In levelRewards it is the exact level; in eachLevelReward it is the repeat interval divisor.")]
        public int level;

        [Tooltip("Reward asset granted by this rule. Leave null to create a visible rule placeholder that grants nothing.")]
        public Reward levelReward;
    }
}
