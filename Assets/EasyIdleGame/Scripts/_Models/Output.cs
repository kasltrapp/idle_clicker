using System;
using UnityEngine;
namespace EasyIdleGame
{
    /// <summary>
    /// Class that stores output data
    /// </summary>
    [Serializable]
    public class Output
    {
        [Tooltip("Business granted by this output. Leave null if this output should not grant business copies; if multiple output targets are assigned, each non-null target receives the rolled amount.")]
        public Business businessOutput;

        [Tooltip("Currency granted by this output. Leave null if this output should not grant currency; if multiple output targets are assigned, each non-null target receives the rolled amount.")]
        public Currency currencyOutput;

        [Tooltip("Boost added to the player's inventory by this output. Leave null if this output should not grant a boost.")]
        public Boost boostOutput;

        [Tooltip("Manager copies granted by this output. Leave null if this output should not grant a manager.")]
        public Manager managerOutput;

        [Tooltip("Amount granted when this output is applied. If outputMaxAmount is greater than this value, this becomes the inclusive minimum of the random range.")]
        public BigNumber outputAmount = 1;

        [Tooltip("Inclusive maximum amount for randomized rewards. Set lower than or equal to outputAmount to disable randomization and always use outputAmount.")]
        public BigNumber outputMaxAmount = -1;

        [Range(0f, 1f)]
        [Tooltip("Independent chance used by DropStrategy.All. 1.0 is guaranteed, 0 never drops; Weighted Random tables ignore this field.")]
        public float dropChance = 1f;

        [Min(0)]
        [Tooltip("Relative pick weight used by DropStrategy.WeightedRandom. Higher values are more likely; 0 removes this output from weighted picks.")]
        public float weight = 1f;

        public BigNumber GetAmount()
        {
            if (outputMaxAmount > outputAmount)
                return Mathb.RandomBetween(outputAmount, outputMaxAmount);
            return outputAmount;
        }

        public BigNumber GetAverageAmount()
        {
            if (outputMaxAmount > outputAmount)
                return (outputAmount + outputMaxAmount) / 2;
            return outputAmount;
        }

        public void OnProductionRoundFinished_Apply(BigNumber amount, object context = null, SourceType sourceType = SourceType.Generic)
        {
            BigNumber appliedAmount = GetAmount() * amount;
            if (context is UpgradeEffectContext effectContext)
                sourceType = effectContext.Source;

            if (businessOutput != null)
                BusinessesManager.Instance.GetOrAddHolder(businessOutput).AddAmount(appliedAmount, context, sourceType);

            if (currencyOutput != null)
                CurrencyManager.Instance.AddCurrencyAmount(currencyOutput, appliedAmount, sourceType);

            if (boostOutput != null && BoostsManager.Instance)
                BoostsManager.Instance.ForceBuyItemsForFree(boostOutput, appliedAmount, sourceType);

            if (managerOutput != null && ManagersManager.Instance)
                ManagersManager.Instance.ForceBuyItemsForFree(managerOutput, appliedAmount, sourceType);
        }

        public Output Multiply(BigNumber multiplier)
        {
            return new Output
            {
                businessOutput = businessOutput,
                currencyOutput = currencyOutput,
                boostOutput = boostOutput,
                managerOutput = managerOutput,
                outputAmount = outputAmount * multiplier,
                outputMaxAmount = outputMaxAmount * multiplier,
                dropChance = dropChance,
                weight = weight
            };
        }

        public void MultiplyInPlace(BigNumber multiplier)
        {
            outputAmount *= multiplier;
            if (outputMaxAmount >= 0)
                outputMaxAmount *= multiplier;
        }

        public void MultiplyInPlace(float multiplier)
        {
            MultiplyInPlace((BigNumber)multiplier);
        }

        public override string ToString()
        {
            string str = "";

            string amountStr = outputMaxAmount > outputAmount ? $"{outputAmount} - {outputMaxAmount}" : $"x{outputAmount}";

            if (businessOutput != null)
                str += $"Business: {businessOutput.GetDisplayName()} {amountStr}\n";

            if (currencyOutput != null)
                str += $"Currency: {currencyOutput.GetIconString()} {amountStr}\n";

            return str;
        }
    }
}
