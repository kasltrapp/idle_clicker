using System;
using System.Collections.Generic;

namespace EasyIdleGame.UI
{
    [Obsolete("Use Unity Building Layout Handling instead")]
    public class CurrencyListDisplayer : BaseListDisplayer<CurrencyDisplayer>
    {
        public Currency[] currencies;

        public bool spawnOnStart = true;

        private void Start()
        {
            if (spawnOnStart)
                SpawnCurrencies();
        }

        public void SpawnCurrencies() => SpawnCurrencies((_) => true);

        public void SpawnCurrencies(Func<Currency, bool> req)
        {
            int id = 0;

            displayers = new List<CurrencyDisplayer>();
            for (int i = 0; i < currencies.Length; i++)
            {
                if (!req(currencies[i])) continue;

                CurrencyDisplayer displayer = SpawnPrefab(id++, false).GetComponent<CurrencyDisplayer>();

                displayer.currency = currencies[i];
                displayers.Add(displayer);
            }
        }

        public void DestroyCurrencies() => ClearDisplayers();
    }
}
