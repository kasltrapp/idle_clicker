using System.Collections.Generic;
using System.Linq;

namespace EasyIdleGame.Demos
{
    public class PrestigeResetsDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 80);
            AddCurrency("Relics", 0);
            GrantBusiness("PrestigeWorkshop", 1);

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
                new UpgradeBlock(UpgradeType.production, 1.5f),
                new UpgradeBlock(UpgradeType.speed, 1.15f)
            };
            PrestigeManager.Instance.prestigeRewardTables = new List<DropTable>
            {
                new DropTable
                {
                    strategy = DropStrategy.All,
                    totalDropAmount = 1,
                    outputs = new List<Output>
                    {
                        new Output { currencyOutput = relics, outputAmount = 2 }
                    }
                }
            };
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Prestige Run Source", BusinessPanelMode.Progression, "PrestigeWorkshop");
            AddPlayerProgressionPanel();
            AddPrestigePanel();
            AddOutputPanel("Last prestige rewards", LastOutputs);
        }

        protected override void RunPrestige()
        {
            bool prestiged = PrestigeManager.Instance.TryPrestigeGame(out List<Output> outputs);
            LastOutputs = outputs == null ? new List<Output>() : outputs.Squash();

            if (prestiged)
            {
                GrantBusiness("PrestigeWorkshop", 1);
            }

            LastPrestigeResult = prestiged
                ? $"TryPrestigeGame: true, count={PrestigeManager.Instance.prestiges.prestigeCount}; restored Prestige workshop x1."
                : $"TryPrestigeGame: false, count={PrestigeManager.Instance.prestiges.prestigeCount}.";
            Log($"{LastPrestigeResult} rewards={DescribeOutputs(LastOutputs)}.");
        }
    }
}
