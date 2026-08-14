# Certified Aura: Idle Influencer Tycoon — Project Index

This file is the entry point Claude Code reads automatically. Read the linked docs before making changes — do not guess conventions.

## What this project is

A mobile idle-clicker built on the **Easy Idle Game** Unity asset (satirical "influencer/main-character" theme). Title: **Certified Aura: Idle Influencer Tycoon**. Engine: Unity 6.5 (6000.5.8f1). Target: mobile (iOS + Android), with real-money IAP and ads.

## Read these before doing any work

- `docs/GameDesignDocument.md` — theme, tone, core loop, UI/UX direction (including the locked row/popup/skin template).
- `docs/EconomySchema.md` — the authoritative list of every Currency, Business, Location, Manager, Upgrade, Boost, Shop Item, Achievement asset name, the Level Progression unlock schedule, and current build status per item. **Single source of truth for asset names. Never invent a new name for something already listed here. Never rename an entry once created**, except approved one-time exceptions already used (see Watch-list).
- `docs/EngineCapabilities.md` — what Easy Idle Game already does natively vs. what needs custom code, including all audit findings this project has run. **Check this before writing any new system.**
- `docs/NamingConventions.md` — folder placement and naming rules for new assets/scripts.
- `docs/MonetizationPlan.md` — Shop Item list, Capsule weights/costs, IAP product IDs, ad placements.
- `docs/BuildReleaseChecklist.md` — steps required before any store submission build.
- `docs/Roadmap.md` — current phase, completion status, and the V1/V1.1/Post-Launch scope split.
- `docs/BugTracker.md` — running log of confirmed bugs found/fixed this project, and known open issues. Check here before investigating something that looks like a regression.

## Hard rules — do not violate these

1. **Asset names are save-file identifiers.** Never rename a persisted definition asset once it exists in a build anyone has played. Treat names in `EconomySchema.md` as locked once added. (Two approved one-time exceptions have already been used — see Watch-list — because no player has ever loaded a real save. This exception does not reopen once the game ships.)
2. **All persisted definition assets must live under a folder literally named `Resources`**, inside `Assets/InfluencerRise/Resources/<category>/`.
3. **Never edit `.scene` or `.prefab` files as raw text.** Use Unity MCP scene/GameObject tools, or the Editor directly.
4. **Mutate economy state only through manager methods** (`TryBuyItems`, `TryTravelTo`, `CompleteExternalPurchase`, `TryPrestigeGame`, etc.) — never edit holder fields directly.
5. **Real-money purchases go through the documented external-completion contract** — subscribe to `OnAttemptRealMoneyPurchase`, run the real Unity IAP flow, verify the receipt, then call `CompleteExternalPurchase(item, sourceType)` exactly once.
6. **Don't touch anything under `Assets/EasyIdleGame/Demos/`.** Reference only. Removed entirely before final release build per `docs/BuildReleaseChecklist.md`.
7. **All new work goes under `Assets/InfluencerRise/`.**
8. Follow `docs/NamingConventions.md` for every new file/folder/asset name.
9. **Never present a paraphrase or reconstruction as a verbatim quote of a prior message.** If asked to quote something exactly, either produce it exactly or state plainly that you cannot verify it.
10. **When verification is requested on a factual/historical claim, prefer raw evidence (exact tool output, exact file diff) over a narrated summary.** State plainly when raw evidence isn't available.
11. **Label judgment calls as your own inference, explicitly, never as a restated prior instruction.**
12. **Test the way a real player would before declaring something done** — fresh state, UI taps only, no API shortcuts — for anything that ships. API-level verification proves backend logic works; it does not prove the player-facing path works.
13. **All UI work must use proper layout containers — Layout Groups, anchors, Content Size Fitters — never manual (x,y) positioning.** Every panel needs an explicit minimum-size floor (`LayoutElement` minWidth/minHeight) — never rely purely on content-driven auto-sizing, since a sibling's size change can silently collapse an under-specified panel (confirmed regression, see Watch-list). Consistent padding/margins reused throughout, readable mobile font sizes, currency/stat indicators as icon+label+value (never bare numbers). Before reporting any UI task done, self-review against `docs/GameDesignDocument.md`'s UI/UX Direction section, and get a real screenshot reviewed — component-value claims alone have proven insufficient to catch real visual defects on this project (see Watch-list).
14. **When a task boundary conflicts with a technical requirement of the task itself** (e.g. "don't touch Business assets" vs. "build a group containing Business assets"), **stop and ask for a scoped exception rather than silently violating the boundary or silently skipping the requirement.**
15. **After any Play Mode session, re-verify any "confirmed" claim with a fresh read.** Play Mode changes not explicitly saved can be silently discarded on Stop; do not trust in-session state as proof of a persisted fix.

## Known tooling limitations (this environment)

- **Automated Play Mode timing verification does not work through the Unity MCP bridge.** Commands execute synchronously on Unity's main thread, blocking the player loop from ticking during any wait. Set up test state via MCP, then hand off to the user to observe the result directly in the Editor.
- **Camera/scene screenshot capture does not work reliably through the MCP bridge** (`Unity_Camera_Capture` returns blank images; `Unity_SceneView_Capture2DScene` doesn't apply to Screen Space Overlay Canvas UI anyway). Use the same "set up state via MCP, user looks at Game view directly" pattern for visual UI verification — this project has repeatedly found real visual bugs that fresh component-value reads alone missed.
- **`Unity_ManageAsset` Move operations frequently report a false-negative "failed unexpectedly" error even when the move succeeds.** Always verify via a fresh filesystem/GUID check after a Move, don't trust the tool's own success/failure return value.

## Watch-list

- Main Camera's tag unexpectedly changed from "MainCamera" to "GameController" once early in the project (cause unknown at the time, fixed). If this recurs, investigate seriously.
- **Two approved one-time asset renames have been used**, both under Hard Rule #1's "no player has ever loaded a save" exception: `ClipzPlatform` → `QuickTokPlatform`, `ChroniclePlatform` → `BroadcastPlatform`. Also: 8 original Business assets and 5 original Manager assets were renamed in one batch to match final naming (see `EconomySchema.md` for the full old→new mapping). **This exception is now considered closed** — do not rename further assets without explicit fresh authorization, and never after the game has a real player save.
- **A `BusinessGroup` named `ClipzContentGroup` was never renamed** when the platform itself was renamed to QuickTok — a known, minor, currently-harmless naming inconsistency (it still functions correctly, contains TrendChase + ReactionContent, used for `FasterEditingSoftware`'s speed-upgrade targeting). Cosmetic cleanup only if convenient; not a functional bug.
- **Unity's own built-in AI Assistant package (separate from Claude Code/MCP) has its own `EnterPlayMode` tool** and can independently enter Play Mode in this project without any MCP/Claude Code action. Confirmed as a real, capable mechanism (found via source inspection) during an investigation into an unplanned Play Mode entry; never got a definitive causal log for that specific incident. If Unity enters Play Mode unexpectedly again, check whether this Assistant tool is the cause before assuming it's Claude Code's doing.
- `ShopItem` has no native cooldown/claim-interval field (only `maxPurchases`, a lifetime cap) — `FreeCapsule` currently has no cooldown gating as a result. Confirmed gap, needs a small custom script (same category as `BurnoutController.cs`) — not yet built as of last update, see Roadmap.
- **Vendored-code edit, scoped exception to Hard Rule #7**: `Assets/EasyIdleGame/Scripts/PlayerStats/PlayerStats.cs`, `AddXp` method, was patched to fix a confirmed bug — its level-claim loop (`for (int z = 0; z < level.unclaimedLevelups; z++) { level.LevelUp(); ... }`) re-read `level.unclaimedLevelups` every iteration while `LevelUp()` decremented that same field, so the rising counter and shrinking bound crossed at the midpoint and silently dropped the back half of any multi-level XP burst — both the level and its level-up reward. Fixed by caching the claim count before the loop starts. Reproduced and re-verified via Unity MCP RunCommand (14-level, 25-level, 2-level, and organic multi-level bursts from small XP ticks all now claim fully, `unclaimedLevelups` reaching exactly 0). **Because this is native package code, a future EasyIdleGame asset-store update could silently overwrite this fix and reintroduce the bug** — if level-up rewards (e.g. CloutCoin) or manager/business rewards ever start silently under-firing again after a package update, check this fix first before assuming it's a new issue.
- **Separate, unfixed finding from the same investigation**: `Level.AddXp` (`Assets/EasyIdleGame/Scripts/_Models/Levels/Level.cs`) appears to have a deeper internal math inconsistency, independent of the claim-loop bug above. `Level.TargetXp` is a **marginal per-level term** (`Mathb.GeometricSequence_NthTerm(startXp, incrementMultiplier, currentLevel+1)` = `startXp * q^(currentLevel)`), but `Level.AddXp` feeds `currentXp` into `Mathb.GeometricSequence_Amount(startXp, incrementMultiplier, currentXp)`, which interprets its input as an **absolute cumulative sum from level 0** (`Mathb.GeometricSequence_Sum`'s inverse). These are two different scales for the same field. Confirmed via manual trace and Unity MCP RunCommand: feeding a single level's exact marginal `TargetXp` (e.g. 4000 XP, the level-3 term) as `AddXp`'s input yielded `addedLevels=2`, not 1, once past the first couple of levels — because a marginal term value, read as if it were a from-zero cumulative sum, lands past a lower cumulative-sum boundary. Not investigated further or fixed (different bug, needs its own scoped decision) — flagged here so a future investigation into "why did leveling behave oddly" doesn't start from zero. Likely low real-world impact today since typical small production-tick XP gains rarely overshoot this way, but any code that manually grants a "one level's worth" of XP using `TargetXp` directly (rather than small organic increments) should be treated with suspicion.

## Current status

See `docs/Roadmap.md` for the up-to-date phase and completion tracker.