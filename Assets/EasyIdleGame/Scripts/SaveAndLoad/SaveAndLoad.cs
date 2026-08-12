using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that handles saving and loading of the game, but can be assigned to a GameObject
    /// </summary>
    [AddComponentMenu("EasyIdleGame/Managers/Save and Load")]
    public class SaveAndLoad : ManagerBase<SaveAndLoad>
    {
        [CommentArea("Save And Load", "Handles JSON save/load through Application.persistentDataPath and applies offline profit after loading.", "Place one SaveAndLoad component in the first gameplay scene. Set savePath per save slot, tune maxOfflineSeconds, then call Save(), Load(), or IrreversiblyDeleteSave() from menu/UI buttons as needed.")]
        [SerializeField] private string _saveAndLoadComment;

        [Tooltip("Save file name combined with Application.persistentDataPath. Do not include the .json extension; use different values for separate save slots.")]
        public string savePath = "saveFile";

        public string Path => System.IO.Path.Combine(Application.persistentDataPath, savePath.TrimStart('/', '\\') + ".json");

        /// <summary> Save location used before Path.Combine was introduced, where the file name was appended to persistentDataPath without a separator. Kept so old saves can be migrated. </summary>
        public string LegacyPath => Application.persistentDataPath + savePath + ".json";

        [Tooltip("Seconds between automatic saves while this component is active. Lower values save more often but can add IO overhead.")]
        [SerializeField] private float autoSaveInterval = 5;
        private float autoSaveTimer = 0;

        [Tooltip("Maximum offline time in seconds the system will simulate after loading. Default 86400 is 24 hours; use a negative value only if your offline logic explicitly supports it.")]
        public float maxOfflineSeconds = 86400;

        protected virtual void Awake()
        {
            MigrateLegacySave();
        }

        protected void Update()
        {
            if (autoSaveTimer <= 0)
            {
                Save();
                autoSaveTimer = autoSaveInterval;
            }

            autoSaveTimer -= Time.deltaTime;
        }

        public virtual void Save()
        {
            Debug.Log($"Saving");

            SaveAndLoadSystem<SaveFile>.Save(Path);
        }

        public void Load() => Load(out _);

        public virtual void Load(out SaveFile file)
        {
            MigrateLegacySave();
            SaveAndLoadSystem<SaveFile>.Load(Path, out file);

            if (file != null)
            {
                System.TimeSpan offlineTime = System.DateTime.Now - file.saveTime;

                if (offlineTime.TotalSeconds > maxOfflineSeconds)
                    offlineTime = System.TimeSpan.FromSeconds(maxOfflineSeconds);

                if (offlineTime.TotalSeconds > 0)
                    ProfitCalculator.CalculateAndApplyProfit(offlineTime);
            }
        }

        public void IrreversiblyDeleteSave()
        {
            SaveAndLoadSystem<SaveFile>.DeleteSave(Path);
            SaveAndLoadSystem<SaveFile>.DeleteSave(LegacyPath);
        }

        private void MigrateLegacySave()
        {
            try
            {
                string legacy = System.IO.Path.GetFullPath(LegacyPath);
                string current = System.IO.Path.GetFullPath(Path);

                if (legacy == current) return;
                if (System.IO.File.Exists(current) || !System.IO.File.Exists(legacy)) return;

                System.IO.File.Move(legacy, current);
                Debug.Log($"Migrated legacy save file from {legacy} to {current}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not migrate legacy save file: {e.Message}");
            }
        }
    }
}
