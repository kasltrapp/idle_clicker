using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class MergingRecipesDemoController : DemoSceneController
    {
        protected override bool ShowMergeRecipeReadiness => true;

        protected override bool ShouldShowInventoryBuyState(Business business)
        {
            return false;
        }

        protected override BigNumber GetDefaultCurrencyGrantAmount(Currency currency)
        {
            return ResourceNameEquals(currency, "Credits") ? 50 : base.GetDefaultCurrencyGrantAmount(currency);
        }

        protected override string GetDefaultCurrencyGrantLabel(Currency currency, BigNumber amount)
        {
            return ResourceNameEquals(currency, "Credits") ? $"Add +{amount}" : base.GetDefaultCurrencyGrantLabel(currency, amount);
        }

        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 90);
            GrantBusiness("ScrapCart", 3);
            BusinessesManager.RefreshMergeRecipeCache();
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Recipe Inventory", BusinessPanelMode.Inventory, "ScrapCart", "WorkshopBot");
            AddRecipePanel("Merge Recipes", "BusinessPairRecipe", "CurrencyRecipe");
            AddOutputPanel("Last merge outputs", LastOutputs);
        }

        protected override void AddSceneBusinessActions(VisualElement card, string businessName, BusinessPanelMode mode)
        {
            if (businessName == "ScrapCart")
            {
                AddCardAction(card, "Grant +3", () => GrantBusiness("ScrapCart", 3));
            }
        }

        protected override void AddRecipeActions(VisualElement card, string recipeName)
        {
            if (recipeName == "BusinessPairRecipe")
            {
                AddCardAction(card, "Merge", MergePairRecipe, () => CanMergeRecipe(LoadOptional<BusinessMergeRecipe>("BusinessPairRecipe")), false, () => DescribeMergeAvailability(LoadOptional<BusinessMergeRecipe>("BusinessPairRecipe")), false);
            }
            else if (recipeName == "CurrencyRecipe")
            {
                AddCardAction(card, "Merge", MergeCurrencyRecipe, () => CanMergeRecipe(LoadOptional<BusinessMergeRecipe>("CurrencyRecipe")), false, () => DescribeMergeAvailability(LoadOptional<BusinessMergeRecipe>("CurrencyRecipe")), false);
            }
        }
    }
}
