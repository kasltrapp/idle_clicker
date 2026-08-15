using System.Collections.Generic;
using UnityEngine;
using EasyIdleGame;

namespace InfluencerRise.PlayerProgression
{
    /// <summary>
    /// Grants a small amount of Player XP whenever a business finishes a production round -
    /// a second XP source alongside the native purchase-only one (Business.
    /// xpsAddPerPurchaseToPlayer, confirmed via full-codebase investigation to be the ONLY
    /// native player-XP source; production round completion grants none on its own,
    /// Output.cs has exactly 4 grant fields and none of them is XP). Closes a real design
    /// gap independent of any numeric tuning: without this, AFK/idle play - which never
    /// manually triggers production and earns almost entirely via owned Managers'
    /// automation - has essentially no path to earning Player XP at all, since it only
    /// touches the purchase-XP source on the rare occasions its hourly reinvestment
    /// checkpoint affords a new unit. Confirmed via EconomySimulator.cs (see
    /// docs/BugTracker.md for the full investigation and the iteration log this constant
    /// was tuned against alongside PlayerStats.startXp/incrementMultiplier).
    ///
    /// Hooks EasyIdleGame.BusinessesManager.OnProductionRoundFinishedEvent
    /// (UnityEvent&lt;BusinessHolder, ProductionRound&gt;) - the exact same event and
    /// subscribe/unsubscribe pattern BurnoutController.cs Responsibility 5 already uses for
    /// tier-scaled Burnout rise (see that script's own header for the full rationale on why
    /// this event was chosen over OnHolderProductionFinishedEvent/OnProductionFinishedEvent
    /// or EconomyChangedEvents - it fires once per completed round, per business, with the
    /// BusinessHolder as the first argument, confirmed via a full read of
    /// BusinessHolder.OnProductionRoundFinished). Grants XP via PlayerStats.AddXp - the same
    /// manager method every other XP grant in this project uses (CLAUDE.md Hard Rule #4),
    /// so it correctly interacts with the existing Growth Hacker XP top-up
    /// (RareLegendaryManagerEffects.cs Responsibility 1, which listens to
    /// PlayerStats.OnXpAdded and reacts to any AddXp call regardless of source) with no
    /// special-casing needed.
    ///
    /// Tier-scaled using the same BusinessTierByName lookup-table pattern as
    /// BurnoutController.cs Responsibility 5 (duplicated here rather than shared, matching
    /// this project's established one-responsibility-per-script isolation - the table
    /// itself is small and keyed off the 15-business roster EconomySchema.md documents as
    /// structurally LOCKED, so duplication carries negligible drift risk).
    /// </summary>
    [AddComponentMenu("InfluencerRise/Production Xp Grant")]
    public class ProductionXpGrant : MonoBehaviour
    {
        /// <summary>Player XP granted per production round = business tier (1-5) x this. Tuned jointly with PlayerStats.startXp/incrementMultiplier via EconomySimulator.cs - see docs/BugTracker.md for the iteration log.</summary>
        private const float XpPerTierPerRound = 0.5f;

        private static readonly Dictionary<string, int> BusinessTierByName = new Dictionary<string, int>
        {
            { "FirstPost", 1 }, { "SelfieSession", 2 }, { "StoryFeed", 3 }, { "EngagementFarming", 4 }, { "VerifiedStatus", 5 },
            { "TrendChase", 1 }, { "CollabFarming", 2 }, { "ReactionContent", 3 }, { "AlgorithmBaiting", 4 }, { "ViralChallenge", 5 },
            { "HotTake", 1 }, { "UnboxingHaul", 2 }, { "Streaming", 3 }, { "SponsorDeals", 4 }, { "OwnBrand", 5 },
        };

        private void OnEnable()
        {
            if (BusinessesManager.Instance != null)
                BusinessesManager.Instance.OnProductionRoundFinishedEvent.AddListener(HandleProductionRoundFinished);
        }

        private void OnDisable()
        {
            if (BusinessesManager.Instance != null)
                BusinessesManager.Instance.OnProductionRoundFinishedEvent.RemoveListener(HandleProductionRoundFinished);
        }

        private void HandleProductionRoundFinished(BusinessHolder holder, ProductionRound round)
        {
            if (PlayerStats.Instance == null) return;
            if (holder?.business == null) return;
            if (!BusinessTierByName.TryGetValue(holder.business.name, out int tier)) return;

            BigNumber xp = (BigNumber)(tier * XpPerTierPerRound);
            if (xp <= 0) return;

            PlayerStats.Instance.AddXp(xp);
        }
    }
}
