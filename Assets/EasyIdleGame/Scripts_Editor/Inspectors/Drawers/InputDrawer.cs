#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(Input))]
    public class InputDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float warningHeight = GetWarningHeight(property);
            
            Rect mainRect = new Rect(position.x, position.y + warningHeight, position.width, position.height - warningHeight);

            string warningText = GetWarningText(property);
            if (!string.IsNullOrEmpty(warningText))
            {
                Rect helpBoxRect = new Rect(position.x, position.y, position.width, warningHeight - EditorGUIUtility.standardVerticalSpacing);
                EditorGUI.HelpBox(helpBoxRect, warningText, MessageType.Warning);
            }

            EditorGUI.PropertyField(mainRect, property, label, true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = EditorGUI.GetPropertyHeight(property, label, true);
            float warningHeight = GetWarningHeight(property);
            return baseHeight + warningHeight;
        }

        private string GetWarningText(SerializedProperty property)
        {
            SerializedProperty businessInput = property.FindPropertyRelative("businessInput");
            SerializedProperty currencyInput = property.FindPropertyRelative("currencyInput");
            SerializedProperty inputAmount = property.FindPropertyRelative("inputAmount");

            bool hasInputTarget = false;
            if (businessInput != null && businessInput.objectReferenceValue != null) hasInputTarget = true;
            if (currencyInput != null && currencyInput.objectReferenceValue != null) hasInputTarget = true;

            if (!hasInputTarget)
            {
                return "No input target is assigned. This input will do nothing/always be fulfilled.";
            }

            if (inputAmount != null)
            {
                SerializedProperty mantissa = inputAmount.FindPropertyRelative("Mantissa");
                if (mantissa != null && mantissa.doubleValue <= 0)
                {
                    return "Amount is 0.";
                }
            }

            return null;
        }

        private float GetWarningHeight(SerializedProperty property)
        {
            string warningText = GetWarningText(property);
            if (!string.IsNullOrEmpty(warningText))
            {
                return EditorGUIUtility.singleLineHeight * 2.5f;
            }
            return 0f;
        }
    }
}

#endif
