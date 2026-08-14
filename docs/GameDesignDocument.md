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

### Business row anatomy (final, applies to every platform)
Left to right:
1. **Circular icon frame** (skin-system art placeholder slot) with an **owned-count badge** overlapping its bottom-left corner.
2. **Tap the icon to manually produce/collect** (for businesses not yet automated by an owned Manager) — no separate "Produce" button.
3. **Ticker/progress bar** between icon and buy button — fills 0→1 toward next production tick, driven by the business's actual production-round progress.
4. **One large Buy button** spanning most of the row width — dynamic full label ("BUY {amount}x {ShortName}"), amount driven by the shared Buy Amount selector.
5. **Small "i" info button**, dedicated tap target, opens the Detail Popup.

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