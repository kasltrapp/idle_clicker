#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(Output))]
    public class OutputDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float warningHeight = GetWarningHeight(property);
            
            string warningText = GetWarningText(property);
            if (!string.IsNullOrEmpty(warningText))
            {
                Rect helpBoxRect = new Rect(position.x, position.y, position.width, warningHeight - EditorGUIUtility.standardVerticalSpacing);
                EditorGUI.HelpBox(helpBoxRect, warningText, MessageType.Warning);
            }

            Rect currentRect = new Rect(position.x, position.y + warningHeight, position.width, EditorGUIUtility.singleLineHeight);
            
            property.isExpanded = EditorGUI.Foldout(currentRect, property.isExpanded, label);
            currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Output Targets
                SerializedProperty businessOutput = property.FindPropertyRelative("businessOutput");
                SerializedProperty currencyOutput = property.FindPropertyRelative("currencyOutput");
                SerializedProperty boostOutput = property.FindPropertyRelative("boostOutput");
                SerializedProperty managerOutput = property.FindPropertyRelative("managerOutput");

                EditorGUI.PropertyField(currentRect, businessOutput);
                currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(currentRect, currencyOutput);
                currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(currentRect, boostOutput);
                currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(currentRect, managerOutput);
                currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Amounts
                SerializedProperty outputAmount = property.FindPropertyRelative("outputAmount");
                SerializedProperty outputMaxAmount = property.FindPropertyRelative("outputMaxAmount");

                float amountHeight = EditorGUI.GetPropertyHeight(outputAmount, true);
                currentRect.height = amountHeight;
                EditorGUI.PropertyField(currentRect, outputAmount, true);
                currentRect.y += amountHeight + EditorGUIUtility.standardVerticalSpacing;

                float maxAmountHeight = EditorGUI.GetPropertyHeight(outputMaxAmount, true);
                currentRect.height = maxAmountHeight;
                EditorGUI.PropertyField(currentRect, outputMaxAmount, true);
                currentRect.y += maxAmountHeight + EditorGUIUtility.standardVerticalSpacing;

                currentRect.height = EditorGUIUtility.singleLineHeight;

                // Check Strategy Context
                DropStrategy? strategy = GetParentStrategy(property);

                SerializedProperty dropChance = property.FindPropertyRelative("dropChance");
                SerializedProperty weight = property.FindPropertyRelative("weight");

                if (strategy == null || strategy == DropStrategy.All)
                {
                    EditorGUI.PropertyField(currentRect, dropChance);
                    currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                if (strategy == null || strategy == DropStrategy.WeightedRandom)
                {
                    EditorGUI.PropertyField(currentRect, weight);
                    currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = GetWarningHeight(property);
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Foldout

            if (property.isExpanded)
            {
                height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4; // 4 targets

                SerializedProperty outputAmount = property.FindPropertyRelative("outputAmount");
                SerializedProperty outputMaxAmount = property.FindPropertyRelative("outputMaxAmount");

                height += EditorGUI.GetPropertyHeight(outputAmount, true) + EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(outputMaxAmount, true) + EditorGUIUtility.standardVerticalSpacing;

                DropStrategy? strategy = GetParentStrategy(property);
                if (strategy == null || strategy == DropStrategy.All)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                if (strategy == null || strategy == DropStrategy.WeightedRandom)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }

        private DropStrategy? GetParentStrategy(SerializedProperty property)
        {
            string path = property.propertyPath;
            int arrayIndex = path.IndexOf(".outputs.Array.data");
            if (arrayIndex >= 0)
            {
                string parentPath = path.Substring(0, arrayIndex);
                SerializedProperty strategyProp = property.serializedObject.FindProperty(parentPath + ".strategy");
                if (strategyProp != null)
                {
                    return (DropStrategy)strategyProp.enumValueIndex;
                }
            }
            return null;
        }

        private string GetWarningText(SerializedProperty property)
        {
            SerializedProperty businessOutput = property.FindPropertyRelative("businessOutput");
            SerializedProperty currencyOutput = property.FindPropertyRelative("currencyOutput");
            SerializedProperty boostOutput = property.FindPropertyRelative("boostOutput");
            SerializedProperty managerOutput = property.FindPropertyRelative("managerOutput");
            SerializedProperty outputAmount = property.FindPropertyRelative("outputAmount");
            SerializedProperty outputMaxAmount = property.FindPropertyRelative("outputMaxAmount");

            bool hasOutputTarget = false;
            if (businessOutput != null && businessOutput.objectReferenceValue != null) hasOutputTarget = true;
            if (currencyOutput != null && currencyOutput.objectReferenceValue != null) hasOutputTarget = true;
            if (boostOutput != null && boostOutput.objectReferenceValue != null) hasOutputTarget = true;
            if (managerOutput != null && managerOutput.objectReferenceValue != null) hasOutputTarget = true;

            if (!hasOutputTarget)
            {
                return "No output target is assigned. This output will do nothing.";
            }

            if (outputAmount != null)
            {
                SerializedProperty mantissa = outputAmount.FindPropertyRelative("Mantissa");
                if (mantissa != null && mantissa.doubleValue <= 0)
                {
                    return "Output Amount is 0.";
                }

                if (outputMaxAmount != null)
                {
                    SerializedProperty maxMantissa = outputMaxAmount.FindPropertyRelative("Mantissa");
                    SerializedProperty maxExponent = outputMaxAmount.FindPropertyRelative("Exponent");
                    
                    if (maxMantissa != null && maxExponent != null)
                    {
                        // Exponent of -1 usually means it's not set (if we use -1 as the default BigNumber)
                        // Wait, -1 is stored as Mantissa = -1, Exponent = 0
                        bool isNegativeOne = maxMantissa.doubleValue < 0; 
                        
                        if (!isNegativeOne)
                        {
                            SerializedProperty amountExponent = outputAmount.FindPropertyRelative("Exponent");
                            
                            double val1 = mantissa.doubleValue * Mathf.Pow(10, amountExponent.longValue);
                            double val2 = maxMantissa.doubleValue * Mathf.Pow(10, maxExponent.longValue);
                            
                            // Simplistic comparison for inspector warning
                            if (val2 <= val1)
                            {
                                return "Max Amount must be strictly greater than Amount. (in this scenario, the Amount will always be used, use -1 to explicitly disable it)";
                            }
                        }
                    }
                }
            }

            DropStrategy? strategy = GetParentStrategy(property);
            if (strategy == null || strategy == DropStrategy.All)
            {
                SerializedProperty dropChance = property.FindPropertyRelative("dropChance");
                if (dropChance != null && dropChance.floatValue <= 0)
                {
                    return "Drop chance is 0. This output will never drop.";
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
