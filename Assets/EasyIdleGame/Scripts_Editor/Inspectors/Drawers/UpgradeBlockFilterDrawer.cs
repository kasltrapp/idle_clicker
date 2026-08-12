#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomPropertyDrawer(typeof(UpgradeBlockFilter))]
    public class UpgradeBlockFilterDrawer : PropertyDrawer
    {
        [Flags]
        private enum FilterMask
        {
            None = 0,
            SourceBusiness = 1 << 0,
            SourceBusinessGroups = 1 << 1,
            RewardSource = 1 << 2,
            TargetCurrency = 1 << 3,
            TargetCurrencyGroups = 1 << 4,
            TargetBusiness = 1 << 5,
            TargetBusinessGroups = 1 << 6,
            TargetUpgrade = 1 << 7,
            TargetUpgradeGroups = 1 << 8,
            TargetBoost = 1 << 9,
            TargetBoostGroups = 1 << 10,
            TargetManager = 1 << 11,
            TargetMergeRecipe = 1 << 12,
            BusinessLevelRange = 1 << 13,
            Runtime = 1 << 14
        }

        private readonly struct FilterField
        {
            public readonly string Group;
            public readonly string PropertyName;
            public readonly FilterMask Mask;

            public FilterField(string group, string propertyName, FilterMask mask)
            {
                Group = group;
                PropertyName = propertyName;
                Mask = mask;
            }
        }

        private static readonly FilterField[] Fields =
        {
            new FilterField("Source Filters", "sourceBusiness", FilterMask.SourceBusiness),
            new FilterField("Source Filters", "sourceBusinessGroups", FilterMask.SourceBusinessGroups),
            new FilterField("Source Filters", "source", FilterMask.RewardSource),
            new FilterField("Target Filters", "targetCurrency", FilterMask.TargetCurrency),
            new FilterField("Target Filters", "targetCurrencyGroups", FilterMask.TargetCurrencyGroups),
            new FilterField("Target Filters", "targetBusiness", FilterMask.TargetBusiness),
            new FilterField("Target Filters", "targetBusinessGroups", FilterMask.TargetBusinessGroups),
            new FilterField("Target Filters", "targetUpgrade", FilterMask.TargetUpgrade),
            new FilterField("Target Filters", "targetUpgradeGroups", FilterMask.TargetUpgradeGroups),
            new FilterField("Target Filters", "targetBoost", FilterMask.TargetBoost),
            new FilterField("Target Filters", "targetBoostGroups", FilterMask.TargetBoostGroups),
            new FilterField("Target Filters", "targetManager", FilterMask.TargetManager),
            new FilterField("Target Filters", "targetMergeRecipe", FilterMask.TargetMergeRecipe),
            new FilterField("Business Level Range", "minBusinessLevel", FilterMask.BusinessLevelRange),
            new FilterField("Business Level Range", "maxBusinessLevel", FilterMask.BusinessLevelRange),
            new FilterField("Runtime Filters", "runtimeFilter", FilterMask.Runtime)
        };

        private static readonly string[] Groups =
        {
            "Source Filters",
            "Target Filters",
            "Business Level Range",
            "Runtime Filters"
        };

        private const FilterMask AllFilters =
            FilterMask.SourceBusiness |
            FilterMask.SourceBusinessGroups |
            FilterMask.RewardSource |
            FilterMask.TargetCurrency |
            FilterMask.TargetCurrencyGroups |
            FilterMask.TargetBusiness |
            FilterMask.TargetBusinessGroups |
            FilterMask.TargetUpgrade |
            FilterMask.TargetUpgradeGroups |
            FilterMask.TargetBoost |
            FilterMask.TargetBoostGroups |
            FilterMask.TargetManager |
            FilterMask.TargetMergeRecipe |
            FilterMask.BusinessLevelRange |
            FilterMask.Runtime;

        private static float LineHeight => EditorGUIUtility.singleLineHeight;
        private static float Spacing => EditorGUIUtility.standardVerticalSpacing;
        private static float HelpBoxHeight => LineHeight * 2.5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            UpgradeType? upgradeType = GetParentUpgradeType(property);
            FilterMask relevantMask = GetRelevantMask(upgradeType);
            Rect currentRect = new Rect(position.x, position.y, position.width, LineHeight);

            property.isExpanded = EditorGUI.Foldout(currentRect, property.isExpanded, GetFoldoutLabel(label, upgradeType), true);
            currentRect.y += LineHeight + Spacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                if (relevantMask == FilterMask.None)
                {
                    DrawHelpBox(ref currentRect, "This upgrade type currently has no type-specific block filters.", MessageType.Info);
                }
                else
                {
                    DrawRelevantFields(ref currentRect, property, relevantMask);
                }

                if (HasUnusedValues(property, relevantMask))
                {
                    DrawSection(ref currentRect, "Unused Filters With Values");
                    DrawHelpBox(ref currentRect, "These filters are not used by the selected upgrade type. Clear them or change the upgrade type if they were intentional.", MessageType.Warning);
                    DrawUnusedFields(ref currentRect, property, relevantMask);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            UpgradeType? upgradeType = GetParentUpgradeType(property);
            FilterMask relevantMask = GetRelevantMask(upgradeType);
            float height = LineHeight + Spacing;

            if (property.isExpanded)
            {
                if (relevantMask == FilterMask.None)
                {
                    height += HelpBoxHeight + Spacing;
                }
                else
                {
                    height += GetRelevantFieldsHeight(property, relevantMask);
                }

                if (HasUnusedValues(property, relevantMask))
                {
                    height += GetSectionHeight();
                    height += HelpBoxHeight + Spacing;
                    height += GetUnusedFieldsHeight(property, relevantMask);
                }
            }

            return height;
        }

        private static GUIContent GetFoldoutLabel(GUIContent label, UpgradeType? upgradeType)
        {
            if (!upgradeType.HasValue) return label;
            return new GUIContent($"{label.text} ({ObjectNames.NicifyVariableName(upgradeType.Value.ToString())})", label.tooltip);
        }

        private static UpgradeType? GetParentUpgradeType(SerializedProperty property)
        {
            const string filterSuffix = ".filters";
            string path = property.propertyPath;
            if (!path.EndsWith(filterSuffix, StringComparison.Ordinal)) return null;

            string parentPath = path.Substring(0, path.Length - filterSuffix.Length);
            SerializedProperty typeProperty = property.serializedObject.FindProperty(parentPath + ".upgradeType");
            if (typeProperty == null) return null;

            return (UpgradeType)typeProperty.intValue;
        }

        private static FilterMask GetRelevantMask(UpgradeType? type)
        {
            if (!type.HasValue) return AllFilters;

            switch (type.Value)
            {
                case UpgradeType.production:
                    return FilterMask.SourceBusiness |
                           FilterMask.SourceBusinessGroups |
                           FilterMask.RewardSource |
                           FilterMask.TargetCurrency |
                           FilterMask.TargetCurrencyGroups |
                           FilterMask.TargetBusiness |
                           FilterMask.TargetBusinessGroups |
                           FilterMask.TargetBoost |
                           FilterMask.TargetBoostGroups |
                           FilterMask.TargetManager |
                           FilterMask.TargetMergeRecipe |
                           FilterMask.BusinessLevelRange |
                           FilterMask.Runtime;

                case UpgradeType.purchaseCost:
                    return FilterMask.TargetBusiness |
                           FilterMask.TargetBusinessGroups |
                           FilterMask.TargetBoost |
                           FilterMask.TargetBoostGroups |
                           FilterMask.TargetManager;

                case UpgradeType.productionInputCost:
                    return FilterMask.SourceBusiness |
                           FilterMask.SourceBusinessGroups |
                           FilterMask.TargetCurrency |
                           FilterMask.TargetCurrencyGroups |
                           FilterMask.TargetBusiness |
                           FilterMask.TargetBusinessGroups |
                           FilterMask.BusinessLevelRange;

                case UpgradeType.upgradePurchaseCost:
                    return FilterMask.TargetUpgrade |
                           FilterMask.TargetUpgradeGroups;

                case UpgradeType.levelUpCost:
                    return FilterMask.TargetBusiness |
                           FilterMask.TargetBusinessGroups |
                           FilterMask.TargetManager |
                           FilterMask.BusinessLevelRange;

                case UpgradeType.mergeInputCost:
                    return FilterMask.TargetCurrency |
                           FilterMask.TargetCurrencyGroups |
                           FilterMask.TargetBusiness |
                           FilterMask.TargetBusinessGroups |
                           FilterMask.TargetMergeRecipe;

                case UpgradeType.speed:
                    return FilterMask.SourceBusiness |
                           FilterMask.SourceBusinessGroups |
                           FilterMask.RewardSource |
                           FilterMask.TargetBusiness |
                           FilterMask.TargetBusinessGroups |
                           FilterMask.TargetBoost |
                           FilterMask.TargetBoostGroups |
                           FilterMask.TargetManager |
                           FilterMask.TargetMergeRecipe |
                           FilterMask.BusinessLevelRange;

                case UpgradeType.duration:
                    return FilterMask.TargetBoost |
                           FilterMask.TargetBoostGroups |
                           FilterMask.TargetManager;

                case UpgradeType.dropChance:
                case UpgradeType.dropWeight:
                    return FilterMask.SourceBusiness |
                           FilterMask.SourceBusinessGroups |
                           FilterMask.RewardSource |
                           FilterMask.TargetCurrency |
                           FilterMask.TargetCurrencyGroups |
                           FilterMask.TargetBusiness |
                           FilterMask.TargetBusinessGroups |
                           FilterMask.TargetBoost |
                           FilterMask.TargetBoostGroups |
                           FilterMask.TargetManager |
                           FilterMask.TargetMergeRecipe |
                           FilterMask.BusinessLevelRange |
                           FilterMask.Runtime;

                case UpgradeType.productionOutputScaling:
                    return FilterMask.SourceBusiness |
                           FilterMask.SourceBusinessGroups |
                           FilterMask.TargetBusiness |
                           FilterMask.TargetBusinessGroups |
                           FilterMask.BusinessLevelRange |
                           FilterMask.Runtime;

                case UpgradeType.cooldown:
                    return FilterMask.TargetManager;

                case UpgradeType.playerXp:
                case UpgradeType.businessXp:
                    return FilterMask.None;

                default:
                    return AllFilters;
            }
        }

        private static void DrawRelevantFields(ref Rect currentRect, SerializedProperty property, FilterMask relevantMask)
        {
            for (int i = 0; i < Groups.Length; i++)
            {
                if (!HasRelevantFieldInGroup(Groups[i], relevantMask)) continue;

                DrawSection(ref currentRect, Groups[i]);
                for (int fieldIndex = 0; fieldIndex < Fields.Length; fieldIndex++)
                {
                    FilterField field = Fields[fieldIndex];
                    if (field.Group != Groups[i] || !IsRelevant(field, relevantMask)) continue;
                    DrawProperty(ref currentRect, property.FindPropertyRelative(field.PropertyName));
                }
            }
        }

        private static void DrawUnusedFields(ref Rect currentRect, SerializedProperty property, FilterMask relevantMask)
        {
            for (int i = 0; i < Fields.Length; i++)
            {
                FilterField field = Fields[i];
                if (IsRelevant(field, relevantMask)) continue;

                SerializedProperty fieldProperty = property.FindPropertyRelative(field.PropertyName);
                if (!HasNonDefaultValue(fieldProperty, field)) continue;

                DrawProperty(ref currentRect, fieldProperty);
            }
        }

        private static float GetRelevantFieldsHeight(SerializedProperty property, FilterMask relevantMask)
        {
            float height = 0f;

            for (int i = 0; i < Groups.Length; i++)
            {
                if (!HasRelevantFieldInGroup(Groups[i], relevantMask)) continue;

                height += GetSectionHeight();
                for (int fieldIndex = 0; fieldIndex < Fields.Length; fieldIndex++)
                {
                    FilterField field = Fields[fieldIndex];
                    if (field.Group != Groups[i] || !IsRelevant(field, relevantMask)) continue;
                    height += GetPropertyHeight(property.FindPropertyRelative(field.PropertyName));
                }
            }

            return height;
        }

        private static float GetUnusedFieldsHeight(SerializedProperty property, FilterMask relevantMask)
        {
            float height = 0f;

            for (int i = 0; i < Fields.Length; i++)
            {
                FilterField field = Fields[i];
                if (IsRelevant(field, relevantMask)) continue;

                SerializedProperty fieldProperty = property.FindPropertyRelative(field.PropertyName);
                if (!HasNonDefaultValue(fieldProperty, field)) continue;

                height += GetPropertyHeight(fieldProperty);
            }

            return height;
        }

        private static bool HasRelevantFieldInGroup(string group, FilterMask relevantMask)
        {
            for (int i = 0; i < Fields.Length; i++)
            {
                if (Fields[i].Group == group && IsRelevant(Fields[i], relevantMask)) return true;
            }

            return false;
        }

        private static bool HasUnusedValues(SerializedProperty property, FilterMask relevantMask)
        {
            for (int i = 0; i < Fields.Length; i++)
            {
                FilterField field = Fields[i];
                if (IsRelevant(field, relevantMask)) continue;
                if (HasNonDefaultValue(property.FindPropertyRelative(field.PropertyName), field)) return true;
            }

            return false;
        }

        private static bool IsRelevant(FilterField field, FilterMask relevantMask)
        {
            return (relevantMask & field.Mask) != 0;
        }

        private static bool HasNonDefaultValue(SerializedProperty property, FilterField field)
        {
            if (property == null) return false;

            switch (field.PropertyName)
            {
                case "source":
                    return property.enumValueIndex != 0;
                case "minBusinessLevel":
                    return property.intValue != 0;
                case "maxBusinessLevel":
                    return property.intValue != -1;
                case "runtimeFilter":
                    return property.intValue != 0;
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference)
                return property.objectReferenceValue != null;

            if (property.isArray)
                return property.arraySize > 0;

            return false;
        }

        private static void DrawSection(ref Rect currentRect, string title)
        {
            currentRect.height = LineHeight;
            EditorGUI.LabelField(currentRect, title, EditorStyles.boldLabel);
            currentRect.y += LineHeight + Spacing;
        }

        private static void DrawHelpBox(ref Rect currentRect, string message, MessageType messageType)
        {
            currentRect.height = HelpBoxHeight;
            EditorGUI.HelpBox(currentRect, message, messageType);
            currentRect.y += HelpBoxHeight + Spacing;
        }

        private static void DrawProperty(ref Rect currentRect, SerializedProperty property)
        {
            if (property == null) return;

            float height = EditorGUI.GetPropertyHeight(property, true);
            currentRect.height = height;
            EditorGUI.PropertyField(currentRect, property, true);
            currentRect.y += height + Spacing;
        }

        private static float GetSectionHeight()
        {
            return LineHeight + Spacing;
        }

        private static float GetPropertyHeight(SerializedProperty property)
        {
            if (property == null) return 0f;
            return EditorGUI.GetPropertyHeight(property, true) + Spacing;
        }
    }
}

#endif
