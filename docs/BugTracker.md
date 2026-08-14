# Bug Tracker & Fix Log

Running record of confirmed bugs found and fixed during development, plus known open issues. Check here first whenever something behaves like a regression — it may be a previously-fixed issue that resurfaced (e.g. via a package update overwriting a vendored-code fix).

Format per entry: **Date/session · Component · Severity · Status**

---

## Fixed

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