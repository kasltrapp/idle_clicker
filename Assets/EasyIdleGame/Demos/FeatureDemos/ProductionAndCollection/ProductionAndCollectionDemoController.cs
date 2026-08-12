using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class ProductionAndCollectionDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            Currency credits = Load<Currency>("Credits");
            Currency tickets = LoadOptional<Currency>("Tickets");

            CurrencyManager.Instance.AddCurrencyAmount(credits, 160);
            if (tickets != null) CurrencyManager.Instance.AddCurrencyAmount(tickets, 4);

            GrantBusiness("ManualKiosk", 2);
            GrantBusiness("AutoBakery", 2);
            GrantBusiness("BatchWorkshop", 3);

            StartAllOwnedBusinessCopies("ManualKiosk");
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Production Businesses", BusinessPanelMode.Production, "ManualKiosk", "AutoBakery", "BatchWorkshop", "KioskBusiness");
            AddOutputPanel("Last outputs", LastOutputs);
        }

        protected override void AddSceneBusinessActions(VisualElement card, string businessName, BusinessPanelMode mode)
        {
            if (businessName == "ManualKiosk")
            {
                AddCardAction(card, "Buy +1", () => BuyBusiness("ManualKiosk", 1), () => CanBuyBusinessAmount("ManualKiosk", 1), false, () => DescribeBusinessBuyAvailability("ManualKiosk", 1));
            }
            else if (businessName == "BatchWorkshop")
            {
                AddCardAction(card, "Grant +3", () => GrantBusiness("BatchWorkshop", 3));
                AddCardAction(card, "Start one", () => StartBusiness("BatchWorkshop", 1), () => CanStartBusinessProduction("BatchWorkshop", 1), false, () => DescribeStartProductionAvailability("BatchWorkshop", 1));
                AddCardAction(card, "Start x2", () => StartBusiness("BatchWorkshop", 2), () => CanStartBusinessProduction("BatchWorkshop", 2), false, () => DescribeStartProductionAvailability("BatchWorkshop", 2));
            }
            else if (businessName == "AutoBakery")
            {
                AddCardAction(card, "Buy +1", () => BuyBusiness("AutoBakery", 1), () => CanBuyBusinessAmount("AutoBakery", 1), false, () => DescribeBusinessBuyAvailability("AutoBakery", 1));
            }
            else if (businessName == "KioskBusiness")
            {
                AddCardAction(card, "Buy +1", () => BuyBusiness("KioskBusiness", 1), () => CanBuyBusinessAmount("KioskBusiness", 1), false, () => DescribeBusinessBuyAvailability("KioskBusiness", 1));
            }
        }
    }
}
