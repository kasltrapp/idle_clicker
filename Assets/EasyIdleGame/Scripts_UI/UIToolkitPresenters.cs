using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyIdleGame.UI.Toolkit
{
    public class UIToolkitListRowPresenter
    {
        public Metadata metadata;
        public string title;
        public string description;
        public string amountText;
        public string costText;
        public string lockText;
        public string effectText;
        public string rewardText;
        public string actionText;
        public Sprite icon;
        public bool isUnlocked = true;
        public bool canAct = true;
        public bool isMaxed;
        public bool isSelected;
        public float progress = -1f;

        public static UIToolkitListRowPresenter FromMetadata(Metadata metadata)
        {
            return new UIToolkitListRowPresenter
            {
                metadata = metadata,
                title = metadata?.name ?? string.Empty,
                description = metadata?.description ?? string.Empty,
                icon = metadata?.icon
            };
        }
    }

    public static class UIToolkitListRowBinder
    {
        public static void Bind(VisualElement root, UIToolkitListRowPresenter presenter)
        {
            if (root == null || presenter == null) return;

            SetText(root, "title", presenter.title);
            SetText(root, "name", presenter.title);
            SetText(root, "description", presenter.description);
            SetText(root, "amount", presenter.amountText);
            SetText(root, "cost", presenter.costText);
            SetText(root, "locks", presenter.lockText);
            SetText(root, "effects", presenter.effectText);
            SetText(root, "rewards", presenter.rewardText);
            SetText(root, "action", presenter.actionText);
            SetImage(root, "icon", presenter.icon);
            SetProgress(root, "progress-fill", presenter.progress);

            SetClass(root, "is-locked", !presenter.isUnlocked);
            SetClass(root, "can-act", presenter.canAct);
            SetClass(root, "is-maxed", presenter.isMaxed);
            SetClass(root, "is-selected", presenter.isSelected);

            SetButtonEnabled(root, "action-button", presenter.canAct);
            SetButtonEnabled(root, "buy-button", presenter.canAct);
        }

        public static void SetText(VisualElement root, string elementName, string value)
        {
            Label label = root.Q<Label>(elementName);
            if (label != null) label.text = value ?? string.Empty;
        }

        public static void SetImage(VisualElement root, string elementName, Sprite sprite)
        {
            VisualElement element = root.Q<VisualElement>(elementName);
            if (element == null) return;

            element.style.backgroundImage = sprite != null
                ? new StyleBackground(sprite)
                : new StyleBackground(StyleKeyword.None);
        }

        public static void SetProgress(VisualElement root, string elementName, float progress)
        {
            VisualElement element = root.Q<VisualElement>(elementName);
            if (element == null) return;

            if (progress < 0f)
            {
                element.style.display = DisplayStyle.None;
                return;
            }

            element.style.display = DisplayStyle.Flex;
            element.style.width = Length.Percent(Mathf.Clamp01(progress) * 100f);
        }

        public static void SetClass(VisualElement element, string className, bool enabled)
        {
            if (element == null) return;

            if (enabled)
            {
                if (!element.ClassListContains(className)) element.AddToClassList(className);
            }
            else
            {
                element.RemoveFromClassList(className);
            }
        }

        public static void SetButtonEnabled(VisualElement root, string elementName, bool enabled)
        {
            Button button = root.Q<Button>(elementName);
            if (button != null) button.SetEnabled(enabled);
        }
    }

    public static class UIToolkitBusinessPresenter
    {
        public static UIToolkitListRowPresenter Create(Business business, BusinessHolder holder = null, BigNumber buyAmount = default)
        {
            if (business == null) return new UIToolkitListRowPresenter { title = "Business", isUnlocked = false, canAct = false };

            Metadata metadata = holder != null ? business.GetEffectiveMetadata(holder) : business.GetEffectiveMetadata(null);
            UIToolkitListRowPresenter row = UIToolkitListRowPresenter.FromMetadata(metadata);
            row.amountText = holder != null ? holder.Amount.ToString(false) : "0";
            row.lockText = EconomyTextFormatter.LocksToString(business.Locks, false);
            row.costText = holder != null && buyAmount > 0
                ? EconomyTextFormatter.InputsToString(holder.iBuyable.GetRealCostToInputs(buyAmount), false)
                : EconomyTextFormatter.InputsToString(business.Cost, false);
            row.effectText = holder?.iProducable.GetActualProductionPerSecond().CustomToString() ?? string.Empty;
            row.progress = holder != null ? holder.GetProductionProgress() : -1f;
            row.isUnlocked = holder == null || holder.iUnlockable.IsUnlocked();
            row.canAct = holder != null && buyAmount > 0 && holder.iBuyable.CanBuy(buyAmount);
            row.isMaxed = holder != null && holder.GetRemainingBuyableAmountConstrain() <= 0;
            row.actionText = row.isMaxed ? "Max" : "Buy";
            return row;
        }

        public static void Bind(VisualElement root, Business business, BusinessHolder holder = null, BigNumber buyAmount = default)
        {
            UIToolkitListRowBinder.Bind(root, Create(business, holder, buyAmount));
        }
    }

    public static class UIToolkitUpgradePresenter
    {
        public static UIToolkitListRowPresenter Create(Upgrade upgrade, UpgradeHolder holder = null, BigNumber buyAmount = default)
        {
            if (upgrade == null) return new UIToolkitListRowPresenter { title = "Upgrade", isUnlocked = false, canAct = false };

            if (buyAmount <= 0) buyAmount = 1;
            UIToolkitListRowPresenter row = UIToolkitListRowPresenter.FromMetadata(upgrade.Metadata);
            row.amountText = holder != null ? holder.Amount.ToString(false) : "0";
            row.lockText = EconomyTextFormatter.LocksToString(upgrade.Locks, false);
            row.costText = holder != null
                ? EconomyTextFormatter.InputsToString(holder.iBuyable.GetRealCostToInputs(buyAmount), false)
                : EconomyTextFormatter.InputsToString(upgrade.Cost, false);
            row.effectText = EconomyTextFormatter.UpgradeBlocksToString(upgrade.upgrades);
            row.isUnlocked = holder == null || holder.iUnlockable.IsUnlocked();
            row.isMaxed = holder != null && holder.iBuyable.Maxed;
            row.canAct = holder != null && holder.iBuyable.CanBuy(buyAmount);
            row.actionText = row.isMaxed ? "Max" : "Buy";
            return row;
        }

        public static void Bind(VisualElement root, Upgrade upgrade, UpgradeHolder holder = null, BigNumber buyAmount = default)
        {
            UIToolkitListRowBinder.Bind(root, Create(upgrade, holder, buyAmount));
        }
    }

    public static class UIToolkitBoostPresenter
    {
        public static UIToolkitListRowPresenter Create(Boost boost, BoostHolder holder = null, BigNumber buyAmount = default)
        {
            if (boost == null) return new UIToolkitListRowPresenter { title = "Boost", isUnlocked = false, canAct = false };

            if (buyAmount <= 0) buyAmount = 1;
            UIToolkitListRowPresenter row = UIToolkitListRowPresenter.FromMetadata(boost.Metadata);
            row.amountText = holder != null ? holder.Amount.ToString(false) : "0";
            row.lockText = EconomyTextFormatter.LocksToString(boost.Locks, false);
            row.costText = holder != null
                ? EconomyTextFormatter.InputsToString(holder.iBuyable.GetRealCostToInputs(buyAmount), false)
                : EconomyTextFormatter.InputsToString(boost.Cost, false);
            row.isUnlocked = holder == null || holder.iUnlockable.IsUnlocked();
            row.canAct = holder != null && holder.iBuyable.CanBuy(buyAmount);
            row.actionText = "Buy";
            return row;
        }

        public static void Bind(VisualElement root, Boost boost, BoostHolder holder = null, BigNumber buyAmount = default)
        {
            UIToolkitListRowBinder.Bind(root, Create(boost, holder, buyAmount));
        }
    }

    public static class UIToolkitShopPresenter
    {
        public static UIToolkitListRowPresenter Create(ShopItem item, ShopItemHolder holder = null)
        {
            if (item == null) return new UIToolkitListRowPresenter { title = "Shop Item", isUnlocked = false, canAct = false };

            UIToolkitListRowPresenter row = UIToolkitListRowPresenter.FromMetadata(item.Metadata);
            row.amountText = holder != null ? holder.Amount.ToString(false) : "0";
            row.lockText = EconomyTextFormatter.LocksToString(item.Locks, false);
            row.costText = GetCostText(item, holder);
            row.rewardText = item.rewardTables != null ? item.rewardTables.GetAverageOutputs(1).CustomToString() : string.Empty;
            row.isUnlocked = holder == null || holder.iUnlockable.IsUnlocked();
            row.isMaxed = holder != null && holder.iBuyable.Maxed;
            row.canAct = holder != null && holder.CanBuy(1);
            row.actionText = row.isMaxed ? "Max" : "Buy";
            return row;
        }

        public static void Bind(VisualElement root, ShopItem item, ShopItemHolder holder = null)
        {
            UIToolkitListRowBinder.Bind(root, Create(item, holder));
        }

        private static string GetCostText(ShopItem item, ShopItemHolder holder)
        {
            if (item.buyWithAd) return "Ad";
            if (!string.IsNullOrEmpty(item.productId)) return item.productId;
            return holder != null
                ? EconomyTextFormatter.InputsToString(holder.iBuyable.GetRealCostToInputs(1), false)
                : EconomyTextFormatter.InputsToString(item.Cost, false);
        }
    }

    public static class UIToolkitPrestigePresenter
    {
        public static UIToolkitListRowPresenter Create(PrestigeManager prestigeManager)
        {
            if (prestigeManager == null) return new UIToolkitListRowPresenter { title = "Prestige", isUnlocked = false, canAct = false };

            bool canPrestige = prestigeManager.CanPrestige();
            return new UIToolkitListRowPresenter
            {
                title = $"Prestige {prestigeManager.prestiges.prestigeCount}",
                description = canPrestige ? "Ready" : "Locked",
                lockText = EconomyTextFormatter.LocksToString(prestigeManager.currentPrestigeRequirements, false),
                rewardText = prestigeManager.GetExpectedPrestigeRewards().CustomToString(),
                effectText = EconomyTextFormatter.UpgradeBlocksToString(prestigeManager.GetPrestigeMultipliers()),
                isUnlocked = canPrestige,
                canAct = canPrestige,
                actionText = "Prestige"
            };
        }

        public static void Bind(VisualElement root, PrestigeManager prestigeManager)
        {
            UIToolkitListRowBinder.Bind(root, Create(prestigeManager));
        }
    }
}
