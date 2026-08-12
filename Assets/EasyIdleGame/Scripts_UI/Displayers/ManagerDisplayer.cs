using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Manager Displayer")]
    public class ManagerDisplayer : BaseDisplayer, IMetadataDisplayer
    {
        // IMetadataDisplayer
        public Image iconImage => image;
        TMP_Text IMetadataDisplayer.nameText => nameText;
        TMP_Text IMetadataDisplayer.descriptionText => descriptionText;
        public Manager manager;

        [Header("UI")]
        public Image image;
        public TMP_Text nameText, descriptionText;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public Slider slider;
        public TMP_Text levelText;
        public TMP_Text promotionText;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public ButtonHolder buyButton;
        public ButtonHolder upgradeButton;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public TMP_Text costText;
        public TMP_Text lockText;

        [Header("Other")]
        public bool showPriceInBuyButton = false;
        public bool showIconStringInBuyButton = true;
        public bool showBuyAmount = true;
        public int buyAmount = 1;

        private string GetBuyButtonsString(IBuyableHolder iBuyable)
        {
            string businessNameString = showIconStringInBuyButton ? manager.GetIconString() : string.Empty;
            string priceString = showPriceInBuyButton ? InputsToString(iBuyable.GetRealCostToInputs(buyAmount)) : string.Empty;
            string buyAmountString = showBuyAmount ? $"{buyAmount}x" : string.Empty;
            string start = $"Buy";

            return Regex.Replace(string.Join(' ', start, buyAmountString, businessNameString, priceString), @" {2,}", " ").Trim();
        }

        public void Buy()
        {
            ManagersManager.Instance.iBuyable.TryBuyItems(manager, buyAmount);
        }

        public override void Redraw_Default()
        {
            base.Redraw_Default();
            if (!manager) return;

            (this as IMetadataDisplayer).Redraw_Metadata(manager.Metadata);

            // when currencies manager is not asssigned yet or loaded
            try
            {
                UpdateText(costText, "Cost: " + InputsToString(manager.cost));
                UpdateText(lockText, "Locks: " + LocksToString(manager.locks));
            }
            catch { }

            UpdateText(descriptionText, manager.Metadata.description);
        }

        public override void Redraw_Default_Empty()
        {
            base.Redraw_Default_Empty();

            (this as IMetadataDisplayer).Redraw_Metadata("Manager Name", "Manager Description", null);

            UpdateText(costText, "Cost: 1$");
            UpdateText(lockText, "Locks: lvl 1");

            UpdateText(levelText, $"lvl {0}");
            UpdateText(promotionText, $"<color=green>{0}/{1}</color> copies until promotion");

            UpdateText(buyButton, "Buy");
        }

        public override void Redraw()
        {
            base.Redraw();
            if (!manager) return;

            ManagerHolder managerHolder = ManagersManager.Instance.GetHolder(manager);

            if (managerHolder != null)
            {
                SetButtonInteractable(buyButton, managerHolder.iBuyable.CanBuy(buyAmount));

                UpdateText(buyButton, GetBuyButtonsString(managerHolder.iBuyable));

                SetSliderValues(slider, 0, 1, (managerHolder.count / managerHolder.iUpgradable.GetCountToNextLevel()).ToDouble());

                UpdateText(levelText, $"level {managerHolder.level}");

                UpdateText(promotionText, $"<color=green>{managerHolder.count}/{managerHolder.iUpgradable.GetCountToNextLevel()}</color> copies until promotion");

                SetButtonInteractable(upgradeButton, managerHolder.iUpgradable.CanUpgrade());

                UpdateText(costText, "Cost: " + InputsToString(managerHolder.iBuyable.GetRealCostToInputs(buyAmount)));
            }

            if (!ManagersManager.Instance.iUnlockable.IsItemUnlocked(manager))
            {
                SetButtonInteractable(buyButton, false);
            }
        }

        public void TryUpgradeManager()
        {
            ManagersManager.Instance.GetHolder(manager).iUpgradable.TryUpgrade();
        }

        // experimental
        public void ActivateManager()
        {
            ManagersManager.Instance.TryActivateManager(manager);
        }
    }
}
