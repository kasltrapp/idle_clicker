using System;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    [Obsolete("Use TestUtilities instead for non-inheriting test setups")]
    public class TestsBase
    {
        protected void SetupManagerObject()
        {
            if (GameObject.Find("EmptyObject") != null)
            {
                ClearManagerObjects();
                return;
            }

            GameObject emptyObj = new GameObject("EmptyObject");

            emptyObj.AddComponent<BusinessesManager>();
            emptyObj.AddComponent<CurrencyManager>();
            emptyObj.AddComponent<BoostsManager>();
            emptyObj.AddComponent<ManagersManager>();
            emptyObj.AddComponent<UpgradesManager>();

            emptyObj.AddComponent<SaveAndLoad>();
            SaveAndLoad.Instance.savePath = "testSaveFile+1212718218279817281";

            PlayerStats stats = emptyObj.AddComponent<PlayerStats>();
            stats.firstLevelXp = 100;
            stats.levelIncrementMultiplier = 2;
            stats.Start();
        }

        protected void ClearManagerObjects()
        {
            BusinessesManager.Instance.holders.Clear();
            CurrencyManager.Instance.holders.Clear();

            BoostsManager.Instance.holders.Clear();
            BoostsManager.Instance.activeBoosts.Clear();

            ManagersManager.Instance.holders.Clear();

            UpgradesManager.Instance.holders.Clear();

            PlayerStats.Instance.ResetLevel();

            SaveAndLoad.Instance.IrreversiblyDeleteSave();
        }
    }
}