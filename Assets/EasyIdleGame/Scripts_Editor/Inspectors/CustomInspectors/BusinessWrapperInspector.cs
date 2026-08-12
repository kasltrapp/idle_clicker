#if UNITY_EDITOR
using UnityEditor;

namespace EasyIdleGame
{
    [CustomEditor(typeof(BusinessWrapper))]
    public class BusinessWrapperInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty comment = serializedObject.FindProperty("_businessWrapperComment");
            if (comment != null)
            {
                EditorGUILayout.PropertyField(comment);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("underlyingBusiness"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scale Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("costScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("productionCostScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("upgradeCostScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToProduceScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("outputsScale"));

            serializedObject.ApplyModifiedProperties();

            // Provide a button to manually sync just in case
            if (UnityEngine.GUILayout.Button("Force Sync Data"))
            {
                ((BusinessWrapper)target).SyncData();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
#endif
