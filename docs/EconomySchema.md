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
| `CloutCoin` | Premium currency — earned via level-up rewards, purchasable via IAP, spent on Manager Capsules | Uncapped | No — excluded (added to `PrestigeManager.exludedCurrencies` alongside `Aura`, which was confirmed still present before the edit — both now excluded) | ✅ |

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
| `YourgramAllBusinesses` (new, Batch D) | `FirstPost`, `SelfieSession`, `StoryFeed`, `EngagementFarming`, `VerifiedStatus` (all 5) | Platform-wide targeting for Rare Manager `TheAlgorithmWhisperer`'s passive boost | `Resources/BusinessGroups/` |
| `QuickTokAllBusinesses` (new, Batch D) | `TrendChase`, `CollabFarming`, `ReactionContent`, `AlgorithmBaiting`, `ViralChallenge` (all 5) | Platform-wide targeting for Rare Manager `TheCloutChaser`'s passive boost | `Resources/BusinessGroups/` |
| `BroadcastAllBusinesses` (new, Batch D) | `HotTake`, `UnboxingHaul`, `Streaming`, `SponsorDeals`, `OwnBrand` (all 5) | Platform-wide targeting for Rare Manager `ThePrestigePress`'s passive boost | `Resources/BusinessGroups/` |

**⚠️ Scoped exception used this pass:** `BusinessGroup` (and its base `ItemGroup<T>`) has no member-list field of its own — confirmed via direct read of both `BusinessGroup.cs` and `ItemGroup.cs`. Group membership is defined entirely from the Business side, via each `Business.businessGroups` list. Populating the 3 groups above therefore required appending one entry to `businessGroups` on all 15 existing Business assets. This directly conflicted with this task's own "no changes to any business asset" hard boundary; flagged and explicitly authorized by the user as a narrowly scoped exception — **`businessGroups` only, nothing else** — before proceeding. Verified via `git diff` after the fact: all 15 changed files show exactly one field touched (an empty `businessGroups: []` becoming a 1-entry list, or — for `TrendChase`/`ReactionContent`, which already carried `ClipzContentGroup` — a second entry appended alongside the first). No cost, output, timing, lock, or manager field touched on any of the 15.

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

**Rarity — ✅ system built.** `Rarity` enum (`InfluencerRise.Managers`, `Assets/InfluencerRise/Scripts/Managers/Rarity.cs`): originally `Common`/`Rare`/`Epic`/`Mythic`. **Renamed after Batch E** (Mythic-tier managers already built by that point): `Epic` removed entirely (never part of the actual locked design — a leftover default value from the original enum stub, confirmed via project-wide grep that zero code branched on it), `Mythic` renamed to `Legendary` — the locked design language throughout every doc, now matching the enum too. Locked value now: `Common`/`Rare`/`Legendary` (3 tiers). `Legendary` is explicitly pinned to integer `3` (`Legendary = 3`) rather than left to auto-renumber to `2`, so the 3 `ManagerRarity` assets already serialized against the old `Mythic = 3` value keep deserializing correctly — verified via a live C# read post-rename, all 3 report `Legendary` with no data loss. Assigned via a companion `ManagerRarity` ScriptableObject per Manager (`Assets/InfluencerRise/Scripts/Managers/ManagerRarity.cs`, one instance per Manager under `Resources/ManagerRarities/`, also renamed this pass: `RonnyRingoRarity`, `ToriStellerRarity`, `DanaTrentRarity`, `ReggieReactingtonRarity`, `HattieTakersonRarity`) rather than a field on `Manager` itself or a subclass — `Manager.cs` is vendor code and the 5 assets above already exist as plain `Manager`-typed, name-locked assets, so re-typing them via subclassing was ruled out as unnecessarily invasive. `Manager.managerGroups` was also ruled out as an attachment point since it's an active functional dependency of `PrestigeManager.excludedManagerGroups`. All 5 Managers above are currently assigned **Common** — a placeholder, not a design decision on which managers should be rarer; that's a future task once actual Rare/Legendary manager content is designed (now built, see below).

## Manager Rarity — mechanics (revised per audit)

**Leveling curve is universal, not rarity-dependent.** Every Manager, regardless of rarity, levels on the same fixed geometric curve (1, 2, 4, 8... copies doubling per level — native to IUpgradable, not configurable per-asset without custom code). Rarity does NOT change how many copies are needed to level up.

**What rarity actually controls instead:**
1. **Pull odds** — how likely this Manager is to drop from a Capsule, via `Output.weight` on the Capsule's weighted DropTable. Rarer = lower weight = harder to pull. This is the primary "feels rare" lever.
2. **Power per level** — `businessSpeedMultiplier` (and other native per-Manager fields) configured stronger on higher-rarity Managers, so a Rare/Legendary pull is worth more even before factoring in how hard it was to get. Fully native, freely configurable per Manager asset today.

**No max level, no salvage system at launch.** Managers level infinitely (native default, `MaxLevel = BigNumber.MaxValue`). A maxed-out Legendary just keeps getting stronger — there's no cap to hit, so no "wasted duplicates" problem to solve. Revisit as a possible V1.1 feature only if real player data suggests infinite scaling feels ungrounded rather than rewarding — not a launch requirement.

**`ManagerRarity` companion asset's actual job:** a rarity tag + Capsule-pull-weight lookup value for our own Capsule-building logic to read from. It does NOT and cannot feed the native leveling system (confirmed: `IUpgradableHolder` never reads from it) — kept deliberately simple rather than holding dead fields that look functional but aren't.

**Manager acquisition model — REVISED (see Roadmap Phase 3):** originally planned as direct-currency purchase. Now redesigned per the AdVenture Communist reference: Managers are acquired via **weighted Capsule pulls** (Shop Items with `DropStrategy.WeightedRandom`, confirmed structurally supported by code inspection though not yet proven with a working example — proof-of-concept required before full build).

## Rare Managers — ✅ all 6 built (Batch D)

**Investigation performed before this batch (read-only pass):** confirmed `Manager.passiveBoosts` is `List<UpgradeBlock>` — the identical type `Upgrade.upgrades` uses, not a separate/weaker structure. Confirmed `UpgradeBlockFilter.targetBusinessGroups` (`List<BusinessGroup>`) is reachable from a Manager's passive boost, so platform-wide effects are possible the same way `FasterEditingSoftware` already uses `ClipzContentGroup` — but only by targeting a `BusinessGroup` containing every business on that platform, which none of the existing groups did (see the Business Groups section above). Confirmed `UpgradeType.purchaseCost`/`playerXp`/`businessXp` exist natively and work globally with empty filters, same convention as `BulkContentBatching`. Confirmed, by direct read of `Output.cs`, `UpgradeType.cs`, and `UpgradeBlockFilter.cs`, that **no** native path exists from a Manager (or Upgrade, or Output) to a CustomStat like `Burnout` — same gap already logged for `TherapySessionUpgrade` in `EngineCapabilities.md`.

| Asset name | Automates | Effect | Rarity |
|---|---|---|---|
| `TheAlgorithmWhisperer` | none (passive-only) | +40%/level production, all `YourgramAllBusinesses` (`passiveBoosts`: `UpgradeType.production`, value 1.4, `targetBusinessGroups` = [`YourgramAllBusinesses`]) | Rare |
| `TheCloutChaser` | none (passive-only) | +40%/level production, all `QuickTokAllBusinesses` | Rare |
| `ThePrestigePress` | none (passive-only) | +40%/level production, all `BroadcastAllBusinesses` | Rare |
| `TheBudgetBestie` | none (passive-only) | Global purchase cost ×0.85 (-15%). **Phase 6 placeholder (my own inference)** — no percentage was specified for this one; picked stronger than `BulkContentBatching`'s -10% to reflect Rare tier, empty `filters` = global | Rare |
| `TheGrowthHacker` | none (passive-only) | Global player XP ×1.25 (+25%). **Phase 6 placeholder (my own inference)** — same reasoning, no percentage specified, empty `filters` = global | Rare |
| `TheWellnessGuru` | none (passive-only) | **No `passiveBoosts` entry** — confirmed engine gap (CustomStat not a valid UpgradeBlock target). Effect implemented entirely in `BurnoutController.cs` instead (see below) | Rare |

The "+40%/level" platform managers' per-level scaling comes from the unmodified `businessSpeedMultiplier` field default (2, geometric per level) — same as every existing Common manager; the task didn't ask this to be changed, so it wasn't. `passiveBoosts[0].value = 1.4` is the level-1 base.

All 6 confirmed manager-less (no Business.manager points to any of them) and all 6 `ManagerRarity` companions (`TheAlgorithmWhispererRarity`, `TheCloutChaserRarity`, `ThePrestigePressRarity`, `TheBudgetBestieRarity`, `TheGrowthHackerRarity`, `TheWellnessGuruRarity`) created with `rarity: 1` (Rare), each cross-referenced by GUID against its manager.

**`TheWellnessGuru`'s effect — `BurnoutController.cs` extension (Responsibility 4):** the controller gained a `[SerializeField] private Manager wellnessGuru` field (wired to `TheWellnessGuru.asset` on the scene's `BurnoutController` component) and a `GetEffectiveDecayIntervalSeconds()` method, used in place of the raw `DecayIntervalSeconds` constant inside the existing decay tick in `Update()`. **Placeholder curve, explicitly flagged, not balance-tested:** `effectiveInterval = DecayIntervalSeconds / (1 + level)` — level 1 halves the decay interval, level 2 divides it by 3, level 3 by 4, and so on. When `wellnessGuru` is unassigned or not yet owned (`level <= 0`), the method returns the unmodified `DecayIntervalSeconds`, so Responsibilities 1–3's existing behavior is byte-for-byte unchanged until the manager is actually acquired. XML doc comments added for the class summary and the new method, matching the file's existing per-responsibility documentation pattern; Responsibilities 2 and 3 (`HandleRebrand`, `HandleShopItemRewardsGranted`) were not touched.

## Legendary Managers — ✅ all 3 built (Batch E, final manager batch — 24-manager roster complete)

**⚠️ NAMING FLAG — RESOLVED (Batch F):** at build time, this batch's task referred to these as "Legendary" Managers, but `Rarity.cs` had exactly 4 values — `Common`/`Rare`/`Epic`/`Mythic` — no `Legendary` value existed. Treated "Legendary" as the working name for the enum's top tier, `Mythic`, and built these 3 managers with `rarity: 3` on that basis. A later task confirmed "Legendary" is in fact the locked design language throughout every doc, that `Epic` was never part of the actual design (leftover default value), and explicitly approved renaming the enum: `Mythic` → `Legendary`, `Epic` removed, `Legendary` pinned to its original integer value (3) so already-serialized assets keep working. That rename is now done — see the Rarity system note above. The table and prose below have been updated from "Mythic" to "Legendary" throughout to match; no functional change, naming only.

**`TheNepoBaby` targeting — investigated before building, per explicit instruction.** Confirmed via full read of `UpgradeBlockFilter.cs`, `UpgradeEffectContext.cs`, and `UpgradeEffectResolver.cs` that the design intent ("broadly boost the player's odds at rarer Capsule tiers") **cannot be cleanly expressed with native filter options**, for two independent reasons: (1) Rarity has no native representation anywhere in the `Output`/`UpgradeBlockFilter`/`UpgradeEffectContext` system — it is purely our own `ManagerRarity` companion asset, invisible to the engine's filter/context code; (2) unlike Business/Currency/Upgrade/Boost, which all pair an exact-target field with a `List<Group>` field, `Manager` targeting has only `targetManager` (single, exact) — no `targetManagerGroups` counterpart exists, even though `Manager.managerGroups` exists as a field. A fully global (empty-filter) `dropWeight` block was also confirmed mathematically inert for this goal: uniformly scaling every weight in a weighted-random `DropTable` leaves relative odds (`weight_i / sum(weights)`) completely unchanged.

Presented to the user as a stop-and-ask per the task's own instruction. Decision: build `TheNepoBaby` with **individual per-manager targeting** — one explicit `UpgradeBlock` per target, `UpgradeType.dropWeight`, `filters.targetManager` pinned to each target — rather than any group/rarity-based mechanism (none exists) or deferring to future custom code.

**Count discrepancy flagged, not silently resolved:** the task's instruction said "9 separate UpgradeBlock entries" but then enumerated "the 6 Rare Managers + the other 2 Legendary Managers" — 6 + 2 = **8**, not 9. Built exactly the enumerated, unambiguous set of 8 (`TheAlgorithmWhisperer`, `TheCloutChaser`, `ThePrestigePress`, `TheBudgetBestie`, `TheGrowthHacker`, `TheWellnessGuru`, `TheMainCharacter`, `TheAlgorithmItself`) rather than inventing an unspecified 9th target. All 8 confirmed via fresh disk read, each entry's `targetManager` GUID cross-referenced against the actual target asset's `.meta` file — all 8 correct, `TheNepoBaby` correctly excludes itself.

**Manual-maintenance tradeoff — accepted for launch scope:** because `UpgradeBlockFilter` has no group-based Manager targeting, `TheNepoBaby`'s boost is enumerated per-manager (8 explicit `UpgradeBlock` entries) rather than expressed as a single group reference. **Any future Rare or Legendary manager added to the roster must get a matching entry manually added to `TheNepoBaby.passiveBoosts`, or it will not benefit from the luck bonus.** There is no automated way to keep this in sync; it is a deliberate, accepted tradeoff for launch scope, not an oversight.

| Asset name | Automates | Effect | Rarity |
|---|---|---|---|
| `TheMainCharacter` | none (passive-only) | Global production ×1.6 (+60%), empty filters. **Phase 6 placeholder**, per task-specified value | Legendary |
| `TheAlgorithmItself` | none (passive-only) | Global speed ×1.6 (+60%), empty filters. **Phase 6 placeholder**, per task-specified value | Legendary |
| `TheNepoBaby` | none (passive-only) | 8× `UpgradeType.dropWeight` blocks, value 1.3 (+30%/level) each, one per target manager (see above). **Phase 6 placeholder value**, explicitly untested — no Capsule DropTable exists yet to test against | Legendary |

All 3 confirmed manager-less (no Business.manager points to any of them) and all 3 `ManagerRarity` companions (`TheMainCharacterRarity`, `TheAlgorithmItselfRarity`, `TheNepoBabyRarity`) created with `rarity: 3` (Legendary, was Mythic — see the Rarity system rename note above), each cross-referenced by GUID against its manager. **24-manager roster complete: 15 Common + 6 Rare + 3 Legendary.**

**Never-via-ad enforcement:** see `MonetizationPlan.md`'s Manager Capsules section — a hard constraint documented there (Legendary-tier entries must never appear in a Watch-Ad Capsule's DropTable) for the future Capsule-building task to follow, since no Capsule Shop Items exist yet to enforce this in code today.

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

## Shop Items — ✅ 12 built (7 core + 5 real Capsule tiers, POC replaced with production assets)

| Asset name | Type | Grants | Status |
|---|---|---|---|
| `FollowerPackSmall` | IAP | 1,000 Followers | ✅ (productId deferred to Phase 4) |
| `CashPackSmall` | IAP | 500 Cash | ✅ (productId deferred to Phase 4) |
| `SpaDayBurnoutReliefAd` | Ad | -40 Burnout (via BurnoutController) | ✅ |
| `SpaDayBurnoutReliefPaid` | IAP | -40 Burnout (via BurnoutController) | ✅ (productId deferred to Phase 4) |
| `RemoveAdsPermanent` | IAP | No economy grant — ownership itself is the flag (`GetItemAmount > 0`), maxPurchases = 1 | ✅ |
| `RewardedBoostAd` | Ad | `AlgorithmPushBoost` | ✅ |
| `FreeCapsule` | Free | 1 Manager, weighted (`DropStrategy.WeightedRandom`, `totalDropAmount=1`, all 24 Managers as `Output` entries): Common weight 10, Rare weight 2, **Legendary weight 0** | ✅ — **zero-Legendary-weight verified**, see below |
| `WatchAdCapsule` | Ad | Same pool, weighted better toward Rare: Common weight 6, Rare weight 4, **Legendary weight 0** | ✅ — **zero-Legendary-weight verified**, see below |
| `CloutCoinCapsuleLow` | In-game cost | 50 `CloutCoin`. Same pool, Legendary now reachable: Common weight 4, Rare weight 5, Legendary weight 1 | ✅ — cost is a **Phase 6 placeholder (my own inference)**, no amount was specified |
| `CloutCoinCapsuleHigh` | In-game cost | 250 `CloutCoin`. Same pool, best free-currency odds: Common weight 1, Rare weight 5, Legendary weight 4 | ✅ — cost is a **Phase 6 placeholder (my own inference)**, no amount was specified; 5× `CloutCoinCapsuleLow`'s cost |
| `IAPCapsule` | IAP | Same pool, best odds overall: Common weight 0, Rare weight 5, Legendary weight 5 | ✅ (productId deferred to Phase 4). **Weights are my own inference** — task asked for "same as or better than `CloutCoinCapsuleHigh`"; chose to remove Common entirely and even the Rare/Legendary split, reasoning that a real-money shortcut should feel categorically better, not just marginally |

**Weighted-manager-output mechanism — statistically confirmed, not just structurally sound.** `docs/EngineCapabilities.md` had this listed as "unproven by any working example" (a stale/inaccurate state — a prior commit's message claimed it was "confirmed" but the actual doc diff in that commit shows the "unproven" caveat being *added*, not resolved, and it was still unresolved at the start of this task). Rather than build 5 production Shop Items on an unverified mechanism, ran the actual proof-of-concept myself before building anything: `DropTable.GenerateDrops()` is a pure synchronous method, callable directly in Edit Mode with no Play Mode needed. Built an in-memory 3-entry weighted `DropTable` (Common weight 10 / Rare weight 5 / Legendary weight 1) using real Manager assets and ran it 20,000 times: observed 62.6% / 31.1% / 6.3% against expected 62.5% / 31.25% / 6.25% — all three outcomes reachable, distribution matches weights within 2%. Separately ran a zero-weight exclusion test (10,000 trials, one output at weight 0): **0 hits**, confirming a `weight: 0` entry is genuinely never selected — the exact mechanism `FreeCapsule`/`WatchAdCapsule`'s Legendary-exclusion depends on. `docs/EngineCapabilities.md` updated to reflect this confirmed finding (see that file).

**Zero-Legendary-weight verification on `FreeCapsule` and `WatchAdCapsule` — done via two independent methods, both after a fresh disk/asset-database load (not reusing any in-memory reference from the creation script):**
1. A C# script built a Manager→Rarity lookup from a fresh load of all 24 `ManagerRarity` assets, freshly loaded both Shop Items via `AssetDatabase.LoadAssetAtPath`, and asserted every Output whose Manager resolves to `Rarity.Legendary` has `weight == 0`. Result: both capsules have 24 total outputs, 3 correctly-present-but-zero-weight Legendary entries each, zero violations.
2. Independently, raw `grep` directly against the saved `.asset` YAML for all 3 Legendary managers' GUIDs in both files, reading the literal `weight:` line each time. All 6 (3 managers × 2 capsules) read `weight: 0`.

**Cooldown/claim-interval — confirmed does not exist, flagged rather than faked.** `ShopItem.cs` was read in full: its only limiting field is `maxPurchases` (a lifetime total cap, 0 = infinite) — no `claimCooldownSeconds` or any recurring-interval field exists anywhere on the class. Per the task's explicit instruction not to invent a fake cost to simulate one, `FreeCapsule`'s reward pool was built correctly, but **the "long cooldown timer" behavior described in this doc's Manager Capsules design intent is NOT implemented** — as built, `FreeCapsule` is purchasable with no gating at all once exposed in UI. This needs a small custom script (same category as `BurnoutController.cs` — a `MonoBehaviour` tracking last-claim-timestamp per Shop Item) as a separate future task, not attempted this batch.

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