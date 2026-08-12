# Roadmap

## Phase 0 — Tooling (COMPLETE)
- Git repo linked, Unity Version Control disabled.
- Claude Code connected to Unity via MCP.
- Documentation system (CLAUDE.md + docs/) established.

## Phase 1 — Design (COMPLETE)
- Game Design Document finalized.
- Economy Schema finalized (Currencies, Businesses, Locations, Managers, Upgrades, Boosts, Shop, Prestige).
- Naming Conventions, Engine Capabilities, Monetization Plan, Build/Release Checklist finalized.

## Phase 2 — Core loop build (CURRENT)
- Create StarterFeed platform: Currencies, Burnout stat, first Business.
- Set up manager GameObject in Main scene.
- Verify basic buy → produce → collect loop works in Play Mode.
- Build Burnout decay script (the one confirmed custom-code piece).
- Burnout script (decay ticker + PrestigeManager.OnPrestige reset hook) — confirmed as the only custom gameplay script needed for the whole v1 economy. Build once core loop assets are fully in place.

## Phase 3 — Full economy build
- ClipzPlatform and ChroniclePlatform businesses, managers, upgrades, boosts.
- Prestige (Rebrand) configuration and testing.
- Achievements and daily rewards.
- Design and implement Burnout rise-on-production (rate per business, possibly tiered by platform) — decay/reset/shop-relief already implemented in BurnoutController.cs.

## Phase 4 — Monetization integration
- Unity IAP package integration, sandbox testing.
- Ad SDK integration, rewarded ad flow.
- Shop Items wired to real purchase completion contract.

## Phase 5 — UI and art
- Replace/theme demo displayers with real art direction.
- Full UI pass matching satirical tone.

## Phase 6 — Polish and balance
- Economy balancing pass using real playtest data.
- Burnout/production-modifier tuning.
- Bug fixing, performance pass.

## Phase 7 — Release prep
- Work through BuildReleaseChecklist.md fully.
- Store listing assets, screenshots, description copy.
- Submission.