# Roadmap

## Phase 0 — Tooling (COMPLETE)
Git, Claude Code + Unity MCP, documentation system.

## Phase 1 — Design (COMPLETE)
Game Design Document, Economy Schema, all supporting docs finalized. Revised post-AdVenture-Communist-reference-review (Manager acquisition model, UI direction, monetization design).

## Phase 2 — Core loop build (COMPLETE)
All 3 platforms, all Businesses/Managers/Upgrades/Boosts/Shop Items/Achievements/Prestige built and verified. `BurnoutController.cs` written (decay/reset/shop-relief). Minimal functional UI built and player-tested end to end (buy → produce → currency increases, confirmed via actual button taps, not API shortcuts).

## Phase 3 — V1 Launch scope (CURRENT)

Build order:
1. **Manager Rarity system** — new field/subclass (confirmed not native).
2. **Weighted-manager-output proof-of-concept** — narrow test before full build (confirmed structurally sound, unproven by example).
3. **CloutCoin currency** + Manager Capsule Shop Items (Free/paid tiers).
4. **Level-up rewards configuration** — grant CloutCoin on level-up (confirmed native, zero custom code).
5. **Manager acquisition UI** — capsule-pull based, not flat currency-purchase (revised design).
6. **SaveAndLoad** — local persistence (currently every session resets to 0).
7. **Burnout rise-on-production** — design pass (rate per business/platform) + implementation.
8. Clipz + Chronicle: platform switching + business list UI (extend proven StarterFeed pattern).
9. Upgrade purchase UI, Boost UI.
10. Prestige (Rebrand) UI.
11. Achievements UI.
12. Shop UI shell (purchase buttons disabled until Phase 4 IAP).
13. UI/UX pass matching AdVenture Communist-inspired layout: top-left player profile icon, top-right hamburger dropdown menu (Settings/Inbox/News/Connect/Support), compact business row density, sequential tutorial speech-bubbles, legal/consent gate, loading screen, "welcome back" offline-summary screen (maps to native `ProfitApplicationSummary`).

## Phase 4 — Monetization integration
Unity IAP integration, sandbox testing, ad SDK integration, rewarded ad flow, real product IDs.

## Phase 5 — Account/Login
Google Sign-In (Android) + Sign in with Apple (iOS, required if Google offered — App Store Guideline 4.8). Cloud save sync, extending Phase 3's local SaveAndLoad.

## V1.1 — Fast follow (post-launch, not blocking initial ship)
- **Player Cosmetics** (avatar unlocks via Achievements/events/progress) — confirmed needs new definition-asset type + new Output target, not present in engine natively.
- Additional Manager rarity tiers/pool depth, if v1's capsule system needs more content.

## Post-Launch Content (evaluate based on real player data, not pre-built)
- 4th platform (brand/merch).
- Additional Achievements, Boosts, Upgrades.
- Deeper business chains per platform (reference shows 4+ tiers; v1 ships with 2-3).

## Explicitly cut from scope — deliberate exclusions, not oversights
- **Trades** (resource-conversion economy, tiered milestone unlocks) — judged too specific to the AdVenture Communist reference's own bespoke fiction; risks being a direct copy rather than a genre convention.
- **Missions as a separate quest-tracker system** — deliberately replaced with native level-up rewards instead. Reference game's mission-chest pattern was identified as progress-gating tied to purchase (a dark pattern) and rejected on principle, not just scope.
- **Progress-gated reward chests requiring purchase to unlock required progression currency** — explicitly rejected, see MonetizationPlan.md.

## Phase 6 — Balancing
All placeholder numbers (costs, multipliers, production rates, Burnout rates, capsule odds) tuned against real playtest data. Full list of flagged placeholders lives in EconomySchema.md's per-item notes.

## Phase 7 — Release prep
Full BuildReleaseChecklist.md pass, store listing assets, submission.