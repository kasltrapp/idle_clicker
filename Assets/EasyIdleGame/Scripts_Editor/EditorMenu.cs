#if UNITY_EDITOR
using EasyIdleGame.UI;
using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    public class EditorMenu : EditorWindow
    {
        const string BASE_PATH = "Tools/EasyIdleGame";

        [MenuItem(BASE_PATH + "/ExecuteTests")]
        public static void RunTests()
        {
            Debug.LogWarning("Custom tests are deprecated, use Unity's Test Runner instead");
        }

        [MenuItem(BASE_PATH + "/RedrawAll")]
        public static void RedrawAllDisplayers()
        {
            BaseDisplayer[] disps = FindObjectsOfType<BaseDisplayer>(true);

            foreach (BaseDisplayer disp in disps)
            {
                disp.Redraw_Default();
            }

            Debug.Log("Displayers were redrawn");
        }


        [MenuItem(BASE_PATH + "/InspectorMode/All")]
        public static void SetInspectorModeAll()
        {
            InspectorMode = InspectorDebugMode.All;
        }

        [MenuItem(BASE_PATH + "/InspectorMode/NoDebug")]
        public static void SetInspectorModeOnlyBusinesses()
        {
            InspectorMode = InspectorDebugMode.NoDebug;
        }

        [MenuItem(BASE_PATH + "/InspectorMode/DebugOnly")]
        public static void SetInspectorModeOnlyBusinessesAndUpgrades()
        {
            InspectorMode = InspectorDebugMode.DebugOnly;
        }

        public static InspectorDebugMode InspectorMode
        {
            get
            {
                return (InspectorDebugMode)EditorPrefs.GetInt("InspectorDebugMode", 0);
            }
            set
            {
                EditorPrefs.SetInt("InspectorDebugMode", (int)value);
            }
        }

    }
}

#endif