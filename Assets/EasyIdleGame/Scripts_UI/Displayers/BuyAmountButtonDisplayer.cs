using TMPro;
using UnityEngine;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Buy Amount Button Displayer")]
    public class BuyAmountButtonDisplayer : BaseDisplayer
    {
        [Header("UI Elements")]
        public TMP_Text buyAmountButtonText;

        public override void Redraw()
        {
            base.Redraw();
            UpdateText(buyAmountButtonText, BusinessesManager.Instance.GetBuyAmount().ToString());
        }

        public void OnClick()
        {
            BusinessesManager.Instance.ChangeBuyAmount();
        }
    }
}