using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyIdleGame
{
    public interface IProducableHolder
    {
        public Action<IProducableHolder> OnProductionStarted { get; set; }
        public Action<IProducableHolder> OnProductionFinished { get; set; }

        public IProducable ProducableItem { get; }

        public BigNumber ProductionAmount { get; set; }

        public BigNumber Amount { get; }

        public BigNumber CurrentlyProducing { get; }

        public List<ProductionRound> ActiveProductions { get; }

        public bool TryProduceManually(BigNumber instancesToProduce, out ProductionRound round, object context = null);

        /// <returns> Time to produce adjusted for speed multipliers </returns>
        public float GetActualTimeToProduce()
        {
            return (float)(BigNumber.Max(ProducableItem.InitialTimeToProduce / GetSpeedMultiplier(), BigNumber.MinPositiveValue)).ToDouble();
        }

        /// <returns> List of outputs with everything taken into account (such as Boosts, Prestiges and amount) </returns>
        public List<Output> GetActualProductionPerSecond(bool ignoreProductionAmount = false) => GetActualProductionPerSecond(0, ignoreProductionAmount);

        /// <returns> List of outputs with everything taken into account (such as Boosts, Prestiges and amount) </returns>
        public List<Output> GetActualProductionPerSecond(BigNumber additionalAmount, bool ignoreProductionAmount = false)
        {
            BigNumber amount = ignoreProductionAmount ? Amount : ProductionAmount;
            BigNumber exactAmount = amount + additionalAmount;

            return ProductionOutputCalculator.CalculateAverageProduction(
                this,
                exactAmount,
                ProductionOutputCalculationMode.ProductionPerSecondPreview,
                additionalAmount,
                ignoreProductionAmount);
        }

        /// <returns> List of outputs based on what is currently actively producing </returns>
        public List<Output> GetCurrentProductionPerSecond()
        {
            return ProductionOutputCalculator.CalculateAverageProduction(
                this,
                CurrentlyProducing,
                ProductionOutputCalculationMode.CurrentProductionPerSecondPreview);
        }

        /// <returns> Production adjusted for production multipliers </returns>
        public List<Output> GetOptimisticActualProduction(bool ignoreProductionAmount = false) => GetOptimisticActualProduction(0, ignoreProductionAmount);

        /// <returns> Production adjusted for production multipliers </returns>
        public List<Output> GetOptimisticActualProduction(BigNumber additionalAmount, bool ignoreProductionAmount = false)
        {
            return GetOptimisticActualProduction(
                additionalAmount,
                ignoreProductionAmount,
                ProductionOutputCalculationMode.OptimisticPreview);
        }

        /// <returns> Production adjusted for production multipliers </returns>
        public List<Output> GetOptimisticActualProduction(
            BigNumber additionalAmount,
            bool ignoreProductionAmount,
            ProductionOutputCalculationMode calculationMode)
        {
            BigNumber amount = ignoreProductionAmount ? Amount : ProductionAmount;
            BigNumber exactAmount = amount + additionalAmount;

            return ProductionOutputCalculator.CalculateRolledProduction(
                this,
                exactAmount,
                calculationMode,
                additionalAmount,
                ignoreProductionAmount);
        }

        /// <returns> Production taking all currently producing rounds and summing their outputs </returns>
        public List<Output> GetActualProduction()
        {
            var outputs = new List<Output>();
            if (ActiveProductions == null) return outputs;

            for (int i = 0; i < ActiveProductions.Count; i++)
            {
                ProductionRound round = ActiveProductions[i];
                if (round == null) continue;

                if (round.rolledOutputs != null)
                {
                    outputs.AddRange(round.rolledOutputs);
                }
                else
                {
                    outputs.AddRange(GetActualProductionForAmount(round.productionAmount));
                }
            }

            return outputs.Squash();
        }

        /// <returns> Production adjusted for production multipliers for an exact amount of instances </returns>
        public List<Output> GetActualProductionForAmount(BigNumber exactAmount, bool isManual = false)
        {
            return ProductionOutputCalculator.CalculateRolledProduction(
                this,
                exactAmount,
                ProductionOutputCalculationMode.ActualProductionRound,
                0,
                false,
                isManual);
        }

        /// <returns> Returns production multiplier based on Boosts, Prestiges and other multipliers </returns>
        public BigNumber GetProductionMultiplier();

        /// <returns> Returns speed multiplier based on Boosts, Prestiges and other multipliers </returns>
        public BigNumber GetSpeedMultiplier();

        /// <returns> Whether the business can autproduce </returns>
        public bool CanAutoProduce();

        /// <returns> Whether the business can produce </returns>
        public bool CanProduce()
        {
            if (ProducableItem.ProductionRoundMode != ProductionRoundMode.ConcurrentRounds && ActiveProductions.Count > 0) return false;

            if (ProducableItem.ProductionLocks != null)
            {
                for (int i = 0; i < ProducableItem.ProductionLocks.Count; i++)
                {
                    if (ProducableItem.ProductionLocks[i] != null && !ProducableItem.ProductionLocks[i].IsUnlocked()) return false;
                }
            }

            BigNumber amountToProduce = Amount - CurrentlyProducing;
            return amountToProduce > 0 && ProducableItem.GetAffordableProductionAmount(amountToProduce) > 0;
        }

        /// <summary> Applies outputs </summary>
        public List<Output> ClaimOutputs(object context = null);

        /// <returns> Production cost </returns>
        public List<Input> GetProductionCost(BigNumber additionalAmount, bool ignoreProductionAmount = false)
        {
            BigNumber amount = ignoreProductionAmount ? Amount : ProductionAmount;

            return ProducableItem.GetProductionCost(amount + additionalAmount);
        }

        /// <returns> Returns production cost per second </returns>
        public List<Input> GetActualProductionCostPerSecond(bool ignoreProductionAmount = false) => GetActualProductionCostPerSecond(0, ignoreProductionAmount);

        /// <returns> Returns production cost per second </returns>
        public List<Input> GetActualProductionCostPerSecond(BigNumber additionalAmount, bool ignoreProductionAmount = false)
        {
            return GetProductionCost(additionalAmount, ignoreProductionAmount).Select(x => new Input()
            {
                currencyInput = x.currencyInput,
                businessInput = x.businessInput,
                inputAmount = x.inputAmount / GetActualTimeToProduce(),
            }).ToList();
        }
    }
}
