using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    /// <summary>
    /// Project-owned state that is not part of the base economy SaveFile.
    /// Static so it survives ResetDemoState, like real project state would survive a scene reload.
    /// </summary>
    public static class CustomSaveDemoState
    {
        public static string Note = "fresh session";
        public static int VisualBoardVersion;
    }

    /// <summary>
    /// Custom save file: the constructor captures state, Load() restores it after base.Load().
    /// </summary>
    [Serializable]
    public class DemoProjectSaveFile : SaveFile
    {
        public string demoNote;
        public int visualBoardVersion;

        public DemoProjectSaveFile()
        {
            demoNote = CustomSaveDemoState.Note;
            visualBoardVersion = CustomSaveDemoState.VisualBoardVersion;
        }

        public override void Load()
        {
            base.Load();
            CustomSaveDemoState.Note = demoNote;
            CustomSaveDemoState.VisualBoardVersion = visualBoardVersion;
        }
    }

    /// <summary>
    /// Custom SaveAndLoad that coalesces autosaves: it writes only after a save-worthy change,
    /// at most once per cooldown, and never while a load is in progress.
    /// </summary>
    public class DemoCustomSaveAndLoad : SaveAndLoad
    {
        public bool AutosaveEnabled { get; set; } = true;
        public bool SuppressAutosave { get; set; }
        public bool Dirty { get; private set; }
        public int CoalescedSaveCount { get; private set; }
        public string LastLifecycleResult { get; private set; } = "no save/load yet";

        private float writeCooldown;

        public void MarkDirty() => Dirty = true;

        // Hides the base per-interval autosave; this component writes only when dirty.
        private new void Update()
        {
            if (writeCooldown > 0) writeCooldown -= Time.deltaTime;

            if (AutosaveEnabled && Dirty && !SuppressAutosave && writeCooldown <= 0)
            {
                Save();
                CoalescedSaveCount++;
                writeCooldown = 2f;
                LastLifecycleResult = "autosaved after save-worthy change";
            }
        }

        public override void Save()
        {
            SaveAndLoadSystem<DemoProjectSaveFile>.Save(Path);
            Dirty = false;
        }

        public override void Load(out SaveFile file)
        {
            SuppressAutosave = true;
            try
            {
                SaveAndLoadSystem<DemoProjectSaveFile>.Load(Path, out DemoProjectSaveFile typedFile);
                file = typedFile;

                if (typedFile == null)
                {
                    LastLifecycleResult = "no save file found";
                    return;
                }

                // Overriding Load means reapplying the offline-time policy explicitly.
                TimeSpan offlineTime = DateTime.Now - typedFile.saveTime;
                if (offlineTime.TotalSeconds > maxOfflineSeconds)
                    offlineTime = TimeSpan.FromSeconds(maxOfflineSeconds);
                if (offlineTime.TotalSeconds > 0)
                    ProfitCalculator.CalculateAndApplyProfit(offlineTime);

                LastLifecycleResult = $"loaded; offline={offlineTime.TotalSeconds:0}s; board v{CustomSaveDemoState.VisualBoardVersion} reconciled";
            }
            finally
            {
                Dirty = false;
                SuppressAutosave = false;
            }
        }
    }

    public class CustomSaveAndAutosaveDemoController : DemoSceneController
    {
        private DemoCustomSaveAndLoad saveComponent;

        protected override void SeedDemoState()
        {
            EnsureManager<DemoCustomSaveAndLoad>();
            saveComponent = FindObjectOfType<DemoCustomSaveAndLoad>();
            saveComponent.savePath = "EasyIdleGameDemo_CustomSave";

            EconomyChangedEvents.OnSaveWorthyStateChanged.RemoveListener(MarkSaveComponentDirty);
            EconomyChangedEvents.OnSaveWorthyStateChanged.AddListener(MarkSaveComponentDirty);

            AddCurrency("Gold", 100);
            GrantBusiness("TrackedBusiness", 1);
            BuyBoostForFree("SaveSessionAutomation", 1);
        }

        private void MarkSaveComponentDirty(string source)
        {
            if (saveComponent != null) saveComponent.MarkDirty();
        }

        private void OnDisable()
        {
            EconomyChangedEvents.OnSaveWorthyStateChanged.RemoveListener(MarkSaveComponentDirty);
        }

        protected override void BuildSceneDashboard()
        {
            AddBusinessPanel("Tracked Business", BusinessPanelMode.Production, "TrackedBusiness");
            AddBoostPanel();
            AddSaveEventPanel();
            AddCustomSavePanel();
        }

        private void AddCustomSavePanel()
        {
            VisualElement section = CreateSection("Custom Save File");
            VisualElement card = CreateObjectCard("DemoCustomSaveAndLoad", "Save And Load", AccentColor);

            AddKeyValue(card, "Save file type", "DemoProjectSaveFile : SaveFile adds a project note and a visual board version");
            AddLiveKeyValue(card, "Path", () => saveComponent == null ? "not created" : saveComponent.Path);
            AddLiveKeyValue(card, "File exists", () => saveComponent != null && System.IO.File.Exists(saveComponent.Path) ? "yes" : "no");
            AddLiveKeyValue(card, "Autosave", () => saveComponent == null
                ? "not created"
                : !saveComponent.AutosaveEnabled ? saveComponent.Dirty ? "disabled (unsaved changes)" : "disabled"
                : saveComponent.SuppressAutosave ? "suppressed (loading)" : saveComponent.Dirty ? "dirty, write queued" : "enabled, idle");
            AddLiveKeyValue(card, "Coalesced writes", () => saveComponent == null ? "0" : saveComponent.CoalescedSaveCount.ToString());
            AddLiveMessage(card, "Lifecycle", () => saveComponent == null ? "not created" : saveComponent.LastLifecycleResult);
            AddLiveKeyValue(card, "Project note", () => CustomSaveDemoState.Note);
            AddLiveKeyValue(card, "Visual board version", () => CustomSaveDemoState.VisualBoardVersion.ToString());

            AddCardAction(card, "Change project state", () =>
            {
                CustomSaveDemoState.VisualBoardVersion++;
                CustomSaveDemoState.Note = $"board v{CustomSaveDemoState.VisualBoardVersion}";
                EconomyChangedEvents.NotifySaveWorthyStateChanged("DemoProjectState");
                Log($"Project state changed to board v{CustomSaveDemoState.VisualBoardVersion}.");
            });
            AddLiveCardAction(card, () => saveComponent != null && saveComponent.AutosaveEnabled
                ? "Disable autosave"
                : "Enable autosave", () =>
            {
                saveComponent.AutosaveEnabled = !saveComponent.AutosaveEnabled;
                Log(saveComponent.AutosaveEnabled
                    ? "Autosave enabled. Any pending changes will be saved automatically."
                    : "Autosave disabled. Changes remain pending until Save now or autosave is enabled again.");
            });
            AddCardAction(card, "Save now", () =>
            {
                saveComponent.Save();
                Log("Saved custom file.");
            });
            AddCardAction(card, "Load saved state", () =>
            {
                saveComponent.Load(out _);
                Log(saveComponent.LastLifecycleResult);
                RefreshDashboard();
            });
            AddCardAction(card, "Delete save file", () =>
            {
                saveComponent.IrreversiblyDeleteSave();
                Log("Deleted custom save file.");
            });

            section.Add(card);
        }
    }
}
