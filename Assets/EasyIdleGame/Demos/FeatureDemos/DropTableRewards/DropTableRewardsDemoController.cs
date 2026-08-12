using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class DropTableRewardsDemoController : DemoSceneController
    {
        protected override string RecipeObjectType => "Drop Table Recipe";

        protected override string RecipeTriggerName => "roll";

        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 10);
        }

        protected override void BuildSceneDashboard()
        {
            AddRecipePanel("Drop Table Assets", "AllDropRecipe", "WeightedDropRecipe");
            AddOutputPanel("Last rolled outputs", LastOutputs);
        }

        protected override void AddRecipeActions(VisualElement card, string recipeName)
        {
            if (recipeName == "WeightedDropRecipe")
            {
                AddCardAction(card, "Roll x5", () => RollDropRecipe("WeightedDropRecipe", 5));
            }
            else if (recipeName == "AllDropRecipe")
            {
                AddCardAction(card, "Roll once", () => ApplyDropRecipe("AllDropRecipe", 1));
            }
        }
    }
}
