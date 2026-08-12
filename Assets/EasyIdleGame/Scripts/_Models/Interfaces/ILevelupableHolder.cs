using System;
using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// Interface for HOLDER OBJECTS that can be leveled up, holder scriptable objects have to implement ILevelupable
    /// </summary>
    public interface ILevelupableHolder
    {
        public Action<ILevelupableHolder> OnLeveledUp { get; set; }

        public ILevelupable holdedItem { get; }

        /// <summary> current level of the item </summary>
        public Level Level { get; set; }

        public void Init()
        {
            Level = new Level(holdedItem.FirstLevelXp, holdedItem.IncrementMultiplier);
        }

        /// <summary> Adds xps and tries to levelup </summary>
        public void AddXps(BigNumber xps)
        {
            Level.AddXp(xps);

            if (holdedItem.AutoLevelup) LevelUpAll();
        }

        /// <summary> Level up the item to the max possible level (based on current xps) </summary>
        public void LevelUpAll()
        {
            while (Level.unclaimedLevelups > 0) LevelUp();
        }

        public void LevelUp()
        {
            Level.LevelUp();
            holdedItem.LevelupData.ApplyLevelReward(Level.currentLevel);
            OnLeveledUp?.Invoke(this);
        }

        /// <returns> Levelup reward for the current level (Averaged outputs for previewing) </returns>
        public List<Output> GetLevelupReward() => GetLevelupRewardForLevel(Level.currentLevel);

        /// <returns> Levelup reward for a level (Averaged outputs for previewing) </returns>
        public List<Output> GetLevelupRewardForLevel(int level)
        {
            return holdedItem.LevelupData.GetLevelReward(level);
        }
    }
}
