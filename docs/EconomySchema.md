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
| `Burnout` (Custom Stat) | 0–100, decays passively, rises with production (rise-rate design pending), resets on Rebrand | 0–100 | Yes → 0 |

---

## Locations (Platforms) — ✅ all 3 built and correctly named

| Asset name | Display | Unlock condition | Default active? |
|---|---|---|---|
| `StarterFeed` | Yourgram | Free | Yes |
| `QuickTokPlatform` | QuickTok | Level ≥12 AND own 1x `StoryFeed` | No |
| `BroadcastPlatform` | Broadcast | Level ≥34 AND own 1x `ReactionContent` | No |

---

## Businesses — ✅ all 15 built, full Lock chains wired

### Yourgram

| Asset name | Display | Manager | Lock |
|---|---|---|---|
| `FirstPost` | First Post | Ronny Ringo | None (free start) |
| `SelfieSession` | Selfie Session | Faye Filterton | Own 10x FirstPost AND Level ≥3 |
| `StoryFeed` | Story Feed | Tori Steller | Own 15x SelfieSession AND Level ≥7 |
| `EngagementFarming` | Engagement Farming | Farmer Bob | Own 20x StoryFeed AND Level ≥14 |
| `VerifiedStatus` | Verified Status (capstone) | Chuck Verifiington | Own 25x EngagementFarming AND Level ≥24 |

### QuickTok

| Asset name | Display | Manager | Lock |
|---|---|---|---|
| `TrendChase` | Trend Chase | Dana Trent | None (platform entry point) |
| `CollabFarming` | Collab Farming | Coco Labor | Own 10x TrendChase AND Level ≥18 |
| `ReactionContent` | Reaction Content | Reggie Reactington | Own 15x CollabFarming AND Level ≥28 |
| `AlgorithmBaiting` | Algorithm Baiting | Al Gorithm | Own 20x ReactionContent AND Level ≥42 |
| `ViralChallenge` | Viral Challenge (capstone) | Val Iral | Own 25x AlgorithmBaiting AND Level ≥65 |

### Broadcast

| Asset name | Display | Manager | Lock |
|---|---|---|---|
| `HotTake` | Hot Take | Hattie Takerson | None (platform entry point) |
| `UnboxingHaul` | Unboxing Haul | Foxy Haulwell | Own 10x HotTake AND Level ≥55 |
| `Streaming` | Streaming | Cas Flowton | Own 15x UnboxingHaul AND Level ≥80 |
| `SponsorDeals` | Sponsor Deals | Dee Sponsorman | Own 20x Streaming AND Level ≥110 |
| `OwnBrand` | Own Brand (capstone, end of v1 content) | Baron Von Brand | Own 25x SponsorDeals AND Level ≥150 |

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

All levels grant a Level-Up Reward (CloutCoin) — config pending (native, see EngineCapabilities.md).

---

## Business Groups — ✅ 4 built

| Asset name | Members | Purpose |
|---|---|---|
| `ClipzContentGroup` | TrendChase, ReactionContent | Speed-upgrade targeting (`FasterEditingSoftware`). *Naming inconsistency: not renamed when platform became QuickTok — cosmetic only, see CLAUDE.md Watch-list.* |
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

| Manager | Effect | Mechanism |
|---|---|---|
| The Algorithm Whisperer | +40%/lvl production, all Yourgram | `passiveBoosts`, targetBusinessGroups=YourgramAllBusinesses |
| The Clout Chaser | +40%/lvl production, all QuickTok | targetBusinessGroups=QuickTokAllBusinesses |
| The Prestige Press | +40%/lvl production, all Broadcast | targetBusinessGroups=BroadcastAllBusinesses |
| The Wellness Guru | Reduces Burnout decay interval while owned, scaling per level | **Custom code** — `BurnoutController.cs` responsibility #4 (native passiveBoosts can't target CustomStats) |
| The Budget Bestie | -15%/lvl global purchase cost (0.85 placeholder) | `passiveBoosts`, UpgradeType.purchaseCost, empty filters (global) |
| The Growth Hacker | +25%/lvl global player XP (1.25 placeholder) | `passiveBoosts`, UpgradeType.playerXp, empty filters (global) |

### Legendary (3) — global, CloutCoin/IAP Capsules only, never ad

| Manager | Effect | Mechanism |
|---|---|---|
| The Main Character | +60%/lvl global production (1.6 placeholder) | `passiveBoosts`, empty filters |
| The Algorithm Itself | +60%/lvl global speed (1.6 placeholder) | `passiveBoosts`, empty filters |
| The Nepo Baby | +30%/lvl boost to own Capsule pull odds toward the other 8 Rare/Legendary managers | 8 individually-targeted `passiveBoosts` entries (UpgradeType.dropWeight, one per target — no native group-targeting for Managers, confirmed gap). **Manual maintenance required**: any future Rare/Legendary manager needs a matching entry added here or it won't benefit. |

*Rarity enum note: internally `Rarity.cs` uses `Legendary` (int 3) after a rename from a leftover default `Mythic`; an unused `Epic` value was removed and `Legendary` was explicitly pinned to int 3 to avoid a silent renumber breaking already-serialized assets.*

---

## Upgrades — ✅ 3 built, 1 deferred

| Asset name | Type | Target | Effect |
|---|---|---|---|
| `BetterRingLight` | production | FirstPost/SelfieSession area (contextual filter) | +25% output |
| `FasterEditingSoftware` | speed | ClipzContentGroup | -20% production time |
| `BulkContentBatching` | purchaseCost | Global | -10% purchase cost |
| `TherapySessionUpgrade` | — | Burnout | **Deferred** — Upgrades can't target CustomStats |

## Aura-purchased Permanent Upgrades — ✅ 3 built (locked selection, no more planned)

| Asset name | Aura cost/level | Effect |
|---|---|---|
| `ClaimYourAlgorithm` | 10, x1.5/lvl | Global production multiplier |
| `LuckyBreakInvestment` | 15, x1.5/lvl | Boosts Manager Capsule drop odds toward rarer tiers |
| `PassiveIncomeGrindset` | 12, x1.5/lvl | Boosts offline-progress efficiency |

All 3 added to `PrestigeManager.exludedUpgrades` — persist across every Rebrand.

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

- Requirement: Cash ≥10,000 AND Player Level ≥5.
- `resetLevel = false` (Player Level persists — LOCKED decision).
- Reset: Followers, Cash, Business/Manager holders, Boost inventory. Excluded: Aura, CloutCoin, Player Level, and the 3 Aura Upgrades.
- Burnout reset via `BurnoutController.cs` (native has no CustomStat reset hook).
- `prestigeUpgrades` (automatic, native): production/speed both compound ×2 per Rebrand via `currentPrestigeUpgradesMultiplier` — separate mechanism from Aura spend, already primed.
- `multiplyLevelOnPrestige = false` — Cash requirement scales ×2/Rebrand, Level requirement stays fixed at 5. Open balance question for later.