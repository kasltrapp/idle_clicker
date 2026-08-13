using EasyIdleGame;
using EasyIdleGame.UI;

namespace InfluencerRise.UI
{
    /// <summary>
    /// BuyAmount.ToString() renders the percent-type entry as "100%", which is technically
    /// correct but reads as "MAX" in the AdVenture Communist reference this UI follows. This
    /// override only changes that specific label; the shared selection/purchase-amount logic
    /// itself is entirely native (BusinessesManager.buyAmounts/ChangeBuyAmount/GetBuyAmount).
    /// </summary>
    public sealed class BuyAmountLabelDisplayer : BuyAmountButtonDisplayer
    {
        public override void Redraw()
        {
            base.Redraw();

            BuyAmount current = BusinessesManager.Instance.GetBuyAmount();
            string label = current.type == BuyAmount.Type.percent && current.amount >= 100
                ? "MAX"
                : current.ToString();

            UpdateText(buyAmountButtonText, label);
        }
    }
}
