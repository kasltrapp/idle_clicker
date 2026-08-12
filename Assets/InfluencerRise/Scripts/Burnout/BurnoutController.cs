using System.Collections.Generic;
using UnityEngine;
using EasyIdleGame;

namespace InfluencerRise.Burnout
{
    /// <summary>
    /// Ties the Burnout CustomStat into the two gameplay systems the Easy Idle Game
    /// package cannot drive on its own. Three independent responsibilities:
    ///
    /// 1. Passive decay: Burnout slowly falls back toward 0 over real time, so a player
    ///    who stops actively producing isn't stuck at a high Burnout value forever.
    ///    NOTE: this script only implements the downward (decay) side. The upward side
    ///    ("Burnout rises with active production", per EconomySchema.md) is NOT wired up
    ///    here yet - no rise rate has been specified, and doing so would mean listening
    ///    to business production-finished events, which is a separate future task.
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

        // --- Fixed range, per EconomySchema.md's Custom Stat table (0-100) ---
        private const float BurnoutMin = 0f;
        private const float BurnoutMax = 100f;

        [Tooltip("The Burnout CustomStat asset this controller manages. Assign the existing Burnout.asset here.")]
        [SerializeField] private CustomStat burnoutStat;

        [Tooltip("The ad-based Spa Day shop item. Purchasing it reduces Burnout by 40.")]
        [SerializeField] private ShopItem spaDayBurnoutReliefAd;

        [Tooltip("The real-money Spa Day shop item. Purchasing it reduces Burnout by 40.")]
        [SerializeField] private ShopItem spaDayBurnoutReliefPaid;

        private float _secondsSinceLastDecayTick;

        private void OnEnable()
        {
            if (PrestigeManager.Instance != null)
                PrestigeManager.Instance.OnPrestige.AddListener(HandleRebrand);

            if (ShopManager.Instance != null)
                ShopManager.Instance.OnShopItemRewardsGranted.AddListener(HandleShopItemRewardsGranted);
        }

        private void OnDisable()
        {
            if (PrestigeManager.Instance != null)
                PrestigeManager.Instance.OnPrestige.RemoveListener(HandleRebrand);

            if (ShopManager.Instance != null)
                ShopManager.Instance.OnShopItemRewardsGranted.RemoveListener(HandleShopItemRewardsGranted);
        }

        /// <summary>Responsibility 1: ticks passive Burnout decay over real time.</summary>
        private void Update()
        {
            if (burnoutStat == null || PlayerStats.Instance == null) return;

            _secondsSinceLastDecayTick += Time.deltaTime;
            if (_secondsSinceLastDecayTick < DecayIntervalSeconds) return;

            _secondsSinceLastDecayTick -= DecayIntervalSeconds;
            ApplyBurnoutDelta(-DecayAmountPerTick);
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
