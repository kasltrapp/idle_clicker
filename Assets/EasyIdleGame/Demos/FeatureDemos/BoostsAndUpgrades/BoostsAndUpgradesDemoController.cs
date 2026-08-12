namespace EasyIdleGame.Demos
{
    public class BoostsAndUpgradesDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 80);
            GrantBusiness("BoostedBusiness", 3);
            BuyBoostForFree("ProductionBoost", 2);
            BuyBoostForFree("ProductionBoostVariant", 1);
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Boost Target", BusinessPanelMode.BoostTarget, "BoostedBusiness");
            AddBoostPanel();
            AddUpgradePanel();
        }
    }
}
