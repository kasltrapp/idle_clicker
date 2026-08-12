using System;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that stores prestige data
    /// </summary>
    [Serializable]
    public class PrestigeData
    {
        public int prestigeCount = 0;
        public float currentCostMultiplier = 1;
        public float currentPrestigeUpgradesMultiplier = 1;
    }
}