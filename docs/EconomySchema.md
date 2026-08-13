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

**⚠️ NAMING FLAG (not resolved by this pass):** a rename task described "Clipz has been renamed to QuickTok throughout" as already-done fact. Checked directly — it is not. `ClipzPlatform.asset`, `ClipzContentGroup.asset`, and references in `Main.unity` all still use the Clipz name, unchanged. This section still reflects that unrenamed state. Flagging per instruction rather than renaming silently — Location/BusinessGroup renames were not part of the approved exception for this pass (only the 8 Businesses + 5 Managers below were).

| Asset name | Display name | Unlock cost | Default active? | Status |
|---|---|---|---|---|
| `StarterFeed` | Your First Post | Free | Yes | ✅ |
| `ClipzPlatform` | Clipz | 500 `Followers` | No | ✅ — name NOT yet updated to QuickTok, see flag above |
| `ChroniclePlatform` | The Chronicle | 5,000 `Cash` | No | ✅ |

Brand/merch platform: post-launch, not yet designed.

## Businesses

**Renamed** (approved one-time exception to Hard Rule #1 — confirmed safe, no player has loaded a real save file; project not shipped). Old names kept in parentheses for anyone cross-referencing prior notes/screenshots.

### StarterFeed — ✅ all 5 tiers built (Batch A complete)

**⚠️ NAMING FLAG:** this task's instructions referred to this platform as "Yourgram" throughout. Checked directly — no asset, file, or doc anywhere in the project uses that name; the actual `Location` asset is still `StarterFeed`, unrenamed. Same pattern as the earlier "Clipz → QuickTok" claim (also unrealized). Treated "Yourgram" as referring to `StarterFeed` since the named businesses matched exactly — did not rename the Location (out of scope, not part of any approved exception). Also: the new `SelfieSession` Business below reuses the exact string that used to be `FirstPost`'s name before the earlier rename pass — worth knowing if anyone cross-references old notes/screenshots, since "SelfieSession" now means something different (Yourgram tier 2, not tier 1).

**Locks restructured this pass:** every tier now uses a compound AND lock (ownership threshold + player level), same pattern proven for the Prestige requirement. `StoryFeed`'s old "own 1x FirstPost" lock and `EngagementFarming`'s old `businessUpgradeLevelLock`-on-StoryFeed lock are both fully retired — confirmed via GUID grep that nothing else in the project depended on either before removing them (only the UI row's own `business` field binding referenced StoryFeed's GUID, unrelated and untouched). `StoryFeed`'s own `upgradeCost` (its business-leveling system, separate from unlock Locks) was left as-is — nothing asked for its removal, and nothing but the now-retired lock ever depended on it.

| Asset name | Base cost | Produces | Time | Manager | Lock | Notes |
|---|---|---|---|---|---|---|
| `FirstPost` (was `SelfieSession`) | 1 `Followers` | 1 `Followers` | 3s | `RonnyRingo` | none (free/instant) | costMultiplier 1.15 |
| `SelfieSession` (new tier 2 — reuses the old FirstPost name, see flag above) | 3 `Followers` | 2 `Followers` | 5s | `FayeFilterton` | own 10× `FirstPost` AND level ≥ 3 | costMultiplier 1.15. **Phase 6 placeholder numbers** — ~3x/2x FirstPost's cost/output, interpolated between FirstPost and StoryFeed's existing curve |
| `StoryFeed` (was `StoryPost`) | 15 `Followers` | 3 `Followers` | 8s | `ToriSteller` | own 15× `SelfieSession` AND level ≥ 7 | costMultiplier 1.15, upgradeCost 40 `Followers`/level (unchanged, unrelated to the lock restructure), `autoUpgradeForFree` = false |
| `EngagementFarming` (was `RingLightSetup`) | 100 `Followers` | 8 `Followers` | 20s | `FarmerBob` (new — confirmed had no manager before this pass) | own 20× `StoryFeed` AND level ≥ 14 | costMultiplier 1.15. **⚠️ Display name "Engagement Farming" confirmed text-overflowing in `UI_BusinessRow.prefab`'s NameText slot** (pre-existing flag, not resolved by this pass) |
| `VerifiedStatus` (new tier 5, capstone) | 500 `Followers` | 20 `Followers` | 45s | `ChuckVerifiington` | own 25× `EngagementFarming` AND level ≥ 24 | costMultiplier 1.15. **Phase 6 placeholder numbers** — highest cost/output on the platform, ~5x/2.5x EngagementFarming's cost/output as capstone scaling |

### ClipzPlatform — ✅ all built (Location name itself not yet renamed — see flag above)

| Asset name | Base cost | Produces | Time | Notes |
|---|---|---|---|---|
| `TrendChase` (name already correct, unchanged) | 50 `Cash` | 12 `Followers` | 10s | costMultiplier 1.15, manager `DanaTrent`. Display name was stale ("Chase a Trend") — corrected to "Trend Chase" as part of this pass. |
| `ReactionContent` (was `DuetFarm`) | 300 `Cash` | 6 `Cash` | 25s | costMultiplier 1.15, manager `ReggieReactington`, production input: 20 `Followers`/round |
| `ViralChallenge` (was `ViralAttempt`) | 1,500 `Cash` | 40 `Followers` + 10 `Cash` (guaranteed, `DropStrategy.All`) | 60s | costMultiplier 1.15, no manager, bonus drop-table output deferred to Phase 6 balancing |

### ChroniclePlatform — ✅ all built

| Asset name | Base cost | Produces | Time | Notes |
|---|---|---|---|---|
| `HotTake` (was `LongformEssay`) | 2,000 `Cash` | 25 `Cash` | 90s | costMultiplier 1.15, manager `HattieTakerson`, upgradeCost 150 `Cash`/level, `autoUpgradeForFree` = false |
| `SponsorDeals` (was `SubscriberFunnel`) | 8,000 `Cash` | 60 `Cash` | 180s | costMultiplier 1.15, no manager, unlock: `HotTake` at upgrade level 3 |

## Business Groups — ✅ built

| Asset name | Members | Purpose | Location |
|---|---|---|---|
| `ClipzContentGroup` | `TrendChase`, `ReactionContent` (was `DuetFarm`) | Scoping for speed-type upgrades (contextual filters don't work for speed effects; group targeting does) | `Resources/BusinessGroups/` |

## Managers — ✅ 8 built and functional (auto-leveling from duplicates is native, confirmed)

**Renamed** (same approved one-time exception as the Businesses above). Old names kept in parentheses.

| Asset name | Automates | autoProduceOnLevel | autoCollectOnLevel | Rarity |
|---|---|---|---|---|
| `RonnyRingo` (was `GhostwriterManager`) | `FirstPost` | 0 | 0 | Common |
| `FayeFilterton` (new) | `SelfieSession` | 0 | 0 | Common |
| `ToriSteller` (was `SocialMediaManager`) | `StoryFeed` | 1 | 0 | Common |
| `FarmerBob` (new) | `EngagementFarming` | 0 | 0 | Common |
| `ChuckVerifiington` (new) | `VerifiedStatus` | 0 | 0 | Common |
| `DanaTrent` (was `EditorManager`) | `TrendChase` | 0 | 0 | Common |
| `ReggieReactington` (was `TalentAgentManager`) | `ReactionContent` | 2 | 1 | Common |
| `HattieTakerson` (was `PublicistManager`) | `HotTake` | 0 | 0 | Common |

Every manager-to-business assignment above was re-verified by raw GUID cross-reference (not name matching) directly against each `Business.manager` field — all 8 confirmed correct, no mismatches found. `FarmerBob`/`FayeFilterton`/`ChuckVerifiington` follow the exact same cost/lock/upgrade pattern as the 5 pre-existing Common managers (free acquisition — `cost: []` — per the Capsule-based acquisition model; default `passiveBoosts` = speed ×2, Manager.cs's own field default, left unchanged as a Phase 6 placeholder like the other 5).

**Rarity — ✅ system built.** `Rarity` enum (`InfluencerRise.Managers`, `Assets/InfluencerRise/Scripts/Managers/Rarity.cs`): `Common`/`Rare`/`Epic`/`Mythic`. Assigned via a companion `ManagerRarity` ScriptableObject per Manager (`Assets/InfluencerRise/Scripts/Managers/ManagerRarity.cs`, one instance per Manager under `Resources/ManagerRarities/`, also renamed this pass: `RonnyRingoRarity`, `ToriStellerRarity`, `DanaTrentRarity`, `ReggieReactingtonRarity`, `HattieTakersonRarity`) rather than a field on `Manager` itself or a subclass — `Manager.cs` is vendor code and the 5 assets above already exist as plain `Manager`-typed, name-locked assets, so re-typing them via subclassing was ruled out as unnecessarily invasive. `Manager.managerGroups` was also ruled out as an attachment point since it's an active functional dependency of `PrestigeManager.excludedManagerGroups`. All 5 Managers above are currently assigned **Common** — a placeholder, not a design decision on which managers should be rarer; that's a future task once actual Rare/Epic/Mythic manager content is designed.

## Manager Rarity — mechanics (revised per audit)

**Leveling curve is universal, not rarity-dependent.** Every Manager, regardless of rarity, levels on the same fixed geometric curve (1, 2, 4, 8... copies doubling per level — native to IUpgradable, not configurable per-asset without custom code). Rarity does NOT change how many copies are needed to level up.

**What rarity actually controls instead:**
1. **Pull odds** — how likely this Manager is to drop from a Capsule, via `Output.weight` on the Capsule's weighted DropTable. Rarer = lower weight = harder to pull. This is the primary "feels rare" lever.
2. **Power per level** — `businessSpeedMultiplier` (and other native per-Manager fields) configured stronger on higher-rarity Managers, so a Rare/Legendary pull is worth more even before factoring in how hard it was to get. Fully native, freely configurable per Manager asset today.

**No max level, no salvage system at launch.** Managers level infinitely (native default, `MaxLevel = BigNumber.MaxValue`). A maxed-out Legendary just keeps getting stronger — there's no cap to hit, so no "wasted duplicates" problem to solve. Revisit as a possible V1.1 feature only if real player data suggests infinite scaling feels ungrounded rather than rewarding — not a launch requirement.

**`ManagerRarity` companion asset's actual job:** a rarity tag + Capsule-pull-weight lookup value for our own Capsule-building logic to read from. It does NOT and cannot feed the native leveling system (confirmed: `IUpgradableHolder` never reads from it) — kept deliberately simple rather than holding dead fields that look functional but aren't.

**Manager acquisition model — REVISED (see Roadmap Phase 3):** originally planned as direct-currency purchase. Now redesigned per the AdVenture Communist reference: Managers are acquired via **weighted Capsule pulls** (Shop Items with `DropStrategy.WeightedRandom`, confirmed structurally supported by code inspection though not yet proven with a working example — proof-of-concept required before full build).

## Upgrades — ✅ built (3 of 4 planned; TherapySessionUpgrade deferred)

| Asset name | Type | Target | Effect |
|---|---|---|---|
| `BetterRingLight` | production | `FirstPost` (was `SelfieSession`; via contextual filter, no group needed) | +25% output |
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
| `FirstPost` | Lifetime own 1x `FirstPost` Business (`businessLock`, `TotalAmount`). **⚠️ Naming note:** this Achievement asset was already named `FirstPost` before this rename pass; the Business it locks on (formerly `SelfieSession`) is now *also* named `FirstPost` after this rename. Same name, two different asset types (Achievement vs. Business) — no functional collision (`Resources.LoadAll<T>` is type-scoped) but confusing in the Editor's asset list. Flagging, not renaming — Achievement renames weren't part of this approved exception. |
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
| First `FirstPost` purchase (was `SelfieSession`) | Immediate |
| First `StoryFeed` unlock (was `StoryPost`) | ~2 min |
| First Manager owned | ~5 min |
| `ClipzPlatform` unlock | ~15 min |
| First Boost use | ~20 min |
| First Shop interaction | ~25 min |
| `ChroniclePlatform` unlock | ~45–60 min |