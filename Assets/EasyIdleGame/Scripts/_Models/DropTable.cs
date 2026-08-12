using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyIdleGame
{
    public enum DropStrategy
    {
        [InspectorName("Independent (All)")]
        All,
        [InspectorName("Weighted Random")]
        WeightedRandom
    }

    [Serializable]
    public class DropTable
    {
        [CommentArea("Drop Table", "A pool of outputs that handles probabilities and weighting. 'Independent (All)' rolls each output's chance separately, while 'Weighted Random' picks exactly 'totalDropAmount' outputs based on their relative weights.", "Use Independent (All) for fixed rewards like +10 Coins and a 20% gem chance. Use Weighted Random for loot-style rewards, such as picking 1 prize from Common, Rare, and Epic outputs.")]
        [SerializeField] private string _dropTableComment;

        [Tooltip("How this table chooses outputs. Independent rolls every output using dropChance; Weighted Random selects entries using weight.")]
        public DropStrategy strategy = DropStrategy.All;

        [Tooltip("Number of picks made when strategy is Weighted Random. Ignored by Independent (All), which evaluates every output once.")]
        [Min(1)]
        public int totalDropAmount = 1;

        [Tooltip("Possible outputs in this table. Empty tables grant nothing; configure each entry's amount range, chance, or weight based on the selected strategy.")]
        public List<Output> outputs = new List<Output>();

        public List<Output> GenerateDrops(BigNumber amountMultiplier, DropEvaluationMode evaluationMode = DropEvaluationMode.EvaluateOnce)
        {
            return GenerateDrops(amountMultiplier, evaluationMode, null);
        }

        public List<Output> GenerateDrops(BigNumber amountMultiplier, UpgradeEffectContext modifierContext)
        {
            return GenerateDrops(amountMultiplier, DropEvaluationMode.EvaluateOnce, modifierContext);
        }

        public List<Output> GenerateDrops(BigNumber amountMultiplier, DropEvaluationMode evaluationMode, UpgradeEffectContext modifierContext)
        {
            bool evaluatePerInstance = evaluationMode == DropEvaluationMode.EvaluatePerInstance;
            List<Output> result = new List<Output>();

            if (strategy == DropStrategy.All)
            {
                foreach (var output in outputs)
                {
                    float dropChance = UpgradeEffectResolver.ModifyDropChance(output.dropChance, modifierContext, output);

                    if (evaluatePerInstance)
                    {
                        if (amountMultiplier <= 100)
                        {
                            BigNumber totalAmount = 0;
                            long rolls = (long)amountMultiplier.ToDouble();
                            for (long i = 0; i < rolls; i++)
                            {
                                if (UnityEngine.Random.Range(0f, 1f) <= dropChance)
                                {
                                    totalAmount += output.GetAmount();
                                }
                            }
                            if (totalAmount > 0)
                            {
                                Output newOut = output.Multiply(1);
                                newOut.outputAmount = totalAmount;
                                newOut.outputMaxAmount = totalAmount;
                                result.Add(newOut);
                            }
                        }
                        else
                        {
                            Output avg = output.Multiply(amountMultiplier);
                            BigNumber floorMin = (avg.outputAmount * dropChance).Floor();
                            BigNumber floorMax = (avg.outputMaxAmount * dropChance).Floor();

                            // Since n > 100, Law of Large Numbers applies; lock into the exact average
                            BigNumber expectedAmount = floorMax > floorMin ? (floorMin + floorMax) / 2 : floorMin;

                            avg.outputAmount = expectedAmount;
                            avg.outputMaxAmount = expectedAmount;

                            if (avg.outputAmount > 0) result.Add(avg);
                        }
                    }
                    else
                    {
                        if (UnityEngine.Random.Range(0f, 1f) <= dropChance)
                        {
                            Output rolledOut = output.Multiply(amountMultiplier);
                            BigNumber rolledAmount = rolledOut.GetAmount();
                            rolledOut.outputAmount = rolledAmount;
                            rolledOut.outputMaxAmount = rolledAmount;
                            result.Add(rolledOut);
                        }
                    }
                }
            }
            else if (strategy == DropStrategy.WeightedRandom)
            {
                List<WeightedOutput> weightedOutputs = GetWeightedOutputs(modifierContext);

                if (weightedOutputs.Count > 0)
                {
                    float totalWeight = weightedOutputs.Sum(o => o.Weight);

                    if (totalWeight > 0)
                    {
                        if (evaluatePerInstance && amountMultiplier > 100)
                        {
                            foreach (var weightedOutput in weightedOutputs)
                            {
                                Output output = weightedOutput.Output;
                                float chance = (weightedOutput.Weight / totalWeight) * totalDropAmount;
                                Output avg = output.Multiply(amountMultiplier);
                                BigNumber floorMin = (avg.outputAmount * chance).Floor();
                                BigNumber floorMax = (avg.outputMaxAmount * chance).Floor();

                                BigNumber expectedAmount = floorMax > floorMin ? (floorMin + floorMax) / 2 : floorMin;

                                avg.outputAmount = expectedAmount;
                                avg.outputMaxAmount = expectedAmount;

                                if (avg.outputAmount > 0) result.Add(avg);
                            }
                        }
                        else
                        {
                            long loops = evaluatePerInstance ? (long)(totalDropAmount * amountMultiplier).ToDouble() : totalDropAmount;
                            BigNumber mult = evaluatePerInstance ? 1 : amountMultiplier;

                            for (long i = 0; i < loops; i++)
                            {
                                float randomValue = UnityEngine.Random.Range(0f, totalWeight);
                                float currentWeight = 0f;

                                foreach (var weightedOutput in weightedOutputs)
                                {
                                    currentWeight += weightedOutput.Weight;
                                    if (randomValue <= currentWeight)
                                    {
                                        Output rolledOut = weightedOutput.Output.Multiply(mult);
                                        BigNumber rolledAmount = rolledOut.GetAmount();
                                        rolledOut.outputAmount = rolledAmount;
                                        rolledOut.outputMaxAmount = rolledAmount;
                                        result.Add(rolledOut);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result.Squash();
        }

        public List<Output> GetAverageOutputs(BigNumber amountMultiplier, DropEvaluationMode evaluationMode = DropEvaluationMode.EvaluateOnce)
        {
            return GetAverageOutputs(amountMultiplier, evaluationMode, null);
        }

        public List<Output> GetAverageOutputs(BigNumber amountMultiplier, UpgradeEffectContext modifierContext)
        {
            return GetAverageOutputs(amountMultiplier, DropEvaluationMode.EvaluateOnce, modifierContext);
        }

        public List<Output> GetAverageOutputs(BigNumber amountMultiplier, DropEvaluationMode evaluationMode, UpgradeEffectContext modifierContext)
        {
            bool evaluatePerInstance = evaluationMode == DropEvaluationMode.EvaluatePerInstance;
            List<Output> result = new List<Output>();

            if (strategy == DropStrategy.All)
            {
                foreach (var output in outputs)
                {
                    float dropChance = UpgradeEffectResolver.ModifyDropChance(output.dropChance, modifierContext, output);
                    var avg = output.Multiply(amountMultiplier);
                    avg.outputAmount = avg.outputAmount * dropChance;
                    avg.outputMaxAmount = avg.outputMaxAmount * dropChance;
                    result.Add(avg);
                }
            }
            else if (strategy == DropStrategy.WeightedRandom)
            {
                List<WeightedOutput> weightedOutputs = GetWeightedOutputs(modifierContext);

                if (weightedOutputs.Count > 0)
                {
                    float totalWeight = weightedOutputs.Sum(o => o.Weight);

                    if (totalWeight > 0)
                    {
                        foreach (var weightedOutput in weightedOutputs)
                        {
                            Output output = weightedOutput.Output;
                            float chance = (weightedOutput.Weight / totalWeight) * totalDropAmount;

                            var avg = output.Multiply(amountMultiplier);
                            avg.outputAmount = avg.outputAmount * chance;
                            avg.outputMaxAmount = avg.outputMaxAmount * chance;
                            result.Add(avg);
                        }
                    }
                }
            }

            return result.Squash();
        }

        public List<Output> ApplyOutputs(BigNumber amountMultiplier, DropEvaluationMode evaluationMode = DropEvaluationMode.EvaluateOnce)
        {
            return ApplyOutputs(amountMultiplier, evaluationMode, null);
        }

        public List<Output> ApplyOutputs(BigNumber amountMultiplier, UpgradeEffectContext modifierContext)
        {
            return ApplyOutputs(amountMultiplier, DropEvaluationMode.EvaluateOnce, modifierContext);
        }

        public List<Output> ApplyOutputs(BigNumber amountMultiplier, DropEvaluationMode evaluationMode, UpgradeEffectContext modifierContext)
        {
            List<Output> drops = GenerateDrops(amountMultiplier, evaluationMode, modifierContext);
            RewardCalculator.ApplyOutputMultipliers(drops, modifierContext);
            drops.ForEach(x => x.OnProductionRoundFinished_Apply(1, modifierContext));
            return drops;
        }

        public void MultiplyInPlace(BigNumber multiplier)
        {
            if (outputs != null)
            {
                foreach (var output in outputs)
                {
                    output.MultiplyInPlace(multiplier);
                }
            }
        }

        public void MultiplyInPlace(float multiplier)
        {
            MultiplyInPlace((BigNumber)multiplier);
        }

        private List<WeightedOutput> GetWeightedOutputs(UpgradeEffectContext modifierContext)
        {
            List<WeightedOutput> weightedOutputs = new List<WeightedOutput>();

            if (outputs == null) return weightedOutputs;

            foreach (Output output in outputs)
            {
                float weight = UpgradeEffectResolver.ModifyDropWeight(output.weight, modifierContext, output);
                if (weight > 0) weightedOutputs.Add(new WeightedOutput(output, weight));
            }

            return weightedOutputs;
        }

        private readonly struct WeightedOutput
        {
            public readonly Output Output;
            public readonly float Weight;

            public WeightedOutput(Output output, float weight)
            {
                Output = output;
                Weight = weight;
            }
        }
    }
}