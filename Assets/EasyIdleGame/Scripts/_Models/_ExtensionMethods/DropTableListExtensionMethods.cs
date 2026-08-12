using System.Collections.Generic;

namespace EasyIdleGame
{
    public static class DropTableListExtensionMethods
    {
        public static List<Output> ApplyOutputs(this List<DropTable> tables, BigNumber multiplier)
        {
            return ApplyOutputs(tables, multiplier, null);
        }

        public static List<Output> ApplyOutputs(this List<DropTable> tables, BigNumber multiplier, UpgradeEffectContext modifierContext)
        {
            if (tables == null) return new List<Output>();
            List<Output> allDrops = tables.GenerateDrops(multiplier, modifierContext);
            RewardCalculator.ApplyOutputMultipliers(allDrops, modifierContext);
            allDrops.ForEach(x => x.OnProductionRoundFinished_Apply(1, modifierContext));
            return allDrops.Squash();
        }

        public static List<Output> GenerateDrops(this List<DropTable> tables, BigNumber multiplier)
        {
            return GenerateDrops(tables, multiplier, null);
        }

        public static List<Output> GenerateDrops(this List<DropTable> tables, BigNumber multiplier, UpgradeEffectContext modifierContext, DropEvaluationMode evaluationMode = DropEvaluationMode.EvaluateOnce)
        {
            if (tables == null) return new List<Output>();
            List<Output> allDrops = new List<Output>();
            foreach (var table in tables)
            {
                allDrops.AddRange(table.GenerateDrops(multiplier, evaluationMode, modifierContext));
            }
            return allDrops.Squash();
        }

        public static List<Output> GetAverageOutputs(this List<DropTable> tables, BigNumber multiplier)
        {
            return GetAverageOutputs(tables, multiplier, null);
        }

        public static List<Output> GetAverageOutputs(this List<DropTable> tables, BigNumber multiplier, UpgradeEffectContext modifierContext, DropEvaluationMode evaluationMode = DropEvaluationMode.EvaluateOnce)
        {
            if (tables == null) return new List<Output>();

            List<Output> expected = new List<Output>();
            foreach (var table in tables)
            {
                expected.AddRange(table.GetAverageOutputs(multiplier, evaluationMode, modifierContext));
            }
            return expected.Squash();
        }

        public static void MultiplyInPlace(this List<DropTable> tables, BigNumber multiplier)
        {
            if (tables == null) return;
            foreach (var table in tables)
            {
                table.MultiplyInPlace(multiplier);
            }
        }

        public static void MultiplyInPlace(this List<DropTable> tables, float multiplier)
        {
            tables.MultiplyInPlace((BigNumber)multiplier);
        }
    }
}