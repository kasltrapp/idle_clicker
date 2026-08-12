using System.Collections.Generic;

namespace EasyIdleGame
{
    public static class RewardCalculator
    {
        public static List<Output> GenerateDrops(List<DropTable> tables, BigNumber multiplier, UpgradeEffectContext context = null, DropEvaluationMode evaluationMode = DropEvaluationMode.EvaluateOnce)
        {
            List<Output> outputs = tables.GenerateDrops(multiplier, context, evaluationMode);
            ApplyOutputMultipliers(outputs, context);
            return outputs.Squash();
        }

        public static List<Output> GetAverageOutputs(List<DropTable> tables, BigNumber multiplier, UpgradeEffectContext context = null, DropEvaluationMode evaluationMode = DropEvaluationMode.EvaluateOnce)
        {
            List<Output> outputs = tables.GetAverageOutputs(multiplier, context, evaluationMode);
            ApplyOutputMultipliers(outputs, context);
            return outputs.Squash();
        }

        public static List<Output> ApplyOutputs(List<DropTable> tables, BigNumber multiplier, UpgradeEffectContext context = null, DropEvaluationMode evaluationMode = DropEvaluationMode.EvaluateOnce)
        {
            List<Output> outputs = GenerateDrops(tables, multiplier, context, evaluationMode);
            outputs.ForEach(x => x.OnProductionRoundFinished_Apply(1, context));
            return outputs.Squash();
        }

        public static void ApplyOutputMultipliers(List<Output> outputs, UpgradeEffectContext context)
        {
            if (outputs == null || context == null) return;

            foreach (Output output in outputs)
            {
                BigNumber multiplier = UpgradeEffectResolver.GetRewardOutputMultiplier(context, output);
                output.MultiplyInPlace(multiplier);
            }
        }

        public static UpgradeEffectContext CreateContext(SourceType source)
        {
            return new UpgradeEffectContext { Source = source };
        }
    }
}
