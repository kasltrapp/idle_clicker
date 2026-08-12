using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class DailyRewardsManagerTests
    {
        private GameObject _managerObject;

        [SetUp]
        public void SetUp()
        {
            _managerObject = TestUtilities.SetupManagerObject();
        }

        [TearDown]
        public void TearDown()
        {
            TestUtilities.TearDownManagerObject(_managerObject);
        }

        [Test]
        public void GetCurrentDay_CalculatesCorrectly()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;

            manager.initDate = DateTime.Now.AddDays(-5);
            Assert.AreEqual(6, manager.GetCurrentDay(), "Current day should be 6 if init was 5 days ago (default 24h).");

            manager.dayLengthHours = 12f; // 12 hour days
            // 5 days = 120 hours. 120 / 12 = 10. Day 10 + 1 = 11.
            Assert.AreEqual(11, manager.GetCurrentDay(), "Current day should be 11 if init was 120 hours ago with 12h day length.");
        }

        [Test]
        public void GetTimeToNextDay_CalculatesCorrectly()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;
            
            manager.dayLengthHours = 24f;
            manager.initDate = DateTime.Now.AddHours(-10); // 10 hours passed
            // Next day should be in 14 hours
            TimeSpan timeToNext = manager.GetTimeToNextDay();
            Assert.IsTrue(Mathf.Abs((float)timeToNext.TotalHours - 14f) < 0.01f, "Time to next day should be ~14 hours.");

            manager.dayLengthHours = 18f;
            manager.initDate = DateTime.Now.AddHours(-20); // 20 hours passed. 20 % 18 = 2 hours into current day.
            // Next day should be in 16 hours
            timeToNext = manager.GetTimeToNextDay();
            Assert.IsTrue(Mathf.Abs((float)timeToNext.TotalHours - 16f) < 0.01f, "Time to next day should be ~16 hours.");
        }

        [Test]
        public void GetDailyRewards_ReturnsCorrectOutputs()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;

            var rewardOutput = ScriptableObject.CreateInstance<Reward>();
            rewardOutput.rewardTables.Add(new DropTable { 
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output { outputAmount = 10, currencyOutput = TestUtilities.TestCurrency } }
            });

            manager.dailyRewards.Add(new DailyRewardHolder { day = 1, reward = rewardOutput });
            manager.eachDayReward.Add(new LevelRewardHolder { level = 5, levelReward = rewardOutput });

            var day1Rewards = manager.GetDailyRewards(1);
            Assert.AreEqual(1, day1Rewards.Count, "Should return 1 reward for day 1.");
            Assert.AreEqual(new BigNumber(10), day1Rewards[0].outputAmount, "Reward amount should be 10.");

            var day5Rewards = manager.GetDailyRewards(5);
            Assert.AreEqual(1, day5Rewards.Count, "Should return 1 reward for day 5 from eachDayReward.");

            var day2Rewards = manager.GetDailyRewards(2);
            Assert.AreEqual(0, day2Rewards.Count, "Should return 0 rewards for day 2.");

            var rewardsZero = manager.GetDailyRewards(0);
            Assert.AreEqual(0, rewardsZero.Count, "Should return empty for day 0.");

            var rewardsNegative = manager.GetDailyRewards(-5);
            Assert.AreEqual(0, rewardsNegative.Count, "Should return empty for negative day.");

            UnityEngine.Object.DestroyImmediate(rewardOutput);
        }

        [Test]
        public void ExactDayRewards_DefaultMode_CombinesAllMatchingEntriesForPreviewAndClaim()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;
            Reward firstReward = CreateCurrencyReward(10);
            Reward secondReward = CreateCurrencyReward(20);

            manager.dailyRewards.Add(new DailyRewardHolder { day = 1, reward = firstReward });
            manager.dailyRewards.Add(new DailyRewardHolder { day = 1, reward = secondReward });

            Assert.IsTrue(manager.combineExactDayRewards, "Exact-day rewards should combine by default.");

            List<Output> preview = manager.GetDailyRewards(1);
            Assert.AreEqual(1, preview.Count, "Matching currency outputs should be squashed into one preview output.");
            Assert.AreEqual(new BigNumber(30), preview[0].outputAmount, "Preview should include both exact-day rewards.");

            int rewardClaimedEvents = 0;
            manager.OnRewardClaimed.AddListener(_ => rewardClaimedEvents++);

            bool claimed = manager.TryClaimDailyRewards(1, out List<Output> outputs);

            Assert.IsTrue(claimed, "Day 1 should be claimable.");
            Assert.AreEqual(1, outputs.Count, "Matching currency outputs should be squashed into one claimed output.");
            Assert.AreEqual(new BigNumber(30), outputs[0].outputAmount, "Claim output should include both exact-day rewards.");
            Assert.AreEqual(new BigNumber(30), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Both exact-day rewards should be applied.");
            Assert.AreEqual(2, rewardClaimedEvents, "Each matching Reward asset should raise OnRewardClaimed.");

            UnityEngine.Object.DestroyImmediate(firstReward);
            UnityEngine.Object.DestroyImmediate(secondReward);
        }

        [Test]
        public void ExactDayRewards_CompatibilityMode_UsesOnlyFirstMatchingEntry()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;
            manager.combineExactDayRewards = false;
            Reward firstReward = CreateCurrencyReward(10);
            Reward secondReward = CreateCurrencyReward(20);

            manager.dailyRewards.Add(new DailyRewardHolder { day = 1, reward = firstReward });
            manager.dailyRewards.Add(new DailyRewardHolder { day = 1, reward = secondReward });

            List<Output> preview = manager.GetDailyRewards(1);
            Assert.AreEqual(1, preview.Count, "Compatibility mode should preview one exact-day reward.");
            Assert.AreEqual(new BigNumber(10), preview[0].outputAmount, "Compatibility mode should preview only the first matching entry.");

            int rewardClaimedEvents = 0;
            manager.OnRewardClaimed.AddListener(_ => rewardClaimedEvents++);

            bool claimed = manager.TryClaimDailyRewards(1, out List<Output> outputs);

            Assert.IsTrue(claimed, "Day 1 should be claimable.");
            Assert.AreEqual(new BigNumber(10), outputs[0].outputAmount, "Compatibility mode should claim only the first matching entry.");
            Assert.AreEqual(new BigNumber(10), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Only the first exact-day reward should be applied.");
            Assert.AreEqual(1, rewardClaimedEvents, "Only the first matching Reward asset should raise OnRewardClaimed.");

            UnityEngine.Object.DestroyImmediate(firstReward);
            UnityEngine.Object.DestroyImmediate(secondReward);
        }

        [Test]
        public void ClaimDailyRewards_AppliesRewardsAndRecordsDay()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;

            var rewardOutput = ScriptableObject.CreateInstance<Reward>();
            rewardOutput.rewardTables.Add(new DropTable { 
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output { outputAmount = 10, currencyOutput = TestUtilities.TestCurrency } }
            });

            manager.dailyRewards.Add(new DailyRewardHolder { day = 1, reward = rewardOutput });

            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Initial currency is 0.");

            manager.TryClaimDailyRewards(1, out _);

            Assert.AreEqual(new BigNumber(10), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Currency should increase by 10.");
            Assert.Contains(1, manager.claimedDays, "Day 1 should be recorded as claimed.");

            UnityEngine.Object.DestroyImmediate(rewardOutput);
        }

        [Test]
        public void GetMissedDays_CalculatesCorrectly()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;

            var rewardOutput = ScriptableObject.CreateInstance<Reward>();
            rewardOutput.rewardTables.Add(new DropTable { 
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output { outputAmount = 10, currencyOutput = TestUtilities.TestCurrency } }
            });

            manager.dailyRewards.Add(new DailyRewardHolder { day = 1, reward = rewardOutput });
            manager.dailyRewards.Add(new DailyRewardHolder { day = 2, reward = rewardOutput });
            manager.dailyRewards.Add(new DailyRewardHolder { day = 3, reward = rewardOutput });

            manager.initDate = DateTime.Now.AddDays(-3); // Today is day 4

            manager.TryClaimDailyRewards(1, out _); // Day 1 claimed

            List<int> missedDays = manager.GetMissedDays();

            Assert.AreEqual(2, missedDays.Count, "Should have 2 missed days (2 and 3).");
            Assert.Contains(2, missedDays, "Should contain day 2.");
            Assert.Contains(3, missedDays, "Should contain day 3.");

            // Claim remaining days
            manager.TryClaimDailyRewards(2, out _);
            manager.TryClaimDailyRewards(3, out _);
            manager.TryClaimDailyRewards(4, out _);

            List<int> noMissedDays = manager.GetMissedDays();
            Assert.AreEqual(0, noMissedDays.Count, "Should have 0 missed days after all are claimed.");

            UnityEngine.Object.DestroyImmediate(rewardOutput);
        }

        [Test]
        public void LastClaimedDay_EmptyAndFilled_ReturnsCorrectly()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;
            Assert.AreEqual(0, manager.lastClaimedDay, "Should return 0 when empty.");

            manager.claimedDays.Add(1);
            manager.claimedDays.Add(5);
            manager.claimedDays.Add(3);

            Assert.AreEqual(5, manager.lastClaimedDay, "Should return max value (5).");
        }


        [Test]
        public void GetDailyRewards_WithScaleEachDayReward_ScalesOutput()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;
            manager.scaleEachDayReward = true;

            var rewardOutput = ScriptableObject.CreateInstance<Reward>();
            rewardOutput.rewardTables.Add(new DropTable { 
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output { outputAmount = 10, currencyOutput = TestUtilities.TestCurrency } }
            });

            manager.eachDayReward.Add(new LevelRewardHolder { level = 5, levelReward = rewardOutput });

            var day5Rewards = manager.GetDailyRewards(5);
            Assert.AreEqual(1, day5Rewards.Count, "Should return 1 reward.");
            Assert.AreEqual(new BigNumber(50), day5Rewards[0].outputAmount, "Reward amount should be scaled 10 * 5 = 50.");

            UnityEngine.Object.DestroyImmediate(rewardOutput);
        }

        [Test]
        public void ClaimDailyRewards_WithScaleEachDayReward_AppliesScaledOutput()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;
            manager.scaleEachDayReward = true;

            var rewardOutput = ScriptableObject.CreateInstance<Reward>();
            rewardOutput.rewardTables.Add(new DropTable { 
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output { outputAmount = 10, currencyOutput = TestUtilities.TestCurrency } }
            });

            manager.eachDayReward.Add(new LevelRewardHolder { level = 5, levelReward = rewardOutput });
            manager.initDate = DateTime.Now.AddDays(-4); // Today is day 5

            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Initial currency is 0.");

            manager.TryClaimDailyRewards(5, out _);

            Assert.AreEqual(new BigNumber(50), CurrencyManager.Instance.GetCurrencyAmount(TestUtilities.TestCurrency), "Currency should increase by scaled amount (50).");

            UnityEngine.Object.DestroyImmediate(rewardOutput);
        }

        [Test]
        public void GetCalendarView_GeneratesCorrectHolders()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;
            manager.initDate = DateTime.Now.AddDays(-3); // Today is day 4

            var rewardOutput = ScriptableObject.CreateInstance<Reward>();
            rewardOutput.rewardTables.Add(new DropTable { 
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output { outputAmount = 10, currencyOutput = TestUtilities.TestCurrency } }
            });

            // Explicit holder on day 4
            manager.dailyRewards.Add(new DailyRewardHolder { day = 4, reward = rewardOutput });
            // EachDayReward every 5 days
            manager.eachDayReward.Add(new LevelRewardHolder { level = 5, levelReward = rewardOutput });

            // Request view: 2 days before (2, 3), today (4), 2 days after (5, 6)
            List<DailyRewardHolder> view = manager.GetCalendarView(2, 2);

            Assert.AreEqual(2, view.Count, "Should return 2 holders: one explicit for day 4, one implicit for day 5.");

            Assert.AreEqual(4, view[0].day, "First holder should be day 4.");
            Assert.IsNotNull(view[0].reward, "Day 4 holder should have explicit reward.");

            Assert.AreEqual(5, view[1].day, "Second holder should be day 5.");
            Assert.IsNull(view[1].reward, "Day 5 holder should be implicit (reward == null).");

            UnityEngine.Object.DestroyImmediate(rewardOutput);
        }

        [Test]
        public void GetCalendarView_OutOfBoundsDay_IsSkipped()
        {
            DailyRewardsManager manager = DailyRewardsManager.Instance;
            manager.initDate = DateTime.Now; // Today is day 1

            var rewardOutput = ScriptableObject.CreateInstance<Reward>();
            rewardOutput.rewardTables.Add(new DropTable { 
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output { outputAmount = 10, currencyOutput = TestUtilities.TestCurrency } }
            });
            manager.dailyRewards.Add(new DailyRewardHolder { day = 1, reward = rewardOutput });

            // Request view: 2 days before (this would be day -1 and 0), today (1), 0 days after
            List<DailyRewardHolder> view = manager.GetCalendarView(2, 0);

            Assert.AreEqual(1, view.Count, "Should only return day 1 holder, skipping day -1 and 0.");
            Assert.AreEqual(1, view[0].day, "Only day 1 is included.");

            UnityEngine.Object.DestroyImmediate(rewardOutput);
        }

        private static Reward CreateCurrencyReward(int amount)
        {
            Reward reward = ScriptableObject.CreateInstance<Reward>();
            reward.rewardTables.Add(new DropTable
            {
                strategy = DropStrategy.All,
                outputs = new List<Output>
                {
                    new Output { outputAmount = amount, currencyOutput = TestUtilities.TestCurrency }
                }
            });
            return reward;
        }
    }
}
