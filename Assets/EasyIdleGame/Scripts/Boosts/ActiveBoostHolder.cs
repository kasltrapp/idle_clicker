using System;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that holds active boost
    /// </summary>
    [Serializable]
    public class ActiveBoostHolder
    {
        public SaveableScriptableObject<Boost> boostSaveable;
        public Boost boost
        {
            get => boostSaveable.Value;
            set => boostSaveable = new SaveableScriptableObject<Boost>(value);
        }

        public double timeLeft;

        public ActiveBoostHolder(Boost boost_, double timeLeft_)
        {
            boost = boost_;
            timeLeft = timeLeft_;
        }

        /// <returns> New instance of ActiveBoostHolder </returns>
        public ActiveBoostHolder Clone()
        {
            return new ActiveBoostHolder(boost, timeLeft);
        }

        public override string ToString()
        {
            return $"{boost.GetDisplayName()}: {timeLeft}";
        }
    }
}
