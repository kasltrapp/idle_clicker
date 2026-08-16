using EasyIdleGame;
using EasyIdleGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfluencerRise.UI
{
    /// <summary>
    /// Fills the display gap BusinessDisplayer.Redraw() leaves when a business's production
    /// round completes faster than fastFillTreshold: the vendored code enables the static
    /// "READY!" fastFill label and skips SetSliderValues entirely (BusinessDisplayer.cs
    /// ~lines 83-91), which was fine for an occasional near-instant round but becomes a
    /// permanently frozen display once a business scales up enough that every round clears
    /// the threshold (confirmed empirically at First Post x158: GetActualTimeToProduce()
    /// ~0.023s, 11x under the 0.25s default threshold, static not transient - see
    /// docs/BugTracker.md).
    ///
    /// Does not touch vendored BusinessDisplayer.cs. Hides timeLeftText and the fastFill
    /// GameObject's own "READY!" text via TMP_Text.enabled (UIUpdater.UpdateText only ever
    /// sets .text, never .enabled - confirmed via source read - so this can't be fought by
    /// the vendored Redraw() call that keeps running every frame) and drives a continuous,
    /// production-timing-independent pulse on the Ticker's existing Fill/background Images
    /// as the "generating continuously, too fast to show discrete rounds" indicator in that
    /// same state.
    /// </summary>
    [AddComponentMenu("InfluencerRise/UI/Fast Production Indicator")]
    public class FastProductionIndicator : MonoBehaviour
    {
        public BusinessDisplayer displayer;
        public TMP_Text timeLeftText;
        public TMP_Text fastFillText;
        public Image tickerBackgroundImage;
        public Image fillImage;
        public Slider productionSlider;

        public float pulseSpeed = 2.5f;
        public Color tickerColorPulse = new Color(0.14f, 0.22f, 0.32f, 1f);
        public Color fillColorPulse = new Color(0.35f, 0.75f, 0.95f, 1f);

        /// <summary>
        /// Captured once in Awake(), before this component's Update() ever runs, from whatever
        /// Image.color the project owner set in the Editor (e.g. the current skin sprite's tint).
        /// Using a live-captured baseline instead of a hardcoded constant means this script can't
        /// silently fight future art/skin changes on these two Images - confirmed necessary after
        /// hardcoded near-black constants (matching the old pre-skin placeholder look) were found
        /// unconditionally overwriting the new skin sprites' colors every non-fast frame.
        /// </summary>
        private Color _tickerColorNormal;
        private Color _fillColorNormal;

        private void Awake()
        {
            if (tickerBackgroundImage != null) _tickerColorNormal = tickerBackgroundImage.color;
            if (fillImage != null) _fillColorNormal = fillImage.color;
        }

        private void Update()
        {
            if (displayer == null || displayer.business == null) return;
            if (BusinessesManager.Instance == null) return;

            BusinessHolder holder = BusinessesManager.Instance.GetHolder(displayer.business);
            if (holder == null) return;

            bool isFast = holder.iProducable.GetActualTimeToProduce() < displayer.fastFillTreshold && holder.activeProductions.Count > 0;

            if (isFast)
            {
                if (timeLeftText != null) timeLeftText.enabled = false;
                if (fastFillText != null) fastFillText.enabled = false;
                if (productionSlider != null) productionSlider.value = 1f;

                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                if (fillImage != null) fillImage.color = Color.Lerp(_fillColorNormal, fillColorPulse, pulse);
                if (tickerBackgroundImage != null) tickerBackgroundImage.color = Color.Lerp(_tickerColorNormal, tickerColorPulse, pulse);
            }
            else
            {
                if (timeLeftText != null) timeLeftText.enabled = true;
                if (fastFillText != null) fastFillText.enabled = true;
                if (fillImage != null) fillImage.color = _fillColorNormal;
                if (tickerBackgroundImage != null) tickerBackgroundImage.color = _tickerColorNormal;
            }
        }
    }
}
