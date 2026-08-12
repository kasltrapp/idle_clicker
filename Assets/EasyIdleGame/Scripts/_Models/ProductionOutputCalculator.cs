using System.Collections.Generic;
using System.Linq;

namespace EasyIdleGame
{
    public static class ProductionOutputCalculator
    {
        public static List<Output> CalculateAverageProduction(
            IProducableHolder holder,
            BigNumber productionAmount,
            ProductionOutputCalculationMode calculationMode)
        {
            return CalculateAverageProduction(holder, productionAmount, calculationMode, 0, false);
        }

        public static List<Output> CalculateAverageProduction(
            IProducableHolder holder,
            BigNumber productionAmount,
            ProductionOutputCalculationMode calculationMode,
            BigNumber additionalAmount,
            bool ignoreProductionAmount,
            bool isManual = false)
        {
            ProductionOutputContext context = CreateContext(holder, productionAmount, calculationMode, additionalAmount, ignoreProductionAmount, isManual);

            List<Output> outputs = holder.ProducableItem.OutputTables
                .SelectMany(t => t.GetAverageOutputs(productionAmount, context.DropEvaluationMode, CreateModifierContext(context)))
                .Select(context.ProjectRoundOutput)
                .ToList();

            ProductionModifierRegistry.ApplyOutputModifiers(context, outputs);
            return outputs;
        }

        public static List<Output> CalculateRolledProduction(
            IProducableHolder holder,
            BigNumber productionAmount,
            ProductionOutputCalculationMode calculationMode)
        {
            return CalculateRolledProduction(holder, productionAmount, calculationMode, 0, false);
        }

        public static List<Output> CalculateRolledProduction(
            IProducableHolder holder,
            BigNumber productionAmount,
            ProductionOutputCalculationMode calculationMode,
            BigNumber additionalAmount,
            bool ignoreProductionAmount,
            bool isManual = false)
        {
            ProductionOutputContext context = CreateContext(holder, productionAmount, calculationMode, additionalAmount, ignoreProductionAmount, isManual);

            List<Output> outputs = holder.ProducableItem.OutputTables
                .SelectMany(t => t.GenerateDrops(productionAmount, context.DropEvaluationMode, CreateModifierContext(context)))
                .Select(context.ProjectRoundOutput)
                .ToList();

            ProductionModifierRegistry.ApplyOutputModifiers(context, outputs);
            return outputs;
        }

        private static ProductionOutputContext CreateContext(
            IProducableHolder holder,
            BigNumber productionAmount,
            ProductionOutputCalculationMode calculationMode,
            BigNumber additionalAmount,
            bool ignoreProductionAmount,
            bool isManual = false)
        {
            DropEvaluationMode evaluationMode = (holder.ProducableItem as Business)?.dropEvaluationMode ?? DropEvaluationMode.EvaluateOnce;

            return new ProductionOutputContext(
                holder,
                productionAmount,
                evaluationMode,
                calculationMode,
                additionalAmount,
                ignoreProductionAmount,
                isManual);
        }

        private static UpgradeEffectContext CreateModifierContext(ProductionOutputContext context)
        {
            return new UpgradeEffectContext
            {
                Source = SourceType.Production,
                SourceBusiness = context.ProducingBusiness,
                SourceBusinessHolder = context.ProducingHolder as BusinessHolder,
                IsOffline = context.IsOffline,
                IsManual = context.IsManual
            };
        }
    }
}
