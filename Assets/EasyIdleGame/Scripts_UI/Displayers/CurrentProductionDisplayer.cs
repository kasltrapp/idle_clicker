
using UnityEngine;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Current Production Displayer")]
    public class CurrentProductionDisplayer : CurrencyDisplayer
    {
        public void Update()
        {
            Redraw(BusinessesManager.Instance.GetTotalProductionPerSecond(currency), start, end);
        }
    }
}
