using System.Linq;
using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Non-generic hooks for SaveableScriptableObject's load-error reporting, so callers (mainly
    /// automated tests) can control it without needing a specific closed generic type.
    /// </summary>
    public static class SaveLoadDiagnostics
    {
        /// <summary>
        /// When true, a missing/duplicate asset lookup during save load still logs an error but
        /// skips the blocking Editor dialog. Automated tests that intentionally exercise this path
        /// should set this to true (and reset it afterwards) so the dialog doesn't stall the run.
        /// </summary>
        public static bool SuppressLoadErrorDialog;
    }

    /// <summary>
    /// Class that saves and loads a scriptable object
    /// </summary>
    /// <typeparam name="T"> Scriptable object that is meant to be saved </typeparam>
    [System.Serializable]
    public class SaveableScriptableObject<T> where T : ScriptableObject
    {
        public T Value;
        public string Id;

        public SaveableScriptableObject(T value)
        {
            Value = value;
            Id = value != null ? value.name : null;
        }

        public T LoadScriptableObject()
        {
            T[] records = Resources.LoadAll<T>("").Where(r => r.name == Id).ToArray();

            if (records.Length == 0)
            {
                ReportLoadError($"Scriptable object of type '{typeof(T).Name}' with name '{Id}' not found while loading save data. It may have been renamed or removed since the save was written.");
                return null;
            }

            if (records.Length != 1)
            {
                ReportLoadError($"Multiple scriptable objects of type '{typeof(T).Name}' with name '{Id}' found while loading save data.");
                return null;
            }

            return records[0];
        }

        private static void ReportLoadError(string message)
        {
            Debug.LogError(message);
#if UNITY_EDITOR
            if (!SaveLoadDiagnostics.SuppressLoadErrorDialog && !Application.isBatchMode)
            {
                UnityEditor.EditorUtility.DisplayDialog("Save Load Error", message, "OK");
            }
#endif
        }
    }
}