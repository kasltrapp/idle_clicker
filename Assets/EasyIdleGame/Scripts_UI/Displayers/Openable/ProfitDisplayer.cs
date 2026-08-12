using TMPro;
using UnityEngine;

namespace EasyIdleGame.UI
{
    public class ProfitDisplayer : BaseDisplayer, IOpenable
    {
        public IOpenable iOpenable => this;

        // IOpenable
        public GameObject GameObject => gameObject;
        public AnimationClip OpenAnimation => openAnimation;
        public MonoBehaviour MonoBehaviour => this;
        public AnimationClip CloseAnimation => closeAnimation;

        public CurrencyDisplayer[] currencyDisplayer;

        [Header("UI")]
        public TMP_Text offlineTimeText;
        public TMP_Text levelAchievedText;

        protected ProfitData data;
        protected int level;
        protected bool timeSkip;

        [Header("Animations")]
        public AnimationClip openAnimation;
        public AnimationClip closeAnimation;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public ButtonHolder adButton;
        public ButtonHolder claimButton;
        public GameObject watchAdText;

        public override void Redraw_Default_Empty()
        {
            base.Redraw_Default_Empty();

            UpdateText(offlineTimeText, "YOU WERE OFFLINE FOR: 0s");
            UpdateText(levelAchievedText, "NEW LEVELS ACHIEVED: 0");
        }

        public void OpenAndRedraw(ProfitData data_, int level_, bool timeSkip_)
        {
            data = data_;
            level = level_;
            timeSkip = timeSkip_;

            iOpenable.Open();
            Redraw();
        }

        public override void Redraw()
        {
            base.Redraw();

            if (data == null) return;

            if (!timeSkip) UpdateText(levelAchievedText, $"NEW LEVELS ACHIEVED: <color=green>{level}");
            else UpdateText(levelAchievedText, $"");

            if (timeSkip) UpdateText(offlineTimeText, $"YOU SKIPPED: <color=green>{data.ElapsedTime()}");
            else UpdateText(offlineTimeText, $"YOU WERE OFFLINE FOR: <color=green>{data.ElapsedTime()}");

            RedrawCurrencyDisplayers();

            EnableComponent(adButton, !timeSkip);
            EnableComponent(watchAdText, !timeSkip);
        }

        protected void RedrawCurrencyDisplayers()
        {
            foreach (CurrencyDisplayer currencyDisplayer in currencyDisplayer)
            {
                if (!currencyDisplayer) continue;

                if (!data.businessProfits.TryGetValue(currencyDisplayer.currency, out BigNumber amount)) amount = 0;

                currencyDisplayer.autoRedraw = false;
                currencyDisplayer.Redraw(amount);
            }
        }

        public void ClaimAndContinue() => iOpenable.Close();

        public void WatchAd()
        {
            // This is where you would implement the logic to watch an ad
            ProfitCalculator.ApplyProfit(data.MultiplyProfits(.3f));
            iOpenable.Close();
        }
    }
}
