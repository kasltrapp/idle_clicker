# Monetization Plan

Philosophy: no pay-to-win. Real money buys convenience and accelerated access to rare content, never something categorically unreachable free. See GameDesignDocument.md.

## Currencies

- **Cash** — earned through play, spent on core progression. Not purchasable directly (Followers/Cash packs are IAP, but represent a head-start/convenience, not the only path).
- **CloutCoin** — premium currency. Primary free-earn source: level-up rewards (`PlayerStats.levelupData`, native, cadence/amount TBD Phase 3). Also purchasable via IAP (Gold Pack-style tiers, see below). Spent on Manager Capsules.

## Shop Items — built (7 of 7 core items)

| Asset name | Type | Price (draft) | Grants |
|---|---|---|---|
| `FollowerPackSmall` | IAP | $0.99 | 1,000 Followers |
| `CashPackSmall` | IAP | $0.99 | 500 Cash |
| `SpaDayBurnoutReliefAd` | Ad | Free | -40 Burnout |
| `SpaDayBurnoutReliefPaid` | IAP | $0.99 | -40 Burnout |
| `RemoveAdsPermanent` | IAP | $2.99 | Disables ad placements (project flag) |
| `RewardedBoostAd` | Ad | Free | AlgorithmPushBoost |

## Manager Capsules — designed, pending proof-of-concept + rarity system

**⚠️ HARD CONSTRAINT — enforce when building Capsule Shop Items (this section is not yet built, so this cannot be enforced in code today; it is a documented requirement for that future task):**
> **Legendary-tier Manager pool entries must NEVER appear in any Watch-Ad Capsule's DropTable.** Only CloutCoin-purchased or direct-IAP Capsule tiers may include Legendary-weight entries (`TheMainCharacter`, `TheAlgorithmItself`, `TheNepoBaby`, and any future Legendary manager). Before shipping any Capsule Shop Item that grants a free/ad-sourced reward, audit every ad-sourced `DropTable`'s `Output` list and confirm zero Legendary-tier manager entries are present. This is a manual audit step, not an automated gate — there is no native "rarity" concept in the `DropTable`/`Output` system (confirmed via code audit, see `EconomySchema.md`'s Rare/Legendary Manager sections), so nothing will stop a Legendary entry from being accidentally added to an ad table except this check.

Modeled after the "Free/Iron/Gold/Diamond Capsule" pattern: a tiered set of Shop Items, each rolling one Manager from a weighted pool, odds skewing toward rarer tiers as capsule tier increases.

**Design intent (Common/Rare/Legendary terminology, locked — see `EconomySchema.md`'s `Rarity` enum):**
- **Free Capsule** — no cost, long cooldown timer (e.g. every few hours), guarantees at least a Common-tier pull. Ensures a genuine free path exists.
- **CloutCoin-purchasable capsule tiers** — spend earned or IAP-bought CloutCoin for better odds/guaranteed minimum rarity.
- **Direct IAP capsule tiers** — real-money shortcut, same reward pool, no CloutCoin spend required.

**Build sequencing (see Roadmap):** requires (1) a Manager Rarity field/system — confirmed not natively supported, needs custom addition; (2) a proof-of-concept confirming `ShopItem` + `Weighted DropTable` + `managerOutput` actually works statistically (structurally sound per code audit, but unproven by any existing package example). Do not build the full tier set until the POC passes.

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