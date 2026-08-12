using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Upgrade Displayer")]
    public class UpgradeDisplayer : BaseDisplayer
    {
        public Upgrade upgrade;

        [Header("UI Elements")]
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text descriptionText;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public ButtonHolder buyButton;

        [Header("Other")]
        public bool showPriceInBuyButton = false;
        public bool showProgressinBuyButton = true;
        public bool showBuyAmount = true;
        public int buyAmount = 1;

        private string GetBuyButtonsString(IBuyableHolder iBuyable)
        {
            string progressionString = showProgressinBuyButton ? $"({UpgradesManager.Instance.iBuyable.GetItemAmount(upgrade)}/{upgrade.MaxBuyableAmount})" : "";
            string priceString = showPriceInBuyButton ? InputsToString(iBuyable.GetRealCostToInputs(buyAmount)) : "";
            string buyAmountString = showBuyAmount ? $"{buyAmount}x" : "";
            string start = $"Buy";

            return Regex.Replace(string.Join(' ', start, buyAmountString, priceString, progressionString), @" {2,}", " ").Trim();
        }

        public override void Redraw_Default()
        {
            base.Redraw_Default();
            if (!upgrade) return;

            UpdateImage(icon, upgrade.Metadata.icon);
            UpdateText(nameText, upgrade.GetDisplayName());
            UpdateText(descriptionText, upgrade.Metadata.description);
            UpdateText(buyButton, $"Buy (0/{upgrade.MaxBuyableAmount})");
        }

        public override void Redraw_Default_Empty()
        {
            base.Redraw_Default_Empty();
            UpdateText(nameText, "UpgradeName");
            UpdateText(descriptionText, "UpgradeDescription");
            UpdateText(buyButton, $"Buy (0/1)");
        }

        public override void Redraw()
        {
            base.Redraw();
            if (!upgrade) return;

            SetButtonInteractable(buyButton, UpgradesManager.Instance.iBuyable.CanBuyItems(upgrade, buyAmount));

            UpgradeHolder upgradeHolder = UpgradesManager.Instance.GetHolder(upgrade);

            if (upgradeHolder == null) return;
            UpdateText(buyButton, GetBuyButtonsString(upgradeHolder.iBuyable));
        }

        public void TryBuyUpgrade()
        {
            UpgradesManager.Instance.iBuyable.TryBuyItems(upgrade, buyAmount);
        }
    }
}
