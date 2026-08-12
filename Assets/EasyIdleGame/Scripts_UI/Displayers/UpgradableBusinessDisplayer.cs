using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Upgradable Business Displayer")]
    public class UpgradableBusinessDisplayer : BaseDisplayer
    {
        public Business business;

        [Header("UI Elements")]
        public Image image;
        public Slider levelSlider;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public TMP_Text amountText;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public Slider productionSlider;
        public GameObject fastFill;
        public float fastFillTreshold = 0.25f; // in seconds
        public TMP_Text productionPerSecondText;
        public TMP_Text timeLeftText;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public ButtonHolder buyButton;
        public ButtonHolder infoButton;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public Button mainButton;
        public TMP_Text mainButtonText;

        protected BigNumber GetBuyAmount() => BusinessesManager.Instance.GetTargetBusinessBuyAmount(business);

        protected BigNumber buyAmount;
        protected List<Output> outputs;

        [Header("Other")]
        public bool showPriceInBuyButton = false;
        public bool showIconStringInBuyButton = true;
        public bool showBuyAmount = true;

        private string GetBuyButtonsString(IBuyableHolder iBuyable, Metadata metadata)
        {
            string businessNameString = showIconStringInBuyButton ? metadata?.GetIconString() : string.Empty;
            string priceString = showPriceInBuyButton ? $"({InputsToString(iBuyable.GetRealCostToInputs(buyAmount))})" : string.Empty;
            string buyAmountString = showBuyAmount ? $"{buyAmount}x" : string.Empty;

            string start = $"Buy";

            return Regex.Replace(string.Join(' ', start, buyAmountString, businessNameString, priceString), @" {2,}", " ").Trim();
        }

        public override void Redraw()
        {
            base.Redraw();
            if (business == null) return;

            bool isUnlocked = BusinessesManager.Instance.iUnlockable.IsItemUnlocked(business);

            buyAmount = GetBuyAmount();

            BusinessHolder businessHolder = BusinessesManager.Instance.GetHolder(business);
            Metadata activeMetadata = businessHolder == null ? business.GetEffectiveMetadata(null) : business.GetEffectiveMetadata(businessHolder);
            UpdateImage(image, activeMetadata?.icon);

            if (businessHolder != null)
            {
                SetButtonInteractable(buyButton, isUnlocked && buyAmount != 0 && businessHolder.iBuyable.CanBuy(buyAmount));

                UpdateText(buyButton, GetBuyButtonsString(businessHolder.iBuyable, activeMetadata));

                if (businessHolder.iProducable.GetActualTimeToProduce() < fastFillTreshold && businessHolder.activeProductions.Count > 0)
                {
                    EnableComponent(fastFill, true);
                }
                else
                {
                    EnableComponent(fastFill, false);
                    SetSliderValues(productionSlider, 0, 1, businessHolder.GetProductionProgress());
                }
                UpdateText(timeLeftText, UIHelper.FormatTime(businessHolder.GetActualTimeLeft()));

                SetSliderValues(levelSlider, 0, 1, businessHolder.level.Progress);

                string amountT = businessHolder.GetMaxBuyableAmountConstrain() != BigNumber.MaxValue ? $"{businessHolder.amount.ToString(false)}/{businessHolder.GetMaxBuyableAmountConstrain()}" : businessHolder.amount.ToString(false);
                UpdateText(amountText, amountT);

                outputs = businessHolder.iProducable.GetActualProductionPerSecond();
                if (outputs.Count != 0)
                {
                    outputs.Sort((x, y) => x.outputAmount.CompareTo(y.outputAmount));
                    UpdateText(productionPerSecondText, outputs[0].outputAmount.ToString(true) + " per second");
                }
                else UpdateText(productionPerSecondText, "This business has no outputs");
            }

            SetButtonInteractable(mainButton, isUnlocked);
        }

        public override void Redraw_Default()
        {
            base.Redraw_Default();
            if (business == null) return;

            UpdateText(buyButton, $"Buy 0x {business.GetDisplayName()}");
            UpdateImage(image, business.GetEffectiveMetadata(null).icon);

            UpdateText(productionPerSecondText, business.outputs.Count != 0 ? "0 per second" : "This business has no outputs");
            UpdateText(amountText, "0");
        }

        public override void Redraw_Default_Empty()
        {
            base.Redraw_Default_Empty();

            UpdateText(buyButton, $"Buy 0x Business");

            UpdateText(productionPerSecondText, "outputs per second");
            UpdateText(amountText, "0");
        }

        public void OpenInfo()
        {
            SimpleDemoManager demoManager = FindObjectOfType<SimpleDemoManager>();
            if (!demoManager) return;

            demoManager.OpenBusinessDetail(business);
        }

        public void BuyBusiness()
        {
            BusinessesManager.Instance.iBuyable.TryBuyItems(business, GetBuyAmount());
        }

        public void Click()
        {
            BusinessesManager.Instance.GetHolder(business).TryStartProduction();
        }

        private Metadata GetActiveMetadata(BusinessHolder holder)
        {
            if (holder == null || holder.business == null) return business.GetEffectiveMetadata(null);
            return holder.business.GetEffectiveMetadata(holder);
        }
    }
}