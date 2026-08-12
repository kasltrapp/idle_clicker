using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    public static class UpgradeEffectResolver
    {
        public static BusinessHolder TryGetBusinessHolder(Business business)
        {
            if (business == null || BusinessesManager.Instance == null) return null;
            return BusinessesManager.Instance.GetHolder(business);
        }

        public static BigNumber GetProductionOutputMultiplier(ProductionOutputContext productionContext, Output output)
        {
            UpgradeEffectContext context = CreateProductionContext(productionContext, output);
            return GetMultiplier(UpgradeType.production, context);
        }

        public static BigNumber GetProductionInputCostMultiplier(Business business, Input input, BigNumber productionAmount)
        {
            UpgradeEffectContext context = new UpgradeEffectContext
            {
                Source = SourceType.Production,
                SourceBusiness = business,
                SourceBusinessHolder = TryGetBusinessHolder(business),
                TargetBusiness = input?.businessInput,
                TargetBusinessHolder = TryGetBusinessHolder(input?.businessInput),
                TargetCurrency = input?.currencyInput
            };

            return GetMultiplier(UpgradeType.productionInputCost, context);
        }

        public static BigNumber GetPurchaseCostMultiplier(Business business)
        {
            return GetMultiplier(UpgradeType.purchaseCost, new UpgradeEffectContext
            {
                TargetBusiness = business,
                TargetBusinessHolder = TryGetBusinessHolder(business)
            });
        }

        public static BigNumber GetPurchaseCostMultiplier(Manager manager)
        {
            return GetMultiplier(UpgradeType.purchaseCost, new UpgradeEffectContext
            {
                TargetManager = manager
            });
        }

        public static BigNumber GetPurchaseCostMultiplier(Boost boost)
        {
            return GetMultiplier(UpgradeType.purchaseCost, new UpgradeEffectContext
            {
                TargetBoost = boost
            });
        }

        public static BigNumber GetUpgradePurchaseCostMultiplier(Upgrade upgrade)
        {
            return GetMultiplier(UpgradeType.upgradePurchaseCost, new UpgradeEffectContext
            {
                TargetUpgrade = upgrade
            });
        }

        public static BigNumber GetLevelUpCostMultiplier(IUpgradableHolder holder)
        {
            BusinessHolder businessHolder = holder as BusinessHolder;
            ManagerHolder managerHolder = holder as ManagerHolder;

            return GetMultiplier(UpgradeType.levelUpCost, new UpgradeEffectContext
            {
                SourceBusiness = businessHolder?.business,
                SourceBusinessHolder = businessHolder,
                TargetBusiness = businessHolder?.business,
                TargetBusinessHolder = businessHolder,
                TargetManager = managerHolder?.manager
            });
        }

        public static BigNumber GetBoostDurationMultiplier(Boost boost)
        {
            return GetMultiplier(UpgradeType.duration, new UpgradeEffectContext
            {
                TargetBoost = boost
            });
        }

        public static BigNumber GetBoostEffectMultiplier(Boost boost, UpgradeType type, List<BusinessGroup> businessGroups = null, string group = "")
        {
            UpgradeEffectContext context = new UpgradeEffectContext
            {
                TargetBoost = boost,
                TargetBusinessGroups = businessGroups,
                TargetBusinessGroupName = group
            };

            return GetMultiplier(type, context, false);
        }

        public static BigNumber GetManagerEffectMultiplier(ManagerHolder holder, UpgradeType type, Business business)
        {
            return GetMultiplier(type, new UpgradeEffectContext
            {
                SourceBusiness = business,
                SourceBusinessHolder = TryGetBusinessHolder(business),
                TargetBusiness = business,
                TargetBusinessHolder = TryGetBusinessHolder(business),
                TargetManager = holder?.manager
            });
        }

        public static BigNumber GetManagerActiveDurationMultiplier(ManagerHolder holder)
        {
            return GetMultiplier(UpgradeType.duration, new UpgradeEffectContext
            {
                TargetManager = holder?.manager
            });
        }

        public static BigNumber GetManagerCooldownDurationMultiplier(ManagerHolder holder)
        {
            return GetMultiplier(UpgradeType.cooldown, new UpgradeEffectContext
            {
                TargetManager = holder?.manager
            });
        }

        public static float GetManagerActiveDurationSeconds(ManagerHolder holder)
        {
            if (holder?.manager?.activeDuration == null) return 0f;
            return Mathf.Max(0f, holder.manager.activeDuration.TotalSeconds * (float)GetManagerActiveDurationMultiplier(holder).ToDouble());
        }

        public static float GetManagerCooldownDurationSeconds(ManagerHolder holder)
        {
            if (holder?.manager?.cooldownDuration == null) return 0f;
            return Mathf.Max(0f, holder.manager.cooldownDuration.TotalSeconds * (float)GetManagerCooldownDurationMultiplier(holder).ToDouble());
        }

        public static BigNumber GetRewardOutputMultiplier(UpgradeEffectContext context, Output output)
        {
            UpgradeEffectContext outputContext = (context ?? new UpgradeEffectContext()).WithOutput(output);
            return GetMultiplier(UpgradeType.production, outputContext);
        }

        public static BigNumber GetMergeInputCostMultiplier(BusinessMergeRecipe recipe, Input input)
        {
            return GetMultiplier(UpgradeType.mergeInputCost, new UpgradeEffectContext
            {
                Source = SourceType.Conversion,
                TargetMergeRecipe = recipe,
                TargetBusiness = input?.businessInput,
                TargetBusinessHolder = TryGetBusinessHolder(input?.businessInput),
                TargetCurrency = input?.currencyInput
            });
        }

        public static BigNumber GetMergeSpeedMultiplier(BusinessMergeRecipe recipe)
        {
            return GetMultiplier(UpgradeType.speed, new UpgradeEffectContext
            {
                Source = SourceType.Conversion,
                TargetMergeRecipe = recipe
            });
        }

        public static BigNumber GetMergeCooldownMultiplier(BusinessMergeRecipe recipe)
        {
            return GetMultiplier(UpgradeType.cooldown, new UpgradeEffectContext
            {
                Source = SourceType.Conversion,
                TargetMergeRecipe = recipe
            });
        }

        public static BigNumber GetProductionOutputScalingMultiplier(ProductionOutputContext productionContext, Business outputBusiness)
            => GetProductionOutputScalingMultiplier(productionContext, new Output { businessOutput = outputBusiness });

        public static BigNumber GetProductionOutputScalingMultiplier(ProductionOutputContext productionContext, Output output)
        {
            UpgradeEffectContext context = new UpgradeEffectContext
            {
                Source = SourceType.Production,
                SourceBusiness = productionContext?.ProducingBusiness,
                SourceBusinessHolder = productionContext?.ProducingHolder as BusinessHolder,
                IsOffline = productionContext != null && productionContext.IsOffline,
                IsManual = productionContext != null && productionContext.IsManual
            }.WithOutput(output);

            return GetMultiplier(UpgradeType.productionOutputScaling, context);
        }

        public static float ModifyDropChance(float baseChance, UpgradeEffectContext context, Output output)
        {
            return Mathf.Clamp01(ModifyFloat(baseChance, UpgradeType.dropChance, context?.WithOutput(output)));
        }

        public static float ModifyDropWeight(float baseWeight, UpgradeEffectContext context, Output output)
        {
            return Mathf.Max(0f, ModifyFloat(baseWeight, UpgradeType.dropWeight, context?.WithOutput(output)));
        }

        public static BigNumber GetMultiplier(UpgradeType type, UpgradeEffectContext context)
        {
            return GetMultiplier(type, context, true);
        }

        private static BigNumber GetMultiplier(UpgradeType type, UpgradeEffectContext context, bool includeActiveBoosts)
        {
            if (context == null) context = new UpgradeEffectContext();
            context = context.ForType(type);

            BigNumber multiplier = 1;
            BigNumber additive = 0;

            foreach (var pair in GetMatchingBlocks(context, includeActiveBoosts))
            {
                BigNumber value = GetScaledValue(pair, context);

                if (pair.Block.operation == UpgradeBlockOperation.Add)
                    additive += value;
                else
                    multiplier *= value;
            }

            return BigNumber.Max(0, multiplier + additive);
        }

        public static BigNumber ScaleMultiplierValue(BigNumber value, BigNumber strength)
        {
            if (strength == 1) return value;
            if (value >= 1) return value * strength;
            if (strength <= 0) return value;
            return value / strength;
        }

        private static float ModifyFloat(float baseValue, UpgradeType type, UpgradeEffectContext context)
        {
            if (context == null) context = new UpgradeEffectContext();
            context = context.ForType(type);

            float value = baseValue;
            foreach (var pair in GetMatchingBlocks(context, true))
            {
                float modifier = (float)GetScaledValue(pair, context).ToDouble();
                if (pair.Block.operation == UpgradeBlockOperation.Add)
                    value += modifier;
                else
                    value *= modifier;
            }

            return value;
        }

        private static UpgradeEffectContext CreateProductionContext(ProductionOutputContext productionContext, Output output)
        {
            return new UpgradeEffectContext
            {
                Source = SourceType.Production,
                SourceBusiness = productionContext?.ProducingBusiness,
                SourceBusinessHolder = productionContext?.ProducingHolder as BusinessHolder,
                TargetBusiness = output?.businessOutput,
                TargetBusinessHolder = TryGetBusinessHolder(output?.businessOutput),
                TargetCurrency = output?.currencyOutput,
                TargetBoost = output?.boostOutput,
                TargetManager = output?.managerOutput,
                IsOffline = productionContext != null && productionContext.IsOffline,
                IsManual = productionContext != null && productionContext.IsManual
            };
        }

        private static IEnumerable<MatchingBlock> GetMatchingBlocks(UpgradeEffectContext context, bool includeActiveBoosts)
        {
            if (UpgradesManager.Instance != null)
            {
                foreach (UpgradeHolder holder in UpgradesManager.Instance.upgrades)
                {
                    if (holder == null || holder.amount <= 0 || holder.upgrade == null || holder.upgrade.upgrades == null) continue;
                    if (!UpgradeAppliesToContext(holder.upgrade, context)) continue;

                    foreach (UpgradeBlock block in holder.upgrade.upgrades)
                    {
                        if (block != null && block.IsContextual && block.Matches(context))
                            yield return new MatchingBlock(holder, null, block);
                    }
                }
            }

            if (!includeActiveBoosts || BoostsManager.Instance == null) yield break;

            foreach (ActiveBoostHolder holder in BoostsManager.Instance.activeBoosts)
            {
                if (holder == null || holder.timeLeft <= 0f || holder.boost is not GenericMultiplierBoost boost || boost.upgradeBlocks == null) continue;
                if (!BoostAppliesToContext(boost, context)) continue;

                foreach (UpgradeBlock block in boost.upgradeBlocks)
                {
                    if (block != null && block.IsContextual && block.Matches(context))
                        yield return new MatchingBlock(null, boost, block);
                }
            }
        }

        private static bool UpgradeAppliesToContext(Upgrade upgrade, UpgradeEffectContext context)
        {
            if (upgrade == null) return false;
            if (string.IsNullOrEmpty(upgrade.Group) && (upgrade.TargetBusinessGroups == null || upgrade.TargetBusinessGroups.Count == 0)) return true;

            if (context.SourceBusiness != null && context.SourceBusiness.HasGroupMatch(upgrade.TargetBusinessGroups, upgrade.Group)) return true;
            if (context.TargetBusiness != null && context.TargetBusiness.HasGroupMatch(upgrade.TargetBusinessGroups, upgrade.Group)) return true;
            if (Business.HasGroupMatch(context.TargetBusinessGroups, context.TargetBusinessGroupName, upgrade.TargetBusinessGroups, upgrade.Group)) return true;

            return false;
        }

        private static bool BoostAppliesToContext(GenericMultiplierBoost boost, UpgradeEffectContext context)
        {
            if (boost == null) return false;
            if (string.IsNullOrEmpty(boost.Group) && (boost.TargetBusinessGroups == null || boost.TargetBusinessGroups.Count == 0)) return true;

            if (context.SourceBusiness != null && context.SourceBusiness.HasGroupMatch(boost.TargetBusinessGroups, boost.Group)) return true;
            if (context.TargetBusiness != null && context.TargetBusiness.HasGroupMatch(boost.TargetBusinessGroups, boost.Group)) return true;
            if (Business.HasGroupMatch(context.TargetBusinessGroups, context.TargetBusinessGroupName, boost.TargetBusinessGroups, boost.Group)) return true;

            return false;
        }

        private static BigNumber GetScaledValue(MatchingBlock pair, UpgradeEffectContext context)
        {
            if (pair.UpgradeHolder != null)
                return GetScaledUpgradeValue(pair.UpgradeHolder, pair.Block);

            BigNumber value = pair.Block.value;
            if (pair.Boost != null)
            {
                BigNumber strength = GetBoostEffectMultiplier(pair.Boost, pair.Block.upgradeType, context?.TargetBusinessGroups, context?.TargetBusinessGroupName ?? "");
                value = ScaleMultiplierValue(value, strength);
            }

            return value;
        }

        private static BigNumber GetScaledUpgradeValue(UpgradeHolder holder, UpgradeBlock block)
        {
            if (block.operation == UpgradeBlockOperation.Add)
                return block.value * holder.amount;

            return Mathb.GeometricSequence_NthTerm(block.value, block.value, holder.amount);
        }

        private readonly struct MatchingBlock
        {
            public readonly UpgradeHolder UpgradeHolder;
            public readonly Boost Boost;
            public readonly UpgradeBlock Block;

            public MatchingBlock(UpgradeHolder upgradeHolder, Boost boost, UpgradeBlock block)
            {
                UpgradeHolder = upgradeHolder;
                Boost = boost;
                Block = block;
            }
        }
    }
}
