using System.Collections.Generic;
using System.Linq;

namespace EasyIdleGame.Demos
{
    public class BasicFlowDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 75);
            AddCurrency("Relics", 0);
            GrantBusiness("StartupStand", 1);
            BuyBoostForFree("RushBoost", 1);

            Currency credits = Load<Currency>("Credits");
            Currency relics = Load<Currency>("Relics");
            PrestigeManager.Instance.maxPrestigeCount = 3;
            PrestigeManager.Instance.scaleRewardsWithPrestigeCount = true;
            PrestigeManager.Instance.exludedCurrencies = new[] { relics };
            PrestigeManager.Instance.prestigeRequirements = new List<Lock>
            {
                new Lock { currencyLock = credits, inputAmount = 150 },
                new Lock { playerLevelLock = true, inputAmount = 2 }
            };
            PrestigeManager.Instance.currentPrestigeRequirements = PrestigeManager.Instance.prestigeRequirements.ToList();
            PrestigeManager.Instance.prestigeUpgrades = new List<UpgradeBlock>
            {
                new UpgradeBlock(UpgradeType.production, 1.25f)
            };
            PrestigeManager.Instance.prestigeRewardTables = new List<DropTable>
            {
                new DropTable
                {
                    strategy = DropStrategy.All,
                    totalDropAmount = 1,
                    outputs = new List<Output>
                    {
                        new Output { currencyOutput = relics, outputAmount = 1 }
                    }
                }
            };
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Minimal Idle Loop", BusinessPanelMode.Automation, "StartupStand");
            AddManagerAutomationPanel();
            AddUpgradePanel();
            AddBoostPanel();
            AddPlayerProgressionPanel();
            AddPrestigePanel();
            AddOutputPanel("Last core-flow outputs", LastOutputs);
        }
    }
}
