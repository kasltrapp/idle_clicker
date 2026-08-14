# Monetization Plan

Philosophy: no pay-to-win. Real money buys convenience and accelerated access, never something categorically unreachable free.

## Currencies

- **CloutCoin** — ✅ built. Primary free-earn source: level-up rewards (native, config pending). Also IAP-purchasable (packs TBD). Spent on Manager Capsules.

## Shop Items — original 6, ✅ all built (see EconomySchema.md for full table)

## Manager Capsules — ✅ BUILT AND VERIFIED (was "designed, pending POC" — POC passed, real assets now exist)

| Asset name | Type | Cost | Common weight | Rare weight | Legendary weight |
|---|---|---|---|---|---|
| `FreeCapsule` | Free (4h cooldown via `FreeCapsuleCooldown.cs`) | Free | 10 | 2 | **0** |
| `WatchAdCapsule` | Ad | Free | 6 | 4 | **0** |
| `CloutCoinCapsuleLow` | CloutCoin | 50 (placeholder) | 4 | 5 | 1 |
| `CloutCoinCapsuleHigh` | CloutCoin | 250 (placeholder) | 1 | 5 | 4 |
| `IAPCapsule` | IAP | productId deferred (Phase 4) | 0 | 5 | 5 |

### 🔒 HARD CONSTRAINT — verified, not just documented

**Legendary-tier managers must NEVER appear with non-zero weight in `FreeCapsule` or `WatchAdCapsule`.** This has been verified twice independently: (1) a C# script asserting every Legendary-resolving Output has weight==0 across both capsules, (2) raw YAML grep against all 3 Legendary GUIDs in both asset files, (3) a 10,000-trial statistical run confirming a weight-0 entry is never actually selected. **Any future edit to either capsule's DropTable must re-run this verification before shipping.**

### ✅ FreeCapsule cooldown — built

`ShopItem` has no native cooldown/interval field, so `FreeCapsule` used to be infinitely re-claimable. `Assets/InfluencerRise/Scripts/Shop/FreeCapsuleCooldown.cs` (same category as `BurnoutController.cs`) now gates it: 4 hours between claims (**placeholder, Phase 6 tunable**, matching AdVenture Communist's "Next Ready In: Xh Ym" reference pattern). Attached to `GameManagers`. Timestamp persisted via PlayerPrefs (not SaveFile.cs — see script header for why). A future "Claim Free Capsule" UI must call `FreeCapsuleCooldown.TryClaimFreeCapsule()` instead of `ShopManager.AttemptPurchase` directly, since FreeCapsule's free/no-cost purchase path has no cancellable pre-purchase event to hook. `GetTimeRemaining()`/`CanClaim` are exposed for that future UI.

## IAP Product IDs (Phase 4)

All Shop Items' `productId` fields are placeholder-empty pending real store product creation.

## Ad placements

Rewarded video only at launch (`RewardedBoostAd`, `SpaDayBurnoutReliefAd`, `WatchAdCapsule`). No forced interstitials.

## Receipt validation

Unity IAP's built-in validator for v1.

## Compliance

Privacy policy, ATT prompt (iOS), age rating 12+, Sign in with Apple required if Google Sign-In is offered on Android (App Store Guideline 4.8).

## Explicitly rejected patterns

No progress-gated "mission chests" requiring purchase to unlock required progression currency.