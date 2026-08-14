# Bug Tracker & Fix Log

Running record of confirmed bugs found and fixed during development, plus known open issues. Check here first whenever something behaves like a regression — it may be a previously-fixed issue that resurfaced (e.g. via a package update overwriting a vendored-code fix).

Format per entry: **Date/session · Component · Severity · Status**

---

## Fixed

### Rare/Legendary Manager passiveBoosts structurally unreachable
**Component:** `Assets/EasyIdleGame/Scripts/Managers/ManagersManager.cs` + `Assets/EasyIdleGame/Scripts/Upgrades/UpgradeEffectResolver.cs` (vendored, root cause) · **Severity: High** · **Status: Partially Fixed (5 of 9 non-Common managers)**

Found during a full-project manager audit. `ManagersManager.GetPassiveMultiplierOfType`/`GetActiveMultiplierOfType` — the only native code that ever reads a Manager's `passiveBoosts` for business production — only ever look up the one Manager assigned as a *specific Business's own dedicated* `.manager` field. `UpgradeEffectResolver.GetMatchingBlocks` (the generic resolver used for purchaseCost/dropWeight/dropChance/productionOutputScaling) never iterates `ManagersManager` at all. Net effect: any Manager using `targetBusinessGroups` (platform-wide) or an empty/global filter — i.e. all 6 Rare and 3 Legendary managers, by design, per EconomySchema.md — was structurally unreachable. Confirmed empirically in Play Mode (not just code-read): owning 5 levels of a Rare manager left the target business's actual computed production multiplier exactly unchanged. The manager `.asset` files themselves were confirmed correctly configured (e.g. `TheAlgorithmWhisperer.asset` has `value:1.4`, `targetBusinessGroups` correctly pointing at `YourgramAllBusinesses`'s GUID) — this was purely an engine reachability gap, not a content/wiring mistake.

**Fixed via custom code, no vendored edits, for 5 of 9:**
- Production/global-production effects (The Algorithm Whisperer, The Clout Chaser, The Prestige Press, The Main Character) — `Assets/InfluencerRise/Scripts/Managers/RareLegendaryManagerProductionModifier.cs`, a `ProductionModifierAsset` registered on `GameManagers`' `ProductionModifierManager`. Plugs into Easy Idle Game's own already-wired production-output-modifier extension point (`ProductionOutputCalculator` → `ProductionModifierRegistry.ApplyOutputModifiers`). Generic — reads each owned manager's own `targetBusinessGroups` filter directly, so any future Rare/Legendary manager with a production-type passiveBoost is covered automatically, no per-manager entry needed. Empirically verified via a register/unregister toggle around the same production calculation (e.g. HotTake: 8.96K WITH vs 250 WITHOUT The Prestige Press + The Main Character both owned).
- Player XP (The Growth Hacker) — `Assets/InfluencerRise/Scripts/Managers/RareLegendaryManagerEffects.cs`, listens to `PlayerStats.OnXpAdded` and tops up the owed bonus via a second, reentrancy-guarded `AddXp` call. Empirically verified via component-enabled/disabled toggle (50 XP → 375 credited enabled, exactly 50 disabled).

**Still open, no clean non-vendored hook found — NOT worked around with something fragile:**
- The Budget Bestie (purchaseCost) — read live and repeatedly from `Business.GetBusinessMultiplierOfType` wherever cost is checked or displayed; no single discrete event to intercept the way XP-granting has one.
- The Algorithm Itself (speed) — same problem as purchaseCost; production timing is read live and repeatedly from `BusinessHolder`'s own timer code, not a discrete grant.
- The Nepo Baby (dropWeight / Capsule pull odds) — resolves synchronously inside `ShopItemHolder.ClaimRewards` with no pre-roll event to intercept. The only workaround found — temporarily mutating the shared Capsule `ShopItem`'s `Output.weight` fields at claim time — was rejected as too fragile a pattern for shared asset data. Its own 8 individually-targeted dropWeight passiveBoosts entries (targeting the other Rare/Legendary managers, see EconomySchema.md) are themselves victims of the exact same gap, so fixing The Nepo Baby's *own* effect wouldn't do anything without this too.

The Wellness Guru was never affected by any of this — it already bypasses the whole broken pipeline via `BurnoutController.cs`'s direct-by-name holder lookup (built before this gap was even found, for an unrelated reason — CustomStats aren't a valid passiveBoosts target at all).

**Risk:** the fixed part of this doesn't touch vendored code, so no package-update risk there. The proper long-term fix (a vendored patch making `ManagersManager`'s lookups and/or `UpgradeEffectResolver.GetMatchingBlocks` iterate all owned managers, matching the same precedent as the `AddXp` fix below) would cover all 9 managers uniformly and automatically cover future ones — revisit if the 3 open managers' effects are needed for launch.

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

### `Level.AddXp` XP-math inconsistency
**Component:** `Assets/EasyIdleGame/Scripts/_Models/Levels/Level.cs` (vendored) · **Severity: Medium (needs investigation)** · **Status: Open**
`TargetXp`'s formula appears to compute a marginal (per-level) XP term, while `Mathb.GeometricSequence_Amount`/`Sum` appear to interpret it as an absolute cumulative value — found while investigating the claim-loop bug above, not yet root-caused or fixed. Means external XP-dosing (e.g. a test script trying to grant "exactly enough XP for level N") can't reliably land on an exact level even with the claim-loop bug fixed. Needs its own scoped investigation before touching.

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