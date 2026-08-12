using System.Linq;
using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class TimedUpgradeQueuesDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 240);
            GrantBusiness("QueuedWorkshop", 2);
            UpgradesManager.Instance.queueMode = UpgradeQueueMode.Parallel;
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Upgrade Target", BusinessPanelMode.BoostTarget, "QueuedWorkshop");
            AddQueueModePanel();
            AddUpgradePanel();
        }

        protected override void AddSceneUpgradeActions(VisualElement card, Upgrade upgrade)
        {
            if (upgrade == null || !ResourceNameEquals(upgrade, "TimedProductionUpgrade")) return;

            Upgrade upgradeAsset = upgrade;
            AddCardAction(card, "Buy +2", () => BuyTimedUpgradeTwice(upgradeAsset), () => CanBuyUpgrade(upgradeAsset), false, () => DescribeUpgradeBuyAvailability(upgradeAsset), false);
            AddCardAction(card, "Skip wait", () => SkipTimedUpgradeWait(upgradeAsset), () => HasPendingUpgrade(upgradeAsset), false, () => HasPendingUpgrade(upgradeAsset) ? "ready, will complete pending timed upgrades" : "there are no pending timed upgrades to apply");
        }

        private void AddQueueModePanel()
        {
            VisualElement section = CreateSection("Queue Mode");
            VisualElement card = CreateObjectCard("UpgradesManager.queueMode", "Manager", AccentColor);
            Upgrade upgrade = Load<Upgrade>("TimedProductionUpgrade");
            UpgradeHolder holder = UpgradesManager.Instance.GetOrAddHolder(upgrade);

            AddLiveKeyValue(card, "Current mode", () => UpgradesManager.Instance.queueMode.ToString(), AccentColor, true);
            AddLiveKeyValue(card, "Pending batches", () => holder.pendingUpgrades.Count.ToString(), () => holder.pendingUpgrades.Count > 0 ? AccentColor : MutedTextColor, holder.pendingUpgrades.Count > 0);
            AddCardAction(card, "Parallel", () => SetQueueMode(UpgradeQueueMode.Parallel));
            AddCardAction(card, "Sequential", () => SetQueueMode(UpgradeQueueMode.Sequential));
            section.Add(card);
        }

        private void SetQueueMode(UpgradeQueueMode mode)
        {
            UpgradesManager.Instance.queueMode = mode;
            LastUpgradeResult = $"Queue mode set to {mode}. Buy the timed upgrade and watch Pending change.";
            Log(LastUpgradeResult);
        }

        private void BuyTimedUpgradeTwice(Upgrade upgrade)
        {
            BuyUpgrade(upgrade);
            if (CanBuyUpgrade(upgrade))
            {
                BuyUpgrade(upgrade);
            }
        }

        private bool HasPendingUpgrade(Upgrade upgrade)
        {
            UpgradeHolder holder = upgrade == null ? null : UpgradesManager.Instance.GetHolder(upgrade);
            return holder != null && holder.pendingUpgrades.Any();
        }

        private void SkipTimedUpgradeWait(Upgrade upgrade)
        {
            UpgradesManager.Instance.SkipUpgradeWait(upgrade);
            LastUpgradeResult = $"Skipped pending wait for {upgrade.upgradeName}; owned={UpgradesManager.Instance.GetOrAddHolder(upgrade).amount}.";
            Log(LastUpgradeResult);
        }
    }
}
