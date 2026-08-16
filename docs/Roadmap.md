# Roadmap

## Completion tracker (v1 launch scope)

| Category | Status |
|---|---|
| Tooling & docs | 100% |
| Economy design & backend (15 businesses, 24 managers, 3 platforms, Locks, Prestige/Aura) | ~95% |
| Monetization backend (CloutCoin, 5 Capsules) | 100% — FreeCapsule cooldown (`FreeCapsuleCooldown.cs`) and level-up→CloutCoin config (native, `PlayerStats.levelupData`) both confirmed built, see EngineCapabilities.md/EconomySchema.md. Roadmap previously not updated to reflect this. |
| Core UI — business rows, all 3 platforms + tab switching | ~55% — Yourgram/QuickTok/Broadcast rows all built and reachable via real tab-switching (2026-08-16). Detail popup polish, buy-amount selector edge cases, top bar completion still pending. |
| UI — Manager/Capsule, Prestige, Achievements, Shop screens | 0% |
| UI — meta shell (splash, settings, profile, tutorial) | 0% |
| Monetization integration (Phase 4) | 0%, correctly deferred |
| Account/Login (Phase 5) | 0%, correctly deferred |

**Overall v1: ~40%.** Backend/economy design is essentially done; player-facing UI (screens beyond business rows) is the majority of remaining work.

## Phase 0 — Tooling (COMPLETE)
## Phase 1 — Design (COMPLETE)
## Phase 2 — Core loop build (COMPLETE)
All economy backend systems built and verified: 3 platforms, 15 businesses, 24 managers, full Lock chains, Prestige/Aura, Burnout (4 of 4 planned responsibilities except rise-on-production design), CloutCoin, 5 Capsules, Shop items, Achievements. Minimal UI built and player-tested for Yourgram only.

## Phase 3 — V1 Launch scope (CURRENT)

Done (this list was stale — items below are confirmed built via EngineCapabilities.md/EconomySchema.md and, for the last two, this session's own verified work):
1. ✅ **FreeCapsule cooldown script** — `FreeCapsuleCooldown.cs`, custom PlayerPrefs-based.
2. ✅ **Level-up → CloutCoin reward config** — native, `PlayerStats.levelupData`, see EconomySchema.md Level Progression section.
3. ✅ **Burnout rise-on-production** — `BurnoutController.cs`, tier-scaled placeholder formula (Phase 6 tunable).
4. ✅ **QuickTok + Broadcast business row UI** — extended the proven Yourgram template to all 10 remaining rows (2026-08-16), verified via fresh-disk-reload wiring checks.
5. ✅ **Real tab-switching logic** — all 3 bottom tabs wired and verified via real raycast taps + owner confirmation (2026-08-16). **Open product decision, not yet confirmed with owner:** tabs are NOT gated by platform-unlock state — tapping a locked platform's tab shows its rows correctly rendered as locked/non-interactable rather than blocking the tap or hiding the tab. See BugTracker.md.

Remaining, in recommended order:
6. **Manager/Capsule pull UI** (the screen where players actually acquire all 24 managers).
7. **Prestige (Rebrand) UI**, including Prestige-count display.
8. **Achievements UI.**
9. **Shop UI shell** (purchase buttons disabled until Phase 4 IAP).
10. **Meta shell**: splash/legal gate, loading screen, settings, player profile, hamburger menu, sequential tutorial, welcome-back offline-summary screen.
11. A full end-to-end playtest pass once the above lands.

### Phase 3 — UI polish backlog (not scheduled, deferred)
- **Currency count-up animation** — when a player collects resources (via purchase completion, production ticks, or offline/idle summary), the currency bar values (Followers/Cash/Aura) should visually tween/roll up to the new balance rather than snapping instantly. No animation hooks currently exist anywhere in the UI (confirmed via 2026-08-15 art-readiness audit) — this needs `Animator`/`Animation` components built from scratch, not adapted. Deferred — not in scope until explicitly scheduled. Exact trigger conditions and visual style (simple text tween vs. animated icon movement) still to be decided when this is actually scheduled.

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