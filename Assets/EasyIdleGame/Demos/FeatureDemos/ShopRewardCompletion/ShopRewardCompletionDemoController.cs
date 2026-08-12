using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    public class ShopRewardCompletionDemoController : DemoSceneController
    {
        private ShopItem pendingRewardAdItem;

        protected override void SeedDemoState()
        {
            pendingRewardAdItem = null;
            AddCurrency("Credits", 25);
        }

        protected override void BuildSceneDashboard()
        {
            AddShopPanel();
            AddBoostPanel();
            AddOutputPanel("Last reward outputs", LastOutputs);
        }

        protected override void AddShopItemActions(VisualElement card, ShopItem item)
        {
            ShopItem itemAsset = item;
            if (itemAsset.buyWithAd)
            {
                AddLiveKeyValue(card, "Ad mock state", () => pendingRewardAdItem == itemAsset ? "confirm ad watched to grant reward" : "idle", () => pendingRewardAdItem == itemAsset ? WarningColor : MutedTextColor);
                AddCardAction(card, "Watch ad", () => RequestRewardAd(itemAsset), () => CanAttemptRewardAd(itemAsset) && pendingRewardAdItem == null, false, () => DescribeRewardAdRequestAvailability(itemAsset), false);
                AddCardAction(card, "Confirm ad", () => ConfirmRewardAdWatched(itemAsset), () => CanConfirmRewardAdWatched(itemAsset), false, () => DescribeRewardAdConfirmationAvailability(itemAsset), false);
            }
            else
            {
                AddCardAction(card, "Grant +1", () => CompleteRewardPurchase(itemAsset), () => CanCompleteShopPurchase(itemAsset), false, () => DescribeShopCompleteAvailability(itemAsset), false);
            }

            AddCardAction(card, "Consume", () => ConsumeRewardCrate(itemAsset), () => CanConsumeShopItem(itemAsset), false, () => DescribeShopConsumeAvailability(itemAsset));
        }

        private void RequestRewardAd(ShopItem item)
        {
            ShopManager.Instance.AttemptPurchaseWithAd(item);
            pendingRewardAdItem = item;
            LastOutputs.Clear();
            LastShopRewardResult = $"Mock ad requested for {item.metadata.name}. Confirm ad watched to grant the reward. In a game, subscribe to ShopManager.OnAttemptAdPurchase, show your rewarded ad, then call CompleteExternalPurchase from the ad SDK reward callback.";
            Log(LastShopRewardResult);
        }

        private void ConfirmRewardAdWatched(ShopItem item)
        {
            if (!CanConfirmRewardAdWatched(item))
            {
                LastShopRewardResult = DescribeRewardAdConfirmationAvailability(item);
                Log(LastShopRewardResult);
                return;
            }

            LastOutputs = ShopManager.Instance.CompleteExternalPurchase(item, 1, SourceType.Advertisement).Squash();
            pendingRewardAdItem = null;
            LastShopRewardResult = $"Confirmed mock ad watched; granted reward outputs={DescribeOutputs(LastOutputs)}.";
            Log(LastShopRewardResult);
        }

        private bool CanConfirmRewardAdWatched(ShopItem item)
        {
            return item != null && pendingRewardAdItem == item && CanCompleteShopPurchase(item);
        }

        private string DescribeRewardAdRequestAvailability(ShopItem item)
        {
            if (pendingRewardAdItem == item) return "ad watched is pending; press Confirm ad to grant the reward";
            if (pendingRewardAdItem != null) return $"finish the pending ad for {pendingRewardAdItem.metadata.name} first";
            return DescribeShopAdAvailability(item);
        }

        private string DescribeRewardAdConfirmationAvailability(ShopItem item)
        {
            if (item == null) return "shop item asset is missing";
            if (pendingRewardAdItem != item) return "press Watch ad first; the mock ad has not been confirmed yet";
            return CanCompleteShopPurchase(item)
                ? "ready, confirms the mock ad watched and grants the reward"
                : DescribeShopCompleteAvailability(item);
        }
    }
}
