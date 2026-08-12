#if UNITY_EDITOR

using EasyIdleGame.Demos;
using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomEditor(typeof(DemoSceneReadme))]
    public class DemoSceneReadmeEditor : UnityEditor.Editor
    {
        private static readonly Color HeaderColor = new Color(0.16f, 0.3f, 0.38f, 1f);

        private void OnEnable()
        {
            SerializedProperty dataProperty = serializedObject.FindProperty(nameof(DemoSceneReadme.data));
            if (dataProperty != null)
            {
                dataProperty.isExpanded = false;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DemoSceneReadme readme = (DemoSceneReadme)target;

            DrawHeader(readme);

            EditorGUILayout.Space(6);
            DrawSceneExplanation(readme);

            EditorGUILayout.Space(6);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Resource Prefix", string.IsNullOrEmpty(readme.resourcePrefix) ? "(none)" : readme.resourcePrefix + "_", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(8);

            SerializedProperty dataProperty = serializedObject.FindProperty(nameof(DemoSceneReadme.data));
            if (dataProperty != null)
            {
                EditorGUILayout.PropertyField(dataProperty, new GUIContent("Editable Data"), true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawSceneExplanation(DemoSceneReadme readme)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("What this demonstrates", EditorStyles.boldLabel);
                DrawTextBlock(readme.summary, MessageType.None);

                if (readme.sections == null) return;

                foreach (DemoReadmeSection section in readme.sections)
                {
                    if (section == null || string.IsNullOrWhiteSpace(section.body)) continue;

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField(section.heading, EditorStyles.boldLabel);
                    DrawTextBlock(section.body, MessageType.None);
                }
            }
        }

        private static void DrawHeader(DemoSceneReadme readme)
        {
            string title = string.IsNullOrEmpty(readme.title) ? readme.name : readme.title;
            float contentWidth = Mathf.Max(220f, EditorGUIUtility.currentViewWidth - 36f);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            float titleHeight = Mathf.Max(20f, titleStyle.CalcHeight(new GUIContent(title), contentWidth));
            Rect rect = GUILayoutUtility.GetRect(0f, Mathf.Max(40f, titleHeight + 18f), GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, HeaderColor);

            Rect titleRect = new Rect(rect.x + 10f, rect.y + (rect.height - titleHeight) * 0.5f, rect.width - 20f, titleHeight);
            EditorGUI.LabelField(titleRect, title, titleStyle);
        }

        private static void DrawTextBlock(string text, MessageType messageType = MessageType.Info)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            EditorGUILayout.HelpBox(text, messageType);
        }
    }
}
#endif
