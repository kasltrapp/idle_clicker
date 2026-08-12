using System.Collections.Generic;
using System.Linq;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that extends Input menthods for lists
    /// </summary>
    public static class InputListExtensionMethods
    {
        /// <summary>
        /// if the input has no cost multiplier, set q to 1, and leave the currentAmount at 1 or some other positive number
        /// </summary>
        /// <returns> If amountToBuy can be bought </returns>
        public static bool IsSufficent(this List<Input> inputs, BigNumber amountToBuy, BigNumber currentAmount, BigNumber q, BigNumber priceMultiplier)
        {
            if (inputs == null || inputs.Count == 0) return true;
            return inputs.All(x => x.IsSufficent(amountToBuy, currentAmount, q, priceMultiplier));
        }

        /// <summary>
        /// Removes the cost of the inputs from the player | 
        /// if the input has no cost multiplier, set q to 1, and leave the currentAmount at 1 or some other positive number
        /// </summary>
        public static void RemoveInputs(this List<Input> inputs, BigNumber amountToBuy, BigNumber currentAmount, BigNumber q, BigNumber priceMultiplier)
        {
            if (inputs == null || inputs.Count == 0) return;
            inputs.ForEach(x => x.RemoveInput(amountToBuy, currentAmount, q, priceMultiplier));
        }

        /// <summary>
        /// if the input has no cost multiplier, set q to 1, and leave the currentAmount at 1 or some other positive number
        /// </summary>
        /// <returns> List of trasformed new inputs </returns>
        public static List<Input> GetRealCostToInputs(this List<Input> inputs, BigNumber q, BigNumber amountMultiplier, BigNumber currentAmount, BigNumber priceMultiplier)
        {
            return inputs.Select(x => x.GetRealCostToInput(q, amountMultiplier, currentAmount, priceMultiplier)).ToList();
        }

        public static List<Input> GetCurrencyInputs(this List<Input> inputs)
        {
            return inputs.FindAll(x => x.currencyInput != null);
        }

        public static List<Input> GetBusinessInputs(this List<Input> inputs)
        {
            return inputs.FindAll(x => x.businessInput != null);
        }

        public static List<Input> Squash(this List<Input> inputs)
        {
            List<Input> squashedInputs = new List<Input>();
            foreach (var input in inputs)
            {
                if (input.businessInput != null || input.currencyInput != null)
                {
                    var existingInput = squashedInputs.Find(x =>
                        x.businessInput == input.businessInput &&
                        x.currencyInput == input.currencyInput);
                    if (existingInput != null)
                    {
                        existingInput.inputAmount += input.inputAmount;
                    }
                    else
                    {
                        squashedInputs.Add(new Input
                        {
                            businessInput = input.businessInput,
                            currencyInput = input.currencyInput,
                            inputAmount = input.inputAmount
                        });
                    }
                }
            }
            return squashedInputs;
        }

        public static List<Input> Multiply(this List<Input> inputs, BigNumber multiplier)
        {
            return inputs.Select(x => new Input
            {
                businessInput = x.businessInput,
                currencyInput = x.currencyInput,
                inputAmount = x.inputAmount * multiplier
            }).ToList();
        }

        public static void MultiplyInPlace(this List<Input> inputs, BigNumber multiplier)
        {
            if (inputs == null) return;
            foreach (var input in inputs)
            {
                input.inputAmount *= multiplier;
            }
        }

        public static void MultiplyInPlace(this List<Input> inputs, float multiplier)
        {
            inputs.MultiplyInPlace((BigNumber)multiplier);
        }
    }
}