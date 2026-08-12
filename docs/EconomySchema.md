# Economy Schema — Certified Aura: Idle Influencer Tycoon

This is the single source of truth for every persisted asset name, its category, and its unlock order. Names here are locked once created in the project (see CLAUDE.md Hard Rule #1). Numbers are starting points for playtesting, not final balance.

---

## Currencies

| Asset name | Purpose | Cap | Reset on Rebrand? |
|---|---|---|---|
| `Followers` | Primary production currency, most Business costs | Uncapped | Yes |
| `Cash` | Secondary currency, Upgrades/Managers/Shop | Uncapped | Yes |
| `Aura` | Prestige currency, earned only on Rebrand | Uncapped | **No — excluded, persists forever** |

## Custom Stat (not a Currency)

| Asset name | Purpose | Range | Reset on Rebrand? |
|---|---|---|---|
| `Burnout` | Rises with active production; reduces output above threshold; lowered by self-care Shop items or a slow passive decay | 0–100 | Yes, resets to 0 |

**Burnout implementation note:** engine has no built-in auto-decay ticker for a CustomStat (confirmed in EngineCapabilities.md). Needs one small script — a `MonoBehaviour` calling `PlayerStats.IncrementStat(Burnout, +amount)` on production completion, and a `ProductionOutputModifierAsset` (a supported extension point, not custom-from-scratch) that reduces output when `Burnout` crosses a threshold. This is the one place economy logic needs light custom code — everything else below is Inspector configuration.

---

## Locations (Platforms) — v1 launch order

| Asset name | Display name | Unlock cost | Default active? |
|---|---|---|---|
| `StarterFeed` | Your First Post | Free | Yes — `defaultActiveLocation` |
| `ClipzPlatform` | Clipz (short-form) | 500 `Followers` | No |
| `ChroniclePlatform` | The Chronicle (long-form) | 5,000 `Cash` | No |

Brand/merch platform is a **post-launch** addition — not created yet, don't build it for v1.

---

## Businesses

## Business Groups

| Asset name | Members | Purpose |
|---|---|---|
| `ClipzContentGroup` | `TrendChase`, `DuetFarm` | Scoping target for upgrades/boosts that should apply across ClipzPlatform's fast-content businesses without including ViralAttempt (which is intentionally higher-tier/separate pacing) |

### StarterFeed platform

| Asset name | Display name | Base cost | Produces | Time to produce | Notes |
|---|---|---|---|---|---|
| `SelfieSession` | Selfie Session | 1 `Followers` | 1 `Followers` | 3s | First business, tutorial pace, matches the pattern in `01-create-your-first-idle-game.md` |
| `StoryPost` | Story Post | 15 `Followers` | 3 `Followers` | 8s | Unlocks after owning 1x `SelfieSession` (Lock) |`StoryPost` upgradeCost: 40 `Followers` per level (enables level 2, required by RingLightSetup's unlock lock)
`StoryPost` — upgradeCostMultiplier and autoUpgradeForFree left at package defaults (2x geometric, auto-free-first-level = true); revisit during Phase 6 balancing.
| `RingLightSetup` | Ring Light Setup | 100 `Followers` | 8 `Followers` | 20s | Unlocks after `StoryPost` level 2 |

### ClipzPlatform

| Asset name | Display name | Base cost | Produces | Time to produce | Notes |
|---|---|---|---|---|---|
| `TrendChase` | Chase a Trend | 50 `Cash` | 12 `Followers` | 10s | First business on this platform |
| `DuetFarm` | Duet Farm | 300 `Cash` | 6 `Cash` | 25s | Chains: consumes `Followers` as production input |
| `ViralAttempt` | Manufactured Viral Moment | 1,500 `Cash` | 40 `Followers` + 10 `Cash` | 60s | High-value, slower, random drop table for bonus outputs |
`DuetFarm` — production input: 20 Followers consumed per round (placeholder, tune in Phase 6)
`ViralAttempt` — bonus drop-table output deferred to Phase 6 balancing; currently ships with only the two guaranteed outputs (40 Followers, 10 Cash)

### ChroniclePlatform

| Asset name | Display name | Base cost | Produces | Time to produce | Notes |
|---|---|---|---|---|---|
| `LongformEssay` | Long-form Essay | 2,000 `Cash` | 25 `Cash` | 90s | First business on this platform |
| `SubscriberFunnel` | Subscriber Funnel | 8,000 `Cash` | 60 `Cash` | 180s | Unlocks after `LongformEssay` level 3 |

**Cost multiplier convention:** `1.15` per purchase for all Businesses above, per the standard idle-game curve — adjust globally later during balancing, don't hand-tune per business yet.

---

## Managers (Team)

| Asset name | Automates | `autoProduceOnLevel` | `autoCollectOnLevel` |
|---|---|---|---|
| `GhostwriterManager` | `SelfieSession` | 0 (auto on purchase) | 0 |
| `SocialMediaManager` | `StoryPost` | 1 | 0 |
| `EditorManager` | `TrendChase` | 0 | 0 |
| `TalentAgentManager` | `DuetFarm` | 2 | 1 |
| `PublicistManager` | `LongformEssay` | 0 | 0 |

---

## Upgrades

| Asset name | Type | Target | Effect |
|---|---|---|---|
| `BetterRingLight` | `production` | `SelfieSession` group | +25% output |
| `FasterEditingSoftware` | `speed` | `TrendChase`, `DuetFarm` | -20% production time |
| `BulkContentBatching` | `cost` | All Businesses | -10% purchase cost |
| `TherapySessionUpgrade` | custom (Burnout reduction) | `Burnout` stat | -15 Burnout on purchase, one-time or repeatable — decide during Burnout script design |

---

## Boosts

| Asset name | Type | Effect |
|---|---|---|
| `AllNighterBoost` | Time Skip | Skips 4 hours of production |
| `ViralMomentBoost` | Production multiplier | 3x output, 5 min duration |
| `AlgorithmPushBoost` | Speed multiplier | 2x speed, 10 min duration |

---

## Shop Items (ties to MonetizationPlan.md — full IAP product IDs go there)

| Asset name | Payment type | Grants |
|---|---|---|
| `FollowerPackSmall` | Real money | 1,000 `Followers` |
| `CashPackSmall` | Real money | 500 `Cash` |
| `SpaDayBurnoutReliefAd` | Ad only | -40 Burnout | Grant logic deferred to Burnout custom script (Phase 2) |
| `SpaDayBurnoutReliefPaid` | Real money | -40 Burnout | Grant logic deferred to Burnout custom script (Phase 2); productId deferred to Phase 4 |
| `RemoveAdsPermanent` | Real money | Disables ad placements (project-side flag, not an economy grant) |
| `RewardedBoostAd` | `buyWithAd` | Grants `AlgorithmPushBoost` free |

---

## Prestige — "Rebrand"

- **Requirement:** `Cash` ≥ 10,000 AND player level ≥ 5 (tune later).
- **Reward:** `Aura` — 1 per Rebrand initially, scaling formula TBD during balancing.
- **Reset scope:** `Followers`, `Cash`, `Burnout`, Business holders, Manager holders, Boost inventory all reset. `Aura` and any `Aura`-purchased persistent Upgrades are excluded (`exludedCurrencies` field — note the package's intentional misspelling).
- **Persistent Aura upgrades:** define in a later pass, once first Rebrand loop is playtested — e.g., permanent production multiplier per Aura spent.

---

## First-hour pacing target

| Milestone | Target time |
|---|---|
| First `SelfieSession` purchase | Immediate (tutorial) |
| First `StoryPost` unlock | ~2 minutes |
| First Manager (`GhostwriterManager`) purchase | ~5 minutes |
| `ClipzPlatform` unlock | ~15 minutes |
| First `Boost` use | ~20 minutes |
| First Shop interaction (ad-based, low friction) | ~25 minutes |
| `ChroniclePlatform` unlock | ~45–60 minutes |