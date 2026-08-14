using System;
using System.Globalization;
using UnityEngine;
using EasyIdleGame;

namespace InfluencerRise.Shop
{
    /// <summary>
    /// Adds a recurring cooldown to FreeCapsule, which EasyIdleGame's ShopItem cannot do on
    /// its own - ShopItem.cs only has maxPurchases (a lifetime cap), no repeating-interval
    /// field (confirmed via full class read, see EngineCapabilities.md). Without this
    /// script, FreeCapsule is infinitely re-claimable with no wait at all.
    ///
    /// WHY THIS DOESN'T HOOK ShopManager's purchase events: FreeCapsule has no ad flag, no
    /// productId, and no inGameCost, so ShopManager.AttemptPurchase falls through straight
    /// to TryBuyItems with no cancellable "attempt" event in between (unlike the ad/IAP
    /// paths, which do expose OnAttemptAdPurchase/OnAttemptRealMoneyPurchase for exactly
    /// this kind of external gate). By the time TryBuyItems grants a manager there is no
    /// clean way to undo it. So instead of reacting to a purchase, this script OWNS the
    /// entry point: any "Claim Free Capsule" UI must call TryClaimFreeCapsule() below
    /// instead of calling ShopManager directly, the same way the ad/IAP flows require going
    /// through their own confirmation step before CompleteExternalPurchase.
    ///
    /// PERSISTENCE: the last-claimed time is stored in PlayerPrefs, not in SaveFile.cs.
    /// SaveFile.cs lives under Assets/EasyIdleGame/Scripts/SaveAndLoad/ (the native
    /// package), and CLAUDE.md Hard Rule #7 requires all new work to live under
    /// Assets/InfluencerRise/ - editing a native package file would violate that boundary
    /// and risks being overwritten by a future package update. PlayerPrefs keeps this
    /// entirely self-contained while still surviving app close/reopen.
    /// </summary>
    [AddComponentMenu("InfluencerRise/Free Capsule Cooldown")]
    public class FreeCapsuleCooldown : MonoBehaviour
    {
        /// <summary>
        /// How long a player must wait between FreeCapsule claims, in hours. PLACEHOLDER,
        /// not balance-tested - matches the "Next Ready In: 3h45m" reference pattern from
        /// AdVenture Communist noted during earlier research. Revisit in the Phase 6
        /// balancing pass (see docs/Roadmap.md).
        /// </summary>
        private const float CooldownHours = 4f;

        private const string LastClaimUtcTicksPrefKey = "InfluencerRise.FreeCapsuleCooldown.LastClaimUtcTicks";

        [Tooltip("The FreeCapsule shop item this cooldown gate applies to. Assign the existing FreeCapsule.asset here.")]
        [SerializeField] private ShopItem freeCapsule;

        private void OnEnable()
        {
            if (ShopManager.Instance != null)
                ShopManager.Instance.OnShopItemRewardsGranted.AddListener(HandleShopItemRewardsGranted);
        }

        private void OnDisable()
        {
            if (ShopManager.Instance != null)
                ShopManager.Instance.OnShopItemRewardsGranted.RemoveListener(HandleShopItemRewardsGranted);
        }

        /// <summary>
        /// The single entry point a "Claim Free Capsule" button should call. Returns false
        /// and grants nothing if the cooldown hasn't elapsed yet. Returns true and completes
        /// the FreeCapsule purchase if it has - the resulting OnShopItemRewardsGranted event
        /// (handled below) records the new claim timestamp, so callers don't need to.
        /// </summary>
        public bool TryClaimFreeCapsule()
        {
            if (freeCapsule == null || ShopManager.Instance == null) return false;
            if (GetTimeRemaining() > TimeSpan.Zero) return false;

            ShopManager.Instance.CompleteExternalPurchase(freeCapsule, SourceType.Purchase);
            return true;
        }

        /// <summary>True if the cooldown has fully elapsed and TryClaimFreeCapsule would succeed right now.</summary>
        public bool CanClaim => GetTimeRemaining() <= TimeSpan.Zero;

        /// <summary>
        /// How much longer until the next FreeCapsule claim is allowed. Returns TimeSpan.Zero
        /// (not negative) once the cooldown has elapsed, and immediately if FreeCapsule has
        /// never been claimed yet. A future UI can format this directly into something like
        /// "Next Ready In: Xh Ym" without knowing anything about how the cooldown is stored.
        /// </summary>
        public TimeSpan GetTimeRemaining()
        {
            if (!TryGetLastClaimUtc(out DateTime lastClaimUtc)) return TimeSpan.Zero;

            DateTime nextReadyUtc = lastClaimUtc + TimeSpan.FromHours(CooldownHours);
            TimeSpan remaining = nextReadyUtc - DateTime.UtcNow;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        /// <summary>Records a fresh claim timestamp whenever FreeCapsule's rewards are granted, regardless of entry path.</summary>
        private void HandleShopItemRewardsGranted(ShopItem item, SourceType sourceType, System.Collections.Generic.List<Output> outputs)
        {
            if (item == null || item != freeCapsule) return;

            PlayerPrefs.SetString(LastClaimUtcTicksPrefKey, DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }

        private static bool TryGetLastClaimUtc(out DateTime lastClaimUtc)
        {
            lastClaimUtc = default;

            string stored = PlayerPrefs.GetString(LastClaimUtcTicksPrefKey, string.Empty);
            if (string.IsNullOrEmpty(stored)) return false;
            if (!long.TryParse(stored, NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks)) return false;

            lastClaimUtc = new DateTime(ticks, DateTimeKind.Utc);
            return true;
        }
    }
}
