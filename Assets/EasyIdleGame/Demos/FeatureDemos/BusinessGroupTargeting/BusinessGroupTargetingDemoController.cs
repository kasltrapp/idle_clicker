namespace EasyIdleGame.Demos
{
    public class BusinessGroupTargetingDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 180);
            GrantBusiness("GroupedBusiness", 2);
            GrantBusiness("UngroupedBusiness", 2);
            BuyBoostForFree("TargetedBoost", 1);
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Group Target Businesses", BusinessPanelMode.BoostTarget, "GroupedBusiness", "UngroupedBusiness");
            AddBusinessGroupTargetingPanel();
            AddBoostPanel();
        }
    }
}
