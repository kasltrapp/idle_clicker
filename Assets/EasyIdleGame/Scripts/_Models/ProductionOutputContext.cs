namespace EasyIdleGame
{
    public sealed class ProductionOutputContext
    {
        public IProducableHolder ProducingHolder { get; }
        public IProducable ProducableItem { get; }
        public Business ProducingBusiness { get; }
        public BigNumber OwnedAmount { get; }
        public BigNumber ProductionAmount { get; }
        public BigNumber CurrentlyProducingAmount { get; }
        public BigNumber AdditionalAmount { get; }
        public bool IgnoreProductionAmount { get; }
        public DropEvaluationMode DropEvaluationMode { get; }
        public ProductionOutputCalculationMode CalculationMode { get; }
        public BigNumber ProductionMultiplier { get; }
        public float ActualTimeToProduce { get; }
        public bool IsManual { get; }
        public bool IsOffline => CalculationMode == ProductionOutputCalculationMode.OfflineCalculation;

        public bool IsPerSecond =>
            CalculationMode == ProductionOutputCalculationMode.ProductionPerSecondPreview ||
            CalculationMode == ProductionOutputCalculationMode.CurrentProductionPerSecondPreview;

        public ProductionOutputContext(
            IProducableHolder producingHolder,
            BigNumber productionAmount,
            DropEvaluationMode dropEvaluationMode,
            ProductionOutputCalculationMode calculationMode,
            BigNumber additionalAmount,
            bool ignoreProductionAmount,
            bool isManual = false)
        {
            ProducingHolder = producingHolder;
            ProducableItem = producingHolder?.ProducableItem;
            ProducingBusiness = ProducableItem as Business;
            OwnedAmount = producingHolder != null ? producingHolder.Amount : 0;
            ProductionAmount = productionAmount;
            CurrentlyProducingAmount = producingHolder != null ? producingHolder.CurrentlyProducing : 0;
            AdditionalAmount = additionalAmount;
            IgnoreProductionAmount = ignoreProductionAmount;
            DropEvaluationMode = dropEvaluationMode;
            CalculationMode = calculationMode;
            ProductionMultiplier = producingHolder != null ? producingHolder.GetProductionMultiplier() : 1;
            ActualTimeToProduce = producingHolder != null ? producingHolder.GetActualTimeToProduce() : 1f;
            IsManual = isManual;
        }

        public BigNumber ProjectRoundAmount(BigNumber roundAmount)
        {
            BigNumber projectedAmount = roundAmount * ProductionMultiplier;

            if (IsPerSecond)
                projectedAmount /= ActualTimeToProduce;

            return projectedAmount;
        }

        public Output ProjectRoundOutput(Output roundOutput)
        {
            BigNumber generatedAmount = ProjectRoundAmount(roundOutput.GetAverageAmount());
            generatedAmount *= UpgradeEffectResolver.GetProductionOutputMultiplier(this, roundOutput);

            return new Output
            {
                businessOutput = roundOutput.businessOutput,
                currencyOutput = roundOutput.currencyOutput,
                boostOutput = roundOutput.boostOutput,
                managerOutput = roundOutput.managerOutput,
                outputAmount = generatedAmount,
                outputMaxAmount = generatedAmount
            };
        }

        public Output CreateProjectedBusinessOutput(Business businessOutput, BigNumber roundAmount)
            => CreateProjectedOutput(new Output { businessOutput = businessOutput }, roundAmount);

        public Output CreateProjectedOutput(Output outputTemplate, BigNumber roundAmount)
        {
            BigNumber generatedAmount = ProjectRoundAmount(roundAmount);
            generatedAmount *= UpgradeEffectResolver.GetProductionOutputMultiplier(this, new Output
            {
                businessOutput = outputTemplate?.businessOutput,
                currencyOutput = outputTemplate?.currencyOutput,
                boostOutput = outputTemplate?.boostOutput,
                managerOutput = outputTemplate?.managerOutput,
                outputAmount = roundAmount,
                outputMaxAmount = roundAmount
            });

            return new Output
            {
                businessOutput = outputTemplate?.businessOutput,
                currencyOutput = outputTemplate?.currencyOutput,
                boostOutput = outputTemplate?.boostOutput,
                managerOutput = outputTemplate?.managerOutput,
                outputAmount = generatedAmount,
                outputMaxAmount = generatedAmount
            };
        }
    }
}