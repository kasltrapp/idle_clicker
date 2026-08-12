using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class PlayerProgressionDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 120);
            GrantBusiness("ProgressionStand", 1);
            AchievementsManager.Instance.GetOrAddHolder(Load<Achievement>("QuestAchievement"));
            PlayerStats.Instance.IncrementStat(Load<CustomStat>("QuestCompletions"), 0);
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Progression Sources", BusinessPanelMode.Progression, "ProgressionStand");
            AddPlayerProgressionPanel();
            AddOutputPanel("Last progression rewards", LastOutputs);
        }

        protected override void AddSceneBusinessActions(VisualElement card, string businessName, BusinessPanelMode mode)
        {
            if (businessName == "ProgressionStand")
            {
                AddCardAction(card, "Buy +1", () => BuyBusiness("ProgressionStand", 1), () => CanBuyBusinessAmount("ProgressionStand", 1), false, () => DescribeBusinessBuyAvailability("ProgressionStand", 1));
            }
        }
    }
}
