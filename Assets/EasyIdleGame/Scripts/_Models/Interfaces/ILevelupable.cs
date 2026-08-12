namespace EasyIdleGame
{
    /// <summary>
    /// Interface for SCRIPTABLE OBJECTS that can be leveled up
    /// </summary>
    public interface ILevelupable
    {
        /// <summary> The amount of xp needed to level up to level 1 </summary>
        public int FirstLevelXp { get; }

        /// <summary> The amount xps will be multiplied by </summary>
        public float IncrementMultiplier { get; }

        /// <summary> Reward that will be received after level-up </summary>
        public LevelDataHolder LevelupData { get; }

        /// <summary> If the item should be automatically leveled up when enough xp is gathered </summary>
        public bool AutoLevelup { get; }

        public AudioData LevelUpSound { get; }
    }
}