using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    [System.Serializable]
    public class LevelDataHolder
    {
        [Tooltip("Rewards granted on exact levels. Each entry's level must match the newly reached level to apply.")]
        public List<LevelRewardHolder> levelRewards = new List<LevelRewardHolder>();

        [Tooltip("Repeating rewards. If the current level is divisible by the entry's level, that reward is granted.")]
        public List<LevelRewardHolder> eachLevelReward = new List<LevelRewardHolder>();

        [Tooltip("If enabled, repeating eachLevelReward outputs are multiplied by the current level before being granted.")]
        public bool scaleEachLevelReward = false;

        [Tooltip("Optional labels or descriptions for level ranges. The highest entry with level <= the current level is used.")]
        public List<LevelDescription> descriptions = new List<LevelDescription>();

        public List<Output> GetLevelReward(int level)
        {
            List<Output> combinedReward = new List<Output>();

            foreach (LevelRewardHolder rewardHolder in levelRewards)
            {
                if (rewardHolder.level != level || rewardHolder.levelReward == null) continue;

                combinedReward.AddRange(RewardCalculator.GetAverageOutputs(rewardHolder.levelReward.rewardTables, 1, RewardCalculator.CreateContext(SourceType.Generic)));
                break;
            }

            foreach (LevelRewardHolder rewardHolder in eachLevelReward)
            {
                if (level % rewardHolder.level != 0 || rewardHolder.levelReward == null) continue;

                BigNumber multiplier = scaleEachLevelReward ? level : 1;
                combinedReward.AddRange(RewardCalculator.GetAverageOutputs(rewardHolder.levelReward.rewardTables, multiplier, RewardCalculator.CreateContext(SourceType.Generic)));
            }

            return combinedReward.Squash();
        }

        public List<Output> ApplyLevelReward(int level)
        {
            List<Output> allOutputs = new List<Output>();

            foreach (LevelRewardHolder rewardHolder in levelRewards)
            {
                if (rewardHolder.level != level || rewardHolder.levelReward == null) continue;

                allOutputs.AddRange(RewardCalculator.GenerateDrops(rewardHolder.levelReward.rewardTables, 1, RewardCalculator.CreateContext(SourceType.Generic)));
                break;
            }

            foreach (LevelRewardHolder rewardHolder in eachLevelReward)
            {
                if (level % rewardHolder.level != 0 || rewardHolder.levelReward == null) continue;

                BigNumber multiplier = scaleEachLevelReward ? level : 1;
                allOutputs.AddRange(RewardCalculator.GenerateDrops(rewardHolder.levelReward.rewardTables, multiplier, RewardCalculator.CreateContext(SourceType.Generic)));
            }

            allOutputs.ForEach(x => x.OnProductionRoundFinished_Apply(1));
            return allOutputs.Squash();
        }

        public string GetLevelDescription(int level)
        {
            LevelDescription closest = null;

            foreach (LevelDescription description in descriptions)
            {
                if (description.level <= level && (closest == null || description.level > closest.level))
                {
                    closest = description;
                }
            }
            return closest?.name ?? "";
        }

        public void MultiplyInPlace(BigNumber multiplier)
        {
            if (levelRewards != null)
            {
                foreach (var rewardHolder in levelRewards)
                {
                    if (rewardHolder.levelReward != null && rewardHolder.levelReward.rewardTables != null)
                    {
                        rewardHolder.levelReward.rewardTables.MultiplyInPlace(multiplier);
                    }
                }
            }

            if (eachLevelReward != null)
            {
                foreach (var rewardHolder in eachLevelReward)
                {
                    if (rewardHolder.levelReward != null && rewardHolder.levelReward.rewardTables != null)
                    {
                        rewardHolder.levelReward.rewardTables.MultiplyInPlace(multiplier);
                    }
                }
            }
        }

        public void MultiplyInPlace(float multiplier)
        {
            MultiplyInPlace((BigNumber)multiplier);
        }
    }
}
