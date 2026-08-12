#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(Lock))]
    public class LockDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate height for HelpBox
            float warningHeight = GetWarningHeight(property);
            
            Rect mainRect = new Rect(position.x, position.y + warningHeight, position.width, position.height - warningHeight);

            // Draw HelpBox if needed
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
            SerializedProperty businessLock = property.FindPropertyRelative("businessLock");
            SerializedProperty currencyLock = property.FindPropertyRelative("currencyLock");
            SerializedProperty playerLevelLock = property.FindPropertyRelative("playerLevelLock");
            SerializedProperty customStatLock = property.FindPropertyRelative("customStatLock");
            SerializedProperty prestigeLock = property.FindPropertyRelative("prestigeLock");
            SerializedProperty locationLock = property.FindPropertyRelative("locationLock");
            SerializedProperty inputAmount = property.FindPropertyRelative("inputAmount");

            SerializedProperty businessGroupLock = property.FindPropertyRelative("businessGroupLock");
            SerializedProperty currencyGroupLock = property.FindPropertyRelative("currencyGroupLock");
            SerializedProperty upgradeGroupLock = property.FindPropertyRelative("upgradeGroupLock");

            bool hasLockTarget = false;
            if (businessLock != null && businessLock.objectReferenceValue != null) hasLockTarget = true;
            if (currencyLock != null && currencyLock.objectReferenceValue != null) hasLockTarget = true;
            if (playerLevelLock != null && playerLevelLock.boolValue) hasLockTarget = true;
            if (customStatLock != null && customStatLock.objectReferenceValue != null) hasLockTarget = true;
            if (prestigeLock != null && prestigeLock.boolValue) hasLockTarget = true;
            if (locationLock != null && locationLock.objectReferenceValue != null) hasLockTarget = true;
            if (businessGroupLock != null && businessGroupLock.objectReferenceValue != null) hasLockTarget = true;
            if (currencyGroupLock != null && currencyGroupLock.objectReferenceValue != null) hasLockTarget = true;
            if (upgradeGroupLock != null && upgradeGroupLock.objectReferenceValue != null) hasLockTarget = true;

            if (!hasLockTarget)
            {
                return "No lock target is assigned. This lock will always be unlocked.";
            }

            if (inputAmount != null)
            {
                SerializedProperty mantissa = inputAmount.FindPropertyRelative("Mantissa");
                if (mantissa != null && mantissa.doubleValue <= 0)
                {
                    return "Warning: Amount is 0 or less. This lock will instantly unlock and might not work as intended.";
                }
            }

            return null;
        }

        private float GetWarningHeight(SerializedProperty property)
        {
            string warningText = GetWarningText(property);
            if (!string.IsNullOrEmpty(warningText))
            {
                // Give it enough height for 2 lines just in case
                return EditorGUIUtility.singleLineHeight * 2.5f;
            }
            return 0f;
        }
    }
}

#endif
