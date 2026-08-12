# Influencer Rise — Game Design Document

## Working title
**Certified Aura: Idle Influencer Tycoon**
("Aura" also serves as the in-game prestige currency name — see Economy Schema.)

## Pitch

You are The Main Character. The player builds a self-mythologizing online empire — content, followers, brand deals, a personal cult of personality — playing into the "main character energy" / "I am a god in my own eyes" tone that defines a lot of current online culture. The game is satire wearing the skin of a genuine idle-tycoon: it takes your ambition completely seriously while the copy quietly mocks it.

## Tone & voice

- Deadpan self-importance. Upgrade names, flavor text, and milestones treat mundane content-creation acts as world-historical achievements.
- Never mean-spirited toward the player — the satire targets the *culture*, not the player's choices. The player should feel like they're in on the joke, not the butt of it.
- Example naming direction: "Ascend to Verified," "Acquire a Personal Cult," "Outsource Your Authenticity," "Buy Back Your Ex's Attention," "Monetize Your Trauma."
- "Aura" is a core piece of in-game vocabulary — "Farm Aura," "Aura Rebrand," low-Aura/high-Aura flavor text — leaning on current slang ("aura farming") without overcommitting the permanent app title to a single slang term.

## Target audience

- Mobile idle-game players (broad casual audience) who also engage with social-media/creator culture — the satire lands harder for anyone who's spent time on TikTok/Instagram, but the core loop should work even for someone who doesn't get every joke.
- **General/Teen (12+).** Satire stays playful, never mean-spirited or crude.

## Core loop

1. Create content (Business) → generates Followers/Cash.
2. Reinvest Cash into more content types, Gear/Lifestyle Upgrades, and Team members (Managers) who automate production.
3. Manage **Burnout** (the "stress" stat) — it decays your output/aesthetic if ignored, and is reduced by spending Cash on "self-care" purchases (a built-in soft monetization hook).
4. Unlock new **Platforms** (Locations) — TikTok-style, long-form/Substack-style, merch/brand — each with its own Businesses and pacing.
5. **Rebrand** (Prestige) — walk away from a burned-out persona and relaunch with permanent bonuses, echoing real influencer "reinvention" cycles. Earns **Aura**, the prestige currency.

## Session length target

Standard idle-game cadence: 2–5 minute active check-ins, multiple times daily, with longer 10–20 minute sessions during active spending/upgrade pushes.

## Platforms as Locations

Each Platform is a `Location` with its own unlock cost, starter rewards, and Business roster.

**3 platforms for v1 launch:**
1. **Starter platform** — free, tutorial-pace, teaches core loop.
2. **Short-form platform** (TikTok-style) — fast, volume-based content.
3. **Long-form platform** (Substack/YouTube-style) — slower, higher-value content, unlocked mid-game.

Brand/merch platform reserved as a **post-launch content update**, not part of initial release — ship faster, expand after launch validates the core loop.

## Currencies (overview — full detail goes in EconomySchema.md)

- **Followers** — primary "production" currency, drives most Business costs.
- **Cash** — secondary currency, spent on Upgrades/Team/Shop.
- **Aura** — prestige currency, earned only on Rebrand, spent on permanent bonuses.
- **Burnout** — a CustomStat, not a Currency (doesn't buy anything) — decays engagement if it climbs too high, discussed above.

## Monetization philosophy

**IAP + rewarded ads.** No pay-to-win pressure on core progression — real money buys convenience (time skips, Burnout relief) and cosmetics/vanity (persona skins, verified badge cosmetics), not permanent stat advantages that make free players feel obsoleted. Rewarded ads (via `ShopManager`'s built-in `buyWithAd` hook) sit alongside IAP at near-zero extra dev cost.

## Competitive differentiation (from earlier research)

- Existing "influencer idle" titles (Idle Influencer, Lamar) play the theme fairly literally — follower-counting with light narrative dressing. Ours leans into satire as the actual hook, and uses multi-platform structure (via Locations) for genuine empire-building depth that competitors don't attempt.
- Title checked against existing App Store/Play Store listings — no exact collision found for "Certified Aura: Idle Influencer Tycoon" as of this check. Re-verify manually immediately before store submission, since availability can shift over months of development.