namespace EasyIdleGame.Demos
{
    public class OfflineProfitApplicationDemoController : DemoSceneController
    {
        protected override bool ShowManualProfitControls => true;

        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 25);
            GrantBusiness("OfflineBusiness", 4);
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Offline Producers", BusinessPanelMode.Offline, "OfflineBusiness");
            AddProfitPanel();
        }
    }
}
