#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(BigNumber))]
    public class BigNumberDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            SerializedProperty mantissaProp = property.FindPropertyRelative("Mantissa");
            SerializedProperty exponentProp = property.FindPropertyRelative("Exponent");

            float totalWidth = position.width;
            float eWidth = 15f;
            float remainingWidth = totalWidth - eWidth;
            
            float mantissaWidth = remainingWidth * 0.35f;
            float exponentWidth = remainingWidth * 0.35f;
            float resultWidth = remainingWidth * 0.30f;

            // Draw Mantissa [value]
            Rect mantissaRect = new Rect(position.x, position.y, mantissaWidth - 2, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(mantissaRect, mantissaProp, GUIContent.none);

            // Draw "e"
            Rect eRect = new Rect(mantissaRect.xMax, position.y, eWidth, EditorGUIUtility.singleLineHeight);
            GUIStyle eStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(eRect, "e", eStyle);

            // Draw Exponent
            Rect exponentRect = new Rect(eRect.xMax, position.y, exponentWidth - 2, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(exponentRect, exponentProp, GUIContent.none);

            // Draw uneditable text area for the scientific notation
            Rect textAreaRect = new Rect(exponentRect.xMax + 2, position.y, resultWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(textAreaRect, $"= {new BigNumber(mantissaProp.doubleValue, exponentProp.longValue)}");

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}

#endif