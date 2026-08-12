
#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomEditor(typeof(SaveAndLoad), true)]
    public class SaveAndLoadInspector : CustomInspectorBase
    {
        public override void DrawDebugBottom()
        {
            SaveAndLoad saveAndLoad = (SaveAndLoad)target;

            GUILayout.Space(16);
            GUILayout.Label("Debug", EditorStyles.boldLabel);

            if (GUILayout.Button("Open file location"))
                EditorUtility.RevealInFinder(saveAndLoad.Path);

            if (GUILayout.Button("IRREVERSIBLY Delete Save"))
                saveAndLoad.IrreversiblyDeleteSave();

            if (GUILayout.Button("Save"))
                saveAndLoad.Save();
        }
    }
}

#endif