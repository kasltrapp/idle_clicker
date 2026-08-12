#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(DropTable))]
    public class DropTableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect currentRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(currentRect, property.isExpanded, label, true);
            currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                SerializedProperty strategy = property.FindPropertyRelative("strategy");
                SerializedProperty totalDropAmount = property.FindPropertyRelative("totalDropAmount");
                SerializedProperty outputs = property.FindPropertyRelative("outputs");

                float strategyHeight = EditorGUI.GetPropertyHeight(strategy, true);
                currentRect.height = strategyHeight;
                EditorGUI.PropertyField(currentRect, strategy, true);
                currentRect.y += strategyHeight + EditorGUIUtility.standardVerticalSpacing;

                if (strategy.enumValueIndex == (int)DropStrategy.WeightedRandom)
                {
                    float dropAmountHeight = EditorGUI.GetPropertyHeight(totalDropAmount, true);
                    currentRect.height = dropAmountHeight;
                    EditorGUI.PropertyField(currentRect, totalDropAmount, true);
                    currentRect.y += dropAmountHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                float outputsHeight = EditorGUI.GetPropertyHeight(outputs, true);
                currentRect.height = outputsHeight;
                EditorGUI.PropertyField(currentRect, outputs, true);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                SerializedProperty strategy = property.FindPropertyRelative("strategy");
                SerializedProperty totalDropAmount = property.FindPropertyRelative("totalDropAmount");
                SerializedProperty outputs = property.FindPropertyRelative("outputs");

                height += EditorGUI.GetPropertyHeight(strategy, true) + EditorGUIUtility.standardVerticalSpacing;

                if (strategy.enumValueIndex == (int)DropStrategy.WeightedRandom)
                {
                    height += EditorGUI.GetPropertyHeight(totalDropAmount, true) + EditorGUIUtility.standardVerticalSpacing;
                }

                height += EditorGUI.GetPropertyHeight(outputs, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }
}

#endif
