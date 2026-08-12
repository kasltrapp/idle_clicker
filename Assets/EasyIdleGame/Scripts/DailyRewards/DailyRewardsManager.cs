using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    public class DailyRewardsManager : ManagerBase<DailyRewardsManager>
    {
        [Header("Rewards Setup")]
        [CommentArea("Rewards", "The system provides rewards based on total logged-in days. 'dailyRewards' grants rewards on specific target days. 'eachDayReward' grants rewards whenever the current day is evenly divisible by the reward's target day (level field), optionally scaled if 'scaleEachDayReward' is enabled.", "Add a day 1 Reward to dailyRewards for a welcome gift, then add a LevelRewardHolder with level 7 to eachDayReward for a weekly bonus. Use DailyRewardListDisplayer to show and claim the current calendar.")]
        [SerializeField] private string _rewardsSetupComment;
        [ConditionalCommentArea("Daily rewards setup", "No daily reward rules are configured. Players will not receive daily login rewards until dailyRewards or eachDayReward has entries.", "dailyRewards", ConditionalCommentAreaMode.AllListsEmpty, "eachDayReward")]
        [SerializeField] private string _dailyRewardsInfo;
        [Tooltip("Rewards available on exact login days. Each holder's day must match the current login day to be granted.")]
        public List<DailyRewardHolder> dailyRewards = new List<DailyRewardHolder>();

        [Tooltip("If enabled, all exact-day dailyRewards entries matching the requested day are combined. Disable to preserve the legacy first-match behavior.")]
        public bool combineExactDayRewards = true;

        [Tooltip("Repeating rewards. If the current day is divisible by the entry's level value, that reward is granted.")]
        public List<LevelRewardHolder> eachDayReward = new List<LevelRewardHolder>();

        [Tooltip("If enabled, repeating eachDayReward outputs are multiplied by the current day number before being granted.")]
        public bool scaleEachDayReward = false;

        [Tooltip("Hours that must pass before the next reward day begins. Default is 24; lower values are useful for testing or event calendars.")]
        [Min(0.01f)]
        public float dayLengthHours = 24f;

        [Tooltip("Date and time used as day 1 for this reward cycle. Usually managed by save/load; edit only to restart or offset the calendar.")]
        public DateTime initDate = DateTime.Now;

        [Tooltip("Day numbers already claimed by the player. Usually managed by the reward system and saved; edit only for debugging or migration.")]
        public List<int> claimedDays = new List<int>();
        public int lastClaimedDay
        {
            get
            {
                if (claimedDays.Count == 0) return 0;
                return Mathf.Max(claimedDays.ToArray());
            }
        }

        [Tooltip("Invoked after a daily reward is claimed. Arguments are the claimed day number and the outputs actually granted.")]
        public UnityEvent<int, List<Output>> OnDailyRewardClaimed = new UnityEvent<int, List<Output>>();

        [Tooltip("Invoked once for each Reward asset granted by a daily reward claim.")]
        public UnityEvent<Reward> OnRewardClaimed = new UnityEvent<Reward>();

        public List<Output> GetDailyRewards(int day)
        {
            if (day <= 0) return new List<Output>();

            List<Output> combinedReward = new List<Output>();

            foreach (DailyRewardHolder rewardHolder in dailyRewards)
            {
                if (rewardHolder.day != day || rewardHolder.reward == null) continue;

                combinedReward.AddRange(RewardCalculator.GetAverageOutputs(rewardHolder.reward.rewardTables, 1, RewardCalculator.CreateContext(SourceType.RecurringReward)));
                if (!combineExactDayRewards) break;
            }

            foreach (LevelRewardHolder rewardHolder in eachDayReward)
            {
                if (rewardHolder.level <= 0 || day % rewardHolder.level != 0 || rewardHolder.levelReward == null) continue;

                BigNumber multiplier = scaleEachDayReward ? day : 1;
                combinedReward.AddRange(RewardCalculator.GetAverageOutputs(rewardHolder.levelReward.rewardTables, multiplier, RewardCalculator.CreateContext(SourceType.RecurringReward)));
            }

            return combinedReward.Squash();
        }

        public int GetCurrentDay()
        {
            TimeSpan timeSpan = DateTime.Now - initDate;
            return (int)(timeSpan.TotalHours / dayLengthHours) + 1;
        }

        [ContextMenu("Simulate Next Day")]
        public void SimulateNextDay()
        {
            initDate = initDate.AddHours(-dayLengthHours);
            Log($"Simulated passing of {dayLengthHours} hours. Current Day is now {GetCurrentDay()}", LogCategory.Verbose);
        }

        public TimeSpan GetTimeToNextDay()
        {
            TimeSpan timeSpan = DateTime.Now - initDate;
            double hoursPassed = timeSpan.TotalHours;
            double hoursToNextDay = dayLengthHours - (hoursPassed % dayLengthHours);
            return TimeSpan.FromHours(hoursToNextDay);
        }

        public bool TryClaimDailyRewards(int day, out List<Output> outputs)
        {
            outputs = null;
            if (day <= 0)
            {
                Log($"Cannot claim daily rewards for non-positive day: {day}", LogCategory.Warning);
                return false;
            }
            if (day > GetCurrentDay())
            {
                Log($"Cannot claim daily rewards for future day: {day} (current day: {GetCurrentDay()})", LogCategory.Warning);
                return false;
            }
            if (claimedDays.Contains(day)) return false;

            UpgradeEffectContext context = RewardCalculator.CreateContext(SourceType.RecurringReward);

            List<Output> allOutputs = new List<Output>();
            List<Reward> claimedRewards = new List<Reward>();
            foreach (DailyRewardHolder rewardHolder in dailyRewards)
            {
                if (rewardHolder.day != day || rewardHolder.reward == null) continue;

                allOutputs.AddRange(RewardCalculator.GenerateDrops(rewardHolder.reward.rewardTables, 1, context));
                claimedRewards.Add(rewardHolder.reward);
                if (!combineExactDayRewards) break;
            }

            foreach (LevelRewardHolder rewardHolder in eachDayReward)
            {
                if (rewardHolder.level <= 0 || day % rewardHolder.level != 0 || rewardHolder.levelReward == null) continue;

                BigNumber multiplier = scaleEachDayReward ? day : 1;
                allOutputs.AddRange(RewardCalculator.GenerateDrops(rewardHolder.levelReward.rewardTables, multiplier, context));
                claimedRewards.Add(rewardHolder.levelReward);
            }

            if (claimedRewards.Count == 0)
            {
                Log($"Cannot claim daily rewards for day {day} - no rewards are configured for that day.", LogCategory.Warning);
                return false;
            }

            allOutputs.ForEach(x => x.OnProductionRoundFinished_Apply(1, context));

            claimedDays.Add(day);
            Log($"Claimed daily rewards for day: {day}", LogCategory.Verbose);

            outputs = allOutputs.Squash();
            foreach (Reward reward in claimedRewards)
            {
                OnRewardClaimed?.Invoke(reward);
            }
            OnDailyRewardClaimed?.Invoke(day, outputs);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(DailyRewardsManager));
            return true;
        }

        public List<int> GetMissedDays()
        {
            List<int> missedDays = new List<int>();
            int currentDay = GetCurrentDay();
            for (int day = lastClaimedDay + 1; day <= currentDay; day++)
            {
                if (GetDailyRewards(day).Count > 0)
                    missedDays.Add(day);
            }
            return missedDays;
        }

        public List<DailyRewardHolder> GetCalendarView(int daysBefore, int daysAfter)
        {
            List<DailyRewardHolder> view = new List<DailyRewardHolder>();
            int currentDay = GetCurrentDay();

            for (int i = -daysBefore; i <= daysAfter; i++)
            {
                int day = currentDay + i;
                if (day <= 0) continue;

                // Check if there is an explicit holder
                DailyRewardHolder explicitHolder = dailyRewards.Find(x => x.day == day);
                if (explicitHolder != null)
                {
                    view.Add(explicitHolder);
                }
                else
                {
                    // Generate one if it has rewards from eachDayReward
                    List<Output> outputs = GetDailyRewards(day);
                    if (outputs != null && outputs.Count > 0)
                    {
                        view.Add(new DailyRewardHolder { day = day, reward = null });
                    }
                }
            }

            return view;
        }
    }
}
