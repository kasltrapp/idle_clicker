using TMPro;
using UnityEngine;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Prestige Displayer")]
    public class PrestigeDisplayer : BaseDisplayer
    {
        [Header("UI Elements")]
        public ButtonHolder prestigeButton;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public TMP_Text prestigeText;
        public TMP_Text prestigeLockText;
        public TMP_Text prestigeMultiplierDescriptionText;

        public override void Redraw_Default()
        {
            base.Redraw_Default();
            if (PrestigeManager.Instance == null) return;

            UpdateText(prestigeLockText, "Requirements: " + base.LocksToString(PrestigeManager.Instance.currentPrestigeRequirements));
            UpdateText(prestigeMultiplierDescriptionText, base.UpgradeBlocksToString(PrestigeManager.Instance.GetPrestigeMultipliers()));
        }

        public override void Redraw_Default_Empty()
        {
            base.Redraw_Default_Empty();

            UpdateText(prestigeLockText, "Requirements: lvl 1");
            UpdateText(prestigeText, "Prestige count: 0");
        }

        public override void Redraw()
        {
            base.Redraw();

            SetButtonInteractable(prestigeButton, PrestigeManager.Instance.CanPrestige());
            UpdateText(prestigeText, "Prestige count: " + PrestigeManager.Instance.prestiges.prestigeCount);
        }

        public void Prestige()
        {
            PrestigeManager.Instance.TryPrestigeGame(out _);
        }
    }
}