using System;
using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that holds input data for buying
    /// </summary>
    [Serializable]
    public class Input
    {
        [Tooltip("Business resource required or consumed by this input. Leave null if the input should use only currency.")]
        public Business businessInput;

        [Tooltip("Currency resource required or consumed by this input. Leave null if the input should use only business count.")]
        public Currency currencyInput;

        [Tooltip("Base amount required. Buyable costs can scale this amount with the item's cost multiplier and current owned amount.")]
        public BigNumber inputAmount = 1;

        /// <returns> Current price, not the price that has to be paid </returns>
        private BigNumber GetCurrentPrice(BigNumber q, BigNumber currentAmount)
        {
            return Mathb.GeometricSequence_NthTerm(1, q, currentAmount) * inputAmount;
        }

        /// <returns> Sum of costs for currentAmount + 1 elements, so it's basically "Can I buy the next amount of items" </returns>
        private BigNumber GetRealCost(BigNumber q, BigNumber amountToBuy, BigNumber currentAmount, BigNumber priceMultiplier)
        {
            return Mathb.GeometricSequence_Sum(GetCurrentPrice(q, currentAmount + 1) * priceMultiplier, q, amountToBuy);
        }

        private bool IsSufficent_Internal(BigNumber amountMultiplier, BigNumber currentAmount, BigNumber q, BigNumber valueToCompare, BigNumber priceMultiplier)
        {
            return valueToCompare >= GetRealCost(q, amountMultiplier, currentAmount, priceMultiplier);
        }

        public bool IsBusinessSufficent(BigNumber amountMultiplier, BigNumber currentAmount, BigNumber q, BigNumber priceMultiplier)
            => IsSufficent_Internal(amountMultiplier, currentAmount, q, BusinessesManager.Instance.iBuyable.GetItemAmount(businessInput), priceMultiplier);

        public bool IsCurrencySufficent(BigNumber amountMultiplier, BigNumber currentAmount, BigNumber q, BigNumber priceMultiplier)
            => IsSufficent_Internal(amountMultiplier, currentAmount, q, CurrencyManager.Instance.GetCurrencyAmount(currencyInput), priceMultiplier);

        /// <returns> if 'amountToBuy' items can be bought </returns>
        public bool IsSufficent(BigNumber amountToBuy, BigNumber currentAmount, BigNumber q, BigNumber priceMultiplier)
        {
            bool isSufficent = true;

            if (businessInput != null)
                isSufficent = IsBusinessSufficent(amountToBuy, currentAmount, q, priceMultiplier);

            if (currencyInput != null)
                isSufficent = isSufficent && IsCurrencySufficent(amountToBuy, currentAmount, q, priceMultiplier);

            return isSufficent;
        }

        /// <summary>
        /// Checks if the input is sufficent, does not check any price multipliers or anything like that
        /// </summary>
        /// <returns></returns>
        public bool IsThisInputSufficent()
        {
            bool isSufficent = true;
            if (businessInput != null)
                isSufficent = IsThisInputSufficent_Business();
            if (currencyInput != null)
                isSufficent = isSufficent && IsThisInputSufficent_Currency();
            return isSufficent;
        }

        public bool IsThisInputSufficent_Business()
        {
            return BusinessesManager.Instance.iBuyable.GetItemAmount(businessInput) >= inputAmount;
        }

        public bool IsThisInputSufficent_Currency()
        {
            return CurrencyManager.Instance.GetCurrencyAmount(currencyInput) >= inputAmount;
        }

        public void RemoveInput(BigNumber amountToBuy, BigNumber currentAmount, BigNumber q, BigNumber priceMultiplier)
        {
            BigNumber cost = GetRealCost(q, amountToBuy, currentAmount, priceMultiplier);

            if (businessInput != null)
                BusinessesManager.Instance.RemoveBusiness(businessInput, cost);

            if (currencyInput != null)
                CurrencyManager.Instance.RemoveCurrencyAmount(currencyInput, cost);
        }

        public BigNumber GetMaxBuyAmount(BigNumber priceMultiplier, BigNumber currentAmount, BigNumber q)
        {
            BigNumber maxAmount = new(1, long.MaxValue);

            BigNumber currentPrice = GetCurrentPrice(q, currentAmount + 1) * priceMultiplier;

            if (businessInput != null)
            {
                maxAmount = Mathb.GeometricSequence_Amount(currentPrice, q, BusinessesManager.Instance.iBuyable.GetItemAmount(businessInput));
            }

            if (currencyInput != null)
            {
                BigNumber maxAmount1 = Mathb.GeometricSequence_Amount(currentPrice, q, CurrencyManager.Instance.GetCurrencyAmount(currencyInput));

                maxAmount = BigNumber.Min(
                    maxAmount,
                    maxAmount1
                );
            }

            return maxAmount;
        }

        public override string ToString()
        {
            string str = "";

            if (businessInput != null)
                str += $"{businessInput.GetDisplayName()} x{inputAmount}\n";

            if (currencyInput != null)
                str += $"{currencyInput.GetDisplayName()} x{inputAmount}\n";

            return str;
        }

        // experimental
        /// <returns> Returns the real cost, wrapped in an input class </returns>
        public Input GetRealCostToInput(BigNumber q, BigNumber amountMultiplier, BigNumber currentAmount, BigNumber priceMultiplier)
        {
            return new Input
            {
                businessInput = businessInput,
                currencyInput = currencyInput,
                inputAmount = GetRealCost(q, amountMultiplier, currentAmount, priceMultiplier),
            };
        }
    }
}
