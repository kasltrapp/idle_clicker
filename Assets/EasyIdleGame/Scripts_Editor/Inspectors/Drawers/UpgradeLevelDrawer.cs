using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(UpgradeLevel), true)]
    public class UpgradeLevelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string displayName = BuildDisplayName(property);
            EditorGUI.PropertyField(position, property, new GUIContent(displayName), true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        private string BuildDisplayName(SerializedProperty property)
        {
            string result = "";

            SerializedProperty levelProp = property.FindPropertyRelative("level");
            SerializedProperty nameProp = property.FindPropertyRelative("businessName");
            SerializedProperty maxAmountProp = property.FindPropertyRelative("maxAmount_");
            SerializedProperty buyableProp = property.FindPropertyRelative("maxBuyableAmount");

            if (levelProp != null)
                result = $" Lvl: {FormatBigNumber(levelProp)} - ";

            if (nameProp != null && !string.IsNullOrEmpty(nameProp.stringValue))
                result += nameProp.stringValue;
            else
                result += "Unnamed Level";

            if (maxAmountProp != null)
                result += $" Max: {FormatBigNumber(maxAmountProp)}";

            if (buyableProp != null)
                result += $" / Buyable: {FormatBigNumber(buyableProp)}";

            return result;
        }

        private string FormatBigNumber(SerializedProperty bigNumberProp)
        {
            SerializedProperty mantissa = bigNumberProp.FindPropertyRelative("Mantissa");
            SerializedProperty exponent = bigNumberProp.FindPropertyRelative("Exponent");

            if (mantissa != null && exponent != null)
            {
                return new BigNumber(mantissa.doubleValue, exponent.intValue).ToString();
            }

            return "?";
        }
    }
}

#endif