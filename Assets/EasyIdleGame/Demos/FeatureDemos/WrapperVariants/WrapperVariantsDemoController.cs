using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class WrapperVariantsDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 120);
            GrantBusiness("BaseBusiness", 1);
            GrantBusiness("WrappedBusiness", 1);
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Base And Wrapped Businesses", BusinessPanelMode.Production, "BaseBusiness", "WrappedBusiness");
            AddOutputPanel("Last wrapper outputs", LastOutputs);
        }

        protected override void AddSceneBusinessActions(VisualElement card, string businessName, BusinessPanelMode mode)
        {
            if (businessName == "BaseBusiness")
            {
                AddCardAction(card, "Grant +1", () => GrantBusiness("BaseBusiness", 1));
                AddCardAction(card, "Produce all now", () => ProduceBusinessNow("BaseBusiness"), () => CanStartAnyBusinessProduction("BaseBusiness"), false, () => DescribeStartAllProductionAvailability("BaseBusiness"));
            }
            else if (businessName == "WrappedBusiness")
            {
                AddCardAction(card, "Grant +1", () => GrantBusiness("WrappedBusiness", 1));
                AddCardAction(card, "Produce all now", () => ProduceBusinessNow("WrappedBusiness"), () => CanStartAnyBusinessProduction("WrappedBusiness"), false, () => DescribeStartAllProductionAvailability("WrappedBusiness"));
            }
        }
    }
}
