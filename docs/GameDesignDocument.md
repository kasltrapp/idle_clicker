# Certified Aura: Idle Influencer Tycoon — Game Design Document

## Working title
**Certified Aura: Idle Influencer Tycoon** (locked). "Aura" is also the in-game prestige currency name.

## Pitch

You are The Main Character. The player builds a self-mythologizing online empire across three platforms — Yourgram, QuickTok, and Broadcast — playing into "main character energy" / "I am a god in my own eyes" internet culture. Genuine idle-tycoon mechanics wrapped in satire that takes the player's ambition completely seriously while quietly mocking the culture it's drawn from.

## Tone & voice

- Deadpan self-importance. Never mean-spirited toward the player — satire targets the *culture*, not the player.
- "Aura" and "CloutCoin" (premium currency) are core vocabulary — CloutCoin extends the satire into crypto/ICO-hype territory layered on influencer culture.

## Target audience

Mobile idle-game players who engage with social-media/creator culture. **General/Teen (12+).**

## Core loop

1. Create content (Business) → generates Followers/Cash, across 3 platforms.
2. Reinvest into more businesses and Managers who automate production.
3. Manage **Burnout** — rises with production (design pending, see EngineCapabilities.md), decays passively, reduced by Spa Day Shop items, and by owning/leveling the Rare Manager "The Wellness Guru."
4. Unlock platforms and businesses via the locked **Level Progression** schedule (see EconomySchema.md) — an interleaved structure where all 3 platforms run simultaneously well before any single one is finished.
5. **Rebrand** (Prestige) for `Aura`, spent on 3 permanent, player-chosen Upgrades. Player Level persists across Rebrand (does not reset).
6. Earn **CloutCoin** slowly via level-up rewards (native, config pending); spend it — or buy more via IAP — on **Capsules** for a chance at Rare/Legendary Managers. A genuine free path exists (Free/Watch-Ad Capsules), real money buys better odds, never guaranteed exclusivity — **Legendary managers can never be obtained via the ad path**, enforced and verified in the built Capsule data.

## Platforms (locked names)

**Yourgram** (starter, free) → **QuickTok** (unlocks Level 12) → **Broadcast** (unlocks Level 34). All 3 run in parallel from Level 34 onward. See EconomySchema.md for the full 5-business roster per platform and the complete Level Progression table.

## Currencies

- **Followers** — primary production currency.
- **Cash** — secondary currency.
- **Aura** — prestige currency, Rebrand-only, persists across resets.
- **CloutCoin** — premium currency, level-up rewards + IAP, spent on Capsules, persists across resets.
- **Burnout** — CustomStat (not a Currency), 0–100, resets on Rebrand.

## Monetization philosophy

No pay-to-win. Real money buys convenience and accelerated access to rare content, never something categorically unreachable free. IAP + rewarded ads. **Explicitly rejected**: progress-gated "mission chests" requiring purchase to unlock required progression currency — identified as a real dark pattern in competitive research, rejected on principle.

## UI/UX Direction — LOCKED TEMPLATE (do not re-litigate or rebuild piecemeal)

Reference: AdVenture Communist (Hyper Hippo) — studied for layout/UX patterns, not copied (no reused mechanics like their "Trades" system or bespoke fiction).

### Skin system (foundation)
Every background panel (rows, popups, top bar, tab bar, screen background, buy-amount bar) uses `Image.Type.Sliced` (9-slice) referencing a placeholder sprite in `Assets/InfluencerRise/Art/UI/Skins/`, clearly marked as a swap-ready art slot. Real texture art drops in without re-touching layout code.

### Business row anatomy (FINAL v2 — LOCKED to the approved mockup, do not make further layout changes without a new explicit task)
Superseded the previous "continuous fused bar" version after the approved mockup (`business_row_mockup_v2.html`) specified a different structure. Row height 146. Structure (`UI_BusinessRow.prefab`), two columns, top-aligned (`ChildAlignment = UpperLeft`, root `HorizontalLayoutGroup` padding 14/spacing 14):

- **Row root**: single `Image` (Sliced, skin-system placeholder, dark navy `#1c2338`) spans the entire bar.
- **`IconCol`** (fixed 110 wide, `minWidth` floor — see hardening checklist): `VerticalLayoutGroup` with **negative spacing (-14)** so the pill overlaps up into the icon's bottom edge, matching the mockup's `margin-top:-10px`. `RectMask2D` added (hardening — see below). Contains:
  1. **`Icon`** — circular frame, 104×104, `minWidth`/`minHeight` floor of 104 (Hard Rule #13 — this exact class of bug recurred during the v2 rebuild: an unfloored `IconCol` let a long business name squeeze it, same failure mode as the v1 `IconFrame` bug). `Icon`'s own `Image` (Simple, `Skin_IconFramePlaceholder`, `PreserveAspect`) is the decorative ring and the `Button` target for tap-to-produce. Child `IconArtMask` (fixed 96×96, **center-anchored**, `Mask` + alpha-cutout sprite, `showMaskGraphic=false`) → child `IconArt` (`PreserveAspect=true`) is the real-art drop-in slot (`BusinessDisplayer.image` points here).
  2. **`OwnedPill`**: true pill shape (`Sliced`, high effective corner radius, dark `#0e1220`), width via `ContentSizeFitter` (PreferredSize) so it hugs the owned-count text, height fixed at 28 (**note**: with the parent `VerticalLayoutGroup`'s `childControlHeight=false`, the pill's `RectTransform.sizeDelta` must be set directly — a `LayoutElement.preferredHeight` alone silently does nothing in that configuration; found as a real bug during this build, since the GameObject defaulted to Unity's stock 100×100 otherwise). `AmountText` uses TMP auto-sizing (9–14pt) + Ellipsis overflow as a second line of defense (see hardening checklist).
- **`RightCol`** (flexible width): `VerticalLayoutGroup`, zero spacing, contains:
  1. **`Ticker`** (36 tall, flat `Simple` dark rect, no independent skin border): shows `TimeLeftText` (left) and `ProductionPerSecondText` (right) via a space-between-style two-flexible-child layout. A subtle `Slider`-driven fill (`FillContainer`/`Fill Area`/`Fill`, low-contrast) still drives `BusinessDisplayer.productionSlider` behind the text — the mockup itself is a static HTML file with no animation, so the progress-fill visual is this session's own synthesis to keep the field functionally wired; flagged as inference, not mockup-specified. `FastFill` ("READY!" alt state) unchanged in spirit, repositioned here.
  2. **`BuyRow`** (56 tall): `BuyButton` (flexible width, 150 `minWidth` floor) directly beside `InfoButton` (fixed 46 wide, `minWidth` floor). No separate `NameText` element — the business name only appears inside the buy button's own dynamic label ("Buy {amount}x {Name}"), matching the mockup exactly; `BusinessDisplayer.nameText` is intentionally left unbound (`null`), same pattern as the already-unused `descriptionText`/`levelSlider`/`mainButton` fields.
     - `BuyButton` color is state-driven **natively**, no custom code: `Button.colors.normalColor` = green (`#3cb043`), `disabledColor` = grey (`#4a4f68`), `transition = ColorTint`. Since `BusinessDisplayer.Redraw()` already toggles `Button.interactable` based on affordability/unlock, Unity's own Selectable color-tint system handles the green/grey swap automatically.

Applies to all 5 Yourgram rows (First Post, Selfie Session, Story Feed, Engagement Farming, Verified Status) via the shared prefab — **Story Feed and Engagement Farming previously shipped with only 3 of 5 rows built (Selfie Session and Verified Status had no UI representation at all), which permanently blocked the whole chain past First Post since Story Feed's unlock condition requires owning 15× Selfie Session. Closed as part of this task; see Hard Rule #16.** Verified end-to-end in real Play Mode via genuine raycast-driven taps on the actual rows (not API shortcuts) — Followers-currency growth, a live purchase through Selfie Session's own row, and Story Feed's row flipping from locked/non-interactable to unlocked/interactable/purchasable after its conditions were met. Will be reused as-is for QuickTok/Broadcast rows when those are built — **no further structural changes to this template without a new explicit task.**

#### Hardening checklist (applied and verified this pass — re-check this list whenever the template is touched again)
1. **Image scaling safety**: `IconArt` uses `PreserveAspect=true` inside a fixed-size (96×96) circular mask. Verified with three test sources — 1024×1024 square, 1024×600 wide, and 600×1024 tall — confirming the mask stays a true 96×96 circle and the art letterboxes/pillarboxes rather than stretching, regardless of source aspect ratio.
2. **Texture wrapping safety**: audited `Skin_PanelPlaceholder.png` and `Skin_IconFramePlaceholder.png` import settings — both already `TextureWrapMode.Clamp` (not `Repeat`), preventing tiling/mirroring artifacts at the mask's alpha-cutout edge.
3. **Number overflow safety**: `OwnedPill`'s `ContentSizeFitter` has no upper bound, so an unusually long number string could in principle grow past `IconCol`'s fixed 110px width and bleed into `RightCol`. Hardened two ways: `RectMask2D` on `IconCol` as a hard clip guarantee (nothing can ever visually escape the column, verified with a 12-digit stress value), plus TMP auto-sizing (9–14pt) with Ellipsis overflow on `AmountText` as graceful degradation before clipping kicks in. Realistic BigNumber-formatted values (e.g. "999.9K", "12.3M") fit comfortably without shrinking.
4. **Fixed container safety**: every fixed-size element (`IconCol` 110, `Icon` 104×104, `InfoButton` 46, `BuyButton` 150 minWidth, `OwnedPill` 28 height) has an explicit `LayoutElement` minWidth/minHeight floor per Hard Rule #13. Verified across all 5 rows post-build, including Engagement Farming (the longest business name) — all fixed dimensions held identical across every row regardless of name length.

### Buy Amount selector (global, shared)
One control (x1 / x20 / MAX) above the business list. Every row's buy button reads this single shared selection — never duplicated per row.

### Detail popup
Opened via each row's info button (`BusinessDetailDisplayer`, subclassed to fix two real package gaps found during build — see EngineCapabilities.md). Shows full name, description, cost, production rate, speed, cost multiplier, assigned Manager status, larger art slot.

### Top bar
Player profile icon (top-left, tappable — not yet built), currency displays as icon+label+value fixed-width slots with a "+" quick-purchase icon (unwired placeholder, real Shop wiring pending), hamburger menu (top-right, not yet built — target contents: Settings/Inbox/News/Connect/Support).

### Minimum-size floor rule (standing, see CLAUDE.md Hard Rule #13)
Every HUD panel gets an explicit `LayoutElement` minWidth/minHeight floor. A real regression (Buy Amount bar silently covered by a mis-anchored sibling panel) proved content-driven auto-sizing alone is not sufe against layout drift as more panels get added.

### Not yet built (tracked in Roadmap, not scope creep to add without a task)
- QuickTok/Broadcast business row lists (only Yourgram's 5 rows exist).
- Real tab-switching logic (bottom tabs are currently static hardcoded labels with zero listeners).
- Manager/Capsule pull screen, Prestige screen, Achievements screen, Shop screen.
- Splash/legal-consent gate, loading screen, settings, player profile, sequential tutorial, "welcome back" offline-summary screen.

## Competitive differentiation

Multi-platform Location structure for genuine empire-building depth; satire as the primary hook rather than surface theming.