using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(BuyAmount))]
    public class BuyAmountDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Set the height for each field
            float lineHeight = EditorGUIUtility.singleLineHeight;
            position.height = lineHeight;

            // Draw the label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Get properties for mantissa and exponent
            SerializedProperty amountProp = property.FindPropertyRelative("amount");
            SerializedProperty typeProp = property.FindPropertyRelative("type");

            // Set the width for the two fields
            float fieldWidth = position.width / 2;

            // Draw Mantissa
            Rect mantissaRect = new Rect(position.x, position.y, fieldWidth, lineHeight);
            EditorGUI.PropertyField(mantissaRect, amountProp, GUIContent.none);

            // Draw Exponent
            Rect exponentRect = new Rect(position.x + fieldWidth + 5, position.y, fieldWidth - 5, lineHeight);
            EditorGUI.PropertyField(exponentRect, typeProp, GUIContent.none);

            EditorGUI.EndProperty();
        }

        // Override to set the correct height for the property drawer
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}

#endif