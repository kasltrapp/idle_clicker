namespace EasyIdleGame.Demos
{
    public class OffersAndTimeSkipDemoController : DemoSceneController
    {
        protected override void SeedDemoState()
        {
            AddCurrency("Credits", 120);
            GrantBusiness("SkipProducer", 4);
            BuyBoostForFree("TimeSkipBoost", 1);

            OffersManager.Instance.minSecondsBetweenOffers = 4f;
            OffersManager.Instance.maxSecondsBetweenOffers = 15f;
            OffersManager.Instance.possibleOffers.Add(new OfferPoolItem
            {
                item = Load<ShopItem>("FlashOffer"),
                weight = 1,
                durationSeconds = 10f
            });
            OffersManager.Instance.possibleOffers.Add(new OfferPoolItem
            {
                item = Load<ShopItem>("BoostBundle"),
                weight = 2,
                durationSeconds = 12f
            });
            OffersManager.Instance.ScheduleNextOffer();
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Offer Producers", BusinessPanelMode.Offline, "SkipProducer");
            AddOfferPanel();
            AddBoostPanel();
        }
    }
}
