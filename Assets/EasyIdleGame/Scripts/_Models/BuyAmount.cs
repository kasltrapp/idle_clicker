
using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that stores buy amount (purchase multiplier)
    /// </summary>
    [System.Serializable]
    public class BuyAmount
    {
        public enum Type
        {
            digit,
            percent
        }

        [Tooltip("Buy target value. With Digit, this is the exact count to buy; with Percent, this is the percent of the maximum affordable amount.")]
        public int amount = 1;

        [Tooltip("How amount is interpreted. Digit creates options like x1 or x10; Percent creates options like 25% or 100% of the currently affordable maximum.")]
        public Type type = Type.digit;

        public BuyAmount(int amount, Type type)
        {
            this.amount = amount;
            this.type = type;
        }

        public override string ToString()
        {
            string post = type == Type.digit ? "" : "%";
            string pre = type == Type.digit ? "x" : "";

            return $"{pre}{amount}{post}";
        }
    }
}
