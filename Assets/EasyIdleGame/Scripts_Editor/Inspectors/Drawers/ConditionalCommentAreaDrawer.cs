#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(ConditionalCommentAreaAttribute))]
    public class ConditionalCommentAreaDrawer : PropertyDrawer
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
                    Color infoColor = EditorGUIUtility.isProSkin ? new Color(1.0f, 0.74f, 0.25f) : new Color(0.72f, 0.42f, 0.0f);
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
            if (!ShouldShow(property))
                return;

            var attr = (ConditionalCommentAreaAttribute)attribute;

            string title = attr.Title ?? "Info";
            string body = attr.Content ?? "";

            Rect foldoutRect = new Rect(position.x + Padding, position.y + Padding, position.width - Padding * 2, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, "w  " + title, true, FoldoutStyle);

            if (property.isExpanded && !string.IsNullOrEmpty(body))
            {
                Rect helpBoxRect = new Rect(
                    position.x + Padding,
                    position.y + EditorGUIUtility.singleLineHeight + Padding * 2,
                    position.width - Padding * 2,
                    position.height - EditorGUIUtility.singleLineHeight - Padding * 3
                );
                EditorGUI.HelpBox(helpBoxRect, body, MessageType.Info);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!ShouldShow(property))
                return 0f;

            var attr = (ConditionalCommentAreaAttribute)attribute;
            float height = EditorGUIUtility.singleLineHeight + Padding * 2;

            if (property.isExpanded)
            {
                string body = attr.Content ?? "";

                if (!string.IsNullOrEmpty(body))
                {
                    var content = new GUIContent(body);
                    float width = EditorGUIUtility.currentViewWidth - 40f - Padding * 2;
                    float textHeight = EditorStyles.helpBox.CalcHeight(content, width);
                    // Add extra padding to prevent text overflow due to width miscalculations
                    height += Mathf.Max(textHeight, EditorGUIUtility.singleLineHeight * 2f) + Padding * 4 + EditorGUIUtility.singleLineHeight;
                }
            }

            return height;
        }

        private bool ShouldShow(SerializedProperty property)
        {
            var attr = (ConditionalCommentAreaAttribute)attribute;
            SerializedProperty field = property.serializedObject.FindProperty(attr.FieldName);

            if (field == null)
                return false;

            switch (attr.Mode)
            {
                case ConditionalCommentAreaMode.EmptyList:
                    return field.isArray && field.arraySize == 0;
                case ConditionalCommentAreaMode.BigNumberGreaterThan:
                    SerializedProperty compareField = property.serializedObject.FindProperty(attr.CompareFieldName);
                    return ReadBigNumber(field) > ReadBigNumber(compareField);
                case ConditionalCommentAreaMode.AnyListBigNumberLessThanOrEqual:
                    return AnyListBigNumberLessThanOrEqual(field, attr.ChildFieldName, attr.CompareValue);
                case ConditionalCommentAreaMode.AllListsEmpty:
                    if (!IsEmptyList(field)) return false;

                    if (attr.CompareFieldNames == null || attr.CompareFieldNames.Length == 0)
                    {
                        SerializedProperty otherList = property.serializedObject.FindProperty(attr.CompareFieldName);
                        return IsEmptyList(otherList);
                    }

                    foreach (string compareFieldName in attr.CompareFieldNames)
                    {
                        SerializedProperty otherList = property.serializedObject.FindProperty(compareFieldName);
                        if (!IsEmptyList(otherList)) return false;
                    }

                    return true;
                default:
                    return false;
            }
        }

        private static bool IsEmptyList(SerializedProperty property)
        {
            return property != null && property.isArray && property.arraySize == 0;
        }

        private static bool AnyListBigNumberLessThanOrEqual(SerializedProperty listProperty, string childFieldName, double compareValue)
        {
            if (listProperty == null || !listProperty.isArray)
                return false;

            BigNumber compareNumber = new BigNumber(compareValue);

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty element = listProperty.GetArrayElementAtIndex(i);
                SerializedProperty child = element.FindPropertyRelative(childFieldName);

                if (ReadBigNumber(child) <= compareNumber)
                    return true;
            }

            return false;
        }

        private static BigNumber ReadBigNumber(SerializedProperty property)
        {
            if (property == null)
                return BigNumber.Zero;

            SerializedProperty mantissa = property.FindPropertyRelative("Mantissa");
            SerializedProperty exponent = property.FindPropertyRelative("Exponent");

            if (mantissa == null || exponent == null)
                return BigNumber.Zero;

            return new BigNumber(mantissa.doubleValue, exponent.longValue);
        }
    }
}

#endif
