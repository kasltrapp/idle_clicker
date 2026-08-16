using UnityEngine;
using UnityEngine.UI;
using UIImage = UnityEngine.UI.Image;

namespace InfluencerRise.UI
{
    /// <summary>
    /// Wires the 3 bottom platform tabs to show exactly one business-list panel at a time.
    /// Does not gate switching on platform-unlock state (Level 12/34, EconomySchema.md Locations
    /// table) - an explicit scope decision for this task, not an assumption: tapping a still-locked
    /// platform's tab shows its panel with rows rendering locked/non-interactable via their own
    /// real unlock conditions (already verified previously), rather than blocking the tap or adding
    /// a separate "locked" overlay.
    ///
    /// Selected-tab color swap mirrors the mechanism StarterFeedTab already had hand-set in the
    /// Editor (a distinct blue Image.color vs. the other two tabs' shared neutral dark color) -
    /// found via direct component read, not invented here.
    /// </summary>
    [AddComponentMenu("InfluencerRise/UI/Platform Tab Controller")]
    public class PlatformTabController : MonoBehaviour
    {
        public GameObject yourgramPanel;
        public GameObject quickTokPanel;
        public GameObject broadcastPanel;

        public UIImage starterFeedTabBackground;
        public UIImage clipzTabBackground;
        public UIImage chronicleTabBackground;

        public Color selectedColor = new Color(0.25f, 0.45f, 0.85f, 1f);
        public Color unselectedColor = new Color(0.2f, 0.2f, 0.26f, 1f);

        public void ShowYourgram() => Show(yourgramPanel);
        public void ShowQuickTok() => Show(quickTokPanel);
        public void ShowBroadcast() => Show(broadcastPanel);

        private void Show(GameObject panelToShow)
        {
            SetTab(yourgramPanel, starterFeedTabBackground, panelToShow);
            SetTab(quickTokPanel, clipzTabBackground, panelToShow);
            SetTab(broadcastPanel, chronicleTabBackground, panelToShow);
        }

        private void SetTab(GameObject panel, UIImage tabBackground, GameObject panelToShow)
        {
            bool selected = panel == panelToShow;
            if (panel != null) panel.SetActive(selected);
            if (tabBackground != null) tabBackground.color = selected ? selectedColor : unselectedColor;
        }
    }
}
