using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Player Level Displayer")]
    public class PlayerLevelDisplayer : BaseDisplayer
    {
        [Header("UI Elements")]
        public Slider levelSlider;
        public TMP_Text levelText;

        public override void Redraw_Default()
        {
            base.Redraw_Default();

            UpdateText(levelText, $"{0}");
        }

        public override void Redraw()
        {
            base.Redraw();

            SetSliderValues(levelSlider, 0, 1, (float)(PlayerStats.Instance.level.currentXp / PlayerStats.Instance.level.TargetXp).ToDouble());
            UpdateText(levelText, $"{PlayerStats.Instance.level.currentLevel}");
        }
    }
}
