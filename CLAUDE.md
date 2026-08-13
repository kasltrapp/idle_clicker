# Influencer Rise — Project Index

This file is the entry point Claude Code reads automatically. Read the linked docs before making changes — do not guess conventions.

## What this project is

A mobile idle-clicker built on the **Easy Idle Game** Unity asset (satirical "influencer/main-character" theme). Working title: **Certified Aura: Idle Influencer Tycoon**. Engine: Unity 6.5 (6000.5.8f1). Target: mobile (iOS + Android), with real-money IAP and ads.

## Read these before doing any work

- `docs/GameDesignDocument.md` — theme, tone, core loop, UI/UX direction.
- `docs/EconomySchema.md` — the authoritative list of every Currency, Business, Location, Manager, Upgrade, Boost, Shop Item, Achievement asset name and its unlock order. **Single source of truth for asset names. Never invent a new name for something already listed here. Never rename an entry once created.**
- `docs/EngineCapabilities.md` — what Easy Idle Game already does natively vs. what needs custom code, including confirmed audit findings. **Check this before writing any new system.**
- `docs/NamingConventions.md` — folder placement and naming rules for new assets/scripts.
- `docs/MonetizationPlan.md` — Shop Item list, IAP product IDs, ad placements, CloutCoin/capsule design.
- `docs/BuildReleaseChecklist.md` — steps required before any store submission build.
- `docs/Roadmap.md` — current phase, and the V1/V1.1/Post-Launch scope split.

## Hard rules — do not violate these

1. **Asset names are save-file identifiers.** Never rename a persisted definition asset once it exists in a build anyone has played. Treat names in `EconomySchema.md` as locked once added.
2. **All persisted definition assets must live under a folder literally named `Resources`**, inside `Assets/InfluencerRise/Resources/<category>/`.
3. **Never edit `.scene` or `.prefab` files as raw text.** Use Unity MCP scene/GameObject tools, or the Editor directly.
4. **Mutate economy state only through manager methods** (`TryBuyItems`, `TryTravelTo`, `CompleteExternalPurchase`, `TryPrestigeGame`, etc.) — never edit holder fields directly.
5. **Real-money purchases go through the documented external-completion contract** — subscribe to `OnAttemptRealMoneyPurchase`, run the real Unity IAP flow, verify the receipt, then call `CompleteExternalPurchase(item, sourceType)` exactly once.
6. **Don't touch anything under `Assets/EasyIdleGame/Demos/`.** Reference only. Removed entirely before final release build per `docs/BuildReleaseChecklist.md`.
7. **All new work goes under `Assets/InfluencerRise/`.**
8. Follow `docs/NamingConventions.md` for every new file/folder/asset name.
9. **Never present a paraphrase or reconstruction as a verbatim quote of a prior message.** If asked to quote something exactly, either produce it exactly or state plainly that you cannot verify it — do not generate a plausible-sounding reconstruction and present it as confirmed fact.
10. **When verification is requested on a factual/historical claim, prefer raw evidence (exact tool output, exact file diff) over a narrated summary.** If raw evidence isn't available, say so explicitly rather than asserting confidence.
11. **Label judgment calls as your own inference, explicitly, never as a restated prior instruction.** If you made a design decision the user or schema didn't specify, say "my own inference" — don't imply it was previously agreed.
12. **Test the way a real player would before declaring something done** — fresh state, UI taps only, no API shortcuts — for anything that will ship. API-level verification proves backend logic works; it does not prove the player-facing path works.

## Known tooling limitations (this environment)

- **Automated Play Mode timing verification does not work through the Unity MCP bridge.** Commands execute synchronously on Unity's main thread, blocking the player loop from ticking during any wait. For features involving production timers, boost durations, or decay: set up test state via MCP, then hand off to the user to observe the result directly in the Editor.
- **Camera/scene screenshot capture does not work reliably through the MCP bridge** (`Unity_Camera_Capture` and `Unity_SceneView_Capture2DScene` both fail or return blank images in this environment). Use the same "set up state via MCP, user looks at Game view directly" pattern for visual UI verification.

## Watch-list

- Main Camera's tag unexpectedly changed from "MainCamera" to "GameController" once (cause unknown, no git history available to trace it, fixed). If this recurs, treat it as a real bug to actively investigate — check what operation immediately preceded it.

## Current status

See `docs/Roadmap.md` for the up-to-date phase.