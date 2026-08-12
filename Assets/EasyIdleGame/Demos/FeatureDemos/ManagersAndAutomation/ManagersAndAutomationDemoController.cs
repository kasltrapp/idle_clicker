using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class ManagersAndAutomationDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 250);
            GrantBusiness("ManagedWorkshop", 2);
            GrantBusiness("AutoDepot", 2);
            ManagersManager.Instance.GetOrAddHolder(Load<Manager>("Foreman"));
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Managed Businesses", BusinessPanelMode.Automation, "ManagedWorkshop", "AutoDepot");
            AddManagerAutomationPanel();
        }

        protected override void AddSceneBusinessActions(VisualElement card, string businessName, BusinessPanelMode mode)
        {
            if (businessName == "ManagedWorkshop")
            {
                AddCardAction(card, "Tick 6s", () => TickManagersAndBusinesses(6f));
            }
            else if (businessName == "AutoDepot")
            {
                AddCardAction(card, "Tick 4s", () => TickManagersAndBusinesses(4f));
            }
        }
    }
}
