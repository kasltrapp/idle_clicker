using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class ProductionModifiersDemoController : DemoSceneController
    {
        private ProductionModifierManager modifierManager;
        private ProductionBusinessOutputScalingModifierAsset scalingModifier;
        private Business modifierTargetFactory;
        private AutoProduceBoost offlineAutomationBoost;

        protected override bool ShowManualProfitControls => true;

        protected override void SeedDemoState()
        {
            modifierManager = FindFirstObjectByType<ProductionModifierManager>();
            scalingModifier = Load<ProductionBusinessOutputScalingModifierAsset>("ScalingModifier");
            modifierTargetFactory = Load<Business>("ModifierTargetFactory");
            offlineAutomationBoost = Load<AutoProduceBoost>("OfflineAutomation");

            if (modifierManager != null && !modifierManager.enabled)
            {
                modifierManager.enabled = true;
            }

            AddCurrency("Coins", 300);
            GrantBusiness("ModifierTargetFactory", 12);
            BuyBoostForFree("OfflineAutomation", 1);
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Modified Production", BusinessPanelMode.Production, "ModifierTargetFactory");
            AddModifierPanel();
            AddBoostPanel();
            AddProfitPanel();
        }

        protected override bool ShouldAddDefaultCurrencyAction(Currency currency)
        {
            return ResourceNameEquals(currency, "Coins");
        }

        protected override BigNumber GetDefaultCurrencyGrantAmount(Currency currency)
        {
            return ResourceNameEquals(currency, "Coins") ? 300 : base.GetDefaultCurrencyGrantAmount(currency);
        }

        protected override void AddSceneBusinessActions(VisualElement card, string businessName, BusinessPanelMode mode)
        {
            if (businessName != "ModifierTargetFactory") return;

            AddCardAction(card, "Buy +1", () => BuyBusiness("ModifierTargetFactory", 1), () => CanBuyBusinessAmount("ModifierTargetFactory", 1), false, () => DescribeBusinessBuyAvailability("ModifierTargetFactory", 1));
            AddCardAction(card, "Buy +3", () => BuyBusiness("ModifierTargetFactory", 3), () => CanBuyBusinessAmount("ModifierTargetFactory", 3), false, () => DescribeBusinessBuyAvailability("ModifierTargetFactory", 3));
        }

        private bool ModifierEnabled => modifierManager != null && modifierManager.enabled;

        private void AddModifierPanel()
        {
            ProductionBusinessOutputScalingModifierAsset modifier = scalingModifier ?? Load<ProductionBusinessOutputScalingModifierAsset>("ScalingModifier");
            Business factory = modifierTargetFactory ?? Load<Business>("ModifierTargetFactory");
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(factory);

            VisualElement section = CreateSection("Production Output Modifier");
            VisualElement card = CreateObjectCard("Output Scaling", "Production Modifier", AccentColor, true, modifier);

            AddKeyValue(card, "Output target", modifier.outputCurrency != null ? modifier.outputCurrency.currencyName : "not configured");
            AddKeyValue(card, "Scaling source", modifier.scalingSource == ProductionBusinessOutputScalingSource.OwnedAmount ? "owned factory copies" : "copies assigned to this round");
            AddKeyValue(card, "Rule", modifier.scalingMode == ProductionBusinessOutputScalingMode.LinearFloor
                ? $"floor(source / {modifier.scalingBase}) x {modifier.outputAmountPerStep} Coins/round"
                : $"floor(log base {modifier.scalingBase} of source) x {modifier.outputAmountPerStep} Coins/round");
            AddKeyValue(card, "Priority", modifier.priority.ToString());
            AddLiveKeyValue(card, "Owned factory copies", () => holder.amount.ToString());
            AddLiveKeyValue(card, "Base output/copy", () => DescribeOutputs(factory.outputTables.GetAverageOutputs(1)));
            AddLiveKeyValue(card, "Base output/round (all owned)", () => DescribeOutputs(factory.outputTables.GetAverageOutputs(holder.amount)));
            AddLiveKeyValue(card, "Modified output/next round", () => DescribeOutputs(holder.iProducable.GetOptimisticActualProduction()));
            AddLiveKeyValue(card, "Modified average output/sec", () => DescribeOutputs(holder.iProducable.GetActualProductionPerSecond(true)));
            AddLiveKeyValue(card, "Current active output/sec", () => DescribeOutputs(holder.iProducable.GetCurrentProductionPerSecond()));
            AddLiveKeyValue(card, "Offline eligible",
                () => holder.IsAutoProductionPossible() ? "yes" : "no - activate Offline automation",
                () => holder.IsAutoProductionPossible() ? PositiveColor : WarningColor);

            AddLiveCardAction(
                card,
                () => ModifierEnabled ? "Disable bonus" : "Enable bonus",
                ToggleModifier,
                () => modifierManager != null,
                false,
                () => modifierManager == null ? "the demo modifier is not configured" : "ready");

            AddKeyValue(card, "Live comparison", "disable and enable the modifier; base output stays fixed while modified round and average-per-second output change");
            AddKeyValue(card, "Active-rate check", "press Start all on the factory card to populate Current active output/sec");
            AddKeyValue(card, "Offline check", $"use the pre-granted {offlineAutomationBoost?.boostName ?? "Offline automation"} boost, then calculate the same number of minutes with the bonus enabled and disabled; offline calculation applies the same rule");

            section.Add(card);
        }

        private void ToggleModifier()
        {
            if (modifierManager == null)
            {
                Log("The demo modifier is not configured.");
                return;
            }

            modifierManager.enabled = !modifierManager.enabled;
            Log(modifierManager.enabled
                ? "Production bonus enabled."
                : "Production bonus disabled.");
        }
    }
}
