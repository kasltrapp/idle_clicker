# Economy Schema — Certified Aura: Idle Influencer Tycoon

Single source of truth for every persisted asset name, category, and unlock order. Names are locked once created (CLAUDE.md Hard Rule #1). Numbers are playtesting placeholders, not final balance, unless noted otherwise.

**Build status: all entries below marked ✅ are created and verified in the project. Entries marked 🔲 are designed but not yet built.**

---

## Currencies

| Asset name | Purpose | Cap | Reset on Rebrand? | Status |
|---|---|---|---|---|
| `Followers` | Primary production currency | Uncapped | Yes | ✅ |
| `Cash` | Secondary currency | Uncapped | Yes | ✅ |
| `Aura` | Prestige currency, earned only on Rebrand | Uncapped | No — excluded | ✅ |
| `CloutCoin` | Premium currency — earned via level-up rewards, purchasable via IAP, spent on Manager Capsules | Uncapped | No — excluded (same treatment as Aura) | 🔲 |

## Custom Stat

| Asset name | Purpose | Range | Reset on Rebrand? | Status |
|---|---|---|---|---|
| `Burnout` | Rises with production, decays passively, reduced by Spa Day Shop items | 0–100 | Yes, resets to 0 | ✅ decay/reset/shop-relief implemented (`BurnoutController.cs`); rise-on-production deferred to Phase 3 (design pass needed — rate per business, possibly tiered by platform) |

## Locations (Platforms) — v1 launch order

| Asset name | Display name | Unlock cost | Default active? | Status |
|---|---|---|---|---|
| `StarterFeed` | Your First Post | Free | Yes | ✅ |
| `ClipzPlatform` | Clipz | 500 `Followers` | No | ✅ |
| `ChroniclePlatform` | The Chronicle | 5,000 `Cash` | No | ✅ |

Brand/merch platform: post-launch, not yet designed.

## Businesses

### StarterFeed — ✅ all built

| Asset name | Base cost | Produces | Time | Notes |
|---|---|---|---|---|
| `SelfieSession` | 1 `Followers` | 1 `Followers` | 3s | costMultiplier 1.15, manager `GhostwriterManager` |
| `StoryPost` | 15 `Followers` | 3 `Followers` | 8s | costMultiplier 1.15, manager `SocialMediaManager`, unlock: own 1x `SelfieSession`, upgradeCost 40 `Followers`/level, `autoUpgradeForFree` = false |
| `RingLightSetup` | 100 `Followers` | 8 `Followers` | 20s | costMultiplier 1.15, unlock: `StoryPost` at upgrade level 2 (`businessUpgradeLevelLock`) |

### ClipzPlatform — ✅ all built

| Asset name | Base cost | Produces | Time | Notes |
|---|---|---|---|---|
| `TrendChase` | 50 `Cash` | 12 `Followers` | 10s | costMultiplier 1.15, manager `EditorManager` |
| `DuetFarm` | 300 `Cash` | 6 `Cash` | 25s | costMultiplier 1.15, manager `TalentAgentManager`, production input: 20 `Followers`/round |
| `ViralAttempt` | 1,500 `Cash` | 40 `Followers` + 10 `Cash` (guaranteed, `DropStrategy.All`) | 60s | costMultiplier 1.15, no manager, bonus drop-table output deferred to Phase 6 balancing |

### ChroniclePlatform — ✅ all built

| Asset name | Base cost | Produces | Time | Notes |
|---|---|---|---|---|
| `LongformEssay` | 2,000 `Cash` | 25 `Cash` | 90s | costMultiplier 1.15, manager `PublicistManager`, upgradeCost 150 `Cash`/level, `autoUpgradeForFree` = false |
| `SubscriberFunnel` | 8,000 `Cash` | 60 `Cash` | 180s | costMultiplier 1.15, no manager, unlock: `LongformEssay` at upgrade level 3 |

## Business Groups — ✅ built

| Asset name | Members | Purpose | Location |
|---|---|---|---|
| `ClipzContentGroup` | `TrendChase`, `DuetFarm` | Scoping for speed-type upgrades (contextual filters don't work for speed effects; group targeting does) | `Resources/BusinessGroups/` |

## Managers — ✅ all 5 built and functional (auto-leveling from duplicates is native, confirmed)

| Asset name | Automates | autoProduceOnLevel | autoCollectOnLevel | Rarity |
|---|---|---|---|---|
| `GhostwriterManager` | `SelfieSession` | 0 | 0 | Common |
| `SocialMediaManager` | `StoryPost` | 1 | 0 | Common |
| `EditorManager` | `TrendChase` | 0 | 0 | Common |
| `TalentAgentManager` | `DuetFarm` | 2 | 1 | Common |
| `PublicistManager` | `LongformEssay` | 0 | 0 | Common |

**Rarity — ✅ system built.** `Rarity` enum (`InfluencerRise.Managers`, `Assets/InfluencerRise/Scripts/Managers/Rarity.cs`): `Common`/`Rare`/`Epic`/`Mythic`. Assigned via a companion `ManagerRarity` ScriptableObject per Manager (`Assets/InfluencerRise/Scripts/Managers/ManagerRarity.cs`, one instance per Manager under `Resources/ManagerRarities/`) rather than a field on `Manager` itself or a subclass — `Manager.cs` is vendor code and the 5 assets above already exist as plain `Manager`-typed, name-locked assets, so re-typing them via subclassing was ruled out as unnecessarily invasive. `Manager.managerGroups` was also ruled out as an attachment point since it's an active functional dependency of `PrestigeManager.excludedManagerGroups`. All 5 Managers above are currently assigned **Common** — a placeholder, not a design decision on which managers should be rarer; that's a future task once actual Rare/Epic/Mythic manager content is designed.

**Manager acquisition model — REVISED (see Roadmap Phase 3):** originally planned as direct-currency purchase. Now redesigned per the AdVenture Communist reference: Managers are acquired via **weighted Capsule pulls** (Shop Items with `DropStrategy.WeightedRandom`, confirmed structurally supported by code inspection though not yet proven with a working example — proof-of-concept required before full build).

## Upgrades — ✅ built (3 of 4 planned; TherapySessionUpgrade deferred)

| Asset name | Type | Target | Effect |
|---|---|---|---|
| `BetterRingLight` | production | `SelfieSession` (via contextual filter, no group needed) | +25% output |
| `FasterEditingSoftware` | speed | `ClipzContentGroup` (group required — speed effects have no contextual path) | -20% production time |
| `BulkContentBatching` | purchaseCost | All Businesses (global) | -10% purchase cost |
| `TherapySessionUpgrade` | — | `Burnout` stat | -15 Burnout on purchase. **Deferred** — Upgrades can't target CustomStats; needs custom code same category as Burnout script. |

## Boosts — ✅ all built

| Asset name | Type | Effect |
|---|---|---|
| `AllNighterBoost` | TimeSkipBoost | Skips 4 hours of production |
| `ViralMomentBoost` | GenericMultiplierBoost | 3x production, 5 min duration, global |
| `AlgorithmPushBoost` | GenericMultiplierBoost | 2x speed, 10 min duration, global |

## Shop Items — ✅ 7 built; Capsule tiers 🔲 pending POC (see MonetizationPlan.md)

| Asset name | Type | Grants | Status |
|---|---|---|---|
| `FollowerPackSmall` | IAP | 1,000 Followers | ✅ (productId deferred to Phase 4) |
| `CashPackSmall` | IAP | 500 Cash | ✅ (productId deferred to Phase 4) |
| `SpaDayBurnoutReliefAd` | Ad | -40 Burnout (via BurnoutController) | ✅ |
| `SpaDayBurnoutReliefPaid` | IAP | -40 Burnout (via BurnoutController) | ✅ (productId deferred to Phase 4) |
| `RemoveAdsPermanent` | IAP | No economy grant — ownership itself is the flag (`GetItemAmount > 0`), maxPurchases = 1 | ✅ |
| `RewardedBoostAd` | Ad | `AlgorithmPushBoost` | ✅ |
| Manager Capsules (Free/tiered-paid) | Ad/Free/IAP | Weighted-random Manager pull by rarity | 🔲 pending POC + rarity system |

## Achievements — ✅ 3 built

| Asset name | Condition |
|---|---|
| `FirstPost` | Lifetime own 1x `SelfieSession` (`businessLock`, `TotalAmount`) |
| `HundredFollowers` | 100 lifetime `Followers` earned (`currencyLock`, `TotalAmount`) |
| `FirstRebrand` | 1 Rebrand completed (`prestigeLock`) |

Player Cosmetics (avatar unlocks via achievements/progress) — deferred to V1.1, confirmed NOT natively supported (needs new definition-asset type + new Output target).

## Prestige — "Rebrand" — ✅ configured

- Requirement: `Cash` ≥ 10,000 AND player level ≥ 5.
- Reward: `Aura` — scaling formula TBD (Phase 6).
- Reset scope: `Followers`, `Cash`, `Business` holders, `Manager` holders, `Boost` inventory = reset. `Aura`, `CloutCoin` = excluded (`exludedCurrencies`, intentional package misspelling, do not correct). `Burnout` reset handled by `BurnoutController.cs` listening to `OnPrestige` (engine has no native CustomStat reset hook).
- Persistent Aura upgrades: not yet defined, deferred to a later pass per original schema.

## Level-Up Rewards — 🔲 not yet configured (confirmed native, zero custom code required)

`PlayerStats.levelupData` (`LevelDataHolder`) supports both exact-level (`levelRewards`) and repeating/divisor-based (`eachLevelReward`, optional `scaleEachLevelReward`) rewards, using the same `Reward` → `DropTable` → `Output` system as everything else. **To be configured**: grant `CloutCoin` on level-up (primary intended source of free premium currency, per monetization design) — exact cadence/amounts TBD during Phase 3 build.

## First-hour pacing target

| Milestone | Target time |
|---|---|
| First `SelfieSession` purchase | Immediate |
| First `StoryPost` unlock | ~2 min |
| First Manager owned | ~5 min |
| `ClipzPlatform` unlock | ~15 min |
| First Boost use | ~20 min |
| First Shop interaction | ~25 min |
| `ChroniclePlatform` unlock | ~45–60 min |