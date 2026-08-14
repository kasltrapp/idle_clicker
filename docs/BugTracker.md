# Bug Tracker & Fix Log

Running record of confirmed bugs found and fixed during development, plus known open issues. Check here first whenever something behaves like a regression — it may be a previously-fixed issue that resurfaced (e.g. via a package update overwriting a vendored-code fix).

Format per entry: **Date/session · Component · Severity · Status**

---

## Fixed

### `Level.AddXp` compounding level-math corruption (marginal `TargetXp` fed to cumulative `GeometricSequence_Amount`)
**Component:** `Assets/EasyIdleGame/Scripts/_Models/Levels/Level.cs` (vendored) · **Severity: Critical** · **Status: Fixed**

Escalated from the "Known open issue" below after a dedicated, definitive investigation (read-only first, then fixed under an explicit scoped exception). Root cause confirmed precisely: `Level.TargetXp` returns the **marginal** cost of the single next level (`Mathb.GeometricSequence_NthTerm` — the n-th term of the sequence, e.g. 1000 → 2000 → 4000 → ...). `Level.AddXp` compared `currentXp` against this marginal value correctly, but then fed that same `currentXp` into `Mathb.GeometricSequence_Amount`, which always interprets its input as an **absolute cumulative sum measured from level 0** — it has no concept of "starting from the current level." Since `currentXp` only ever holds the *leftover remainder* after a level-up (a marginal, current-level-scoped quantity), re-interpreting it as an absolute cumulative total silently mis-counted levels gained and left a nonzero, wrong leftover behind on every level-up past the first — and because each call's leftover was itself already corrupted, the error compounded multiplicatively across successive level-ups.

**Confirmed empirically, isolated from all other code** (direct `Level.AddXp` calls, bypassing `PlayerStats`/claim-loop/custom multiplier systems), granting exactly one level's worth of `TargetXp` at a time, sequentially, before the fix:

| Step | Granted (exact TargetXp) | Resulting level | Expected | Leftover XP |
|---|---|---|---|---|
| 1 | 1,000 | 1 | 1 | 0 (clean) |
| 2 | 2,000 | 2 | 2 | **1,000** (should be 0) |
| 3 | 4,000 | **4** | 3 | 2,000 |
| 5 | 256,000 | **16** | 5 | 4,000 |
| 10 | (continuing) | **512** | 10 | 0 |

Level 1 was clean (no leftover exists yet to corrupt anything); every level from 2 onward was wrong, and by "level 10" the player was actually at level 512 — under the cleanest possible protocol (exact marginal grants, one level at a time), not a crafted edge case. This directly corrupts `currentLevel`, which gates all 15 Businesses, all 3 platform unlocks, and every CloutCoin level-up reward via the Level Progression schedule (see EconomySchema.md) — hence Critical, not Medium as originally flagged.

**Fix:** confined entirely to `Level.AddXp` (no changes to `Mathb.cs` — its functions are correct in isolation and used correctly elsewhere in the codebase; the bug was in how `Level.AddXp` was feeding them, not in the functions themselves). Re-baselines the leftover into the same absolute-from-zero frame the geometric math expects before calling it:

```csharp
// Before:
int addedLevels = (int)Mathb.GeometricSequence_Amount(startXp, incrementMultiplier, currentXp).ToDouble();
currentXp = currentXp - Mathb.GeometricSequence_Sum(startXp, incrementMultiplier, addedLevels);

// After:
BigNumber cumulativeThroughCurrentLevel = Mathb.GeometricSequence_Sum(startXp, incrementMultiplier, currentLevel);
BigNumber absoluteXp = cumulativeThroughCurrentLevel + currentXp;
int newAbsoluteLevel = (int)Mathb.GeometricSequence_Amount(startXp, incrementMultiplier, absoluteXp).ToDouble();
int addedLevels = newAbsoluteLevel - currentLevel;
currentXp = absoluteXp - Mathb.GeometricSequence_Sum(startXp, incrementMultiplier, newAbsoluteLevel);
```

**Re-verified after the fix, all via Unity MCP RunCommand:**
- All 14 of this project's actual milestone levels (3, 7, 12, 14, 18, 24, 28, 34, 42, 55, 65, 80, 110, 150) — granted exact cumulative XP for each from a fresh level-0 reset — landed on the exact expected level with exactly 0 leftover XP, 14/14.
- Sequential levels 1 through 10, one level's worth of marginal XP at a time cumulatively — 10/10 clean, matching the correct doubling costs (1000, 2000, 4000, ... 512000) with 0 drift at any step.
- Interaction with the (separately, previously fixed) `PlayerStats.AddXp` claim-loop bug: a single large XP burst landing exactly on level 55 via the real `PlayerStats.AddXp` path (Growth Hacker's XP top-up temporarily disabled to isolate the two fixes cleanly) resulted in `currentLevel=55` exact, `unclaimedLevelups=0` exact, and CloutCoin level-up rewards totaling 10,150 (7,700 repeating + 2,450 across all 10 milestones in range) — correct to within a `~1.8e-12` floating-point rounding artifact from summing 55 separate `BigNumber` additions (the same class of expected noise as `0.1+0.2≠0.3` in IEEE754 doubles, not a defect). The two fixes coexist correctly.

**Risk:** vendored-file edit — could be silently overwritten by a future EasyIdleGame package update, same as the claim-loop fix. If levels behave oddly after ever updating the package (players not reaching expected levels from their XP, or reaching implausibly high levels from small grants), check this fix first.

### Rare/Legendary Manager passiveBoosts structurally unreachable
**Component:** `Assets/EasyIdleGame/Scripts/Managers/ManagersManager.cs` + `Assets/EasyIdleGame/Scripts/Upgrades/UpgradeEffectResolver.cs` (vendored, root cause) · **Severity: High** · **Status: Fixed/Redesigned (9 of 9 non-Common managers) — all resolved, 0 vendored patches used for any of them**

Found during a full-project manager audit. `ManagersManager.GetPassiveMultiplierOfType`/`GetActiveMultiplierOfType` — the only native code that ever reads a Manager's `passiveBoosts` for business production — only ever look up the one Manager assigned as a *specific Business's own dedicated* `.manager` field. `UpgradeEffectResolver.GetMatchingBlocks` (the generic resolver used for purchaseCost/dropWeight/dropChance/productionOutputScaling) never iterates `ManagersManager` at all. Net effect: any Manager using `targetBusinessGroups` (platform-wide) or an empty/global filter — i.e. all 6 Rare and 3 Legendary managers, by design, per EconomySchema.md — was structurally unreachable. Confirmed empirically in Play Mode (not just code-read): owning 5 levels of a Rare manager left the target business's actual computed production multiplier exactly unchanged. The manager `.asset` files themselves were confirmed correctly configured (e.g. `TheAlgorithmWhisperer.asset` has `value:1.4`, `targetBusinessGroups` correctly pointing at `YourgramAllBusinesses`'s GUID) — this was purely an engine reachability gap, not a content/wiring mistake.

**Fixed via custom code, no vendored edits, for 5 of 9:**
- Production/global-production effects (The Algorithm Whisperer, The Clout Chaser, The Prestige Press, The Main Character) — `Assets/InfluencerRise/Scripts/Managers/RareLegendaryManagerProductionModifier.cs`, a `ProductionModifierAsset` registered on `GameManagers`' `ProductionModifierManager`. Plugs into Easy Idle Game's own already-wired production-output-modifier extension point (`ProductionOutputCalculator` → `ProductionModifierRegistry.ApplyOutputModifiers`). Generic — reads each owned manager's own `targetBusinessGroups` filter directly, so any future Rare/Legendary manager with a production-type passiveBoost is covered automatically, no per-manager entry needed. Empirically verified via a register/unregister toggle around the same production calculation (e.g. HotTake: 8.96K WITH vs 250 WITHOUT The Prestige Press + The Main Character both owned).
- Player XP (The Growth Hacker) — `Assets/InfluencerRise/Scripts/Managers/RareLegendaryManagerEffects.cs`, listens to `PlayerStats.OnXpAdded` and tops up the owed bonus via a second, reentrancy-guarded `AddXp` call. Empirically verified via component-enabled/disabled toggle (50 XP → 375 credited enabled, exactly 50 disabled).

**Redesigned rather than bridged, 3 of 9 — the original mechanism was confirmed to have no discrete event to intercept at all (unlike XP-granting, which does; The Nepo Baby additionally had a second, independent blocker — see its entry below), so bridging any of them as originally specified would require either a live-read vendored patch to a core, constantly-read calculation path, or bypassing a documented completion contract. Deliberately avoided given the severity of the `Level.AddXp` fix above (a vendored patch to core math already caused one Critical bug this project) — redesigned to a different, still level-scaled mechanism instead, implemented via `RareLegendaryManagerEffects.cs`:**
- The Budget Bestie — was -15%/lvl global purchaseCost (confirmed non-functional: cost is read live/repeatedly from `Business.GetBusinessMultiplierOfType` wherever checked or displayed, no discrete grant event). Native `levelRewards`/`eachLevelReward` (`PlayerStats.levelupData`) investigated first as a zero-code alternative and confirmed NOT viable — `LevelRewardHolder` has no ownership/condition field, so a level-up reward can't be gated on owning a Manager natively. Redesigned as a Cash burst granted on every player level-up while owned (responsibility 2, hooks `PlayerStats.OnLevelUpEvent`, grants via `CurrencyManager.AddCurrencyAmount`), scaled by `newPlayerLevel × managerLevel × 10` (placeholder constant, not balance-tested). `passiveBoosts` cleared to `[]`. Verified via MCP with a controlled multi-level burst — Cash gained matched the formula's sum over every level actually crossed, exactly.
- The Algorithm Itself — was +60%/lvl global speed (confirmed non-functional: production timing is read live/repeatedly from `BusinessHolder`'s own timer code, same "no discrete hook" problem as purchaseCost). `Manager.cs` checked first for any native "periodic grant while owned" mechanism — none exists; `activeDuration`/`cooldownDuration`/`activeUpgrade` is a player-*activated* temporary system (`TryActivateManager`), not passive/automatic. Redesigned as periodically re-activating the existing, already-working `AlgorithmPushBoost` (2× speed, 10 min) via a timer in `Update()` (responsibility 3) for as long as owned — `BoostsManager.RemoveActiveBoost` then `SetActiveBoost` (confirmed via full read of `BoostsManager.cs` that `ForceBuyItemsForFree` only adds inventory and does NOT activate anything; these are the real activation methods). **Behavior change, flagged deliberately**: the effect no longer scales with the manager's own level the way "+60%/lvl" implied — owning it now means "AlgorithmPushBoost is always active," not "always active AND stronger at higher levels." `passiveBoosts` cleared to `[]`. The underlying action was verified correct via MCP (manual `RemoveActiveBoost`+`SetActiveBoost` call produces the expected active-boost state); the real-time 60s timer firing on its own could NOT be verified through the MCP bridge — every `Unity_RunCommand` call compiles code, which triggers a domain reload that resets Play Mode's `Time.time`/frame count, so a real-time wait-and-check approach is fundamentally unreliable here (this is the same class of limitation already documented in CLAUDE.md's "Automated Play Mode timing verification does not work through the Unity MCP bridge"). Needs direct Editor observation by a human: enter Play Mode with The Algorithm Itself owned, wait ~60+ seconds, confirm `AlgorithmPushBoost` shows active with a fresh ~600s timer.
- The Nepo Baby — was 8 individually-targeted dropWeight passiveBoosts entries (one per other Rare/Legendary manager, +30%/lvl each), confirmed non-functional for two independent reasons: dropWeight resolves synchronously inside `ShopItemHolder.ClaimRewards` with no pre-roll event to intercept, AND `UpgradeEffectResolver.GetMatchingBlocks` — the code `ModifyDropWeight` itself relies on — never iterates `ManagersManager`'s owned managers at all, so it structurally could never see a Manager-sourced dropWeight passiveBoost regardless of the first problem. A non-vendored in-memory `DropTable`/`Output` clone-and-reweight fix was investigated and found technically feasible ONLY in isolation (both are plain `[Serializable]` C# classes, trivially cloneable) — actually making a reweighted clone's odds govern a real purchase would require bypassing `ShopManager.CompleteExternalPurchase`'s hardcoded `ClaimRewards()` call (the single documented completion path per CLAUDE.md Hard Rule #5, used by all 4 Capsule purchase types), which is not a small pre-roll patch but closer to a parallel purchase-completion path reimplementing that method's full contract. Rejected as too risky/large in scope, same reasoning as avoiding a live-read vendored patch for Budget Bestie/Algorithm Itself. Redesigned instead to amplify an existing, already-working mechanism the same way those two were: grants a bonus CloutCoin amount on top of whatever the native level-up reward already granted, whenever the player levels up while owned (responsibility 4, hooks `PlayerStats.OnLevelUpEvent`, grants via `CurrencyManager.AddCurrencyAmount`) — "born lucky" flavor via more currency to spend on Capsules, rather than directly manipulating pull odds. Formula: `managerLevel × 5` (placeholder `NepoBabyCloutCoinPerLevelMultiplier`, not balance-tested) — a flat per-level-up amount, deliberately not also scaled by player level the way Budget Bestie's two-factor formula is, matching this task's explicit formula rather than silently reusing Budget Bestie's shape. `passiveBoosts` cleared to `[]`. Verified via MCP with a controlled multi-level burst — both the native CloutCoin level-up reward and the Nepo Baby bonus landed exactly matching their respective formulas.

The Wellness Guru was never affected by any of this — it already bypasses the whole broken pipeline via `BurnoutController.cs`'s direct-by-name holder lookup (built before this gap was even found, for an unrelated reason — CustomStats aren't a valid passiveBoosts target at all).

**Risk:** none of the fixes or redesigns touch vendored code, so no package-update risk there. All 9 non-Common managers (6 Rare + 3 Legendary) are now resolved without a single vendored patch — 5 bridged to their originally-specified mechanism via custom code (Algorithm Whisperer, Clout Chaser, Prestige Press, Main Character, Growth Hacker), 3 (Budget Bestie, Algorithm Itself, Nepo Baby) deliberately redesigned to a different, still level-scaled mechanism rather than requiring a live-read vendored patch to a core, constantly-read calculation path, and 1 (Wellness Guru) never affected in the first place. The proper long-term fix (a vendored patch making `ManagersManager`'s lookups and/or `UpgradeEffectResolver.GetMatchingBlocks` iterate all owned managers, matching the same precedent as the `AddXp` fix below) remains available if a future manager's intended mechanism can't be reached any other way, but nothing currently needs it.

### AddXp claim-loop drops levels on large XP bursts
**Component:** `Assets/EasyIdleGame/Scripts/PlayerStats/PlayerStats.cs` (vendored package code) · **Severity: High** · **Status: Fixed**

`AddXp`'s level-claim loop re-read `unclaimedLevelups` every iteration while `LevelUp()` simultaneously decremented that same field — loop bound and counter crossed mid-loop, silently dropping the back half of any multi-level XP burst (both level advancement and level-up rewards). Confirmed via repro: 14 levels of XP only advanced the player to level 7, `unclaimedLevelups` stuck at 7. Fixed by caching the target claim count before the loop starts. **Risk: this is a vendored-file edit — could be silently overwritten by a future EasyIdleGame package update.** If levels/rewards start under-firing again after ever updating the package, check this fix first.

### Blank `Id` field on stale Burnout CustomStat holder
**Component:** `PlayerStats.customStats` (scene data) · **Severity: High** · **Status: Fixed**
A leftover test-era Burnout holder with a blank `Id` field sat invisibly correct in the Editor but threw a blocking `EditorUtility.DisplayDialog` the moment it round-tripped through JSON save/load — would have hit any real player on their first app reopen after any session that had touched Burnout ad-hoc. Cleared the stale holder; added `SaveLoadDiagnostics.SuppressLoadErrorDialog` to test scripts going forward.

### Pause-triggered save race condition on startup
**Component:** `AutosaveTrigger.cs` / `GameBootstrap.cs` · **Severity: High** · **Status: Fixed**
`OnApplicationPause(true)` fires automatically and immediately in MCP-driven Play Mode (Unity treats the automation-driven window as backgrounded). This wrote a near-empty save file before `GameBootstrap`'s starting-currency grant ran, making the grant look already-applied on the next load — permanently losing starting Followers. Fixed with `GameBootstrap.HasCompletedInitialLoad`, gating the autosave flush on it.

### Buy Amount bar covered by mis-anchored sibling panel
**Component:** `BusinessListPanel` (UI) · **Severity: Medium** · **Status: Fixed**
`BusinessListPanel`'s top edge was anchored to the same world-Y as `BuyAmountBar`'s top edge instead of its bottom edge, so the list panel rendered directly over and fully covered the buy-amount bar. Looked like a squashed/collapsed layout; was actually a full-coverage overlap. Fixed by correcting `BusinessListPanel`'s offset.

### "Buy 0x" default label/behavior
**Component:** `BusinessDisplayer.cs` (vendor Editor-only default draw) + `UI_BusinessRow.prefab` static text · **Severity: Low (cosmetic)** · **Status: Fixed**
Two separate hardcoded "0x" string literals (an Editor-only placeholder default-draw method, and a static default value baked into the prefab's Text component) — not an actual purchase-logic bug, `buyAmountId` correctly defaulted to index 0 (=1x) the whole time.

### `BigNumber.ToString()` rounds small fractional values for display, not just large ones
**Component:** `Assets/EasyIdleGame/Scripts/BigNumber/BigNumber.cs` (vendor) · **Severity: N/A — not a bug, testing gotcha** · **Status: Resolved (no code change, documentation only)**
Found while testing `BurnoutController.cs` Responsibility 5's tier-scaled Burnout gain (placeholder values 0.5/2.5, not whole numbers). Logging a `BigNumber` via its default `{0}`/`ToString()` formatting - built for compact big-currency display like "1.5K" or "2.3M" - rounds small fractional values to the nearest whole number too: a Burnout stat that had genuinely and precisely gone from 0 to 0.5 displayed as "1" in test logs, momentarily looking like the gain had been rounded or doubled. Confirmed via `.Mantissa`/`.Exponent`/`.ToDouble()` read directly: the underlying value was exactly 0.5 the whole time, full precision preserved through `PlayerStats.IncrementStat`. Not a bug in any game logic - flagged here purely so a future session testing a small/fractional CustomStat or currency value doesn't lose time re-diagnosing the same false alarm. When precision matters for a test assertion, read `.ToDouble()` (or `.Mantissa`/`.Exponent`) directly instead of trusting the logged/displayed string.

---

## Known open issues (not yet fixed)

### `FreeCapsule` had no native cooldown field
**Component:** `ShopItem.cs` (vendor) · **Severity: N/A — workaround shipped** · **Status: Resolved via custom code**
Not a bug, a gap: no native recurring-interval field on ShopItem. Resolved via `FreeCapsuleCooldown.cs` (custom, PlayerPrefs-based). Listed here for traceability, not as an active issue.

---

## Template for new entries

```
### [Short title]
**Component:** [file/system] · **Severity: [Low/Medium/High/Critical]** · **Status: [Open/Fixed/Investigating]**
[What was wrong, how it was found, the fix if any, any residual risk.]
```