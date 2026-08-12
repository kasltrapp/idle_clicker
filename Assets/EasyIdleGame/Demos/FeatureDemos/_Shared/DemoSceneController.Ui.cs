using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EasyIdleGame.Demos
{
    public abstract partial class DemoSceneController
    {
        protected void EnsureUi()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
            }

            if (uiDocument.panelSettings == null)
            {
                PanelSettings panelSettings = sharedPanelSettings;
                uiDocument.panelSettings = panelSettings != null ? panelSettings : CreateRuntimePanelSettings();
            }

            VisualElement root = uiDocument.rootVisualElement;
            root.Clear();
            runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;
            root.style.minWidth = 0;
            root.style.minHeight = 0;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;
            root.style.backgroundColor = RootBackgroundColor;
            root.style.color = Color.white;
            root.style.unityFont = runtimeFont;

            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.flexShrink = 0;
            header.style.marginBottom = 6;
            root.Add(header);

            VisualElement titleBlock = new VisualElement();
            titleBlock.style.flexGrow = 1;
            titleBlock.style.minWidth = 0;
            header.Add(titleBlock);

            titleText = new Label(demoTitle);
            StyleText(titleText, 22, Color.white, true);
            titleText.style.whiteSpace = WhiteSpace.Normal;
            titleText.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleBlock.Add(titleText);

            statusText = new Label();
            statusText.style.display = DisplayStyle.None;
            titleBlock.Add(statusText);

            VisualElement body = new VisualElement();
            body.style.flexDirection = FlexDirection.Column;
            body.style.flexGrow = 1;
            body.style.minHeight = 0;
            body.style.minWidth = 0;
            root.Add(body);

            dashboard = new ScrollView();
            dashboard.style.flexGrow = 1;
            dashboard.style.minWidth = 0;
            dashboard.style.minHeight = 0;
            dashboard.style.paddingRight = 2;
            body.Add(dashboard);

            UpdateStatusText();
        }

        protected void AddCardAction(VisualElement parent, string label, Action action, Func<bool> isAvailable = null, bool hideWhenUnavailable = false, string unavailableTooltip = null, bool showUnavailableText = true)
        {
            Func<string> unavailableTooltipProvider = null;
            if (!string.IsNullOrWhiteSpace(unavailableTooltip))
            {
                unavailableTooltipProvider = () => unavailableTooltip;
            }

            AddCardAction(
                parent,
                label,
                action,
                isAvailable,
                hideWhenUnavailable,
                unavailableTooltipProvider,
                showUnavailableText);
        }

        protected void AddCardAction(VisualElement parent, string label, Action action, Func<bool> isAvailable, bool hideWhenUnavailable, Func<string> unavailableTooltipProvider, bool showUnavailableText = true)
        {
            AddCardActionInternal(parent, () => label, action, isAvailable, hideWhenUnavailable, unavailableTooltipProvider, showUnavailableText, false);
        }

        protected void AddLiveCardAction(VisualElement parent, Func<string> labelProvider, Action action, Func<bool> isAvailable = null, bool hideWhenUnavailable = false, Func<string> unavailableTooltipProvider = null, bool showUnavailableText = true)
        {
            AddCardActionInternal(parent, labelProvider, action, isAvailable, hideWhenUnavailable, unavailableTooltipProvider, showUnavailableText, true);
        }

        private void AddCardActionInternal(VisualElement parent, Func<string> labelProvider, Action action, Func<bool> isAvailable, bool hideWhenUnavailable, Func<string> unavailableTooltipProvider, bool showUnavailableText, bool refreshLabel)
        {
            bool available = isAvailable == null || isAvailable();
            if (!available && hideWhenUnavailable)
            {
                return;
            }

            VisualElement actionRow = parent.Q<VisualElement>("CardActions");
            if (actionRow == null)
            {
                actionRow = new VisualElement { name = "CardActions" };
                actionRow.style.flexDirection = FlexDirection.Row;
                actionRow.style.flexWrap = Wrap.Wrap;
                actionRow.style.marginTop = 3;
                parent.Add(actionRow);
            }

            string initialLabel = labelProvider == null ? string.Empty : labelProvider();
            Button button = CreateDemoButton(initialLabel, action, 96, 24);
            Label unavailableLabel = null;
            if (showUnavailableText && unavailableTooltipProvider != null)
            {
                unavailableLabel = AddTextLine(parent, string.Empty, WarningColor, true);
            }

            actionRow.Add(button);

            void RefreshAction()
            {
                string currentLabel = labelProvider == null ? string.Empty : labelProvider();
                if (refreshLabel)
                {
                    button.text = currentLabel;
                }

                bool current = isAvailable == null || isAvailable();
                string currentUnavailableTooltip = unavailableTooltipProvider?.Invoke();
                if (string.IsNullOrWhiteSpace(currentUnavailableTooltip))
                {
                    currentUnavailableTooltip = "Not available right now.";
                }

                button.SetEnabled(current);
                button.tooltip = current ? string.Empty : currentUnavailableTooltip;
                button.style.backgroundColor = current ? ButtonAvailableColor : ButtonUnavailableColor;
                button.style.color = current ? Color.white : MutedTextColor;
                button.style.display = hideWhenUnavailable && !current ? DisplayStyle.None : DisplayStyle.Flex;

                if (unavailableLabel != null)
                {
                    unavailableLabel.text = $"{currentLabel} unavailable: {currentUnavailableTooltip}";
                    unavailableLabel.style.display = current || hideWhenUnavailable ? DisplayStyle.None : DisplayStyle.Flex;
                }
            }

            RefreshAction();
            if (isAvailable != null || refreshLabel)
            {
                liveUiRefreshers.Add(RefreshAction);
            }
        }

        protected Button CreateDemoButton(string label, Action action, float width, float height)
        {
            Button button = new Button(() =>
            {
                try
                {
                    action?.Invoke();
                    RefreshDashboard();
                }
                catch (Exception exception)
                {
                    Log($"Action '{label}' failed: {exception.Message}");
                    Debug.LogException(exception);
                }
            })
            {
                text = label
            };

            button.style.minWidth = width;
            button.style.minHeight = height;
            button.style.flexShrink = 0;
            button.style.marginRight = 3;
            button.style.marginBottom = 3;
            button.style.paddingLeft = 6;
            button.style.paddingRight = 6;
            button.style.paddingTop = 2;
            button.style.paddingBottom = 2;
            button.style.backgroundColor = ButtonAvailableColor;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.whiteSpace = WhiteSpace.NoWrap;
            StyleText(button, 11, Color.white, true);
            SetRounded(button, 4);
            return button;
        }

        protected VisualElement CreateSection(string title)
        {
            VisualElement section = new VisualElement();
            section.style.marginBottom = 7;
            section.style.minWidth = 0;

            Label label = new Label(title);
            StyleText(label, 16, Color.white, true);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 3;
            section.Add(label);

            VisualElement grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.alignItems = Align.Stretch;
            grid.style.minWidth = 0;
            section.Add(grid);
            dashboard.Add(section);

            return grid;
        }

        protected VisualElement CreateCard(string title, bool gridCard = true)
        {
            VisualElement card = new VisualElement();
            card.style.flexGrow = gridCard ? 1 : 0;
            card.style.flexShrink = gridCard ? 1 : 0;
            if (gridCard)
            {
                card.style.flexBasis = 224;
                card.style.minWidth = 176;
            }
            else
            {
                card.style.minWidth = 0;
            }
            card.style.minHeight = 62;
            card.style.marginRight = 4;
            card.style.marginBottom = 4;
            card.style.paddingLeft = 5;
            card.style.paddingRight = 5;
            card.style.paddingTop = 4;
            card.style.paddingBottom = 4;
            card.style.backgroundColor = CardDefaultTint;
            SetRounded(card, 6);

            VisualElement header = new VisualElement { name = "CardHeader" };
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.FlexStart;
            header.style.marginBottom = 3;
            header.style.minWidth = 0;
            card.Add(header);

            VisualElement titleGroup = new VisualElement { name = "CardTitleGroup" };
            titleGroup.style.flexDirection = FlexDirection.Row;
            titleGroup.style.alignItems = Align.Center;
            titleGroup.style.flexGrow = 1;
            titleGroup.style.flexShrink = 1;
            titleGroup.style.minWidth = 0;
            titleGroup.style.marginRight = 5;
            header.Add(titleGroup);

            Label label = new Label(title);
            label.name = "CardTitle";
            StyleText(label, 13, Color.white, true);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.flexShrink = 1;
            label.style.marginRight = 4;
            titleGroup.Add(label);

            VisualElement badgeSlot = new VisualElement { name = "CardBadgeSlot" };
            badgeSlot.style.flexShrink = 0;
            badgeSlot.style.alignItems = Align.FlexEnd;
            header.Add(badgeSlot);

            return card;
        }

        protected VisualElement CreateObjectCard(string title, string objectType, Color? accent = null, bool gridCard = true, UnityEngine.Object configurationObject = null)
        {
            VisualElement card = CreateCard(title, gridCard);
            ObjectTypeVisualStyle style = ResolveObjectTypeVisualStyle(objectType, accent ?? MutedTextColor);
            ApplyObjectTypeTint(card, style);
            AddDescriptionInfo(card, configurationObject, style.Accent);
            AddObjectTypeLabel(card, objectType, style.Accent);
            AddConfigurationAction(card, configurationObject);
            return card;
        }

        protected void AddDescriptionInfo(VisualElement card, UnityEngine.Object configurationObject, Color accent)
        {
            string description = GetConfigurationDescription(configurationObject);
            if (string.IsNullOrWhiteSpace(description)) return;

            Label descriptionLabel = new Label(description);
            descriptionLabel.name = "CardDescriptionInfo";
            StyleText(descriptionLabel, 10, TextColor);
            descriptionLabel.style.display = DisplayStyle.None;
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            descriptionLabel.style.marginTop = 1;
            descriptionLabel.style.marginBottom = 3;
            descriptionLabel.style.paddingLeft = 5;
            descriptionLabel.style.paddingRight = 5;
            descriptionLabel.style.paddingTop = 4;
            descriptionLabel.style.paddingBottom = 4;
            descriptionLabel.style.backgroundColor = WithAlpha(accent, 0.08f);
            SetRounded(descriptionLabel, 4);
            AddCardContent(card, descriptionLabel);

            bool isExpanded = false;
            Label info = new Label("i") { tooltip = description };
            info.RegisterCallback<ClickEvent>(_ =>
            {
                isExpanded = !isExpanded;
                descriptionLabel.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            });
            info.name = "CardDescriptionInfoButton";
            StyleText(info, 10, accent, true);
            info.style.width = 16;
            info.style.height = 16;
            info.style.minWidth = 16;
            info.style.marginLeft = 0;
            info.style.marginRight = 3;
            info.style.marginTop = 0;
            info.style.marginBottom = 0;
            info.style.alignSelf = Align.Center;
            info.style.paddingLeft = 0;
            info.style.paddingRight = 0;
            info.style.paddingTop = 0;
            info.style.paddingBottom = 0;
            info.style.unityTextAlign = TextAnchor.MiddleCenter;
            info.style.backgroundColor = WithAlpha(accent, 0.12f);
            info.style.borderTopColor = WithAlpha(accent, 0.42f);
            info.style.borderRightColor = WithAlpha(accent, 0.42f);
            info.style.borderBottomColor = WithAlpha(accent, 0.42f);
            info.style.borderLeftColor = WithAlpha(accent, 0.42f);
            info.style.borderTopWidth = 1;
            info.style.borderRightWidth = 1;
            info.style.borderBottomWidth = 1;
            info.style.borderLeftWidth = 1;
            SetRounded(info, 8);

            VisualElement titleGroup = card.Q<VisualElement>("CardTitleGroup");
            if (titleGroup != null)
            {
                titleGroup.Add(info);
            }
        }

        protected string GetConfigurationDescription(UnityEngine.Object configurationObject)
        {
            if (configurationObject == null) return null;

            if (configurationObject is IMetadataProvider metadataProvider && !string.IsNullOrWhiteSpace(metadataProvider.Metadata?.description))
            {
                return metadataProvider.Metadata.description.Trim();
            }

            Type configurationType = configurationObject.GetType();
            FieldInfo metadataField = configurationType.GetField("metadata", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (metadataField != null && metadataField.GetValue(configurationObject) is Metadata metadata && !string.IsNullOrWhiteSpace(metadata.description))
            {
                return metadata.description.Trim();
            }

            PropertyInfo metadataProperty = configurationType.GetProperty("metadata", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (metadataProperty != null && metadataProperty.GetValue(configurationObject) is Metadata metadataValue && !string.IsNullOrWhiteSpace(metadataValue.description))
            {
                return metadataValue.description.Trim();
            }

            FieldInfo descriptionField = configurationType.GetField("description", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (descriptionField != null && descriptionField.GetValue(configurationObject) is string description && !string.IsNullOrWhiteSpace(description))
            {
                return description.Trim();
            }

            if (configurationObject is BusinessMergeRecipe)
            {
                return "Consumes the listed inputs, then applies the configured output drop tables when the merge succeeds.";
            }

            if (configurationObject is Reward)
            {
                return "Applies these reward drop tables when this reward is claimed or granted.";
            }

            return null;
        }

        protected void AddObjectTypeLabel(VisualElement card, string objectType, Color accent)
        {
            if (string.IsNullOrWhiteSpace(objectType)) return;

            Label label = new Label(objectType.ToUpperInvariant());
            StyleText(label, 9, accent, true);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.paddingLeft = 4;
            label.style.paddingRight = 4;
            label.style.paddingTop = 1;
            label.style.paddingBottom = 1;
            label.style.backgroundColor = WithAlpha(accent, 0.14f);
            SetRounded(label, 4);

            VisualElement slot = card.Q<VisualElement>("CardBadgeSlot");
            if (slot != null)
            {
                slot.Add(label);
                return;
            }

            AddCardContent(card, label);
        }

        protected Label AddKeyValue(VisualElement parent, string key, string value, Color? color = null, bool bold = false)
        {
            Label label = new Label($"{key}: {value}");
            StyleText(label, 11, color ?? TextColor, bold);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginRight = 5;
            label.style.marginBottom = 1;
            AddFieldContent(parent, label, ShouldUseFullWidthField(key, value));
            return label;
        }

        protected Label AddLiveKeyValue(VisualElement parent, string key, Func<string> valueProvider, Color? color = null, bool bold = false)
        {
            Label label = AddKeyValue(parent, key, valueProvider == null ? string.Empty : valueProvider(), color, bold);

            void RefreshLabel()
            {
                label.text = $"{key}: {(valueProvider == null ? string.Empty : valueProvider())}";
            }

            liveUiRefreshers.Add(RefreshLabel);
            return label;
        }

        protected Label AddLiveKeyValue(VisualElement parent, string key, Func<string> valueProvider, Func<Color> colorProvider, bool bold = false)
        {
            Label label = AddKeyValue(parent, key, valueProvider == null ? string.Empty : valueProvider(), colorProvider == null ? TextColor : colorProvider(), bold);

            void RefreshLabel()
            {
                label.text = $"{key}: {(valueProvider == null ? string.Empty : valueProvider())}";
                if (colorProvider != null)
                {
                    label.style.color = colorProvider();
                }
            }

            liveUiRefreshers.Add(RefreshLabel);
            return label;
        }

        protected Label AddLiveLimit(VisualElement parent, string key, Func<BigNumber> limitProvider, bool hideParentWhenUnlimited = false)
        {
            Label label = AddKeyValue(parent, key, string.Empty);

            void RefreshLimit()
            {
                BigNumber limit = limitProvider == null ? BigNumber.MaxValue : limitProvider();
                bool hasLimit = !IsUnlimited(limit);
                label.text = $"{key}: {DescribeLimit(limit)}";
                label.style.display = hasLimit ? DisplayStyle.Flex : DisplayStyle.None;
                if (hideParentWhenUnlimited)
                {
                    parent.style.display = hasLimit ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }

            RefreshLimit();
            liveUiRefreshers.Add(RefreshLimit);
            return label;
        }

        protected Label AddLiveMessage(VisualElement parent, string key, Func<string> valueProvider, bool hideParentWhenEmpty = false)
        {
            Label label = AddKeyValue(parent, key, string.Empty);

            void RefreshMessage()
            {
                string value = valueProvider == null ? string.Empty : valueProvider();
                bool hasMessage = HasRuntimeMessage(value);

                label.text = $"{key}: {value}";
                label.style.display = hasMessage ? DisplayStyle.Flex : DisplayStyle.None;
                if (hideParentWhenEmpty)
                {
                    parent.style.display = hasMessage ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }

            RefreshMessage();
            liveUiRefreshers.Add(RefreshMessage);
            return label;
        }

        protected bool HasRuntimeMessage(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !string.Equals(value, "None", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(value, "Ready", StringComparison.OrdinalIgnoreCase);
        }

        protected void AddProgress(VisualElement parent, float progress)
        {
            progress = Mathf.Clamp01(progress);

            VisualElement bar = new VisualElement();
            bar.style.height = 5;
            bar.style.marginTop = 2;
            bar.style.marginBottom = 3;
            bar.style.backgroundColor = ProgressTrackColor;
            SetRounded(bar, 3);

            VisualElement fill = new VisualElement();
            fill.style.height = 5;
            fill.style.width = Length.Percent(progress * 100f);
            fill.style.backgroundColor = ProgressFillColor;
            SetRounded(fill, 3);
            bar.Add(fill);

            AddCardContent(parent, bar);
        }

        protected void AddLiveProgress(VisualElement parent, Func<float> progressProvider)
        {
            VisualElement bar = new VisualElement();
            bar.style.height = 5;
            bar.style.marginTop = 2;
            bar.style.marginBottom = 3;
            bar.style.backgroundColor = ProgressTrackColor;
            SetRounded(bar, 3);

            VisualElement fill = new VisualElement();
            fill.style.height = 5;
            fill.style.backgroundColor = ProgressFillColor;
            SetRounded(fill, 3);
            bar.Add(fill);

            void RefreshFill()
            {
                float progress = progressProvider == null ? 0f : Mathf.Clamp01(progressProvider());
                fill.style.width = Length.Percent(progress * 100f);
            }

            RefreshFill();
            liveUiRefreshers.Add(RefreshFill);
            AddCardContent(parent, bar);
        }

        protected void AddCardSeparator(VisualElement parent, string label = null)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginTop = 5;
            row.style.marginBottom = 4;
            row.style.minWidth = 0;

            VisualElement line = new VisualElement();
            line.style.height = 1;
            line.style.flexGrow = 1;
            line.style.backgroundColor = WithAlpha(MutedTextColor, 0.34f);

            if (!string.IsNullOrWhiteSpace(label))
            {
                Label text = new Label(label);
                StyleText(text, 10, MutedTextColor, true);
                text.style.marginRight = 5;
                row.Add(text);
            }

            row.Add(line);
            AddFieldContent(parent, row, true);
        }

        protected void AddIntSlider(VisualElement parent, string label, int min, int max, int value, Action<int> onChanged)
        {
            VisualElement row = new VisualElement();
            row.style.marginTop = 2;
            row.style.marginBottom = 2;
            row.style.minWidth = 0;

            Label valueLabel = new Label($"{label}: {value}");
            StyleText(valueLabel, 11, TextColor, true);
            valueLabel.style.marginBottom = 1;
            row.Add(valueLabel);

            SliderInt slider = new SliderInt(min, max)
            {
                value = Mathf.Clamp(value, min, max),
                showInputField = true
            };
            slider.style.height = 18;
            slider.style.minWidth = 0;
            slider.style.marginLeft = 0;
            slider.style.marginRight = 0;
            slider.RegisterValueChangedCallback(evt =>
            {
                int nextValue = Mathf.Clamp(evt.newValue, min, max);
                valueLabel.text = $"{label}: {nextValue}";
                onChanged?.Invoke(nextValue);
            });
            row.Add(slider);
            AddCardContent(parent, row);
        }

        protected void AddDropTableVisualization(VisualElement parent, List<DropTable> tables, string triggerName = "trigger")
        {
            if (tables == null || tables.Count == 0) return;

            for (int tableIndex = 0; tableIndex < tables.Count; tableIndex++)
            {
                DropTable table = tables[tableIndex];
                if (table == null || table.outputs == null || table.outputs.Count == 0) continue;

                string title = table.strategy == DropStrategy.WeightedRandom
                    ? $"Odds/{triggerName}: {table.totalDropAmount} weighted pick(s)"
                    : $"Odds/{triggerName}: independent rolls";
                Label label = new Label(tableIndex == 0 ? title : $"{title} #{tableIndex + 1}");
                StyleText(label, 11, MutedTextColor, true);
                label.style.marginTop = 2;
                label.style.marginBottom = 1;
                AddCardContent(parent, label);

                float totalWeight = table.outputs.Sum(output => Mathf.Max(0f, output.weight));
                List<string> parts = new List<string>();
                foreach (Output output in table.outputs)
                {
                    float chance = table.strategy == DropStrategy.WeightedRandom
                        ? totalWeight <= 0f ? 0f : Mathf.Max(0f, output.weight) / totalWeight
                        : Mathf.Clamp01(output.dropChance);
                    string suffix = table.strategy == DropStrategy.WeightedRandom
                        ? $"{chance * 100f:0.#}%/pick, weight {output.weight:0.#}"
                        : chance >= 0.999f ? "always" : $"{chance * 100f:0.#}%";
                    parts.Add($"{DescribeOutput(output)} ({suffix})");
                }

                AddTextLine(parent, string.Join("; ", parts), TextColor);
            }
        }

        protected Label AddTextLine(VisualElement parent, string text, Color color, bool bold = false)
        {
            Label label = new Label(text);
            StyleText(label, 11, color, bold);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginBottom = 2;
            AddFieldContent(parent, label, true);
            return label;
        }

        protected void AddCardContent(VisualElement parent, VisualElement child)
        {
            VisualElement actionRow = parent.Q<VisualElement>("CardActions");
            if (actionRow != null && actionRow.parent == parent)
            {
                parent.Insert(parent.IndexOf(actionRow), child);
                return;
            }

            parent.Add(child);
        }

        protected void AddFieldContent(VisualElement parent, VisualElement child, bool fullWidth)
        {
            VisualElement grid = parent.Q<VisualElement>("CardFields");
            if (grid == null)
            {
                grid = new VisualElement { name = "CardFields" };
                grid.style.flexDirection = FlexDirection.Row;
                grid.style.flexWrap = Wrap.Wrap;
                grid.style.minWidth = 0;
                grid.style.marginTop = 0;
                AddCardContent(parent, grid);
            }

            child.style.flexGrow = 1;
            child.style.flexShrink = 1;
            child.style.flexBasis = Length.Percent(fullWidth ? 100 : 47);
            grid.Add(child);
        }

        protected bool ShouldUseFullWidthField(string key, string value)
        {
            string lowerKey = key == null ? string.Empty : key.ToLowerInvariant();
            int valueLength = string.IsNullOrEmpty(value) ? 0 : value.Length;
            return valueLength > 34
                || lowerKey.Contains("status")
                || lowerKey.Contains("reward")
                || lowerKey.Contains("output")
                || lowerKey.Contains("description")
                || lowerKey.Contains("requirement")
                || lowerKey.Contains("lock")
                || lowerKey.Contains("result")
                || lowerKey.Contains("pending")
                || lowerKey.Contains("strategy")
                || lowerKey.Contains("expected")
                || lowerKey.Contains("projected")
                || lowerKey.Contains("stockpile")
                || lowerKey.Contains("persistent")
                || lowerKey.Contains("excluded")
                || lowerKey.Contains("auto threshold")
                || lowerKey.Contains("last");
        }

        protected void AddConfigurationAction(VisualElement card, UnityEngine.Object configurationObject)
        {
#if UNITY_EDITOR
            if (configurationObject == null) return;

            UnityEngine.Object asset = configurationObject;
            AddCardAction(card, "Show config", () => ShowConfiguration(asset));
#endif
        }

#if UNITY_EDITOR
        protected void ShowConfiguration(UnityEngine.Object asset)
        {
            if (asset == null) return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            Log($"Selected configuration asset: {AssetDatabase.GetAssetPath(asset)}.");
        }
#endif

        protected string BuildVerificationSummary(string screenshotPath, string summaryPath)
        {
            VisualElement root = uiDocument.rootVisualElement;
            List<Label> labels = new List<Label>();
            List<Button> buttons = new List<Button>();
            List<SliderInt> sliders = new List<SliderInt>();

            root.Query<Label>().ForEach(labels.Add);
            root.Query<Button>().ForEach(buttons.Add);
            root.Query<SliderInt>().ForEach(sliders.Add);

            IEnumerable<string> visibleLabels = labels
                .Where(IsSummaryLabelVisible)
                .Select(label => label.text)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Take(36);
            IEnumerable<string> visibleButtons = buttons
                .Where(IsElementVisibleForSummary)
                .Select(button => button.text)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Take(36);
            int visibleLabelCount = labels.Count(IsSummaryLabelVisible);
            int visibleButtonCount = buttons.Count(IsElementVisibleForSummary);
            int visibleSliderCount = sliders.Count(IsElementVisibleForSummary);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Scene: {gameObject.scene.name}");
            builder.AppendLine($"Demo: {demoTitle}");
            builder.AppendLine($"Controller: {GetType().Name}");
            builder.AppendLine($"Resource prefix: {resourcePrefix}");
            builder.AppendLine($"Root size: {root.resolvedStyle.width:0} x {root.resolvedStyle.height:0}");
            builder.AppendLine($"Labels: {visibleLabelCount}");
            builder.AppendLine($"Buttons: {visibleButtonCount}");
            builder.AppendLine($"Sliders: {visibleSliderCount}");
            builder.AppendLine($"Screenshot: {screenshotPath}");
            builder.AppendLine($"Summary: {summaryPath}");
            builder.AppendLine("Visible labels:");
            foreach (string label in visibleLabels)
            {
                builder.AppendLine("- " + TrimForSummary(label));
            }
            builder.AppendLine("Buttons:");
            foreach (string button in visibleButtons)
            {
                builder.AppendLine("- " + TrimForSummary(button));
            }
            return builder.ToString();
        }

        protected static bool IsElementVisibleForSummary(VisualElement element)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (!current.visible || current.resolvedStyle.display == DisplayStyle.None || current.resolvedStyle.visibility == Visibility.Hidden)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsSummaryLabelVisible(Label label)
        {
            return label != null && label.name != "CardDescriptionInfoButton" && IsElementVisibleForSummary(label);
        }

        private static string TrimForSummary(string text)
        {
            text = text.Replace("\r", " ").Replace("\n", " ").Trim();
            return text.Length <= 120 ? text : text.Substring(0, 117) + "...";
        }

        protected void AccentCard(VisualElement card, Color color)
        {
            card.style.borderLeftWidth = 4;
            card.style.borderLeftColor = color;
            card.style.borderTopColor = color;
            card.style.borderRightColor = WithAlpha(color, 0.35f);
            card.style.borderBottomColor = WithAlpha(color, 0.35f);
            card.style.borderTopWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderBottomWidth = 1;
        }

        protected void ApplyObjectTypeTint(VisualElement card, string objectType, Color accent)
        {
            ApplyObjectTypeTint(card, ResolveObjectTypeVisualStyle(objectType, accent));
        }

        protected void ApplyObjectTypeTint(VisualElement card, ObjectTypeVisualStyle style)
        {
            Color border = style.Border;
            card.style.backgroundColor = style.Tint;
            card.style.borderTopWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderTopColor = border;
            card.style.borderRightColor = border;
            card.style.borderBottomColor = WithAlpha(border, 0.28f);
            card.style.borderLeftColor = border;
        }

        protected void SetRounded(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        protected PanelSettings CreateRuntimePanelSettings()
        {
            runtimePanelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            runtimePanelSettings.name = "RuntimeDemoPanelSettings";
            runtimePanelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
            runtimePanelSettings.scale = 1.25f;
            runtimePanelSettings.referenceResolution = new Vector2Int(1280, 720);
            runtimePanelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            runtimePanelSettings.match = 0.5f;
            runtimeTextSettings = ScriptableObject.CreateInstance<PanelTextSettings>();
            runtimeTextSettings.name = "RuntimeDemoTextSettings";
            runtimePanelSettings.textSettings = runtimeTextSettings;
            return runtimePanelSettings;
        }

        protected void StyleText(TextElement element, int fontSize, Color color, bool bold = false)
        {
            element.style.fontSize = fontSize;
            element.style.color = color;
            element.style.unityFont = runtimeFont;
            element.style.unityFontStyleAndWeight = bold ? FontStyle.Bold : FontStyle.Normal;
        }
    }
}

