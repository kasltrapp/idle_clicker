using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace EasyIdleGame
{
    [Serializable]
    public class UpgradeBlockFilter
    {
        [Tooltip("Restricts production-related modifiers to this producing business. Leave null to allow any source business.")]
        public Business sourceBusiness;

        [Tooltip("Restricts production-related modifiers to producing businesses in any of these groups. Leave empty to allow any source business group.")]
        public List<BusinessGroup> sourceBusinessGroups = new List<BusinessGroup>();

        [Tooltip("Restricts reward/output modifiers to a source, such as daily rewards, ads, production, or merge recipes.")]
        [FormerlySerializedAs("rewardSource")]
        public SourceType source = SourceType.Any;

        [Tooltip("Restricts this block to outputs, input costs, or rewards involving this currency. Leave null to allow any currency.")]
        public Currency targetCurrency;

        [Tooltip("Restricts this block to currencies in any of these groups. Leave empty to allow any currency group.")]
        public List<CurrencyGroup> targetCurrencyGroups = new List<CurrencyGroup>();

        [Tooltip("Restricts this block to outputs, input costs, or upgrade costs involving this business. Leave null to allow any business.")]
        public Business targetBusiness;

        [Tooltip("Restricts this block to businesses in any of these groups. Leave empty to allow any business group.")]
        public List<BusinessGroup> targetBusinessGroups = new List<BusinessGroup>();

        [Tooltip("Restricts this block to buying or affecting this specific upgrade. Leave null to allow any upgrade.")]
        public Upgrade targetUpgrade;

        [Tooltip("Restricts this block to upgrades in any of these groups. Leave empty to allow any upgrade group.")]
        public List<UpgradeGroup> targetUpgradeGroups = new List<UpgradeGroup>();

        [Tooltip("Restricts this block to this specific boost, such as boosting duration or effect strength. Leave null to allow any boost.")]
        public Boost targetBoost;

        [Tooltip("Restricts this block to boosts in any of these groups. Leave empty to allow any boost group.")]
        public List<BoostGroup> targetBoostGroups = new List<BoostGroup>();

        [Tooltip("Restricts this block to this manager's automation or manager-provided multiplier. Leave null to allow any manager.")]
        public Manager targetManager;

        [Tooltip("Restricts this block to this merge recipe's inputs, outputs, duration, or cooldown. Leave null to allow any merge recipe.")]
        public BusinessMergeRecipe targetMergeRecipe;

        [Tooltip("Minimum active business upgrade level required. 0 means no lower bound.")]
        public int minBusinessLevel;

        [Tooltip("Maximum active business upgrade level allowed. -1 means no upper bound. With minBusinessLevel 0, this disables level filtering.")]
        public int maxBusinessLevel = -1;

        [Tooltip("Runtime contexts where this block applies. Leave no flags selected for no runtime restriction; otherwise select one or more contexts such as Active, Offline, and Manual.")]
        public UpgradeRuntimeFilter runtimeFilter;

        public bool HasAnyFilter =>
            source != SourceType.Any ||
            sourceBusiness != null || HasAny(sourceBusinessGroups) ||
            targetCurrency != null || HasAny(targetCurrencyGroups) ||
            targetBusiness != null || HasAny(targetBusinessGroups) ||
            targetUpgrade != null || HasAny(targetUpgradeGroups) ||
            targetBoost != null || HasAny(targetBoostGroups) ||
            targetManager != null ||
            targetMergeRecipe != null ||
            HasBusinessLevelRange ||
            runtimeFilter != (UpgradeRuntimeFilter)0;

        private bool HasBusinessLevelRange => minBusinessLevel > 0 || maxBusinessLevel >= 0;

        public bool Matches(UpgradeEffectContext context)
        {
            if (context == null) return false;
            if (source != SourceType.Any && source != context.Source) return false;
            if (!MatchesRuntimeFilter(context)) return false;

            if (!MatchesBusiness(sourceBusiness, sourceBusinessGroups, context.SourceBusiness)) return false;
            if (!MatchesBusiness(targetBusiness, targetBusinessGroups, context.TargetBusiness, context.TargetBusinessGroups, context.TargetBusinessGroupName)) return false;
            if (!MatchesCurrency(context.TargetCurrency)) return false;
            if (!MatchesUpgrade(context.TargetUpgrade)) return false;
            if (!MatchesBoost(context.TargetBoost)) return false;

            if (targetManager != null && targetManager != context.TargetManager) return false;
            if (targetMergeRecipe != null && targetMergeRecipe != context.TargetMergeRecipe) return false;
            if (!MatchesBusinessLevel(context)) return false;

            return true;
        }

        public UpgradeBlockFilter Clone()
        {
            return new UpgradeBlockFilter
            {
                sourceBusiness = sourceBusiness,
                sourceBusinessGroups = sourceBusinessGroups != null ? new List<BusinessGroup>(sourceBusinessGroups) : new List<BusinessGroup>(),
                source = source,
                targetCurrency = targetCurrency,
                targetCurrencyGroups = targetCurrencyGroups != null ? new List<CurrencyGroup>(targetCurrencyGroups) : new List<CurrencyGroup>(),
                targetBusiness = targetBusiness,
                targetBusinessGroups = targetBusinessGroups != null ? new List<BusinessGroup>(targetBusinessGroups) : new List<BusinessGroup>(),
                targetUpgrade = targetUpgrade,
                targetUpgradeGroups = targetUpgradeGroups != null ? new List<UpgradeGroup>(targetUpgradeGroups) : new List<UpgradeGroup>(),
                targetBoost = targetBoost,
                targetBoostGroups = targetBoostGroups != null ? new List<BoostGroup>(targetBoostGroups) : new List<BoostGroup>(),
                targetManager = targetManager,
                targetMergeRecipe = targetMergeRecipe,
                minBusinessLevel = minBusinessLevel,
                maxBusinessLevel = maxBusinessLevel,
                runtimeFilter = runtimeFilter
            };
        }

        private bool MatchesRuntimeFilter(UpgradeEffectContext context)
        {
            if (runtimeFilter == (UpgradeRuntimeFilter)0) return true;

            UpgradeRuntimeFilter currentContext = context.IsOffline ? UpgradeRuntimeFilter.Offline : UpgradeRuntimeFilter.Active;
            if (context.IsManual) currentContext |= UpgradeRuntimeFilter.Manual;

            return (runtimeFilter & currentContext) != 0;
        }

        private static bool HasAny<T>(List<T> list)
        {
            return list != null && list.Count > 0;
        }

        private static bool MatchesBusiness(Business exact, List<BusinessGroup> groups, Business contextBusiness)
        {
            return MatchesBusiness(exact, groups, contextBusiness, null, "");
        }

        private static bool MatchesBusiness(Business exact, List<BusinessGroup> groups, Business contextBusiness, List<BusinessGroup> contextGroups, string contextGroupName)
        {
            if (exact != null && exact != contextBusiness) return false;

            if (groups != null && groups.Count > 0)
            {
                if (contextBusiness != null) return contextBusiness.HasGroupMatch(groups, "");
                if (!Business.HasGroupMatch(contextGroups, contextGroupName, groups, "")) return false;
            }

            return true;
        }

        private bool MatchesCurrency(Currency currency)
        {
            if (targetCurrency != null && targetCurrency != currency) return false;

            if (targetCurrencyGroups != null && targetCurrencyGroups.Count > 0)
            {
                if (currency == null || currency.currencyGroups == null) return false;
                if (!targetCurrencyGroups.Any(group => group != null && currency.currencyGroups.Contains(group))) return false;
            }

            return true;
        }

        private bool MatchesUpgrade(Upgrade upgrade)
        {
            if (targetUpgrade != null && targetUpgrade != upgrade) return false;

            if (targetUpgradeGroups != null && targetUpgradeGroups.Count > 0)
            {
                if (upgrade == null || upgrade.upgradeGroups == null) return false;
                if (!targetUpgradeGroups.Any(group => group != null && upgrade.upgradeGroups.Contains(group))) return false;
            }

            return true;
        }

        private bool MatchesBoost(Boost boost)
        {
            if (targetBoost != null && targetBoost != boost) return false;

            if (targetBoostGroups != null && targetBoostGroups.Count > 0)
            {
                if (boost == null || boost.boostGroups == null) return false;
                if (!targetBoostGroups.Any(group => group != null && boost.boostGroups.Contains(group))) return false;
            }

            return true;
        }

        private bool MatchesBusinessLevel(UpgradeEffectContext context)
        {
            if (!HasBusinessLevelRange) return true;

            BigNumber? level = context.GetBusinessLevel();
            if (!level.HasValue) return false;

            double levelValue = level.Value.ToDouble();
            if (levelValue < minBusinessLevel) return false;
            if (maxBusinessLevel >= 0 && levelValue > maxBusinessLevel) return false;
            return true;
        }
    }
}
