using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Currency Displayer")]
    public class CurrencyDisplayer : BaseDisplayer
    {
        public Currency currency;

        [Header("UI")]
        public Image image;
        public TMP_Text amountText;

        [Header("Settings")]
        public bool displayNegativeValues;
        public bool displayMaxCurrencyAmount = false;
        public bool executeRedrawCalls = true;
        public string start = "";
        public string end = "";

        public virtual void Redraw(BigNumber amount, string start = "", string end = "", string maxAmountString = "")
        {
            if (!displayNegativeValues && amount < BigNumber.Zero)
                amount = BigNumber.Zero;

            Redraw_Default();
            UpdateText(amountText, $"{start}{amount}{maxAmountString}{end}");
        }

        public override void Redraw_Default()
        {
            base.Redraw_Default();
            if (currency == null) return;

            UpdateImage(image, currency.Metadata.icon);
        }

        public override void Redraw()
        {
            base.Redraw();
            if (currency == null || !executeRedrawCalls) return;

            string maxAmountString = displayMaxCurrencyAmount ? $"/{CurrencyManager.Instance.GetMaxCurrencyConstrain(currency)}" : "";

            Redraw(CurrencyManager.Instance.GetCurrencyAmount(currency), start, end, maxAmountString);
        }
    }
}
