using EasyIdleGame.UI;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    public class DailyRewardListDisplayer : BaseDisplayer, IOpenable
    {
        public IOpenable iOpenable => this;

        public GameObject GameObject => gameObject;
        public MonoBehaviour MonoBehaviour => this;
        public AnimationClip OpenAnimation => null;
        public AnimationClip CloseAnimation => null;

        public List<RewardListDayDisplayer> rewards = new List<RewardListDayDisplayer>();

        [Tooltip("The day the reward cycle starts relative to today, -1 means yesterday")]
        public int rewardStartDay = -1;

        [Header("Countdown")]
        [Tooltip("Text element to display the countdown to the next daily reward")]
        public TMPro.TMP_Text nextRewardCountdownText;
        [Tooltip("Format string for the countdown. {0} is hours, {1} is minutes, {2} is seconds.")]
        public string countdownFormat = "Next reward in {0:D2}:{1:D2}:{2:D2}";

        private void Update()
        {
            if (nextRewardCountdownText != null && nextRewardCountdownText.gameObject.activeInHierarchy)
            {
                System.TimeSpan t = DailyRewardsManager.Instance.GetTimeToNextDay();
                nextRewardCountdownText.text = string.Format(countdownFormat, (int)t.TotalHours, t.Minutes, t.Seconds);
            }
        }

        /// <summary> Opens the displayer only if there is a reward to claim </summary>
        public void TryOpenDisplayer()
        {
            //Debug.Log($"DailyRewardListDisplayer: TryOpenDisplayer called. CurrentDay: {DailyRewardsManager.Instance.GetCurrentDay()}, LastClaimedDay: {DailyRewardsManager.Instance.lastClaimedDay}");

            if (DailyRewardsManager.Instance.GetCurrentDay() <= DailyRewardsManager.Instance.lastClaimedDay)
                return;

            OpenDisplayer();
        }

        public void OpenDisplayer()
        {
            iOpenable.Open();
            Redraw();
        }

        public void CloseDisplayer()
        {
            iOpenable.Close();
        }

        override public void Redraw()
        {
            base.Redraw();

            int currentDay = DailyRewardsManager.Instance.GetCurrentDay();

            List<int> missedDays = DailyRewardsManager.Instance.GetMissedDays();

            for (int i = 0; i < rewards.Count; i++)
            {
                int rewardDay = currentDay + rewardStartDay + i;

                List<Output> dailyRewards = DailyRewardsManager.Instance.GetDailyRewards(rewardDay);

                if (dailyRewards.Count > 0)
                {
                    bool toBeClaimedFuture = currentDay <= rewardDay;
                    bool isMissed = missedDays.Contains(rewardDay);

                    rewards[i].gameObject.SetActive(true);
                    rewards[i].UpdateDataAndRedraw(dailyRewards[0], rewardDay, currentDay == rewardDay, !toBeClaimedFuture && !isMissed, !toBeClaimedFuture && isMissed);
                }
                else
                {
                    rewards[i].gameObject.SetActive(false);
                }
            }
        }

        public void ClaimDailyReward()
        {
            DailyRewardsManager.Instance.TryClaimDailyRewards(DailyRewardsManager.Instance.GetCurrentDay(), out _);
            iOpenable.Close();
        }
    }
}