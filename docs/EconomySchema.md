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

**⚠️ NAMING FLAG — RESOLVED this pass:** an earlier task described "Clipz has been renamed to QuickTok throughout" as already-done fact; that was checked directly and found false at the time (the Location asset was still `ClipzPlatform`, unrenamed). A later task explicitly approved a one-time GUID-preserving rename of that Location asset (same approved exception as the earlier Business/Manager rename batch — no player has ever loaded a save). That rename has now been performed: `ClipzPlatform.asset` → `QuickTokPlatform.asset` (GUID unchanged: `a3f69297faff01c40871ff63deeead96`; `metadata.name` updated from `"Clipz (short-form)"` to `"QuickTok"`). All 5 QuickTok Business assets' `location` field references re-verified pointing at the same GUID post-rename. `ClipzContentGroup.asset` (a BusinessGroup, separate asset type) was **not** renamed — out of scope for this exception, not requested. The bottom-tab UI button for this platform (`ClipzTab`) was inspected directly and found to have **zero persistent `onClick` listeners** — it was never wired to anything, so there was no existing binding to break or preserve.

| Asset name | Display name | Unlock cost | Default active? | Status |
|---|---|---|---|---|
| `StarterFeed` | Your First Post | Free | Yes | ✅ |
| `QuickTokPlatform` (was `ClipzPlatform`, renamed this pass) | QuickTok | 500 `Followers`, plus unlock **Lock**: level ≥ 12 AND own 1× `StoryFeed` | No | ✅ |
| `BroadcastPlatform` (was `ChroniclePlatform`, renamed this pass) | Broadcast | 5,000 `Cash`, plus unlock **Lock**: level ≥ 34 AND own 1× `ReactionContent` | No | ✅ |

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

### QuickTokPlatform (was `ClipzPlatform`, renamed this pass) — ✅ all 5 tiers built (Batch B complete)

**⚠️ NAMING FLAG — RESOLVED:** a Batch B task referred to this platform as "QuickTok" throughout, but at that time no asset anywhere renamed `ClipzPlatform` to that name. A later task explicitly approved the Location rename; it is now done (see the flag at the top of the Locations section above). Section header updated to match.

**Level Progression data — confirmed against the locked table.** At Batch B build time, no Level Progression table existed anywhere in this project to check the platform-unlock condition against; the value used (level ≥ 12 AND own 1× `StoryFeed`) was taken from the task's own explicit instruction text with no independent source to verify against. The Level Progression table has since been added to this doc (see the new section below) and lists Level 12 = "QuickTok platform unlocks (Trend Chase), Own 1x Story Feed" — **this matches exactly** what was built. No drift found.

**Locks wired this pass:** same compound businessLock+playerLevelLock AND pattern as Yourgram. `TrendChase` (platform entry point) verified to have no ownership lock — unchanged. `ReactionContent` had no existing lock to replace (checked the actual asset first, per instructions — it was empty, not populated as the task's phrasing implied) — the new compound lock was simply added fresh. The `QuickTokPlatform` Location's own unlock **Lock** (level+ownership gate) is separate from and additional to its existing 500-`Followers` unlock **cost** (currency price) — both fields left independently intact.

| Asset name | Base cost | Produces | Time | Manager | Lock | Notes |
|---|---|---|---|---|---|---|
| `TrendChase` (name already correct, unchanged) | 50 `Cash` | 12 `Followers` | 10s | `DanaTrent` | none (platform entry point, verified) | costMultiplier 1.15 |
| `CollabFarming` (new tier 2) | 120 `Cash` | 20 `Followers` | 15s | `CocoLabor` (new) | own 10× `TrendChase` AND level ≥ 18 | costMultiplier 1.15. **Phase 6 placeholder numbers (my own inference)** — cost geometrically interpolated between TrendChase (50) and ReactionContent (300); produces Followers (matching the closer tier) rather than Cash |
| `ReactionContent` (was `DuetFarm`) | 300 `Cash` | 6 `Cash` | 25s | `ReggieReactington` (unchanged) | own 15× `CollabFarming` AND level ≥ 28 | costMultiplier 1.15, production input: 20 `Followers`/round (unchanged) |
| `AlgorithmBaiting` (new tier 4) | 700 `Cash` | 15 `Cash` | 40s | `AlGorithm` (new) | own 20× `ReactionContent` AND level ≥ 42 | costMultiplier 1.15. **Phase 6 placeholder numbers** — cost geometrically interpolated between ReactionContent (300) and ViralChallenge (1,500) |
| `ViralChallenge` (was `ViralAttempt`, QuickTok capstone) | 1,500 `Cash` | 40 `Followers` + 10 `Cash` (guaranteed, `DropStrategy.All`) | 60s | `ValIral` (new — confirmed had no manager before this pass, checked the actual asset rather than trusting the earlier build notes) | own 25× `AlgorithmBaiting` AND level ≥ 65 | costMultiplier 1.15, bonus drop-table output deferred to Phase 6 balancing (unchanged) |

### BroadcastPlatform (was `ChroniclePlatform`, renamed this pass) — ✅ all 5 tiers built (Batch C complete, final platform)

**⚠️ NAMING FLAG — RESOLVED:** a Batch C task referred to this platform as "Broadcast" throughout, but at that time no asset anywhere was named that — the Location asset was still `ChroniclePlatform` (display name "The Chronicle (long-form)"), unrenamed, since no rename was requested or approved during that pass. A later task explicitly approved the rename as the second and final exception under the same one-time rule used for Clipz→QuickTok (no player has ever loaded a save). That rename has now been performed: `ChroniclePlatform.asset` → `BroadcastPlatform.asset` (GUID unchanged: `41d6bd6502c27b44a815c989b8f53ae1`; `metadata.name` updated from `"The Chronicle (long-form)"` to `"Broadcast"`). All 5 Broadcast Business assets' `location` field, and the platform's own unlock Lock (`businessLock` → `ReactionContent`), re-verified via fresh disk reads pointing at the same GUID post-rename — no drift. Section header updated to match.

**Level Progression data — confirmed against the locked table before wiring**, not guessed: Level 34 = "Broadcast platform unlocks (Hot Take), Own 1x Reaction Content"; Level 55 = "Broadcast: Unboxing Haul, Own 10x Hot Take"; Level 80 = "Broadcast: Streaming, Own 15x Unboxing Haul"; Level 110 = "Broadcast: Sponsor Deals, Own 20x Streaming"; Level 150 = "Broadcast: Own Brand (capstone), Own 25x Sponsor Deals" — all read directly from the Level Progression table below before any Lock was wired.

**Locks wired this pass:** same compound businessLock+playerLevelLock AND pattern as Yourgram/QuickTok. `HotTake` (platform entry point) verified to have `locks: []` — left unchanged. `SponsorDeals` **did** have an existing lock — a `businessUpgradeLevelLock` gating on `HotTake` reaching upgrade level 3 — confirmed via direct read before touching anything; per this task's explicit instruction it was replaced (not layered on top of) with the new compound own-20x-`Streaming`-AND-level≥110 lock. `BroadcastPlatform`'s own unlock **Lock** (level+ownership gate) is separate from and additional to its existing 5,000-`Cash` unlock **cost** — both fields left independently intact.

**Manager pre-check honored:** `SponsorDeals.manager` was confirmed `{fileID: 0}` (empty) both by a direct read before starting and by a defensive re-check inside the creation script itself, before creating/wiring `DeeSponsorman` — matching the task's "confirm first, stop and report rather than overwriting" instruction. No existing manager was found, so no stop was needed.

**Build note (caught and fixed mid-batch):** the first creation pass set `location`, `manager`, and `locks` on the three newly-created businesses *after* `AssetDatabase.CreateAsset()`, but only called `EditorUtility.SetDirty()` on the pre-existing assets it modified (`HotTake`, `SponsorDeals`, the platform Location) — not on the three new ones. Unity does not auto-mark a `ScriptableObject` dirty on direct field writes after creation, so `AssetDatabase.SaveAssets()` silently skipped those three fields on all three new businesses. Caught by a fresh post-save disk read (not trusting the creation script's own success log), fixed with an explicit repair pass (`EditorUtility.SetDirty()` on all three, re-saved), and re-verified via a second fresh disk read plus independent GUID cross-reference before reporting done.

| Asset name | Base cost | Produces | Time | Manager | Lock | Notes |
|---|---|---|---|---|---|---|
| `HotTake` (was `LongformEssay`) | 2,000 `Cash` | 25 `Cash` | 90s | `HattieTakerson` (unchanged) | none (platform entry point, verified) | costMultiplier 1.15, upgradeCost 150 `Cash`/level (unchanged), `autoUpgradeForFree` = false |
| `UnboxingHaul` (new tier 2) | 4,000 `Cash` | 40 `Cash` | 120s | `FoxyHaulwell` (new) | own 10× `HotTake` AND level ≥ 55 | costMultiplier 1.15. **Phase 6 placeholder numbers (my own inference)** — cost is the exact geometric mean of HotTake (2,000) and SponsorDeals (8,000); output/time interpolated and rounded to clean values |
| `Streaming` (new tier 3) | 6,000 `Cash` | 50 `Cash` | 150s | `CasFlowton` (new) | own 15× `UnboxingHaul` AND level ≥ 80 | costMultiplier 1.15. **Phase 6 placeholder numbers (my own inference)** — interpolated between UnboxingHaul (4,000) and SponsorDeals (8,000) |
| `SponsorDeals` (was `SubscriberFunnel`) | 8,000 `Cash` | 60 `Cash` | 180s | `DeeSponsorman` (new — confirmed had no manager before this pass) | own 20× `Streaming` AND level ≥ 110 (replaced old `businessUpgradeLevelLock`-on-HotTake lock) | costMultiplier 1.15 |
| `OwnBrand` (new tier 5, capstone — also the highest-tier business in all of v1) | 24,000 `Cash` | 150 `Cash` | 300s | `BaronVonBrand` (new) | own 25× `SponsorDeals` AND level ≥ 150 | costMultiplier 1.15. **Phase 6 placeholder numbers (my own inference)** — 3x/2.5x SponsorDeals' cost/output as capstone scaling, deliberately the highest cost/output of any business in the game (exceeds `VerifiedStatus` and `ViralChallenge`) |

## Business Groups — ✅ built

| Asset name | Members | Purpose | Location |
|---|---|---|---|
| `ClipzContentGroup` | `TrendChase`, `ReactionContent` (was `DuetFarm`) | Scoping for speed-type upgrades (contextual filters don't work for speed effects; group targeting does) | `Resources/BusinessGroups/` |

## Managers — ✅ 11 built and functional (auto-leveling from duplicates is native, confirmed)

**Renamed** (same approved one-time exception as the Businesses above). Old names kept in parentheses.

| Asset name | Automates | autoProduceOnLevel | autoCollectOnLevel | Rarity |
|---|---|---|---|---|
| `RonnyRingo` (was `GhostwriterManager`) | `FirstPost` | 0 | 0 | Common |
| `FayeFilterton` | `SelfieSession` | 0 | 0 | Common |
| `ToriSteller` (was `SocialMediaManager`) | `StoryFeed` | 1 | 0 | Common |
| `FarmerBob` | `EngagementFarming` | 0 | 0 | Common |
| `ChuckVerifiington` | `VerifiedStatus` | 0 | 0 | Common |
| `DanaTrent` (was `EditorManager`) | `TrendChase` | 0 | 0 | Common |
| `CocoLabor` (new) | `CollabFarming` | 0 | 0 | Common |
| `ReggieReactington` (was `TalentAgentManager`) | `ReactionContent` | 2 | 1 | Common |
| `AlGorithm` (new) | `AlgorithmBaiting` | 0 | 0 | Common |
| `ValIral` (new — confirmed manager-less before this pass) | `ViralChallenge` | 0 | 0 | Common |
| `HattieTakerson` (was `PublicistManager`) | `HotTake` | 0 | 0 | Common |
| `FoxyHaulwell` (new) | `UnboxingHaul` | 0 | 0 | Common |
| `CasFlowton` (new) | `Streaming` | 0 | 0 | Common |
| `DeeSponsorman` (new — confirmed manager-less on `SponsorDeals` before this pass) | `SponsorDeals` | 0 | 0 | Common |
| `BaronVonBrand` (new) | `OwnBrand` | 0 | 0 | Common |

Every manager-to-business assignment above was re-verified by raw GUID cross-reference (not name matching) directly against each `Business.manager` field — all 15 confirmed correct, no mismatches found. All new managers follow the exact same cost/lock/upgrade pattern as the pre-existing Common managers (free acquisition — `cost: []` — per the Capsule-based acquisition model; default `passiveBoosts` = speed ×2, Manager.cs's own field default, left unchanged as a Phase 6 placeholder like the others).

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

## Level Progression (locked)

| Level | Unlocks | Type | Ownership gate |
|---|---|---|---|
| 1 | Yourgram: First Post | Business | Free, no gate |
| 3 | Yourgram: Selfie Session | Business | Own 10x First Post |
| 7 | Yourgram: Story Feed | Business | Own 15x Selfie Session |
| 12 | QuickTok platform unlocks (Trend Chase) | Platform | Own 1x Story Feed |
| 14 | Yourgram: Engagement Farming | Business | Own 20x Story Feed |
| 18 | QuickTok: Collab Farming | Business | Own 10x Trend Chase |
| 24 | Yourgram: Verified Status (capstone) | Business | Own 25x Engagement Farming |
| 28 | QuickTok: Reaction Content | Business | Own 15x Collab Farming |
| 34 | Broadcast platform unlocks (Hot Take) | Platform | Own 1x Reaction Content |
| 42 | QuickTok: Algorithm Baiting | Business | Own 20x Reaction Content |
| 55 | Broadcast: Unboxing Haul | Business | Own 10x Hot Take |
| 65 | QuickTok: Viral Challenge (capstone) | Business | Own 25x Algorithm Baiting |
| 80 | Broadcast: Streaming | Business | Own 15x Unboxing Haul |
| 110 | Broadcast: Sponsor Deals | Business | Own 20x Streaming |
| 150 | Broadcast: Own Brand (capstone, end of v1 content) | Business | Own 25x Sponsor Deals |

All levels grant a Level-Up Reward (CloutCoin) regardless of whether new content unlocks. All numbers are Phase 6 placeholders; the shape/order is locked.

**Cross-checked against Batch A/B (built content, levels 1–65) via fresh reads of every asset's `locks` field on 2026-08-14 — confirmed no drift:**

| Level | Table says | Asset `locks` field (actual) | Match? |
|---|---|---|---|
| 3 | Own 10x First Post | `SelfieSession`: OwnershipLock(FirstPost,10) + LevelLock(3) | ✅ |
| 7 | Own 15x Selfie Session | `StoryFeed`: OwnershipLock(SelfieSession,15) + LevelLock(7) | ✅ |
| 12 | Own 1x Story Feed | `QuickTokPlatform`: LevelLock(12) + OwnershipLock(StoryFeed,1) | ✅ |
| 14 | Own 20x Story Feed | `EngagementFarming`: OwnershipLock(StoryFeed,20) + LevelLock(14) | ✅ |
| 18 | Own 10x Trend Chase | `CollabFarming`: OwnershipLock(TrendChase,10) + LevelLock(18) | ✅ |
| 24 | Own 25x Engagement Farming | `VerifiedStatus`: OwnershipLock(EngagementFarming,25) + LevelLock(24) | ✅ |
| 28 | Own 15x Collab Farming | `ReactionContent`: OwnershipLock(CollabFarming,15) + LevelLock(28) | ✅ |
| 42 | Own 20x Reaction Content | `AlgorithmBaiting`: OwnershipLock(ReactionContent,20) + LevelLock(42) | ✅ |
| 65 | Own 25x Algorithm Baiting | `ViralChallenge`: OwnershipLock(AlgorithmBaiting,25) + LevelLock(65) | ✅ |
| 34 | Own 1x Reaction Content | `BroadcastPlatform`: LevelLock(34) + OwnershipLock(ReactionContent,1) | ✅ |
| 55 | Own 10x Hot Take | `UnboxingHaul`: OwnershipLock(HotTake,10) + LevelLock(55) | ✅ |
| 80 | Own 15x Unboxing Haul | `Streaming`: OwnershipLock(UnboxingHaul,15) + LevelLock(80) | ✅ |
| 110 | Own 20x Streaming | `SponsorDeals`: OwnershipLock(Streaming,20) + LevelLock(110) | ✅ |
| 150 | Own 25x Sponsor Deals | `OwnBrand`: OwnershipLock(SponsorDeals,25) + LevelLock(150) | ✅ |

All 15 unlock gates across all 3 platforms (levels 1–150) are now built and cross-checked against this table — no drift found anywhere. Batch C (`BroadcastPlatform`) was the final platform; v1's full Business/Manager/Lock content is complete.

## First-hour pacing target

| Milestone | Target time |
|---|---|
| First `FirstPost` purchase (was `SelfieSession`) | Immediate |
| First `StoryFeed` unlock (was `StoryPost`) | ~2 min |
| First Manager owned | ~5 min |
| `QuickTokPlatform` unlock | ~15 min |
| First Boost use | ~20 min |
| First Shop interaction | ~25 min |
| `BroadcastPlatform` unlock | ~45–60 min |