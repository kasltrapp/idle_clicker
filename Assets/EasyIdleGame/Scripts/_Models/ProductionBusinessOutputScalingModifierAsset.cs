using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    public enum ProductionBusinessOutputScalingMode
    {
        LogarithmicFloor,
        LinearFloor
    }

    public enum ProductionBusinessOutputScalingSource
    {
        OwnedAmount,
        ProductionAmount
    }

    [CreateAssetMenu(fileName = "ProductionBusinessOutputScalingModifier", menuName = "EasyIdleGame/Production/Business Output Scaling Modifier", order = 1)]
    public class ProductionBusinessOutputScalingModifierAsset : ProductionOutputModifierAsset
    {
        [CommentArea("Output Scaling", "Production-only output modifier that adds or replaces one output based on the amount of the producing business. The scaled output can grant a currency, business, boost, or manager; every assigned target receives the scaled amount. It runs after production DropTables roll or average their outputs, before those outputs are returned or applied.", "For Cowsheds producing Cowsheds, set Producer Business and Output Business to Cowshed, use Logarithmic Floor with base 10, and enable Replace Existing Matching Output so milk output remains unchanged. For a bonus-Coin effect, set Output Currency to Coin and disable the replace flag.")]
        [SerializeField] private string _businessOutputScalingComment;

        [Tooltip("Only production from this business can trigger the modifier.")]
        public Business producerBusiness;

        [Tooltip("Business granted by the scaled output. Leave null when the modifier should not grant business copies; every assigned output target receives the scaled amount.")]
        public Business outputBusiness;

        [Tooltip("Currency granted by the scaled output. Leave null when the modifier should not grant currency; every assigned output target receives the scaled amount.")]
        public Currency outputCurrency;

        [Tooltip("Boost granted by the scaled output. Leave null when the modifier should not grant a boost.")]
        public Boost outputBoost;

        [Tooltip("Manager granted by the scaled output. Leave null when the modifier should not grant a manager.")]
        public Manager outputManager;

        [Tooltip("How the source amount is converted into the scaled output amount per production cycle.")]
        public ProductionBusinessOutputScalingMode scalingMode = ProductionBusinessOutputScalingMode.LogarithmicFloor;

        [Tooltip("Which amount drives scaling. Owned Amount is useful for self-production like Cowsheds producing Cowsheds; Production Amount uses the amount selected for the current production calculation.")]
        public ProductionBusinessOutputScalingSource scalingSource = ProductionBusinessOutputScalingSource.OwnedAmount;

        [Tooltip("Base used by the scaling mode. Logarithmic Floor with base 10 gives 10 owned -> 1, 100 -> 2, 1000 -> 3. Linear Floor with base 10 gives 10 owned -> 1, 20 -> 2, 30 -> 3.")]
        public BigNumber scalingBase = 10;

        [Tooltip("Round output amount granted for each scaling step before production multipliers or per-second projection are applied.")]
        public BigNumber outputAmountPerStep = 1;

        [Tooltip("When enabled, each output target configured on this modifier is removed from the normal production outputs before the scaled output is added. Targets this modifier does not configure remain unchanged.")]
        public bool replaceExistingMatchingOutput = true;

        private bool HasOutputTarget =>
            outputBusiness != null || outputCurrency != null || outputBoost != null || outputManager != null;

        public override bool AppliesTo(ProductionOutputContext context)
        {
            return context != null &&
                producerBusiness != null &&
                HasOutputTarget &&
                context.ProducingBusiness == producerBusiness;
        }

        public override void ModifyOutputs(ProductionOutputContext context, List<Output> outputs)
        {
            if (context.ProductionAmount <= 0)
            {
                if (replaceExistingMatchingOutput)
                    RemoveMatchingOutputs(outputs);

                return;
            }

            BigNumber sourceAmount = scalingSource == ProductionBusinessOutputScalingSource.OwnedAmount
                ? context.OwnedAmount
                : context.ProductionAmount;

            BigNumber roundAmount = CalculateRoundOutputAmount(sourceAmount, context);

            if (replaceExistingMatchingOutput)
                RemoveMatchingOutputs(outputs);

            if (roundAmount <= 0) return;

            outputs.Add(context.CreateProjectedOutput(CreateOutputTemplate(roundAmount), roundAmount));
        }

        private Output CreateOutputTemplate(BigNumber amount)
        {
            return new Output
            {
                businessOutput = outputBusiness,
                currencyOutput = outputCurrency,
                boostOutput = outputBoost,
                managerOutput = outputManager,
                outputAmount = amount,
                outputMaxAmount = amount
            };
        }

        private BigNumber CalculateRoundOutputAmount(BigNumber sourceAmount, ProductionOutputContext context)
        {
            if (sourceAmount <= 0 || outputAmountPerStep <= 0) return 0;

            BigNumber steps = 0;
            switch (scalingMode)
            {
                case ProductionBusinessOutputScalingMode.LogarithmicFloor:
                    if (scalingBase <= 1) return 0;
                    steps = Mathb.Log(sourceAmount, scalingBase).Floor();
                    break;
                case ProductionBusinessOutputScalingMode.LinearFloor:
                    if (scalingBase <= 0) return 0;
                    steps = (sourceAmount / scalingBase).Floor();
                    break;
            }

            return BigNumber.Max(0, steps * outputAmountPerStep * UpgradeEffectResolver.GetProductionOutputScalingMultiplier(context, CreateOutputTemplate(1)));
        }

        private void RemoveMatchingOutputs(List<Output> outputs)
        {
            for (int i = outputs.Count - 1; i >= 0; i--)
            {
                Output output = outputs[i];
                if (output == null) continue;

                bool removedTarget = false;

                if (outputBusiness != null && output.businessOutput == outputBusiness)
                {
                    output.businessOutput = null;
                    removedTarget = true;
                }

                if (outputCurrency != null && output.currencyOutput == outputCurrency)
                {
                    output.currencyOutput = null;
                    removedTarget = true;
                }

                if (outputBoost != null && output.boostOutput == outputBoost)
                {
                    output.boostOutput = null;
                    removedTarget = true;
                }

                if (outputManager != null && output.managerOutput == outputManager)
                {
                    output.managerOutput = null;
                    removedTarget = true;
                }

                if (!removedTarget) continue;

                bool hasRemainingTargets = output.businessOutput != null ||
                    output.currencyOutput != null ||
                    output.boostOutput != null ||
                    output.managerOutput != null;

                if (!hasRemainingTargets)
                    outputs.RemoveAt(i);
            }
        }
    }
}
