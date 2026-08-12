# Influencer Rise — Project Index

This file is the entry point Claude Code reads automatically. Read the linked docs before making changes — do not guess conventions.

## What this project is

A mobile idle-clicker built on the **Easy Idle Game** Unity asset (satirical "influencer/main-character" theme). Engine: Unity 6.5 (6000.5.8f1). Target: mobile (iOS + Android), with real-money IAP and ads.

## Read these before doing any work

- `docs/GameDesignDocument.md` — theme, tone, core loop. Read this before writing any player-facing text or naming any asset.
- `docs/EconomySchema.md` — the authoritative list of every Currency, Business, Location, Manager, Upgrade, Boost asset name and its unlock order. **This is the single source of truth for asset names. Never invent a new name for something already listed here. Never rename an entry once created — see Hard Rules below.**
- `docs/EngineCapabilities.md` — what Easy Idle Game already does natively vs. what needs custom code. **Check this before writing any new system.** Most features are Inspector configuration on existing ScriptableObjects, not new scripts.
- `docs/NamingConventions.md` — folder placement and naming rules for new assets/scripts.
- `docs/MonetizationPlan.md` — Shop Item list, IAP product IDs, ad placements.
- `docs/BuildReleaseChecklist.md` — steps required before any store submission build, including demo removal.
- `docs/Roadmap.md` — current phase and milestones.

## Hard rules — do not violate these

1. **Asset names are save-file identifiers.** The save system finds ScriptableObjects by their asset `name` via `Resources.LoadAll`. Never rename a persisted definition asset once it exists in a build anyone has played. Treat names in `EconomySchema.md` as locked once added.
2. **All persisted definition assets (Currency, Business, Manager, Upgrade, Boost, Location, ShopItem, Achievement) must live under a folder literally named `Resources`**, inside `Assets/InfluencerRise/Resources/<category>/`. Never place them elsewhere.
3. **Never edit `.scene` or `.prefab` files as raw text.** They're YAML with GUID references — use Unity MCP scene/GameObject tools, or the Unity Editor directly, never a text editor or blind find-replace.
4. **Mutate economy state only through manager methods** (`TryBuyItems`, `TryTravelTo`, `CompleteExternalPurchase`, `TryPrestigeGame`, etc.) — never edit holder fields directly. Direct edits skip events, caps, and save-worthy notifications.
5. **Real-money purchases must go through the documented external-completion contract**: subscribe to `OnAttemptRealMoneyPurchase`, run the actual Unity IAP flow, verify the receipt, then call `CompleteExternalPurchase(item, sourceType)` exactly once. Never call it on button press. Never substitute `ForceBuyItemsForFree` for a real purchase.
6. **Don't touch anything under `Assets/EasyIdleGame/Demos/`.** Reference only. It gets removed entirely per `docs/BuildReleaseChecklist.md` before the final release build.
7. **All new work goes under `Assets/InfluencerRise/`.** Never create loose assets/scripts outside that folder.
8. Follow `docs/NamingConventions.md` for every new file/folder/asset name — don't improvise a naming style mid-project.
9. Never present a paraphrase or reconstruction as a verbatim quote of a prior message. If asked to quote something exactly, either produce it exactly or state plainly that you cannot verify it — do not generate a plausible-sounding reconstruction and present it as confirmed fact.
10. When verification is requested on a factual/historical claim (not a current file state), prefer showing raw evidence (exact tool output, exact file diff) over a narrated summary. If raw evidence isn't available, say so explicitly rather than asserting confidence.

## Current status

See `docs/Roadmap.md` for the up-to-date phase. (Update this line as we progress.)



## Watch-list
- Main Camera's tag unexpectedly changed from "MainCamera" to "GameController" once (cause unknown, no git history available to trace it, fixed). If this recurs, treat it as a real bug to actively investigate, not a one-off glitch — check what operation immediately preceded it.

Camera/scene screenshot capture also does not work reliably through the Unity MCP bridge in this environment (Unity_Camera_Capture and Unity_SceneView_Capture2DScene both fail or return blank images). For visual UI verification, use the same "set up state via MCP, hand off to user to look at the Game view directly" pattern as the Play Mode timing limitation above.