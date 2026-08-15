# Economy Schema — Certified Aura: Idle Influencer Tycoon

Single source of truth for every persisted asset name, category, unlock order, and build status. Names are locked once created. All numeric values are Phase 6 balancing placeholders unless stated otherwise — the *structure* is locked, the *numbers* are not.

---

## Currencies — ✅ all 5 built

| Asset name | Purpose | Cap | Reset on Rebrand? |
|---|---|---|---|
| `Followers` | Primary production currency | Uncapped | Yes |
| `Cash` | Secondary currency | Uncapped | Yes |
| `Aura` | Prestige currency | Uncapped | No — excluded |
| `CloutCoin` | Premium currency, spent on Capsules | Uncapped | No — excluded |
| `Burnout` (Custom Stat) | 0–100, decays passively, ✅ rises with production (built — `BurnoutController.cs` Responsibility 5, tier-scaled: gain = business tier 1-5 × 0.5, placeholder, Phase 6 tunable), resets on Rebrand | 0–100 | Yes → 0 |

---

## Locations (Platforms) — ✅ all 3 built and correctly named

| Asset name | Display | Unlock condition | Default active? |
|---|---|---|---|
| `StarterFeed` | Yourgram | Free | Yes |
| `QuickTokPlatform` | QuickTok | Level ≥12 AND own 1x `StoryFeed` | No |
| `BroadcastPlatform` | Broadcast | Level ≥34 AND own 1x `ReactionContent` | No |

---

## Businesses — ✅ all 15 built, full Lock chains wired, ✅ full Cash-bootstrap trace complete (all 3 stranded businesses fixed)

**Cost currency note:** every business's `Cost` column below is real asset data, confirmed via direct read this session — not the original design intent, which used Cash for every QuickTok/Broadcast business including their entry points. Three QuickTok businesses (`TrendChase`, `CollabFarming`, `ReactionContent`) had their cost **currency** changed from Cash to Followers because each was "stranded" — costing a currency with zero prior source anywhere earlier in the reachable chain, an unreachable circular dependency confirmed via `EconomySimulator.cs`. `costMultiplier` (1.15 throughout) was never touched. Full dependency trace and evidence in `docs/BugTracker.md`'s "QuickTok Cash-bootstrap chain" entry — every other business (all 5 Yourgram, QuickTok's `AlgorithmBaiting`/`ViralChallenge`, all 5 Broadcast) was confirmed, not assumed, to already have a valid currency source in the reachable chain and needed no change.

### Yourgram

| Asset name | Display | Manager | Cost | Lock |
|---|---|---|---|---|
| `FirstPost` | First Post | Ronny Ringo | 1 Followers | None (free start) |
| `SelfieSession` | Selfie Session | Faye Filterton | 3 Followers | Own 10x FirstPost AND Level ≥3 |
| `StoryFeed` | Story Feed | Tori Steller | 15 Followers | Own 15x SelfieSession AND Level ≥7 |
| `EngagementFarming` | Engagement Farming | Farmer Bob | 100 Followers | Own 20x StoryFeed AND Level ≥14 |
| `VerifiedStatus` | Verified Status (capstone) | Chuck Verifiington | 500 Followers | Own 25x EngagementFarming AND Level ≥24 |

### QuickTok

| Asset name | Display | Manager | Cost | Lock |
|---|---|---|---|---|
| `TrendChase` | Trend Chase | Dana Trent | **300 Followers** (was 50 Cash — fixed, stranded) | None (platform entry point) |
| `CollabFarming` | Collab Farming | Coco Labor | **800 Followers** (was 120 Cash — fixed, stranded) | Own 10x TrendChase AND Level ≥18 |
| `ReactionContent` | Reaction Content | Reggie Reactington | **4000 Followers** (was 300 Cash — fixed, stranded) | Own 15x CollabFarming AND Level ≥28 |
| `AlgorithmBaiting` | Algorithm Baiting | Al Gorithm | 700 Cash (confirmed OK — ReactionContent already producing Cash by this point) | Own 20x ReactionContent AND Level ≥42 |
| `ViralChallenge` | Viral Challenge (capstone) | Val Iral | 1500 Cash (confirmed OK) | Own 25x AlgorithmBaiting AND Level ≥65 |

### Broadcast

| Asset name | Display | Manager | Cost | Lock |
|---|---|---|---|---|
| `HotTake` | Hot Take | Hattie Takerson | 2000 Cash (confirmed OK — QuickTok's Cash chain already flowing before Broadcast unlocks) | None (platform entry point) |
| `UnboxingHaul` | Unboxing Haul | Foxy Haulwell | 4000 Cash (confirmed OK) | Own 10x HotTake AND Level ≥55 |
| `Streaming` | Streaming | Cas Flowton | 6000 Cash (confirmed OK) | Own 15x UnboxingHaul AND Level ≥80 |
| `SponsorDeals` | Sponsor Deals | Dee Sponsorman | 8000 Cash (confirmed OK) | Own 20x Streaming AND Level ≥110 |
| `OwnBrand` | Own Brand (capstone, end of v1 content) | Baron Von Brand | 24000 Cash (confirmed OK) | Own 25x SponsorDeals AND Level ≥150 |

**Reachability status:** with all 3 fixes applied, `EconomySimulator.cs` confirms the entire 15-business chain cascades correctly end to end (all businesses through `Streaming` purchased in growing quantity within 60 simulated days; `SponsorDeals`/`OwnBrand` confirmed reachable given more time — Level 110 reached at simulated day 240). **Level 110/150 are not reached within a 60-day cap**, but this is no longer a currency-bootstrap problem — extended-cap testing confirms it's the already-tuned exponential XP curve (`levelIncrementMultiplier=1.13`) decelerating sharply past ~Level 90, a separate, known mechanism out of this fix's scope (see BugTracker.md).

*Old asset names (pre-rename batch), for historical reference: `SelfieSession`→`FirstPost`, `StoryPost`→`StoryFeed`, `RingLightSetup`→`EngagementFarming`, `DuetFarm`→`ReactionContent`, `ViralAttempt`→`ViralChallenge`, `LongformEssay`→`HotTake`, `SubscriberFunnel`→`SponsorDeals`. `TrendChase` name unchanged.*

---

## Level Progression (locked structure, placeholder numbers)

| Level | Unlocks | Type | Ownership gate |
|---|---|---|---|
| 1 | Yourgram: First Post | Business | Free |
| 3 | Yourgram: Selfie Session | Business | Own 10x First Post |
| 7 | Yourgram: Story Feed | Business | Own 15x Selfie Session |
| 12 | **QuickTok platform unlocks** | Platform | Own 1x Story Feed |
| 14 | Yourgram: Engagement Farming | Business | Own 20x Story Feed |
| 18 | QuickTok: Collab Farming | Business | Own 10x Trend Chase |
| 24 | Yourgram: Verified Status (capstone) | Business | Own 25x Engagement Farming |
| 28 | QuickTok: Reaction Content | Business | Own 15x Collab Farming |
| 34 | **Broadcast platform unlocks** | Platform | Own 1x Reaction Content |
| 42 | QuickTok: Algorithm Baiting | Business | Own 20x Reaction Content |
| 55 | Broadcast: Unboxing Haul | Business | Own 10x Hot Take |
| 65 | QuickTok: Viral Challenge (capstone) | Business | Own 25x Algorithm Baiting |
| 80 | Broadcast: Streaming | Business | Own 15x Unboxing Haul |
| 110 | Broadcast: Sponsor Deals | Business | Own 20x Streaming |
| 150 | Broadcast: Own Brand (capstone, end of v1 content) | Business | Own 25x Sponsor Deals |

All levels grant a Level-Up Reward (CloutCoin) — ✅ configured, fully native (`PlayerStats.levelupData`, no code). Two layers, both fire together on the same level-up (verified, not mutually overriding):
- **Repeating**: 5 CloutCoin at level 1, scaled linearly by level (`scaleEachLevelReward=true`) — level N grants 5×N. Placeholder, Phase 6 tunable.
- **Exact-level milestone bonuses** at the 14 levels above, tiered by unlock significance (placeholder, Phase 6 tunable): tier-2 business unlock=50, tier-3=100, tier-4=150, platform-tier capstone business=300, platform unlock (12, 34)=750, final v1 capstone (150, Own Brand)=1000.

Reward assets live under `Resources/PlayerStats/LevelRewards/` (`LevelUpCloutCoinRepeating` + 14 `LevelUpCloutCoinBonusLevel{N}` assets).

**Player XP curve — ✅ fixed (was a structural Level-0 deadlock).** `PlayerStats.firstLevelXp`/`levelIncrementMultiplier` were untouched EasyIdleGame package defaults (1000/2) never actually configured for this schedule; combined with Player XP being purchase-only natively (+1/unit via `Business.xpsAddPerPurchaseToPlayer`), Level 1 alone required 1,000 purchases, permanently blocking all progression. Fixed via `EconomySimulator.cs`-driven iterative tuning: added a second XP source (`InfluencerRise.PlayerProgression.ProductionXpGrant`, tier-scaled XP on production-round completion, so AFK/idle play can also level up) and retuned the curve to **`firstLevelXp=400`, `levelIncrementMultiplier=1.13`** (live on `GameManagers`' `PlayerStats` component). The `1.13` value required a scoped vendored-code exception — `levelIncrementMultiplier` was hard-typed `int`, and no integer value gives sane pacing (`q≥2` walls around Level 15, `q=1` produces meaningless multi-million-level inflation) — changed to `float` (2nd vendored `PlayerStats.cs` patch, see BugTracker.md and CLAUDE.md Watch-list for full detail and risk). Result: Level 3 reachable in ~4 simulated minutes (ACTIVE), full Yourgram chain (through its Level-24 capstone `VerifiedStatus`) reachable with no XP-side wall.

**⚠️ Known open issue — QuickTok/Broadcast unreachable past Level 18, independent of the XP fix above.** Every QuickTok/Broadcast business costs Cash, but no Business anywhere produces Cash — confirmed structural (not time- or XP-curve-limited) via `EconomySimulator.cs`, including a 60-simulated-day test. `TrendChase` (QuickTok's own entry point) can never be purchased, which blocks Levels 18/28/34/42/55/65/80/110/150 entirely. See BugTracker.md's "QuickTok/Broadcast businesses cost Cash" entry for full detail — needs a scoped fix (Cash-producing business, Followers-priced entry point, or similar) before the full 150-level schedule is reachable end-to-end. Out of scope for the task that found it (Business cost-curve/currency changes require separate authorization).

---

## Business Groups — ✅ 4 built

| Asset name | Members | Purpose |
|---|---|---|
| `QuickTokContentGroup` | TrendChase, ReactionContent | Speed-upgrade targeting (`FasterEditingSoftware`). *Renamed from `ClipzContentGroup` to match the platform's current name — GUID preserved, all 3 references (FasterEditingSoftware's targetBusinessGroups, TrendChase's and ReactionContent's businessGroups) verified surviving via fresh disk read.* |
| `YourgramAllBusinesses` | All 5 Yourgram businesses | Rare Manager "The Algorithm Whisperer" platform-wide targeting |
| `QuickTokAllBusinesses` | All 5 QuickTok businesses | Rare Manager "The Clout Chaser" platform-wide targeting |
| `BroadcastAllBusinesses` | All 5 Broadcast businesses | Rare Manager "The Prestige Press" platform-wide targeting |

---

## Managers — ✅ all 24 built (15 Common / 6 Rare / 3 Legendary)

**Rarity mechanics**: leveling curve is universal (geometric ×2 copies/level, native, not configurable per-asset). Rarity controls **pull odds** (Capsule weight) and **per-level power** (`businessSpeedMultiplier`/`passiveBoosts` value), not the leveling curve. No max level; no salvage system (deferred, see EngineCapabilities.md).

### Common (15) — one per business, automation only

| Business | Manager |
|---|---|
| First Post | Ronny Ringo |
| Selfie Session | Faye Filterton |
| Story Feed | Tori Steller |
| Engagement Farming | Farmer Bob |
| Verified Status | Chuck Verifiington |
| Trend Chase | Dana Trent |
| Collab Farming | Coco Labor |
| Reaction Content | Reggie Reactington |
| Algorithm Baiting | Al Gorithm |
| Viral Challenge | Val Iral |
| Hot Take | Hattie Takerson |
| Unboxing Haul | Foxy Haulwell |
| Streaming | Cas Flowton |
| Sponsor Deals | Dee Sponsorman |
| Own Brand | Baron Von Brand |

### Rare (6) — platform-wide or global passive effects, no automation

⚠️ **A Manager's `passiveBoosts` field is NOT reachable by any native code path unless custom code bridges it** — confirmed via a full-project manager audit, empirically verified in Play Mode. `Manager.passiveBoosts` uses the same `UpgradeBlockFilter`/`targetBusinessGroups` type as `Upgrade.upgrades`, but the only native code reading it only ever looks up a Business's own single dedicated `.manager` — never any manager using group or global targeting. See `docs/BugTracker.md`'s "Rare/Legendary Manager passiveBoosts structurally unreachable" entry for the full root cause. The "Mechanism" column below reflects the ACTUAL fix per manager, not just the data configuration.

| Manager | Effect | Mechanism |
|---|---|---|
| The Algorithm Whisperer | +40%/lvl production, all Yourgram | `passiveBoosts`, targetBusinessGroups=YourgramAllBusinesses. ✅ Reaches gameplay via `RareLegendaryManagerProductionModifier.cs` (custom `ProductionModifierAsset`, generic across all production-type managers). |
| The Clout Chaser | +40%/lvl production, all QuickTok | targetBusinessGroups=QuickTokAllBusinesses. ✅ Same fix as above. |
| The Prestige Press | +40%/lvl production, all Broadcast | targetBusinessGroups=BroadcastAllBusinesses. ✅ Same fix as above. |
| The Wellness Guru | Reduces Burnout decay interval while owned, scaling per level | **Custom code** — `BurnoutController.cs` responsibility #4 (native passiveBoosts can't target CustomStats; unrelated to and unaffected by the passiveBoosts-reachability gap above, since it never went through the generic pipeline to begin with). |
| The Budget Bestie | Cash burst on every player level-up while owned, scaled by player level × own manager level | **REDESIGNED**, `passiveBoosts` cleared to `[]` (was: -15%/lvl global purchaseCost passiveBoost, confirmed non-functional — no discrete grant event to intercept a live-read value). ✅ Reaches gameplay via `RareLegendaryManagerEffects.cs` responsibility 2 (custom, listens to `PlayerStats.OnLevelUpEvent`, grants Cash via `CurrencyManager.AddCurrencyAmount`). Formula: `newPlayerLevel × managerLevel × 10` (placeholder `BudgetBestieCashPerLevelMultiplier`, not balance-tested). Verified via MCP: exact Cash total matched formula across a multi-level burst. See BugTracker.md. |
| The Growth Hacker | +25%/lvl global player XP (1.25 placeholder) | `passiveBoosts`, UpgradeType.playerXp, empty filters (global). ✅ Reaches gameplay via `RareLegendaryManagerEffects.cs` (custom, listens to `PlayerStats.OnXpAdded`, tops up via a second `AddXp` call). |

### Legendary (3) — global, CloutCoin/IAP Capsules only, never ad

| Manager | Effect | Mechanism |
|---|---|---|
| The Main Character | +60%/lvl global production (1.6 placeholder) | `passiveBoosts`, empty filters. ✅ Reaches gameplay via `RareLegendaryManagerProductionModifier.cs` (same fix as the platform-wide Rare managers above — empty filter = matches every business). |
| The Algorithm Itself | Periodically re-activates the existing `AlgorithmPushBoost` (2× speed, 10 min) while owned, keeping it perpetually topped up | **REDESIGNED**, `passiveBoosts` cleared to `[]` (was: +60%/lvl global speed passiveBoost, confirmed non-functional — production timing is read live/repeatedly with no discrete grant hook). ✅ Reaches gameplay via `RareLegendaryManagerEffects.cs` responsibility 3 (custom `Update()` timer calling `BoostsManager.RemoveActiveBoost`+`SetActiveBoost`). **Behavior change**: effect no longer scales with the manager's own level (reuses the fixed-strength existing Boost as-is) — deliberate simplification, flagged not silently narrowed. Placeholder refresh interval 60s (well under the Boost's own 10-min duration, so it never actually lapses), not balance-tested. Action verified correct via MCP; the real-time timer itself could not be verified through the MCP bridge (each `RunCommand` call triggers a domain reload that resets Play Mode's frame/time state — see CLAUDE.md's Known Tooling Limitations) and needs direct Editor observation. See BugTracker.md. |
| The Nepo Baby | Bonus CloutCoin on every player level-up while owned, scaled by own manager level only | **REDESIGNED**, `passiveBoosts` cleared to `[]` (was: 8 individually-targeted dropWeight passiveBoosts entries toward the other Rare/Legendary managers, confirmed non-functional for two independent reasons — dropWeight resolves synchronously inside `ShopItemHolder.ClaimRewards` with no pre-roll hook, AND `UpgradeEffectResolver.GetMatchingBlocks` never iterates `ManagersManager`'s owned managers at all, so it structurally could never see a Manager-sourced dropWeight passiveBoost regardless). A non-vendored in-memory DropTable-clone fix was investigated and found feasible only in isolation — making it actually govern a real purchase requires bypassing `ShopManager.CompleteExternalPurchase`'s hardcoded `ClaimRewards()` call, not a small pre-roll patch; rejected as too risky/large in scope, same reasoning as avoiding a live-read vendored patch for Budget Bestie/Algorithm Itself. Redesigned instead to amplify an existing, already-working mechanism — "born lucky" flavor via more currency to spend on Capsules, rather than touching pull odds directly. ✅ Reaches gameplay via `RareLegendaryManagerEffects.cs` responsibility 4 (hooks `PlayerStats.OnLevelUpEvent`, grants CloutCoin via `CurrencyManager.AddCurrencyAmount` on top of the native level-up reward). Formula: `managerLevel × 5` (placeholder `NepoBabyCloutCoinPerLevelMultiplier`, not balance-tested) — flat per-level-up amount, does not also scale with player level unlike Budget Bestie's formula. Verified via MCP: normal native CloutCoin reward and the Nepo Baby bonus both landed exactly matching their respective formulas across a multi-level burst. See BugTracker.md. |

*Rarity enum note: internally `Rarity.cs` uses `Legendary` (int 3) after a rename from a leftover default `Mythic`; an unused `Epic` value was removed and `Legendary` was explicitly pinned to int 3 to avoid a silent renumber breaking already-serialized assets.*

---

## Upgrades — ✅ 3 built, 1 deferred

| Asset name | Type | Target | Effect |
|---|---|---|---|
| `BetterRingLight` | production | FirstPost/SelfieSession area (contextual filter) | +25% output |
| `FasterEditingSoftware` | speed | ClipzContentGroup | -20% production time |
| `BulkContentBatching` | purchaseCost | Global | -10% purchase cost |
| `TherapySessionUpgrade` | — | Burnout | ✅ Built — `passiveBoosts`-equivalent gap (Upgrades can't target CustomStats, same as everything else Burnout-related); effect lives in `BurnoutController.cs` Responsibility 6 instead, hooking `UpgradesManager.OnUpgradeBought`. -15 Burnout per purchase. Repeatable (`maxBuyableAmount=20`), cost 100 Cash base ×2.25/purchase (placeholder, Phase 6 tunable), instant (no purchase delay), no locks. |

## Aura-purchased Permanent Upgrades — ✅ 3 built (locked selection, no more planned)

**Previously documented as "✅ 3 built" while not actually existing as assets at all — confirmed false via git history, see `docs/BugTracker.md`. Built for real this pass, all under `Assets/InfluencerRise/Resources/Upgrades/`, all native (no custom bridging needed), each verified live via MCP.**

| Asset name | Aura cost/level | Effect | Mechanism |
|---|---|---|---|
| `ClaimYourAlgorithm` | 10, x1.5/lvl | Global production ×1.25 (placeholder, not balance-tested) | `upgrades`, empty filters (non-contextual, global) — reaches the live business production multiplier the same native way `BulkContentBatching`/`FasterEditingSoftware` already do. ✅ Verified via MCP: FirstPost real production output 80,000 → 100,000 (×1.25 exact) after granting. |
| `LuckyBreakInvestment` | 15, x1.5/lvl | dropWeight ×1.3/manager (placeholder) toward the 6 Rare + 3 Legendary managers, raising their odds relative to the 15 Common managers | 9 individually-targeted `UpgradeBlock` entries (`targetManager` filter, one per Rare/Legendary manager — no native group-targeting for Managers, same "manual maintenance required" pattern as the old Nepo Baby design, confirmed unavoidable gap). **Investigated native-vs-bridged feasibility first**: unlike a Manager's own `passiveBoosts` (structurally invisible to `UpgradeEffectResolver.GetMatchingBlocks`, the root cause of the original Nepo Baby bug), an *Upgrade's* contextual dropWeight blocks ARE correctly iterated by `GetMatchingBlocks`, confirmed via full code read. ✅ Fully native, verified via MCP: `TheMainCharacter`'s real weight in `CloutCoinCapsuleHigh` went 4 → 5.2 (×1.3 exact); `RonnyRingo` (Common, untargeted) stayed exactly 1. |
| `PassiveIncomeGrindset` | 12, x1.5/lvl | Offline-progress production ×1.25 (placeholder), live production unaffected | `upgrades`, `runtimeFilter=Offline` — confirmed native mechanism (offline calculation's `ProductionOutputCalculator.CreateModifierContext` correctly sets `IsOffline`, matched by the block's `runtimeFilter`). ✅ Verified via MCP: live production ratio exactly 1.0 (unaffected), `ProfitCalculator`'s 300s offline simulation exactly ×1.25 after granting. |

All 3 added to `PrestigeManager.excludedUpgrades` — persist across every Rebrand (verified via fresh disk read of `Main.unity`). `exludedCurrencies` (Aura, CloutCoin) confirmed still intact, not disturbed by this change.

## Boosts — ✅ all 3 built

| Asset name | Type | Effect |
|---|---|---|
| `AllNighterBoost` | TimeSkip | Skips 4 hours production |
| `ViralMomentBoost` | GenericMultiplier | 3x production, 5 min |
| `AlgorithmPushBoost` | GenericMultiplier | 2x speed, 10 min |

## Shop Items — ✅ 11 built total

**Original 6:**
| Asset name | Type | Grants |
|---|---|---|
| `FollowerPackSmall` | IAP | 1,000 Followers |
| `CashPackSmall` | IAP | 500 Cash |
| `SpaDayBurnoutReliefAd` | Ad | -40 Burnout (via BurnoutController) |
| `SpaDayBurnoutReliefPaid` | IAP | -40 Burnout (via BurnoutController) |
| `RemoveAdsPermanent` | IAP | No economy grant — ownership itself is the flag, maxPurchases=1 |
| `RewardedBoostAd` | Ad | AlgorithmPushBoost |

**Manager Capsules (5, real, verified — see MonetizationPlan.md for full weight tables):**
| Asset name | Type | Cost | Zero-Legendary verified? |
|---|---|---|---|
| `FreeCapsule` | Free (4h cooldown gate — `FreeCapsuleCooldown.cs`, see MonetizationPlan.md) | Free | Yes |
| `WatchAdCapsule` | Ad | Free | Yes |
| `CloutCoinCapsuleLow` | CloutCoin | 50 (placeholder) | N/A — Legendary reachable here |
| `CloutCoinCapsuleHigh` | CloutCoin | 250 (placeholder) | N/A |
| `IAPCapsule` | IAP | productId deferred (Phase 4) | N/A |

## Achievements — ✅ 3 built

| Asset name | Condition |
|---|---|
| `FirstPost` (Achievement — name collision with FirstPost the Business, different types, functionally unaffected) | Lifetime own 1x FirstPost |
| `HundredFollowers` | 100 lifetime Followers |
| `FirstRebrand` | 1 Rebrand completed |

Player Cosmetics — deferred to V1.1, confirmed not natively supported.

## Prestige — "Rebrand" — ✅ configured

- Requirement: Cash ≥10,000 AND Player Level ≥5. Verified live via MCP (exact match across 3 sequential real Rebrands).
- `resetLevel = false` (Player Level persists — LOCKED decision). **Was actually `true` on the live scene component from the very first Prestige-configuring commit onward — confirmed via git history to have never been `false` at any point, a documentation error rather than a regression (see BugTracker.md). Fixed for real this pass**: set on the live `PrestigeManager` component via Edit Mode, verified `resetLevel: 0` via fresh disk read of `Main.unity`.
- Reset: Followers, Cash, Business/Manager holders, Boost inventory. Excluded: Aura, CloutCoin, Player Level, and the 3 Aura Upgrades (all verified live/on-disk, see BugTracker.md).
- Burnout reset via `BurnoutController.cs` (native has no CustomStat reset hook).
- `prestigeUpgrades` (automatic, native): production/speed both compound ×2 per Rebrand via `currentPrestigeUpgradesMultiplier` — separate mechanism from Aura spend, already primed.
- `multiplyLevelOnPrestige = false` — Cash requirement scales ×2/Rebrand, Level requirement stays fixed at 5. Open balance question for later.