using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class LocksAndBuyAmountsDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 90);
            AddCurrency("Gems", 3);
            GrantBusiness("StarterStand", 1);
            BusinessesManager.Instance.GetOrAddHolder(Load<Business>("LockedFactory")).iUnlockable.IsUnlocked();
            BusinessesManager.Instance.GetOrAddHolder(Load<Business>("CapacityBooth")).iUnlockable.IsUnlocked();
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Currency Source", BusinessPanelMode.Production, "StarterStand");
            AddBusinessPanel("Locked And Capped Businesses", BusinessPanelMode.Inventory, "LockedFactory", "CapacityBooth");
            AddPlayerProgressionPanel();
            AddLocksAndBuyAmountsPanel();
        }

        protected override bool ShouldAddDefaultCurrencyAction(Currency currency)
        {
            return ResourceNameEquals(currency, "Credits") || ResourceNameEquals(currency, "Gems");
        }

        protected override BigNumber GetDefaultCurrencyGrantAmount(Currency currency)
        {
            return ResourceNameEquals(currency, "Gems") ? 5 : base.GetDefaultCurrencyGrantAmount(currency);
        }

        protected override void AddSceneBusinessActions(VisualElement card, string businessName, BusinessPanelMode mode)
        {
            if (businessName == "StarterStand" || businessName == "LockedFactory" || businessName == "CapacityBooth")
            {
                AddCardAction(card, "Buy target", () => BuyTargetAmount(businessName), () => CanBuyTargetBusinessAmount(businessName), false, () => DescribeTargetBusinessBuyAvailability(businessName), false);
            }
        }

        public string VerifyCapacityBuyTargetForMcp()
        {
            ResetDemoState();

            int guard = 0;
            while (BusinessesManager.Instance.GetBuyAmount().ToString() != "x5" && guard < 10)
            {
                CycleBuyAmount();
                guard++;
            }

            Business business = Load<Business>("CapacityBooth");
            Currency credits = Load<Currency>("Credits");
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);

            BigNumber targetBefore = BusinessesManager.Instance.GetTargetBusinessBuyAmount(business);
            bool canBuyFiveBefore = holder.iBuyable.CanBuy(5);
            BigNumber amountBefore = holder.amount;
            BigNumber boughtBefore = holder.BoughtAmount;
            BigNumber currencyBefore = CurrencyManager.Instance.GetCurrencyAmount(credits);

            bool directBuyFive = BusinessesManager.Instance.iBuyable.TryBuyItems(business, 5);

            BigNumber amountAfterDirect = holder.amount;
            BigNumber boughtAfterDirect = holder.BoughtAmount;
            BigNumber currencyAfterDirect = CurrencyManager.Instance.GetCurrencyAmount(credits);

            bool targetBought = targetBefore > 0 && BusinessesManager.Instance.iBuyable.TryBuyItems(business, targetBefore);
            BigNumber targetAfter = BusinessesManager.Instance.GetTargetBusinessBuyAmount(business);

            RefreshDashboard();

            return string.Join("\n",
                $"Buy amount setting: {BusinessesManager.Instance.GetBuyAmount()}",
                $"Target before buying: {targetBefore}",
                $"Can buy x5 before buying: {canBuyFiveBefore}",
                $"Direct x5 result: {directBuyFive}",
                $"Before direct x5: owned={amountBefore}, bought={boughtBefore}, credits={currencyBefore}",
                $"After direct x5: owned={amountAfterDirect}, bought={boughtAfterDirect}, credits={currencyAfterDirect}",
                $"Target buy result: {targetBought}",
                $"After target buy: owned={holder.amount}, bought={holder.BoughtAmount}, credits={CurrencyManager.Instance.GetCurrencyAmount(credits)}",
                $"Target after buying: {targetAfter}",
                $"Shared cap: {DescribeBusinessGroupCapacity(business)}");
        }
    }
}
