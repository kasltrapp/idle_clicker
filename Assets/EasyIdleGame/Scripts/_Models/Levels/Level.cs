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

            // BUGFIX (see CLAUDE.md Watch-list): Mathb.GeometricSequence_Amount/Sum always
            // interpret their input/output as an absolute cumulative sum measured from
            // term 1 (level 0) - they have no concept of "starting from the current
            // level". But currentXp only ever holds the LEFTOVER remainder since the last
            // level-up, a marginal quantity scoped to the CURRENT level, not an absolute
            // cumulative total. Feeding currentXp into GeometricSequence_Amount directly
            // (the original code) silently re-interpreted that leftover as if the player
            // had earned it from level 0, which both mis-counts the levels gained and
            // leaves a nonzero, wrong leftover behind - the error then compounds on every
            // subsequent level-up, since each call's leftover is itself already corrupted.
            // Fixed by re-baselining into the same absolute-from-zero frame the geometric
            // math expects: add back the cumulative XP already paid to reach currentLevel,
            // compute the resulting ABSOLUTE level from that combined total, then convert
            // back to a level-relative delta by subtracting currentLevel.
            BigNumber cumulativeThroughCurrentLevel = Mathb.GeometricSequence_Sum(startXp, incrementMultiplier, currentLevel);
            BigNumber absoluteXp = cumulativeThroughCurrentLevel + currentXp;

            int newAbsoluteLevel = (int)Mathb.GeometricSequence_Amount(startXp, incrementMultiplier, absoluteXp).ToDouble();
            int addedLevels = newAbsoluteLevel - currentLevel;

            currentXp = absoluteXp - Mathb.GeometricSequence_Sum(startXp, incrementMultiplier, newAbsoluteLevel);

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