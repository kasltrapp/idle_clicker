namespace EasyIdleGame.Demos
{
    public class BusinessChainingDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 90);
            GrantBusiness("Recruiter", 1);
            BuyBoostForFree("ChainAutomationBurst", 1);
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Business Produces Business", BusinessPanelMode.Production, "Recruiter", "Worker");
            AddBoostPanel();
            AddOutputPanel("Last chain outputs", LastOutputs);
        }
    }
}
