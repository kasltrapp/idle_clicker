using TMPro;
using UnityEngine;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Upgrades Summary Displayer")]
    public class UpgradesSummaryDisplayer : BaseDisplayer
    {
        public TMP_Text summaryText;

        public override void Redraw_Default()
        {
            base.Redraw_Default();
            if (UpgradesManager.Instance == null) return;

            UpdateText(summaryText, base.UpgradeBlocksToString(UpgradesManager.Instance.upgrades.MergeUpgradeBlocks()));
        }
    }
}
