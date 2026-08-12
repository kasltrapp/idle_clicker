#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(CommentAreaAttribute))]
    public class CommentAreaDrawer : PropertyDrawer
    {
        private const float Padding = 4f;

        private static GUIStyle s_foldoutStyle;
        private static GUIStyle FoldoutStyle
        {
            get
            {
                if (s_foldoutStyle == null)
                {
                    s_foldoutStyle = new GUIStyle(EditorStyles.foldout);
                    s_foldoutStyle.fontStyle = FontStyle.BoldAndItalic;
                    Color infoColor = EditorGUIUtility.isProSkin ? new Color(0.4f, 0.7f, 1.0f) : new Color(0.1f, 0.4f, 0.8f);
                    s_foldoutStyle.normal.textColor = infoColor;
                    s_foldoutStyle.onNormal.textColor = infoColor;
                    s_foldoutStyle.focused.textColor = infoColor;
                    s_foldoutStyle.onFocused.textColor = infoColor;
                    s_foldoutStyle.active.textColor = infoColor;
                    s_foldoutStyle.onActive.textColor = infoColor;
                }
                return s_foldoutStyle;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (CommentAreaAttribute)attribute;

            string title = attr.Title ?? "Info";
            string body = BuildBody(attr);

            Rect foldoutRect = new Rect(position.x + Padding, position.y + Padding, position.width - Padding * 2, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, "[i]  " + title, true, FoldoutStyle);

            if (property.isExpanded && !string.IsNullOrEmpty(body))
            {
                Rect helpBoxRect = new Rect(
                    position.x + Padding,
                    position.y + EditorGUIUtility.singleLineHeight + Padding * 2,
                    position.width - Padding * 2,
                    position.height - EditorGUIUtility.singleLineHeight - Padding * 3
                );
                EditorGUI.HelpBox(helpBoxRect, body, MessageType.None);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = (CommentAreaAttribute)attribute;
            float height = EditorGUIUtility.singleLineHeight + Padding * 2;

            if (property.isExpanded)
            {
                string body = BuildBody(attr);

                if (!string.IsNullOrEmpty(body))
                {
                    var content = new GUIContent(body);
                    float width = EditorGUIUtility.currentViewWidth - 40f - Padding * 2;
                    float textHeight = EditorStyles.helpBox.CalcHeight(content, width);
                    height += Mathf.Max(textHeight, EditorGUIUtility.singleLineHeight * 2f) + Padding;
                }
            }
            return height;
        }

        private static string BuildBody(CommentAreaAttribute attr)
        {
            string content = attr.Content ?? "";

            if (string.IsNullOrEmpty(attr.ExampleUsage))
                return content;

            if (string.IsNullOrEmpty(content))
                return "Example usage:\n" + attr.ExampleUsage;

            return content + "\n\nExample usage:\n" + attr.ExampleUsage;
        }
    }
}
#endif
