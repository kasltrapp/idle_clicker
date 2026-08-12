using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyIdleGame.Demos
{
    public abstract partial class DemoSceneController
    {
        protected static readonly Color TextColor = new Color(0.87f, 0.9f, 0.92f, 1f);
        protected static readonly Color MutedTextColor = new Color(0.63f, 0.68f, 0.72f, 1f);
        protected static readonly Color PositiveColor = new Color(0.5f, 0.9f, 0.58f, 1f);
        protected static readonly Color AccentColor = new Color(0.45f, 0.73f, 0.95f, 1f);
        protected static readonly Color WarningColor = new Color(0.95f, 0.72f, 0.35f, 1f);
        protected static readonly Color ErrorColor = new Color(0.95f, 0.45f, 0.45f, 1f);
        protected static readonly Color InactiveColor = new Color(0.48f, 0.52f, 0.56f, 1f);

        protected static readonly Color RootBackgroundColor = new Color(0.07f, 0.075f, 0.08f, 1f);
        protected static readonly Color CardDefaultTint = new Color(0.13f, 0.14f, 0.15f, 1f);
        protected static readonly Color ButtonAvailableColor = new Color(0.18f, 0.31f, 0.39f, 1f);
        protected static readonly Color ButtonUnavailableColor = new Color(0.22f, 0.24f, 0.26f, 1f);
        protected static readonly Color ProgressTrackColor = new Color(0.05f, 0.055f, 0.06f, 1f);
        protected static readonly Color ProgressFillColor = new Color(0.36f, 0.68f, 0.78f, 1f);
        protected static readonly Color FallbackSnapshotBackgroundColor = new Color(0.08f, 0.09f, 0.1f, 1f);
        protected static readonly Color FallbackSnapshotAccentColor = new Color(0.35f, 0.68f, 0.78f, 1f);

        private const float ObjectTintStrength = 0.1f;
        private const float ObjectBorderAlpha = 0.54f;

        protected struct ObjectTypeVisualStyle
        {
            public Color Accent;
            public Color Tint;
            public Color Border;

            public ObjectTypeVisualStyle(Color accent)
            {
                Accent = accent;
                Tint = new Color(
                    RootBackgroundColor.r + accent.r * ObjectTintStrength,
                    RootBackgroundColor.g + accent.g * ObjectTintStrength,
                    RootBackgroundColor.b + accent.b * ObjectTintStrength,
                    1f);
                Border = new Color(accent.r, accent.g, accent.b, ObjectBorderAlpha);
            }
        }

        private static readonly IReadOnlyDictionary<string, ObjectTypeVisualStyle> ObjectTypePalette =
            new Dictionary<string, ObjectTypeVisualStyle>(StringComparer.OrdinalIgnoreCase)
            {
                { "Achievement", Style(0.96f, 0.62f, 0.36f) },
                { "Boost", Style(0.73f, 0.56f, 1f) },
                { "Business", Style(0.46f, 0.86f, 0.48f) },
                { "Business Group", Style(0.74f, 0.9f, 0.32f) },
                { "Business Level", Style(0.58f, 0.86f, 0.42f) },
                { "Business Wrapper", Style(0.36f, 0.88f, 0.7f) },
                { "Buy Amount", Style(0.86f, 0.82f, 0.34f) },
                { "Currency", Style(0.32f, 0.68f, 1f) },
                { "Currency Group", Style(0.34f, 0.86f, 0.95f) },
                { "Custom Stat", Style(0.84f, 0.52f, 1f) },
                { "Daily Calendar", Style(0.95f, 0.78f, 0.34f) },
                { "Daily Reward", Style(0.88f, 0.68f, 0.26f) },
                { "Drop Table Recipe", Style(1f, 0.48f, 0.32f) },
                { "Event Source", Style(0.58f, 0.72f, 0.95f) },
                { "Location", Style(0.32f, 0.86f, 0.78f) },
                { "Lock Setup", Style(0.91f, 0.66f, 0.3f) },
                { "Manager", Style(1f, 0.55f, 0.28f) },
                { "Merge Recipe", Style(0.95f, 0.42f, 0.95f) },
                { "Offer Pool Item", Style(0.96f, 0.38f, 0.5f) },
                { "Offer Runtime", Style(0.92f, 0.32f, 0.46f) },
                { "Player Level", Style(0.54f, 0.58f, 1f) },
                { "Player Stats", Style(0.63f, 0.62f, 1f) },
                { "Prestige", Style(0.82f, 0.64f, 1f) },
                { "Production Cost", Style(0.98f, 0.44f, 0.32f) },
                { "Profit Data", Style(0.38f, 0.78f, 0.93f) },
                { "Profit Summary", Style(0.41f, 0.89f, 0.82f) },
                { "Resolved Value", Style(0.58f, 0.82f, 0.98f) },
                { "Runtime State", Style(0.67f, 0.72f, 0.78f) },
                { "Save", Style(0.52f, 0.68f, 0.98f) },
                { "Shop Item", Style(1f, 0.48f, 0.68f) },
                { "Time Skip", Style(0.44f, 0.84f, 0.92f) },
                { "Typed API", Style(0.32f, 0.78f, 0.88f) },
                { "Upgrade", Style(1f, 0.78f, 0.22f) },
            };

        private static ObjectTypeVisualStyle Style(float r, float g, float b)
        {
            return new ObjectTypeVisualStyle(new Color(r, g, b, 1f));
        }

        protected static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        protected ObjectTypeVisualStyle ResolveObjectTypeVisualStyle(string objectType, Color fallbackAccent)
        {
            if (!string.IsNullOrWhiteSpace(objectType) && ObjectTypePalette.TryGetValue(objectType.Trim(), out ObjectTypeVisualStyle style))
            {
                return style;
            }

            return new ObjectTypeVisualStyle(fallbackAccent);
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void ValidateDemoObjectTypePalette()
        {
            string[] duplicateAccents = ObjectTypePalette
                .GroupBy(entry => ColorUtility.ToHtmlStringRGBA(entry.Value.Accent))
                .Where(group => group.Count() > 1)
                .Select(group => $"{group.Key}: {string.Join(", ", group.Select(entry => entry.Key))}")
                .ToArray();

            if (duplicateAccents.Length > 0)
            {
                Debug.LogWarning("Feature demo object type palette has duplicate accent colors: " + string.Join(" | ", duplicateAccents));
            }
        }
#endif
    }
}
