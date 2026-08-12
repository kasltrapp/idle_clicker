using UnityEngine;

namespace EasyIdleGame
{
    [System.Serializable]
    public class DailyRewardHolder
    {
        [Tooltip("Exact login day this reward belongs to. Day 1 starts from DailyRewardsManager.initDate.")]
        public int day;

        [Tooltip("Reward asset granted when this day is claimed. Leave null only for generated calendar placeholders or intentionally empty days.")]
        public Reward reward;

        public bool isClaimed => DailyRewardsManager.Instance.claimedDays.Contains(day);
        public bool isMissed => day < DailyRewardsManager.Instance.GetCurrentDay() && !isClaimed;
        public bool isAvailableToday => day == DailyRewardsManager.Instance.GetCurrentDay() && !isClaimed;
        public bool isUpcoming => day > DailyRewardsManager.Instance.GetCurrentDay();
        public System.Collections.Generic.List<Output> expectedOutputs => DailyRewardsManager.Instance.GetDailyRewards(day);
    }
}
