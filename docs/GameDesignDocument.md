# Influencer Rise — Game Design Document

## Working title
**Certified Aura: Idle Influencer Tycoon**
("Aura" also serves as the in-game prestige currency name — see Economy Schema. Checked against App Store/Play Store listings, no exact collision found as of the last check — re-verify before submission.)

## Pitch

You are The Main Character. The player builds a self-mythologizing online empire — content, followers, brand deals, a personal cult of personality — playing into the "main character energy" / "I am a god in my own eyes" tone that defines a lot of current online culture. The game is satire wearing the skin of a genuine idle-tycoon: it takes your ambition completely seriously while the copy quietly mocks it.

## Tone & voice

- Deadpan self-importance. Upgrade names, flavor text, and milestones treat mundane content-creation acts as world-historical achievements.
- Never mean-spirited toward the player — satire targets the *culture*, not the player's choices.
- Naming direction: "Ascend to Verified," "Acquire a Personal Cult," "Outsource Your Authenticity," "Buy Back Your Ex's Attention," "Monetize Your Trauma."
- "Aura" is core in-game vocabulary ("Farm Aura," "Aura Rebrand"). The premium currency, **CloutCoin**, extends the satire into crypto/ICO-hype territory layered on influencer culture — grift-adjacent flavor text is on-brand ("Convert real money into imaginary relevance").

## Target audience

Mobile idle-game players who also engage with social-media/creator culture. **General/Teen (12+).** Satire stays playful, never mean-spirited or crude.

## Core loop

1. Create content (Business) → generates Followers/Cash.
2. Reinvest into more content types, Gear/Lifestyle Upgrades, and Team members (Managers) who automate production.
3. Manage **Burnout** — rises with production, decays passively, reduced by "self-care" Shop purchases.
4. Unlock new **Platforms** (Locations).
5. **Rebrand** (Prestige) for permanent bonuses, earning **Aura**.
6. Earn **CloutCoin** slowly through play (primarily level-up rewards); spend it — or buy more via IAP — on **Capsules** for a chance at rare/epic/mythic Managers with game-altering boosts. The free path is real but slow; that gap is the monetization hook.

## Session length target

Standard idle-game cadence: 2–5 minute active check-ins, multiple times daily, plus longer 10–20 minute sessions during active spending pushes.

## Platforms as Locations

**3 platforms for v1 launch:** Starter (free, tutorial), Short-form/"Clipz"-equivalent (fast content), Long-form/"Chronicle"-equivalent (slower, higher value). A 4th (brand/merch) platform is a post-launch content addition, not v1 scope.

## Currencies (overview — full detail in EconomySchema.md)

- **Followers** — primary production currency.
- **Cash** — secondary currency, Upgrades/Managers/Shop.
- **Aura** — prestige currency, earned only on Rebrand, persists across resets.
- **CloutCoin** — premium currency, earned slowly via level-up rewards, purchasable via IAP, spent on Manager Capsules.
- **Burnout** — a CustomStat, not a Currency. Decays passively, rises with production, resets on Rebrand.

## Monetization philosophy

No pay-to-win on core progression — real money buys convenience (time skips, Burnout relief) and accelerated access to rare Managers (via CloutCoin/Capsules), never something categorically unreachable through free play. IAP + rewarded ads. Rare/epic/mythic Managers are the primary "aspirational purchase," obtainable free through patient play (low-odds capsule pulls + level-up CloutCoin) or accelerated via real money — a legitimate F2P design pattern, not a dark pattern (no artificial progress-blocking chests tied to purchase, deliberately rejected — see Roadmap "explicitly cut" list).

## UI/UX direction

Reference: AdVenture Communist (Hyper Hippo) — studied closely for layout and UX patterns, not copied wholesale. Adopt the *structure*, not their specific art, IP, or bespoke mechanics (see Roadmap for what was explicitly excluded).

Target layout, our own visual flare within this structure:
- **Top bar:** player avatar/profile icon (top-left, tappable), currency displays (Followers/Cash/Aura/CloutCoin, top-center/right), hamburger menu icon (top-right, opens sliding dropdown: Settings, Inbox, News, Connect, Support — our equivalent of their menu contents).
- **Compact, information-dense business rows** — icon, name, owned count, production rate, buy button with Buy x1/x20/Max toggle — tighter and denser than our current first-pass UI, matching the reference's compact card style rather than our earlier oversized draft.
- **Bottom tab bar** for platform switching (already built, matches this pattern).
- **Sequential contextual tutorial** — speech-bubble style callouts pointing at specific UI elements as they become relevant, not a static text wall.
- **Legal/consent gate + loading screen** before first gameplay, required for store compliance regardless of style preference.
- **"Welcome back" offline-progress summary** on relaunch (maps to `ProfitApplicationSummary`, already native).

## Competitive differentiation

Existing "influencer idle" titles play the theme fairly literally. Ours leans into satire as the actual hook, uses multi-platform Location structure for genuine empire-building depth, and adopts proven idle-game UX patterns (studied via AdVenture Communist) without reproducing their specific bespoke systems (Trades, mission-chest progress-gating) — see Roadmap for the explicit exclusion list and reasoning.