using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Active Boost Displayer")]
    public class ActiveBoostDisplayer : BaseDisplayer
    {
        public UpgradeType upgradeType;
        public string displayedBusinessGroup;
        public Image icon;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public TMP_Text text;
        public TMP_Text timeLeftText;

        public override void Redraw()
        {
            base.Redraw();

            ActiveBoostHolder activeBoost = GetActiveBoost(upgradeType);

            ActiveBoostHolder GetActiveBoost(UpgradeType type)
            {
                return BoostsManager.Instance.activeBoosts.Find(b =>
                    b.boost is GenericMultiplierBoost gBoost &&
                    gBoost.iMultiplierProvider.HasUpgradeOfType(type) &&
                    Business.HasGroupMatch(null, displayedBusinessGroup, gBoost.TargetBusinessGroups, gBoost.Group)
                );
            }

            UpdateText(text, $"{BoostsManager.GetMultiplierOfTypeStatic(upgradeType, displayedBusinessGroup)}x");

            if (activeBoost == null)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                return;
            }

            UpdateImage(icon, activeBoost.boost.Metadata.icon);

            float time = BoostsManager.Instance.GetMinDurationOfBoostOfType(upgradeType, displayedBusinessGroup);

            UpdateText(timeLeftText, $"{time:F2}");

            transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
