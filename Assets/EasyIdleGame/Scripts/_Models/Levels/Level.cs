namespace EasyIdleGame
{
    /// <summary>
    /// Class that stores level data at runtime
    /// </summary>
    [System.Serializable]
    public class Level
    {
        public int unclaimedLevelups = 0;

        public int currentLevel = 0;
        public BigNumber currentXp = 0;
        public float incrementMultiplier = 2;

        public BigNumber startXp;

        public BigNumber TargetXp => Mathb.GeometricSequence_NthTerm(startXp, incrementMultiplier, currentLevel + 1);

        public double Progress => (currentXp / TargetXp).ToDouble();

        public Level(BigNumber target, float multiplier)
        {
            startXp = target;
            incrementMultiplier = multiplier;
        }

        public void AddXp(BigNumber xps)
        {
            currentXp += xps;

            if (currentXp < TargetXp) return;

            int addedLevels = (int)Mathb.GeometricSequence_Amount(startXp, incrementMultiplier, currentXp).ToDouble();
            currentXp = currentXp - Mathb.GeometricSequence_Sum(startXp, incrementMultiplier, addedLevels);

            unclaimedLevelups += addedLevels;
        }

        public void LevelUp()
        {
            unclaimedLevelups--;
            currentLevel++;
        }

        public override string ToString()
        {
            return $"Level: {currentLevel}, XP: {currentXp}/{TargetXp}";
        }
    }
}