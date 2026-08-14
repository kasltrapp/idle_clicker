# Monetization Plan

Philosophy: no pay-to-win. Real money buys convenience and accelerated access to rare content, never something categorically unreachable free. See GameDesignDocument.md.

## Currencies

- **Cash** — earned through play, spent on core progression. Not purchasable directly (Followers/Cash packs are IAP, but represent a head-start/convenience, not the only path).
- **CloutCoin** — premium currency. ✅ Built (`Resources/Currencies/CloutCoin.asset`), uncapped, excluded from Prestige reset (`PrestigeManager.exludedCurrencies`, alongside `Aura`). Primary free-earn source: level-up rewards (`PlayerStats.levelupData`, native, cadence/amount TBD Phase 3). Also purchasable via IAP (Gold Pack-style tiers, see below). Spent on Manager Capsules.

## Shop Items — built (12 of 12: 7 core items + 5 real Capsule tiers)

| Asset name | Type | Price (draft) | Grants |
|---|---|---|---|
| `FollowerPackSmall` | IAP | $0.99 | 1,000 Followers |
| `CashPackSmall` | IAP | $0.99 | 500 Cash |
| `SpaDayBurnoutReliefAd` | Ad | Free | -40 Burnout |
| `SpaDayBurnoutReliefPaid` | IAP | $0.99 | -40 Burnout |
| `RemoveAdsPermanent` | IAP | $2.99 | Disables ad placements (project flag) |
| `RewardedBoostAd` | Ad | Free | AlgorithmPushBoost |
| `FreeCapsule` | Free | Free (no cooldown mechanism yet — see note below) | 1 weighted-random Manager, Common/Rare only |
| `WatchAdCapsule` | Ad | Free | 1 weighted-random Manager, Common/Rare only, better odds than Free |
| `CloutCoinCapsuleLow` | In-game cost | 50 CloutCoin (placeholder) | 1 weighted-random Manager, Legendary now reachable |
| `CloutCoinCapsuleHigh` | In-game cost | 250 CloutCoin (placeholder) | 1 weighted-random Manager, best free-currency odds |
| `IAPCapsule` | IAP | TBD (Phase 4) | 1 weighted-random Manager, best odds overall |

## Manager Capsules — ✅ built (POC replaced with production assets)

**⚠️ HARD CONSTRAINT — enforced:**
> **Legendary-tier Manager pool entries must NEVER appear in any Watch-Ad Capsule's DropTable.** Only CloutCoin-purchased or direct-IAP Capsule tiers may include Legendary-weight entries (`TheMainCharacter`, `TheAlgorithmItself`, `TheNepoBaby`, and any future Legendary manager). **Verified on `FreeCapsule` and `WatchAdCapsule` via two independent methods** (a fresh-loaded C# programmatic check plus a raw-YAML `grep` cross-check) — all 3 Legendary managers are present as entries in both pools (so future rarity additions don't need special-casing) but every one carries `weight: 0`, and a dedicated statistical test (10,000 trials) confirmed a `weight: 0` entry is never actually selected. See `EconomySchema.md`'s Shop Items section for the full verification writeup. **When adding any future Capsule tier**, audit its `DropTable`'s `Output` list the same way before shipping — this is still a manual step, not an automated gate, since the engine has no native rarity concept to gate on.

Modeled after the "Free/Iron/Gold/Diamond Capsule" pattern: a tiered set of Shop Items, each rolling one Manager from a weighted pool, odds skewing toward rarer tiers as capsule tier increases. All 5 tiers share the same 24-Manager pool (all Common/Rare/Legendary managers as `Output` entries in one `DropStrategy.WeightedRandom` `DropTable`, `totalDropAmount=1`); only the per-rarity `weight` values differ per tier — see `EconomySchema.md` for exact numbers.

**Design intent (Common/Rare/Legendary terminology, locked — see `EconomySchema.md`'s `Rarity` enum):**
- **Free Capsule** — no cost. Design intent was a long cooldown timer (e.g. every few hours) to ensure a genuine free path exists without being spammable — **not yet implemented**: `ShopItem.cs` has no native cooldown/claim-interval field (only `maxPurchases`, a lifetime total cap), confirmed via full read of the class rather than guessed. Flagged rather than faked with an invented cost. Needs a small custom script (same category as `BurnoutController.cs`) as a separate future task before this ships.
- **Watch-Ad Capsule** — `buyWithAd = true`, better odds than Free, no cooldown modeled either (same gap as above, shared by both free-tier capsules).
- **CloutCoin-purchasable capsule tiers** — ✅ built, `CloutCoinCapsuleLow`/`CloutCoinCapsuleHigh`, spend earned or IAP-bought CloutCoin for better odds including Legendary access.
- **Direct IAP capsule tiers** — ✅ built, `IAPCapsule`, real-money shortcut, same reward pool, no CloutCoin spend required, `productId` deferred to Phase 4 per existing convention.

**Build sequencing — complete.** (1) Manager Rarity field/system — ✅ built (see `EconomySchema.md`). (2) Proof-of-concept confirming `ShopItem` + `Weighted DropTable` + `managerOutput` works statistically — ✅ run directly (20,000-trial + 10,000-trial statistical tests, not just structural code review), see `EconomySchema.md`'s writeup and the updated finding in `EngineCapabilities.md`.

## IAP Product IDs (fill in once store products are created — Phase 4)

| Asset name | iOS Product ID | Android Product ID |
|---|---|---|
| `FollowerPackSmall` | TBD | TBD |
| `CashPackSmall` | TBD | TBD |
| `SpaDayBurnoutReliefPaid` | TBD | TBD |
| `RemoveAdsPermanent` | TBD | TBD |
| CloutCoin packs (tiered) | TBD | TBD |
| Manager Capsule tiers (IAP versions) | TBD | TBD |

## Ad placements

- Rewarded video only at launch — `RewardedBoostAd`, `SpaDayBurnoutReliefAd`, and a Free Capsule ad-accelerate option (watch ad to reduce cooldown, TBD).
- No forced interstitials at launch.

## Receipt validation

Unity IAP's built-in validator for v1. Server-side validation deferred to post-launch hardening.

## Compliance

- Privacy policy required before submission.
- iOS: App Tracking Transparency prompt if ad SDK does cross-app tracking.
- Age rating 12+.
- If Google Sign-In is implemented on Android, Sign in with Apple (or equivalent) is required on iOS per App Store Review Guideline 4.8 — not optional. See BuildReleaseChecklist.md.

## Explicitly rejected monetization patterns

- **No progress-gated "mission chests"** that require a purchase to unlock currency/resources needed to progress a level. Identified in the AdVenture Communist reference as a real, deliberate dark pattern (hiding required progression inside a paywalled reward) — rejected on principle, not just scope, per direct product decision.