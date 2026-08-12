using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class BusinessLevelUpgradesDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 220);
            GrantBusiness("TieredStand", 1);
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Upgradeable Business", BusinessPanelMode.Production, "TieredStand");
            AddBusinessLevelUpgradePanel();
            AddOutputPanel("Last business-level outputs", LastOutputs);
        }

        protected override void AddSceneBusinessActions(VisualElement card, string businessName, BusinessPanelMode mode)
        {
            if (businessName != "TieredStand") return;

            AddCardAction(card, "Buy +1", () => BuyBusiness("TieredStand", 1), () => CanBuyBusinessAmount("TieredStand", 1), false, () => DescribeBusinessBuyAvailability("TieredStand", 1));
        }

        protected override void AddSceneBusinessDetails(VisualElement card, string businessName, BusinessPanelMode mode)
        {
            if (businessName != "TieredStand") return;

            Business business = Load<Business>("TieredStand");
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            AddLiveKeyValue(card, "Upgrade level", () => holder.iUpgradable.Level.ToString(), AccentColor, true);
            AddLiveKeyValue(card, "Active override", () => DescribeActiveBusinessUpgrade(holder));
            AddLiveLimit(card, "Max owned", () => holder.GetMaxAmountConstrain());
            AddLiveLimit(card, "Max buyable", () => holder.GetMaxBuyableAmountConstrain());
            AddLiveKeyValue(card, "Cost/purchase", () => DescribeBuyCost(holder.iBuyable, 1), () => GetBuyCostColor(holder.iBuyable, 1));
            AddLiveKeyValue(card, "Buy status", () => DescribeBusinessBuyAvailability("TieredStand", 1), () => CanBuyBusinessAmount("TieredStand", 1) ? PositiveColor : WarningColor);
        }

        private void AddBusinessLevelUpgradePanel()
        {
            VisualElement section = CreateSection("Business Level Upgrades");
            Business business = Load<Business>("TieredStand");
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);

            foreach (BusinessUpgradeLevel level in business.upgradeLevels)
            {
                if (level == null) continue;

                BigNumber targetLevel = level.level;
                VisualElement card = CreateObjectCard($"Level {targetLevel} override", "Business Level", AccentColor);
                AddKeyValue(card, "Metadata", string.IsNullOrWhiteSpace(level.Metadata.name) ? "no metadata override" : level.Metadata.name);
                AddKeyValue(card, "Override fields", DescribeBusinessUpgradeLevel(level));
                AddLiveKeyValue(card, "Cost/to reach level", () => DescribeInputs(holder.iUpgradable.GetUpgradeCost(targetLevel - 1)), () => GetInputCostColor(holder.iUpgradable.GetUpgradeCost(targetLevel - 1)));
                AddLiveKeyValue(card, "Status", () => DescribeBusinessLevelUpgradeAvailability(holder, targetLevel), () => CanUpgradeBusinessToLevel(holder, targetLevel) ? PositiveColor : holder.iUpgradable.Level >= targetLevel ? AccentColor : WarningColor);
                AddCardAction(card, $"Upgrade L{targetLevel}", () => UpgradeBusinessToLevel(holder, targetLevel), () => CanUpgradeBusinessToLevel(holder, targetLevel), false, () => DescribeBusinessLevelUpgradeAvailability(holder, targetLevel));
                section.Add(card);
            }
        }

        private string DescribeActiveBusinessUpgrade(BusinessHolder holder)
        {
            BusinessUpgradeLevel level = holder.iUpgradable.GetCurrentUpgradeLevel() as BusinessUpgradeLevel;
            if (level == null) return "base business metadata";

            string name = string.IsNullOrWhiteSpace(level.Metadata.name) ? $"level {level.level}" : level.Metadata.name;
            return $"{name}: {DescribeBusinessUpgradeLevel(level)}";
        }

        private bool CanUpgradeBusinessToLevel(BusinessHolder holder, BigNumber targetLevel)
        {
            return holder != null
                && holder.iUpgradable.Level < targetLevel
                && holder.iUpgradable.CanUpgradeToLevel(targetLevel - 1);
        }

        private string DescribeBusinessLevelUpgradeAvailability(BusinessHolder holder, BigNumber targetLevel)
        {
            if (holder == null) return "business holder is missing";
            if (holder.iUpgradable.Level >= targetLevel) return "already reached";

            string shortfalls = DescribeInputShortfalls(holder.iUpgradable.GetUpgradeCost(targetLevel - 1));
            return shortfalls == "none"
                ? "ready, upgrade applies this level override"
                : $"missing upgrade cost: {shortfalls}";
        }

        private void UpgradeBusinessToLevel(BusinessHolder holder, BigNumber targetLevel)
        {
            bool upgraded = holder.iUpgradable.TryUpgradeToLevel(targetLevel);
            LastActionResult = $"TryUpgradeToLevel {targetLevel}: {upgraded}; active level={holder.iUpgradable.Level}.";
            Log(LastActionResult);
        }
    }
}
