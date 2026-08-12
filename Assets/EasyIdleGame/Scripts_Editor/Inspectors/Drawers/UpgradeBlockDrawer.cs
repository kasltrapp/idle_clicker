#if UNITY_EDITOR

using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(UpgradeBlock))]
    public class UpgradeBlockDrawer : PropertyDrawer
    {
        private static float LineHeight => EditorGUIUtility.singleLineHeight;
        private static float Spacing => EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect currentRect = new Rect(position.x, position.y, position.width, LineHeight);
            string warningText = GetWarningText(property);
            if (!string.IsNullOrEmpty(warningText))
            {
                float warningHeight = GetWarningHeight(property) - Spacing;
                Rect helpBoxRect = new Rect(position.x, currentRect.y, position.width, warningHeight);
                EditorGUI.HelpBox(helpBoxRect, warningText, MessageType.Info);
                currentRect.y += warningHeight + Spacing;
            }

            property.isExpanded = EditorGUI.Foldout(currentRect, property.isExpanded, GetFoldoutLabel(property, label), true);
            currentRect.y += LineHeight + Spacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawProperty(ref currentRect, property.FindPropertyRelative("upgradeType"));
                DrawProperty(ref currentRect, property.FindPropertyRelative("value"));
                DrawProperty(ref currentRect, property.FindPropertyRelative("operation"));
                DrawProperty(ref currentRect, property.FindPropertyRelative("filters"));
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = GetWarningHeight(property);
            height += LineHeight + Spacing;

            if (property.isExpanded)
            {
                height += GetPropertyHeight(property.FindPropertyRelative("upgradeType"));
                height += GetPropertyHeight(property.FindPropertyRelative("value"));
                height += GetPropertyHeight(property.FindPropertyRelative("operation"));
                height += GetPropertyHeight(property.FindPropertyRelative("filters"));
            }

            return height;
        }

        private static void DrawProperty(ref Rect currentRect, SerializedProperty property, GUIContent label = null)
        {
            if (property == null) return;

            float height = EditorGUI.GetPropertyHeight(property, true);
            currentRect.height = height;

            if (label == null)
                EditorGUI.PropertyField(currentRect, property, true);
            else
                EditorGUI.PropertyField(currentRect, property, label, true);

            currentRect.y += height + Spacing;
        }

        private static float GetPropertyHeight(SerializedProperty property)
        {
            if (property == null) return 0;
            return EditorGUI.GetPropertyHeight(property, true) + Spacing;
        }

        private static GUIContent GetFoldoutLabel(SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeProp = property.FindPropertyRelative("upgradeType");
            SerializedProperty valueProp = property.FindPropertyRelative("value");
            SerializedProperty operationProp = property.FindPropertyRelative("operation");

            if (typeProp == null || valueProp == null || operationProp == null)
                return label;

            string typeName = typeProp.enumDisplayNames[typeProp.enumValueIndex];
            string operation = operationProp.enumValueIndex == (int)UpgradeBlockOperation.Add ? "+" : "x";
            string value = valueProp.floatValue.ToString("0.###", CultureInfo.InvariantCulture);
            return new GUIContent($"{label.text} ({typeName} {operation}{value})", label.tooltip);
        }

        private static string GetWarningText(SerializedProperty property)
        {
            SerializedProperty valueProp = property.FindPropertyRelative("value");

            if (valueProp != null)
            {
                if (valueProp.floatValue == 0f)
                {
                    return "Value is exactly 0. Depending on the upgrade type, this might nullify the target.";
                }
                if (valueProp.floatValue < 0f)
                {
                    return "Value is negative. Check if the specific upgrade type supports negative modifiers.";
                }
            }

            return null;
        }

        private static float GetWarningHeight(SerializedProperty property)
        {
            string warningText = GetWarningText(property);
            if (!string.IsNullOrEmpty(warningText))
            {
                return LineHeight * 2.5f;
            }
            return 0f;
        }
    }
}

#endif
