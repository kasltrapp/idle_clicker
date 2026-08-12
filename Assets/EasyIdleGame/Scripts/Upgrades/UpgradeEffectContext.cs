using System.Collections.Generic;

namespace EasyIdleGame
{
    public sealed class UpgradeEffectContext
    {
        public UpgradeType UpgradeType { get; set; }
        public SourceType Source { get; set; } = SourceType.Any;
        public Business SourceBusiness { get; set; }
        public BusinessHolder SourceBusinessHolder { get; set; }
        public Business TargetBusiness { get; set; }
        public BusinessHolder TargetBusinessHolder { get; set; }
        public List<BusinessGroup> TargetBusinessGroups { get; set; }
        public string TargetBusinessGroupName { get; set; } = "";
        public Currency TargetCurrency { get; set; }
        public Upgrade TargetUpgrade { get; set; }
        public Boost TargetBoost { get; set; }
        public Manager TargetManager { get; set; }
        public BusinessMergeRecipe TargetMergeRecipe { get; set; }
        public bool IsOffline { get; set; }
        public bool IsManual { get; set; }

        public UpgradeEffectContext ForType(UpgradeType type)
        {
            UpgradeEffectContext clone = Clone();
            clone.UpgradeType = type;
            return clone;
        }

        public UpgradeEffectContext WithOutput(Output output)
        {
            UpgradeEffectContext clone = Clone();
            if (output != null)
            {
                clone.TargetBusiness = output.businessOutput;
                clone.TargetBusinessHolder = UpgradeEffectResolver.TryGetBusinessHolder(output.businessOutput);
                clone.TargetCurrency = output.currencyOutput;
                clone.TargetBoost = output.boostOutput;
                clone.TargetManager = output.managerOutput;
            }
            return clone;
        }

        public BigNumber? GetBusinessLevel()
        {
            if (TargetBusinessHolder != null) return TargetBusinessHolder.iUpgradable.Level;
            if (SourceBusinessHolder != null) return SourceBusinessHolder.iUpgradable.Level;
            return null;
        }

        public UpgradeEffectContext Clone()
        {
            return new UpgradeEffectContext
            {
                UpgradeType = UpgradeType,
                Source = Source,
                SourceBusiness = SourceBusiness,
                SourceBusinessHolder = SourceBusinessHolder,
                TargetBusiness = TargetBusiness,
                TargetBusinessHolder = TargetBusinessHolder,
                TargetBusinessGroups = TargetBusinessGroups,
                TargetBusinessGroupName = TargetBusinessGroupName,
                TargetCurrency = TargetCurrency,
                TargetUpgrade = TargetUpgrade,
                TargetBoost = TargetBoost,
                TargetManager = TargetManager,
                TargetMergeRecipe = TargetMergeRecipe,
                IsOffline = IsOffline,
                IsManual = IsManual
            };
        }
    }
}
