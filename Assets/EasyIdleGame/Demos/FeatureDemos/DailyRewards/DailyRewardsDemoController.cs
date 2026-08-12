using System.Collections.Generic;

namespace EasyIdleGame.Demos
{
    public class DailyRewardsDemoController : DemoSceneController
    {
        protected override bool RefreshWhenDailyRewardDayChanges => true;

        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 40);

            DailyRewardsManager.Instance.dayLengthHours = 10f / 3600f;
            DailyRewardsManager.Instance.initDate = System.DateTime.Now;
            DailyRewardsManager.Instance.dailyRewards.Add(new DailyRewardHolder
            {
                day = 1,
                reward = Load<Reward>("DayOneReward")
            });
            DailyRewardsManager.Instance.dailyRewards.Add(new DailyRewardHolder
            {
                day = 3,
                reward = Load<Reward>("DayThreeReward")
            });
            DailyRewardsManager.Instance.eachDayReward.Add(new LevelRewardHolder
            {
                level = 2,
                levelReward = Load<Reward>("EveryTwoDaysReward")
            });
            DailyRewardsManager.Instance.scaleEachDayReward = true;
        }

        protected override void BuildSceneDashboard()
        {
            AddDailyRewardsPanel();
            AddOutputPanel("Last reward outputs", LastOutputs);
        }
    }
}
