using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Boost Displayer")]
    public class BoostDisplayer : BaseDisplayer
    {
        public Boost boost;

        [Header("UI")]
        public TMP_Text boostNameText;
        public TMP_Text descriptionText;
        public Image icon;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public TMP_Text amountText;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public TMP_Text costText;
        public TMP_Text lockText;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public ButtonHolder buyButton;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public ButtonHolder activateButton;

        [Header("Other")]
        public bool showPriceInBuyButton = false;
        public bool showIconStringInBuyButton = true;
        public bool showBuyAmount = true;
        public int buyAmount = 1;

        private string GetBuyButtonsString(IBuyableHolder iBuyable)
        {
            string businessNameString = showIconStringInBuyButton ? boost.GetIconString() : string.Empty;
            string priceString = showPriceInBuyButton ? InputsToString(iBuyable.GetRealCostToInputs(buyAmount)) : string.Empty;
            string buyAmountString = showBuyAmount ? $"{buyAmount}x" : string.Empty;
            string start = $"Buy";

            return Regex.Replace(string.Join(' ', start, buyAmountString, businessNameString, priceString), @" {2,}", " ").Trim();
        }

        public override void Redraw_Default()
        {
            base.Redraw_Default();
            if (boost == null) return;

            UpdateText(boostNameText, boost.GetDisplayName());
            UpdateImage(icon, boost.Metadata.icon);
            UpdateText(descriptionText, boost.Metadata.description);

            UpdateText(costText, "Cost: " + base.InputsToString(boost.cost));
            UpdateText(lockText, "Locks: " + base.LocksToString(boost.locks));
        }

        public override void Redraw_Default_Empty()
        {
            base.Redraw_Default_Empty();

            UpdateText(boostNameText, "BoostName");

            UpdateText(costText, "Cost: 1$");
            UpdateText(lockText, "Locks: lvl 1");
        }

        public override void Redraw()
        {
            base.Redraw();
            if (boost == null) return;

            UpdateText(amountText, $"Inv: {BoostsManager.Instance.GetBoostAmountInInventory(boost)}");
            SetButtonInteractable(activateButton, BoostsManager.Instance.GetBoostAmountInInventory(boost) > 0);

            BoostHolder holder = BoostsManager.Instance.GetHolder(boost);
            if (holder == null) return;

            SetButtonInteractable(buyButton, holder.iBuyable.CanBuy(buyAmount));
            UpdateText(buyButton, GetBuyButtonsString(holder.iBuyable));
        }

        public void Buy()
        {
            BoostsManager.Instance.iBuyable.TryBuyItems(boost, buyAmount);
        }

        public void Activate()
        {
            BoostsManager.Instance.TryUseBoost(boost);
        }
    }
}
