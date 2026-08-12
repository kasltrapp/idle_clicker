using System.Collections.Generic;
using System.Linq;

namespace EasyIdleGame
{
    public interface IProducable
    {
        [System.Obsolete("Use OutputTables instead. Note: Outputs is now a flattened list for backward compatibility.", false)]
        public List<Output> Outputs { get; }

        public List<DropTable> OutputTables { get; }

        public float InitialTimeToProduce { get; }

        public List<Input> ProductionCost { get; }

        public bool ScaleProductionCostWithAmount { get; }

        public bool AllowPartialProductionCost { get; }

        public bool ConsumeOnProduce { get; }

        public ProductionRoundMode ProductionRoundMode { get; }

        public List<Lock> ProductionLocks { get; }

        public List<Lock> UpperProductionLocks { get; }

        public AudioData ProductionStartSound { get; }

        public AudioData ProductionEndSound { get; }

        /// <returns> Whether the production cost requirements are met </returns>
        public bool IsProductionCostSufficent(BigNumber currentAmount)
        {
            List<Input> cost = GetProductionCost(currentAmount);
            if (cost == null || cost.Count == 0) return true;
            return cost.All(input => input == null || input.IsThisInputSufficent());
        }

        public List<Input> GetProductionCost(BigNumber currentAmount)
        {
            BigNumber scaledAmount = ScaleProductionCostWithAmount ? currentAmount : 1;

            if (ProductionCost == null) return new List<Input>();

            Business producingBusiness = this as Business;
            return ProductionCost.Select(x =>
            {
                Input projected = new Input()
                {
                    currencyInput = x.currencyInput,
                    businessInput = x.businessInput,
                    inputAmount = x.inputAmount * scaledAmount,
                };

                projected.inputAmount *= UpgradeEffectResolver.GetProductionInputCostMultiplier(producingBusiness, projected, currentAmount);
                return projected;
            }).ToList();
        }

        public BigNumber GetAffordableProductionAmount(BigNumber requestedAmount)
        {
            requestedAmount = requestedAmount.Floor();
            if (requestedAmount <= 0) return 0;

            if (IsProductionCostSufficent(requestedAmount)) return requestedAmount;
            if (!AllowPartialProductionCost || !ScaleProductionCostWithAmount) return 0;
            if (ProductionCost == null || ProductionCost.Count == 0) return requestedAmount;

            BigNumber affordableAmount = requestedAmount;
            bool hasResourceCost = false;
            List<Input> costPerInstance = GetProductionCost(1);

            foreach (Input input in costPerInstance)
            {
                if (input == null || input.inputAmount <= 0) continue;

                if (input.businessInput != null)
                {
                    BigNumber availableAmount = BusinessesManager.Instance.iBuyable.GetItemAmount(input.businessInput);
                    affordableAmount = BigNumber.Min(affordableAmount, (availableAmount / input.inputAmount).Floor());
                    hasResourceCost = true;
                }

                if (input.currencyInput != null)
                {
                    BigNumber availableAmount = CurrencyManager.Instance.GetCurrencyAmount(input.currencyInput);
                    affordableAmount = BigNumber.Min(affordableAmount, (availableAmount / input.inputAmount).Floor());
                    hasResourceCost = true;
                }
            }

            if (!hasResourceCost) return requestedAmount;

            return BigNumber.Max(0, affordableAmount.Floor());
        }

        /// <returns> Applies all outputs and returns the profit data </returns>
        public List<Output> ApplyOutputs(BigNumber amount) => OutputTables.ApplyOutputs(amount);
    }
}