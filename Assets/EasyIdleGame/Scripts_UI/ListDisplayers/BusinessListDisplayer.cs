using System;
using System.Collections.Generic;

namespace EasyIdleGame.UI
{
    [Obsolete("Use Unity Building Layout Handling instead")]
    public class BusinessListDisplayer : BaseListDisplayer<BusinessDisplayer>
    {
        public Business[] businesses;

        public bool spawnOnStart = true;

        private void Start()
        {
            if (spawnOnStart)
                SpawnBusinesses();
        }

        private void SpawnBusinesses()
        {
            displayers = new List<BusinessDisplayer>();
            for (int i = 0; i < businesses.Length; i++)
            {
                BusinessDisplayer displayer = SpawnPrefab(i, false).GetComponent<BusinessDisplayer>();

                displayer.business = businesses[i];
                displayers.Add(displayer);
            }
        }
    }
}
