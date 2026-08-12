using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    public class UpgradableBusinessUpgradeDisplayer : BaseDisplayer, IMetadataDisplayer
    {
        Image IMetadataDisplayer.iconImage => iconImage;
        TMP_Text IMetadataDisplayer.descriptionText => descriptionText;
        TMP_Text IMetadataDisplayer.nameText => nameText;
        public Business business;
        public int level;
        public TMP_Text nameText, descriptionText;
        public ButtonHolder costButton;
        public Image iconImage;

        public override void Redraw()
        {
            base.Redraw();

            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);

            UpdateText(costButton, InputsToString(holder.iUpgradable.GetUpgradeCost(level - 1), true));

            SetButtonInteractable(costButton, holder.iUpgradable.Level < level);
        }

        public override void Redraw_Default()
        {
            base.Redraw_Default();

            UpgradeLevel _level = business.iUpgradable.GetUpgradeLevel(level, true);

            if (_level == null || _level is not BusinessUpgradeLevel upgradeLevel)
            {
                UpdateText(nameText, $"No upgrade lvl found");
                return;
            }

            (this as IMetadataDisplayer).Redraw_Metadata(upgradeLevel.Metadata);
        }

        public override void Redraw_Default_Empty()
        {
            base.Redraw_Default_Empty();
        }

        public void Upgrade()
        {
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);

            holder.iUpgradable.TryUpgradeToLevel(level);
        }
    }
}
