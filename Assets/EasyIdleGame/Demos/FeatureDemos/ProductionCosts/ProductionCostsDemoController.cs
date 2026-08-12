using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class ProductionCostsDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 180);
            AddCurrency("Parts", 18);
            GrantBusiness("PartsPress", 2);
            GrantBusiness("AssemblyLine", 4);
            GrantBusiness("ConsumablePrototype", 2);
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Production Cost Chain", BusinessPanelMode.ProductionCost, "PartsPress", "AssemblyLine", "ConsumablePrototype");
            AddOutputPanel("Last production-cost outputs", LastOutputs);
        }

        protected override bool ShouldAddDefaultCurrencyAction(Currency currency)
        {
            return ResourceNameEquals(currency, "Credits") || ResourceNameEquals(currency, "Parts");
        }

        protected override BigNumber GetDefaultCurrencyGrantAmount(Currency currency)
        {
            if (ResourceNameEquals(currency, "Credits")) return 180;
            if (ResourceNameEquals(currency, "Parts")) return 18;
            return base.GetDefaultCurrencyGrantAmount(currency);
        }

        protected override string GetDefaultCurrencyGrantLabel(Currency currency, BigNumber amount)
        {
            return ResourceNameEquals(currency, "Credits") || ResourceNameEquals(currency, "Parts")
                ? $"Add +{amount}"
                : base.GetDefaultCurrencyGrantLabel(currency, amount);
        }
    }
}
