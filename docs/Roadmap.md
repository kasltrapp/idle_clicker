# Roadmap

## Completion tracker (v1 launch scope)

| Category | Status |
|---|---|
| Tooling & docs | 100% |
| Economy design & backend (15 businesses, 24 managers, 3 platforms, Locks, Prestige/Aura) | ~95% |
| Monetization backend (CloutCoin, 5 Capsules) | ~70% — FreeCapsule cooldown + level-up config pending |
| Core UI — Yourgram only | ~40% |
| UI — QuickTok/Broadcast rows | 0% |
| UI — tab switching (currently static labels) | 0% |
| UI — Manager/Capsule, Prestige, Achievements, Shop screens | 0% |
| UI — meta shell (splash, settings, profile, tutorial) | 0% |
| Monetization integration (Phase 4) | 0%, correctly deferred |
| Account/Login (Phase 5) | 0%, correctly deferred |

**Overall v1: ~35%.** Backend/economy design is nearly done; player-facing UI is the majority of remaining work.

## Phase 0 — Tooling (COMPLETE)
## Phase 1 — Design (COMPLETE)
## Phase 2 — Core loop build (COMPLETE)
All economy backend systems built and verified: 3 platforms, 15 businesses, 24 managers, full Lock chains, Prestige/Aura, Burnout (4 of 4 planned responsibilities except rise-on-production design), CloutCoin, 5 Capsules, Shop items, Achievements. Minimal UI built and player-tested for Yourgram only.

## Phase 3 — V1 Launch scope (CURRENT)

Remaining, in recommended order:
1. **FreeCapsule cooldown script** (small, closes a real gap).
2. **Level-up → CloutCoin reward config** (native, unconfigured).
3. **Burnout rise-on-production design + implementation** (rate per business, possibly tiered by platform).
4. **QuickTok + Broadcast business row UI** (extend the proven Yourgram template).
5. **Real tab-switching logic** (bind bottom tabs to Location assets, wire tap-to-switch).
6. **Manager/Capsule pull UI** (the screen where players actually acquire all 24 managers).
7. **Prestige (Rebrand) UI**, including Prestige-count display.
8. **Achievements UI.**
9. **Shop UI shell** (purchase buttons disabled until Phase 4 IAP).
10. **Meta shell**: splash/legal gate, loading screen, settings, player profile, hamburger menu, sequential tutorial, welcome-back offline-summary screen.
11. A full end-to-end playtest pass once the above lands.

## Phase 4 — Monetization integration
Unity IAP, ad SDK, real product IDs, sandbox testing.

## Phase 5 — Account/Login
Google Sign-In + Sign in with Apple, cloud save sync.

## V1.1 — Fast follow
Player Cosmetics system (confirmed needs new definition-asset type). Additional Rare/Legendary content if v1 pool needs depth. Manager max-level/salvage system, only if real player data suggests it's needed.

## Post-Launch Content
4th platform. Deeper business chains (6+ tiers per platform). Additional Achievements/Boosts/Upgrades.

## Explicitly cut from scope
Trades system, Missions-as-separate-quest-tracker, progress-gated purchase-required reward chests — all deliberate exclusions, not oversights (see GameDesignDocument.md).

## Phase 6 — Balancing
All placeholder numbers (costs, multipliers, Capsule costs/weights, Burnout rates) tuned against real playtest data.

## Phase 7 — Release prep
Full BuildReleaseChecklist.md pass, store listing, submission.