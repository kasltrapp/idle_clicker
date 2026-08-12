using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace EasyIdleGame.Demos
{
    /// <summary>
    /// Runs a tiny "Neon Arcade" with three objects picked for event variety - a manual business,
    /// a timed boost, and an automation manager - and pins a row of listener chips above the
    /// scrolling dashboard. Nothing auto-produces out of the box, so events follow user actions:
    /// start and claim rounds, buy machines, unlock and use the boost, hire the technician. Each
    /// chip is one AddListener subscription; the moment its event fires the chip flashes and the
    /// payload lands in the mini feed. One chip per distinct action - granular twin members
    /// (holder variants, amount-added variants) are covered in the scripting reference.
    /// </summary>
    public class EventListenerShowcaseDemoController : DemoSceneController
    {
        private const float FlashDuration = 0.9f;
        private const int FeedVisibleLines = 2;
        private const int FeedStoredLines = 20;
        private const int PayloadMaxLength = 96;

        private class EventChannel
        {
            public string EventName;
            public string DisplayName;
            public Color Accent;
            public int FireCount;
            public float LastFireTime = float.NegativeInfinity;
        }

        private struct FeedEntry
        {
            public int Sequence;
            public float Time;
            public string Text;
            public Color Accent;
        }

        private readonly List<EventChannel> channels = new List<EventChannel>();
        private readonly List<FeedEntry> feed = new List<FeedEntry>();
        private readonly List<Action> unsubscribers = new List<Action>();
        private readonly List<Action> flashRefreshers = new List<Action>();
        private VisualElement chipWallHost;
        private bool listenersRegistered;
        private int totalFireCount;

        private static readonly Color BusinessAccent = new Color(0.46f, 0.86f, 0.48f, 1f);
        private static readonly Color CurrencyAccent = new Color(0.32f, 0.68f, 1f, 1f);
        private static readonly Color BoostAccent = new Color(0.73f, 0.56f, 1f, 1f);
        private static readonly Color ManagerAccent = new Color(1f, 0.55f, 0.28f, 1f);

        protected new void Update()
        {
            base.Update();

            for (int i = 0; i < flashRefreshers.Count; i++)
            {
                flashRefreshers[i]?.Invoke();
            }
        }

        protected new void OnDestroy()
        {
            foreach (Action unsubscribe in unsubscribers)
            {
                unsubscribe?.Invoke();
            }

            unsubscribers.Clear();
            listenersRegistered = false;
            base.OnDestroy();
        }

        protected override void SeedDemoState()
        {
            ResetChannelStats();
            RegisterShowcaseListeners();

            AddCurrency("Credits", 50);
            GrantBusiness("ClawMachine", 1);
            StartAllOwnedBusinessCopies("ClawMachine");
        }

        protected override void BuildSceneDashboard()
        {
            flashRefreshers.Clear();

            BuildPinnedListenerWall();
            AddArcadeFloorPanel();
        }

        /// <summary>
        /// The chip wall and the mini event feed live next to the scroll view, not inside it, so
        /// they stay on screen while the dashboard content scrolls.
        /// </summary>
        private void BuildPinnedListenerWall()
        {
            if (dashboard?.parent == null) return;

            chipWallHost?.RemoveFromHierarchy();
            chipWallHost = new VisualElement();
            chipWallHost.style.flexShrink = 0;
            chipWallHost.style.paddingBottom = 3;
            chipWallHost.style.marginBottom = 5;
            chipWallHost.style.borderBottomWidth = 1;
            chipWallHost.style.borderBottomColor = WithAlpha(MutedTextColor, 0.34f);

            VisualElement chipRow = new VisualElement();
            chipRow.style.flexDirection = FlexDirection.Row;
            chipRow.style.flexWrap = Wrap.Wrap;
            chipWallHost.Add(chipRow);

            foreach (EventChannel channel in channels)
            {
                AddListenerChip(chipRow, channel);
            }

            List<Label> slots = new List<Label>();
            for (int i = 0; i < FeedVisibleLines; i++)
            {
                Label slot = new Label(string.Empty);
                StyleText(slot, 10, TextColor);
                slot.style.display = DisplayStyle.None;
                slot.style.marginTop = 1;
                chipWallHost.Add(slot);
                slots.Add(slot);
            }

            flashRefreshers.Add(() =>
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (i >= feed.Count)
                    {
                        slots[i].style.display = DisplayStyle.None;
                        continue;
                    }

                    FeedEntry entry = feed[i];
                    float age = Time.unscaledTime - entry.Time;
                    float alpha = Mathf.Lerp(1f, 0.3f, Mathf.Clamp01(age / 10f));
                    slots[i].text = $"#{entry.Sequence}  {entry.Text}";
                    slots[i].style.color = WithAlpha(entry.Accent, alpha);
                    slots[i].style.unityFontStyleAndWeight = i == 0 ? FontStyle.Bold : FontStyle.Normal;
                    slots[i].style.display = DisplayStyle.Flex;
                }
            });

            VisualElement body = dashboard.parent;
            body.Insert(body.IndexOf(dashboard), chipWallHost);
        }

        private void AddArcadeFloorPanel()
        {
            VisualElement grid = CreateSection("Neon Arcade");

            Business business = Load<Business>("ClawMachine");
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            VisualElement businessCard = CreateObjectCard(business.businessName, "Business", BusinessAccent, configurationObject: business);
            businessCard.style.minHeight = 0;
            AddLiveKeyValue(businessCard, "Owned", () => holder.amount.ToString(), () => holder.amount > 0 ? PositiveColor : WarningColor);
            AddLiveKeyValue(businessCard, "Active", () => holder.ActiveProductions.Count == 0 ? "idle" : $"{holder.CurrentlyProducing} producing", () => holder.ActiveProductions.Count == 0 ? InactiveColor : AccentColor);
            AddLiveProgress(businessCard, holder.GetProductionProgress);
            AddCardAction(businessCard, "Buy +1", () => BuyBusiness("ClawMachine", 1), () => CanBuyBusinessAmount("ClawMachine", 1), false, () => DescribeBusinessBuyAvailability("ClawMachine", 1), false);
            AddCardAction(businessCard, "Start all", () => StartAllOwnedBusinessCopies("ClawMachine"), () => CanStartAnyBusinessProduction("ClawMachine"), false, () => DescribeStartAllProductionAvailability("ClawMachine"), false);
            AddCardAction(businessCard, "Claim", () => ClaimBusinessOutputs("ClawMachine"), () => HasStoredBusinessOutputs("ClawMachine"), false, () => DescribeClaimAvailability("ClawMachine"), false);
            grid.Add(businessCard);

            Boost hypeBoost = Load<Boost>("HypeBoost");
            BoostHolder boostHolder = BoostsManager.Instance.GetOrAddHolder(hypeBoost);
            VisualElement boostCard = CreateObjectCard("Hype Boost", "Boost", BoostAccent, configurationObject: hypeBoost);
            boostCard.style.minHeight = 0;
            AddKeyValue(boostCard, "Effect", "x2 production for 20s - watch Boosts Changed fire again on its own when it expires");
            AddLiveKeyValue(boostCard, "State", () => $"owned {boostHolder.amount}, cost 25 Credits, {BoostsManager.Instance.activeBoosts.Count} active");
            AddCardAction(boostCard, "Buy", () => BuyBoost(hypeBoost), () => CanBuyBoost(hypeBoost), false, () => "not enough Credits", false);
            AddCardAction(boostCard, "Use", () => UseBoost(hypeBoost), () => CanUseBoost(hypeBoost), false, () => "none in inventory; buy one first", false);
            grid.Add(boostCard);

            Manager technician = Load<Manager>("ArcadeTechnician");
            ManagerHolder technicianHolder = ManagersManager.Instance.GetOrAddHolder(technician);
            VisualElement managerCard = CreateObjectCard("Arcade Technician", "Manager", ManagerAccent, configurationObject: technician);
            managerCard.style.minHeight = 0;
            AddKeyValue(managerCard, "Role", "hire once to auto start the Claw Machine, twice to auto collect - business events then fire hands-free");
            AddLiveKeyValue(managerCard, "Level", () => $"L{technicianHolder.level:0} - cost {DescribeBuyCost(technicianHolder.iBuyable, 1)}", () => technicianHolder.level > 0 ? PositiveColor : TextColor);
            AddCardAction(managerCard, "Hire", () => BuyAutomationManager(technician), () => CanBuyManager(technician), false, () => DescribeManagerBuyAvailability(technician), false);
            grid.Add(managerCard);
        }

        private bool CanBuyBoost(Boost boost)
        {
            return CanBuyAmount(BoostsManager.Instance.GetOrAddHolder(boost).iBuyable, 1);
        }

        private void BuyBoost(Boost boost)
        {
            bool bought = BoostsManager.Instance.iBuyable.TryBuyItems(boost, 1);
            LastBoostResult = $"TryBuyItems {boost.boostName}: {bought}.";
            Log(LastBoostResult);
        }

        private void AddListenerChip(VisualElement parent, EventChannel channel)
        {
            Label chip = new Label(channel.DisplayName);
            StyleText(chip, 10, MutedTextColor);
            chip.style.paddingLeft = 4;
            chip.style.paddingRight = 4;
            chip.style.paddingTop = 2;
            chip.style.paddingBottom = 2;
            chip.style.marginRight = 2;
            chip.style.marginBottom = 2;
            chip.style.flexGrow = 0;
            chip.style.flexShrink = 0;
            SetRounded(chip, 4);

            ObjectTypeVisualStyle style = new ObjectTypeVisualStyle(channel.Accent);
            Color baseTint = style.Tint;
            Color baseBorder = style.Border;
            Color flashTint = Color.Lerp(baseTint, channel.Accent, 0.6f);
            chip.style.borderTopWidth = 1;
            chip.style.borderRightWidth = 1;
            chip.style.borderBottomWidth = 1;
            chip.style.borderLeftWidth = 1;

            flashRefreshers.Add(() =>
            {
                float age = Time.unscaledTime - channel.LastFireTime;
                float intensity = float.IsInfinity(age) ? 0f : Mathf.Clamp01(1f - age / FlashDuration);
                intensity *= intensity;

                chip.style.color = channel.FireCount > 0 ? Color.Lerp(channel.Accent, Color.white, intensity) : MutedTextColor;
                chip.style.backgroundColor = Color.Lerp(baseTint, flashTint, intensity);
                Color border = Color.Lerp(baseBorder, channel.Accent, intensity);
                chip.style.borderTopColor = border;
                chip.style.borderRightColor = border;
                chip.style.borderBottomColor = border;
                chip.style.borderLeftColor = border;
            });

            parent.Add(chip);
        }

        private void ResetChannelStats()
        {
            foreach (EventChannel channel in channels)
            {
                channel.FireCount = 0;
                channel.LastFireTime = float.NegativeInfinity;
            }

            feed.Clear();
            totalFireCount = 0;
        }

        private void Record(EventChannel channel, string payload)
        {
            payload = string.IsNullOrEmpty(payload) ? "(no payload)" : payload;
            if (payload.Length > PayloadMaxLength)
            {
                payload = payload.Substring(0, PayloadMaxLength - 3) + "...";
            }

            channel.FireCount++;
            channel.LastFireTime = Time.unscaledTime;
            totalFireCount++;

            feed.Insert(0, new FeedEntry
            {
                Sequence = totalFireCount,
                Time = Time.unscaledTime,
                Text = $"{channel.EventName}: {payload}",
                Accent = channel.Accent
            });
            while (feed.Count > FeedStoredLines)
            {
                feed.RemoveAt(feed.Count - 1);
            }
        }

        private EventChannel Channel(Color accent, string eventName, string displayName)
        {
            EventChannel channel = new EventChannel
            {
                Accent = accent,
                EventName = eventName,
                DisplayName = displayName
            };
            channels.Add(channel);
            return channel;
        }

        private void Listen(UnityEvent unityEvent, EventChannel channel, Func<string> describePayload)
        {
            UnityAction handler = () => Record(channel, describePayload());
            unityEvent.AddListener(handler);
            unsubscribers.Add(() => unityEvent.RemoveListener(handler));
        }

        private void Listen<T0>(UnityEvent<T0> unityEvent, EventChannel channel, Func<T0, string> describePayload)
        {
            UnityAction<T0> handler = arg0 => Record(channel, describePayload(arg0));
            unityEvent.AddListener(handler);
            unsubscribers.Add(() => unityEvent.RemoveListener(handler));
        }

        private static string NameOf(BusinessHolder holder)
        {
            return holder != null && holder.business ? holder.business.businessName : "unknown business";
        }

        private static string NameOf(Business business)
        {
            return business ? business.businessName : "unknown business";
        }

        /// <summary>
        /// One chip per distinct action so simultaneous flashes stay meaningful (for example a
        /// purchase lights Business Bought and Currency Changed together, because buying costs
        /// money). Boosts Changed fires once when a boost starts and again ~20s later when it
        /// expires on its own.
        /// </summary>
        private void RegisterShowcaseListeners()
        {
            if (listenersRegistered) return;

            listenersRegistered = true;

            BusinessesManager businesses = BusinessesManager.Instance;
            Listen(businesses.OnHolderProductionStartedEvent,
                Channel(BusinessAccent, "OnHolderProductionStartedEvent", "Production Start"),
                holder => $"{NameOf(holder)} started producing");
            Listen(businesses.OnProductionFinishedEvent,
                Channel(BusinessAccent, "OnProductionFinishedEvent", "Production Finish"),
                holder => $"{NameOf(holder)} finished producing");
            Listen(businesses.OnBusinessBought,
                Channel(BusinessAccent, "OnBusinessBought", "Business Bought"),
                business => $"bought {NameOf(business)}");

            Listen(CurrencyManager.Instance.OnCurrencyAmountChanged,
                Channel(CurrencyAccent, "OnCurrencyAmountChanged", "Currency Changed"),
                currency => $"{currency?.currencyName} is now {CurrencyManager.Instance.GetCurrencyAmount(currency)}");

            BoostsManager boosts = BoostsManager.Instance;
            Listen(boosts.OnBoostBought,
                Channel(BoostAccent, "OnBoostBought", "Boost Bought"),
                boost => $"bought {boost?.boostName}");
            Listen(boosts.OnBoostUsed,
                Channel(BoostAccent, "OnBoostUsed", "Boost Used"),
                boost => $"used {boost?.boostName}");
            Listen(boosts.OnActiveBoostsChanged,
                Channel(BoostAccent, "OnActiveBoostsChanged", "Boosts Changed"),
                () => $"{BoostsManager.Instance.activeBoosts.Count} boost(s) now active");

            Listen(ManagersManager.Instance.OnManagerBought,
                Channel(ManagerAccent, "OnManagerBought", "Manager Hired"),
                manager => $"hired {manager?.managerName}");
        }
    }
}
