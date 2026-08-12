using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame.UI
{
    public class SimpleDemoManager : ManagerBase<SimpleDemoManager>
    {
        [CommentArea("Simple Demo Manager", "Demo-scene helper that grants starting outputs, loads saves, shows offline profit, and optionally opens daily rewards.", "Use this in demo scenes or as a starter reference. Assign startProps for initial resources, profitDisplayer for offline profit, and dailyRewardListDisplayer if the scene uses daily rewards.")]
        [SerializeField] private string _simpleDemoManagerComment;

        [Space(16)]
        [Tooltip("Outputs granted when the demo scene starts. Leave empty if the demo should load only from save data or scene-authored state.")]
        public List<Output> startProps = new List<Output>();

        [Tooltip("Popup used to show offline or time-skip profit. Leave null if this demo should apply profit without showing a popup.")]
        public ProfitDisplayer profitDisplayer;

        [Tooltip("Daily reward popup opened when a reward is available. Leave null for demos that do not use daily rewards.")]
        public DailyRewardListDisplayer dailyRewardListDisplayer;

        // -1 for infinite
        [Tooltip("Maximum offline profit duration in hours for this demo flow. -1 means infinite; override upgrades can replace this cap at runtime.")]
        public BigNumber defaultIdleCap = -1;

        [Header("Debug")]
        [Tooltip("Editor-only minutes added to loaded save time to simulate offline progress. Runtime builds force this to 0.")]
        public float debugMinutesTimeAddition;

        public void ApplyStartProps()
        {
            startProps.ApplyOuputs(1);
        }

        protected void Start()
        {
            ApplyStartProps();

            if (profitDisplayer)
                profitDisplayer.iOpenable.Close();

            if (dailyRewardListDisplayer)
                dailyRewardListDisplayer.iOpenable.Close();

            TryLoadSave();

            if (dailyRewardListDisplayer)
                dailyRewardListDisplayer.TryOpenDisplayer();
        }

        public void TryLoadSave()
        {
            FindObjectOfType<SaveAndLoad>(true).Load(out SaveFile file);

            if (file != null)
            {
#if !UNITY_EDITOR
                debugMinutesTimeAddition = 0;
#endif

                TimeSpan totalTime = DateTime.Now - file.saveTime + TimeSpan.FromMinutes(debugMinutesTimeAddition);

                BigNumber timeCap = defaultIdleCap;

                if (UpgradesManager.Instance)
                {
                    timeCap = UpgradesManager.Instance.GetOverrideUpgradeOfType(OverrideUpgradeType.idleProductionTimeCap) ?? defaultIdleCap;
                }

                if (timeCap >= 0)
                {
                    TimeSpan capSpan = TimeSpan.FromHours(timeCap.ToDouble());
                    if (totalTime > capSpan)
                        totalTime = capSpan;
                }

                ProfitData data = ProfitCalculator.CalculateProfit(totalTime);

                int level = 0;

                if (PlayerStats.Instance)
                    level = PlayerStats.Instance.GetPlayerLevel();

                ProfitCalculator.ApplyProfit(data);

                DisplayProfit(data, false, level);

                FindObjectOfType<SaveAndLoad>(true).Save();
            }
        }

        public virtual void DisplayProfit(ProfitData data, bool timeSkip, int level)
        {
            if (!profitDisplayer) return;

            profitDisplayer.OpenAndRedraw(data, PlayerStats.Instance.GetPlayerLevel() - level, timeSkip);
        }

        public virtual void OpenBusinessDetail(Business business)
        {
            // implement this in the child class

            Debug.LogWarning("OpenBusinessDetail not implicitly implemented in SimpleDemoManager - use DemoManager instead, or override this function");
        }
    }
}
