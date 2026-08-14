using System.Collections.Generic;
using UnityEngine;
using EasyIdleGame;

namespace InfluencerRise.Burnout
{
    /// <summary>
    /// Ties the Burnout CustomStat into the gameplay systems the Easy Idle Game
    /// package cannot drive on its own. Six independent responsibilities:
    ///
    /// 1. Passive decay: Burnout slowly falls back toward 0 over real time, so a player
    ///    who stops actively producing isn't stuck at a high Burnout value forever.
    ///    This is the downward side. See Responsibility 5 below for the upward side.
    ///
    /// 2. Rebrand reset: when the player Rebrands (prestiges), Burnout snaps back to 0,
    ///    matching the "Reset on Rebrand? Yes, resets to 0" rule in EconomySchema.md's
    ///    Custom Stat table.
    ///
    /// 3. Spa Day shop grant: buying either Spa Day shop item (SpaDayBurnoutReliefAd or
    ///    SpaDayBurnoutReliefPaid) reduces Burnout by 40. This has to live in code because
    ///    a Shop Item's normal reward table can only grant currencies, businesses, boosts,
    ///    or managers - it has no field for changing a CustomStat like Burnout.
    ///
    /// 4. Wellness Guru passive decay boost: while the player owns TheWellnessGuru manager,
    ///    Burnout decays faster (shorter effective tick interval), scaling with the
    ///    manager's level. This exists because - same gap as responsibilities 1-3 - a
    ///    Manager's native passiveBoosts (List&lt;UpgradeBlock&gt;) has no CustomStat target;
    ///    UpgradeType only covers production/speed/cost/XP/duration/dropChance/dropWeight/
    ///    cooldown, confirmed via full read of UpgradeType.cs and UpgradeBlockFilter.cs.
    ///    TheWellnessGuru.asset is therefore created with an empty passiveBoosts list, and
    ///    its entire effect is expressed here instead. See GetEffectiveDecayIntervalSeconds
    ///    for the (placeholder) scaling curve.
    ///
    /// 5. Production-triggered rise: Burnout rises whenever a business finishes a
    ///    production round, scaled by that business's tier (1 = earliest/cheapest business
    ///    on a platform, 5 = capstone) so grinding higher-tier content feels like it costs
    ///    more. Hooks EasyIdleGame.BusinessesManager.OnProductionRoundFinishedEvent
    ///    (UnityEvent&lt;BusinessHolder, ProductionRound&gt;, confirmed via a full read of
    ///    BusinessHolder.OnProductionRoundFinished - it fires once per completed round,
    ///    per business, with the BusinessHolder as the first argument, so the actual
    ///    business identity is directly available). Chosen over
    ///    OnHolderProductionFinishedEvent/OnProductionFinishedEvent (same package, fire at
    ///    the same point but don't carry ProductionRound data this script doesn't need
    ///    anyway) and over EconomyChangedEvents (that one is a generic
    ///    "something save-worthy changed" signal with no business identity at all - fires
    ///    for every kind of economy mutation, not specifically production, confirmed
    ///    unsuitable by reading EconomyChangedEvents.cs directly). Business tier is looked
    ///    up from BusinessTierByName, a small static table keyed by asset name (see that
    ///    table's own comment for why a lookup table was chosen over inferring tier
    ///    programmatically). Fires independently of Responsibility 1's decay tick - a
    ///    business finishing production and the passive decay timer elapsing are
    ///    unrelated events that both happen to call ApplyBurnoutDelta, so a rise and a
    ///    decay can never silently cancel or override each other; whichever happens more
    ///    often simply dominates the net trend, which is the intended "opposing forces"
    ///    design per EconomySchema.md.
    ///
    /// 6. Therapy Session relief: buying TherapySessionUpgrade reduces Burnout by 15.
    ///    Same category-of-gap as everything else here - Upgrade.upgrades entries can't
    ///    target CustomStats (confirmed by the same UpgradeType.cs/UpgradeBlockFilter.cs
    ///    read that justified Responsibility 4), so TherapySessionUpgrade.asset is created
    ///    with an empty upgrades list and its entire effect lives here instead. Hooks
    ///    EasyIdleGame.UpgradesManager.OnUpgradeBought (UnityEvent&lt;Upgrade&gt;), confirmed via
    ///    a full read of UpgradesManager.cs: it's the same event IBuyableHolderManager.
    ///    TryBuyItems invokes as OnItemBought for every successful upgrade purchase (the
    ///    manager's own OnItemBought property literally redirects to it), carrying the
    ///    exact Upgrade asset bought - enough identity to ignore every other Upgrade
    ///    (BetterRingLight, FasterEditingSoftware, BulkContentBatching) and react only to
    ///    TherapySessionUpgrade specifically. TherapySessionUpgrade.upgradeDurationInSeconds
    ///    is 0 (instant), so this fires as part of the same purchase click, not a
    ///    delayed/pending completion.
    ///
    /// Every Burnout change goes through PlayerStats.IncrementStat (the same manager
    /// method the rest of the game uses for CustomStat changes - see CLAUDE.md Hard Rule
    /// #4) and is clamped to the 0-100 range stated in EconomySchema.md.
    /// </summary>
    [AddComponentMenu("InfluencerRise/Burnout Controller")]
    public class BurnoutController : MonoBehaviour
    {
        // --- Tunable constants (Phase 6 balancing pass - not final numbers) ---

        /// <summary>How much Burnout drops per decay tick. Placeholder, not yet balance-tested.</summary>
        private const float DecayAmountPerTick = 1f;

        /// <summary>How often (in real seconds) a decay tick happens. Placeholder, not yet balance-tested.</summary>
        private const float DecayIntervalSeconds = 10f;

        /// <summary>How much Burnout is reduced by either Spa Day shop item. Per EconomySchema.md.</summary>
        private const float SpaDayReliefAmount = 40f;

        /// <summary>Burnout gained per production round = business tier (1-5) x this. Placeholder, not yet balance-tested. Tier 1 = +0.5, tier 5 = +2.5 per event.</summary>
        private const float BurnoutRisePerTierMultiplier = 0.5f;

        /// <summary>How much Burnout is reduced by buying TherapySessionUpgrade. Per EconomySchema.md's original schema note. Placeholder, not yet balance-tested.</summary>
        private const float TherapySessionReliefAmount = 15f;

        // --- Fixed range, per EconomySchema.md's Custom Stat table (0-100) ---
        private const float BurnoutMin = 0f;
        private const float BurnoutMax = 100f;

        /// <summary>
        /// Business tier (1 = platform entry point, 5 = capstone) keyed by asset name, per
        /// EconomySchema.md's Businesses tables. A lookup table was chosen over inferring
        /// tier programmatically (e.g. by walking each Business's own Lock chain back to
        /// its platform's entry point, or bucketing by purchase cost) because the 15-business
        /// roster and its 5-tier-per-platform structure are explicitly documented as
        /// LOCKED ("the structure is locked, the numbers are not" - EconomySchema.md's
        /// opening line): a static table is trivially auditable against that source of
        /// truth and cannot silently mis-tier a business if a Lock's shape ever changes,
        /// where a graph-walk would depend on every Business's locks staying in exactly
        /// the shape it happens to be in today. This is a deliberate simplicity-over-
        /// generality tradeoff, not an oversight - flagged per CLAUDE.md Hard Rule #11.
        /// Doesn't need Inspector wiring: business identity comes from the event itself
        /// (see Responsibility 5), this table is just int lookups by name.
        /// </summary>
        private static readonly Dictionary<string, int> BusinessTierByName = new Dictionary<string, int>
        {
            { "FirstPost", 1 }, { "SelfieSession", 2 }, { "StoryFeed", 3 }, { "EngagementFarming", 4 }, { "VerifiedStatus", 5 },
            { "TrendChase", 1 }, { "CollabFarming", 2 }, { "ReactionContent", 3 }, { "AlgorithmBaiting", 4 }, { "ViralChallenge", 5 },
            { "HotTake", 1 }, { "UnboxingHaul", 2 }, { "Streaming", 3 }, { "SponsorDeals", 4 }, { "OwnBrand", 5 },
        };

        [Tooltip("The Burnout CustomStat asset this controller manages. Assign the existing Burnout.asset here.")]
        [SerializeField] private CustomStat burnoutStat;

        [Tooltip("The ad-based Spa Day shop item. Purchasing it reduces Burnout by 40.")]
        [SerializeField] private ShopItem spaDayBurnoutReliefAd;

        [Tooltip("The real-money Spa Day shop item. Purchasing it reduces Burnout by 40.")]
        [SerializeField] private ShopItem spaDayBurnoutReliefPaid;

        [Tooltip("The Wellness Guru manager. While owned, Responsibility 4 shortens the passive Burnout decay interval, scaling with its level. Leave unassigned to disable this responsibility entirely.")]
        [SerializeField] private Manager wellnessGuru;

        [Tooltip("The Therapy Session upgrade. Buying it reduces Burnout by 15. Leave unassigned to disable this responsibility entirely.")]
        [SerializeField] private Upgrade therapySessionUpgrade;

        private float _secondsSinceLastDecayTick;

        private void OnEnable()
        {
            if (PrestigeManager.Instance != null)
                PrestigeManager.Instance.OnPrestige.AddListener(HandleRebrand);

            if (ShopManager.Instance != null)
                ShopManager.Instance.OnShopItemRewardsGranted.AddListener(HandleShopItemRewardsGranted);

            if (BusinessesManager.Instance != null)
                BusinessesManager.Instance.OnProductionRoundFinishedEvent.AddListener(HandleProductionRoundFinished);

            if (UpgradesManager.Instance != null)
                UpgradesManager.Instance.OnUpgradeBought.AddListener(HandleUpgradeBought);
        }

        private void OnDisable()
        {
            if (PrestigeManager.Instance != null)
                PrestigeManager.Instance.OnPrestige.RemoveListener(HandleRebrand);

            if (ShopManager.Instance != null)
                ShopManager.Instance.OnShopItemRewardsGranted.RemoveListener(HandleShopItemRewardsGranted);

            if (BusinessesManager.Instance != null)
                BusinessesManager.Instance.OnProductionRoundFinishedEvent.RemoveListener(HandleProductionRoundFinished);

            if (UpgradesManager.Instance != null)
                UpgradesManager.Instance.OnUpgradeBought.RemoveListener(HandleUpgradeBought);
        }

        /// <summary>
        /// Responsibility 1: ticks passive Burnout decay over real time. The tick interval
        /// is normally the fixed DecayIntervalSeconds constant; Responsibility 4
        /// (GetEffectiveDecayIntervalSeconds) shortens it while Wellness Guru is owned.
        /// With no Wellness Guru owned, this behaves identically to before that manager
        /// existed - the effective interval is exactly DecayIntervalSeconds.
        /// </summary>
        private void Update()
        {
            if (burnoutStat == null || PlayerStats.Instance == null) return;

            _secondsSinceLastDecayTick += Time.deltaTime;
            float effectiveInterval = GetEffectiveDecayIntervalSeconds();
            if (_secondsSinceLastDecayTick < effectiveInterval) return;

            _secondsSinceLastDecayTick -= effectiveInterval;
            ApplyBurnoutDelta(-DecayAmountPerTick);
        }

        /// <summary>
        /// Responsibility 4: while TheWellnessGuru is owned, returns a shorter decay
        /// interval than the base DecayIntervalSeconds so Burnout falls away faster, scaling
        /// with the manager's level. PLACEHOLDER CURVE, not balance-tested: interval is
        /// divided by (1 + level), so level 1 halves the interval, level 2 divides it by 3,
        /// level 3 by 4, and so on. Returns the unmodified DecayIntervalSeconds if
        /// wellnessGuru is unassigned or not yet owned (level &lt;= 0), so this responsibility
        /// is a no-op until the manager is actually acquired.
        /// </summary>
        private float GetEffectiveDecayIntervalSeconds()
        {
            if (wellnessGuru == null || ManagersManager.Instance == null) return DecayIntervalSeconds;

            ManagerHolder holder = ManagersManager.Instance.GetHolder(wellnessGuru);
            if (holder == null || holder.level <= 0) return DecayIntervalSeconds;

            return DecayIntervalSeconds / (1f + holder.level);
        }

        /// <summary>Responsibility 2: resets Burnout to exactly 0 when the player Rebrands.</summary>
        private void HandleRebrand()
        {
            if (burnoutStat == null || PlayerStats.Instance == null) return;

            BigNumber current = PlayerStats.Instance.GetStat(burnoutStat);
            if (current != 0)
                PlayerStats.Instance.IncrementStat(burnoutStat, -current);
        }

        /// <summary>Responsibility 3: reduces Burnout by 40 when a Spa Day shop item is purchased.</summary>
        private void HandleShopItemRewardsGranted(ShopItem item, SourceType sourceType, List<Output> outputs)
        {
            if (burnoutStat == null || PlayerStats.Instance == null) return;
            if (item == null) return;
            if (item != spaDayBurnoutReliefAd && item != spaDayBurnoutReliefPaid) return;

            ApplyBurnoutDelta(-SpaDayReliefAmount);
        }

        /// <summary>
        /// Responsibility 5: raises Burnout whenever a business finishes a production
        /// round, scaled by that business's tier via BusinessTierByName. A business whose
        /// name isn't in the table (shouldn't happen for the 15 documented businesses, but
        /// silently no-ops rather than throwing if content changes) contributes nothing.
        /// </summary>
        private void HandleProductionRoundFinished(BusinessHolder holder, ProductionRound round)
        {
            if (burnoutStat == null || PlayerStats.Instance == null) return;
            if (holder?.business == null) return;
            if (!BusinessTierByName.TryGetValue(holder.business.name, out int tier)) return;

            ApplyBurnoutDelta(tier * BurnoutRisePerTierMultiplier);
        }

        /// <summary>Responsibility 6: reduces Burnout by 15 when TherapySessionUpgrade is bought.</summary>
        private void HandleUpgradeBought(Upgrade upgrade)
        {
            if (burnoutStat == null || PlayerStats.Instance == null) return;
            if (upgrade == null || upgrade != therapySessionUpgrade) return;

            ApplyBurnoutDelta(-TherapySessionReliefAmount);
        }

        /// <summary>Applies a Burnout change through PlayerStats.IncrementStat, clamped to [0, 100].</summary>
        private void ApplyBurnoutDelta(float delta)
        {
            BigNumber current = PlayerStats.Instance.GetStat(burnoutStat);
            BigNumber target = current + delta;

            if (target < BurnoutMin) target = BurnoutMin;
            if (target > BurnoutMax) target = BurnoutMax;

            BigNumber actualDelta = target - current;
            if (actualDelta != 0)
                PlayerStats.Instance.IncrementStat(burnoutStat, actualDelta);
        }
    }
}
