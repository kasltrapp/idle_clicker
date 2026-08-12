using System;

namespace EasyIdleGame
{
    [Serializable]
    public class ProductionRound
    {
        [NonSerialized] public object Context;
        public double productionTime;
        public BigNumber productionAmount;
        public System.Collections.Generic.List<Output> rolledOutputs;
        public bool isPaused;
    }
}
