using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class LocationTravelDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 100);

            Location home = LoadOptional<Location>("HomeLocation");
            if (home != null)
            {
                LocationsManager.Instance.defaultActiveLocation = home;
                LocationsManager.Instance.TryTravelTo(home);
            }
        }

        protected override void BuildSceneDashboard()
        {
            AddPlayerProgressionPanel();
            AddLocationPanel();
        }

        protected override void AddSceneLocationActions(VisualElement card, Location location)
        {
            if (location == null || !ResourceNameEquals(location, "ExpeditionLocation")) return;

            AddCardAction(card, "Add +240 XP", () => AddPlayerXp(240));
        }
    }
}
