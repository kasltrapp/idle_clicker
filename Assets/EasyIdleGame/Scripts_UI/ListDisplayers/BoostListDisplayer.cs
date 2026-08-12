using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame.UI
{
    [Obsolete("Use Unity Building Layout Handling instead")]
    public class BoostListDisplayer : BaseListDisplayer<BoostDisplayer>
    {
        public Boost[] boosts;

        public bool spawnOnStart = true;

        private void Start()
        {
            if (spawnOnStart)
                SpawnBoosts();
        }

        private void SpawnBoosts()
        {
            displayers = new List<BoostDisplayer>();

            for (int i = 0; i < boosts.Length; i++)
            {
                GameObject clone = SpawnPrefab(i, false);
                clone.GetComponent<BoostDisplayer>().boost = boosts[i];

                displayers[i] = clone.GetComponent<BoostDisplayer>();
            }
        }
    }
}