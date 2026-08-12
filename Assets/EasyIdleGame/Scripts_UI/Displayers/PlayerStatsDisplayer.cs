using System.Linq;
using UnityEngine;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Player Stats Displayer")]
    public class PlayerStatsDisplayer : BaseDisplayer
    {
#pragma warning disable CS0618
        public CurrencyListDisplayer totalCurrenciesDisplayer;
#pragma warning restore CS0618

        public override void Redraw()
        {
            base.Redraw();

            if (PlayerStats.Instance.totalMoneyMade.Count != totalCurrenciesDisplayer.displayers.Count) RespawnCurrencies();

            totalCurrenciesDisplayer.displayers.ToList().ForEach(x =>
            {
                if (x == null) return;

                x.autoRedraw = false;
                x.Redraw(PlayerStats.Instance.totalMoneyMade.First(y => y.currency == x.currency).amount);
            });
        }

        protected void RespawnCurrencies()
        {
            float topY = ((RectTransform)totalCurrenciesDisplayer.transform).offsetMax.y;
            Rect parentRect = RectTransformUtility.PixelAdjustRect(totalCurrenciesDisplayer.transform.parent as RectTransform, GetComponentInParent<Canvas>());

            ((RectTransform)totalCurrenciesDisplayer.transform).offsetMin = new Vector2(0, topY - totalCurrenciesDisplayer.GetAutoHeight(PlayerStats.Instance.totalMoneyMade.Where((x) => x.amount > 0).Count()) + parentRect.height);

            totalCurrenciesDisplayer.DestroyCurrencies();

            totalCurrenciesDisplayer.currencies = PlayerStats.Instance.totalMoneyMade.Select(x => x.currency).ToArray();
            totalCurrenciesDisplayer.SpawnCurrencies((c) => PlayerStats.Instance.GetTotalMoneyMade(c) > 0);
        }
    }
}