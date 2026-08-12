# Monetization Plan

Philosophy: no pay-to-win. Real money buys convenience and cosmetics, never a permanent stat edge free players can't reach through play. See GameDesignDocument.md.

## Shop Items (names match EconomySchema.md — do not rename independently)

| Asset name | Type | Price (draft, TBD at store setup) | Grants |
|---|---|---|---|
| `FollowerPackSmall` | IAP | $0.99 | 1,000 Followers |
| `CashPackSmall` | IAP | $0.99 | 500 Cash |
| `SpaDayBurnoutRelief` | IAP or Ad | $0.99 / free via ad | -40 Burnout |
| `RemoveAdsPermanent` | IAP | $2.99 | Disables ad placements (project flag, not an economy grant) |
| `RewardedBoostAd` | Ad only | Free | Grants AlgorithmPushBoost |

Larger currency packs (Medium/Large tiers) — add once initial packs are tested for conversion, don't over-build the store before launch.

## IAP Product IDs (fill in once App Store Connect / Play Console products are created)

| Asset name | iOS Product ID | Android Product ID |
|---|---|---|
| `FollowerPackSmall` | TBD | TBD |
| `CashPackSmall` | TBD | TBD |
| `SpaDayBurnoutRelief` | TBD | TBD |
| `RemoveAdsPermanent` | TBD | TBD |

## Ad placements

- **Rewarded video only at launch** — no forced interstitials. Placement: `RewardedBoostAd` shop item, plus optional rewarded-ad prompt on Burnout threshold (design TBD).
- Interstitials/banners: not planned for v1 — revisit post-launch based on retention data, don't compromise first-session experience before you have players to test against.

## Receipt validation

Unity IAP's built-in receipt validator for v1. Server-side validation is a post-launch hardening step if fraud becomes a measurable problem — don't over-engineer before you have real transaction volume.

## Compliance

- Privacy policy required before submission (data collection disclosure for ads/IAP).
- iOS: App Tracking Transparency prompt required if ad SDK does cross-app tracking.
- Age rating 12+ — confirm IAP/ad content doesn't push this higher during actual store rating questionnaires.