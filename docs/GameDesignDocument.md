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

### Final row/screen template — ✅ built (StarterFeed screen, all 3 rows)

This is the settled, final template. Do not re-litigate or rebuild this piecemeal — extend it the same way for every future Location's business list.

**Skin system (reskin-ready architecture):** every background panel — row backgrounds, the detail popup, the top bar, the bottom tab bar, the screen background, the buy-amount bar — uses a uGUI `Image` set to `Type: Sliced` (9-slice), referencing a placeholder sprite from `Assets/InfluencerRise/Art/UI/Skins/`. Two placeholders exist today: `Skin_PanelPlaceholder.png` (generic rectangular panel, 9-sliced) and `Skin_IconFramePlaceholder.png` (circular, for icon frames and the popup's art slot — a true 9-slice doesn't apply meaningfully to a circle). Real art later replaces only the `Sprite` field on these `Image` components; no layout or hierarchy changes required. See the README in that folder for the full convention.

**Minimum-size floor (standing rule):** every HUD panel must have an explicit minimum size floor, never purely content-driven auto-sizing. Concretely: any panel or interactive element placed inside a Layout Group must carry its own `LayoutElement` with an explicit `minWidth` and/or `minHeight` (whichever axis that element's content actually needs protected — a horizontally-arranged row needs a width floor, a vertically-stacked panel needs a height floor, some need both). Relying on a bare `HorizontalLayoutGroup`/`VerticalLayoutGroup` with `Control Child Size` on but no `LayoutElement` on the child leaves that child's size entirely at the mercy of its siblings and its own transient content — it can silently collapse or clip the moment a neighboring element changes, with no error or warning. This was learned the hard way: the top bar's currency displays had no `LayoutElement` at all and were sized purely from auto-computed text width (49.43px, an arbitrary content-driven value), and the buy-amount bar had no `LayoutElement` and its `HorizontalLayoutGroup` had `Control Child Size Width` off, so its Label/Button children's `RectTransform` widths never honored their own `LayoutElement.preferredWidth` — both shipped squashed/clipped without any single edit that looks obviously wrong in isolation. Every panel audited under this rule so far (top bar currency slots, buy-amount bar, bottom tab bar, business rows) now carries an explicit floor; apply the same treatment to every new HUD panel going forward.

**Screen background vs. panels:** the screen background (`MainCanvas/ScreenBackground`, first sibling so it renders behind everything) uses the same panel skin sprite at a distinctly darker tint than rows, so rows read as raised panels sitting on top of a textured background rather than blending into a flat color.

**Buy amount selector (shared, global):** `BusinessesManager.buyAmounts` = `[x1, x20, Max(100%)]`. One shared control (`SafeAreaRoot/BuyAmountBar`, positioned between the top bar and the business list) cycles through these via `BusinessesManager.ChangeBuyAmount()`. Every row's buy button reads this same shared selection for both its label and its actual purchase amount — this is entirely native (`BusinessesManager.GetTargetBusinessBuyAmount()`, already what `BusinessDisplayer.buyButton` calls); nothing per-row needs to be duplicated. A small subclass, `InfluencerRise.UI.BuyAmountLabelDisplayer`, only overrides the display label so the 100%-percent entry reads "MAX" instead of the native "100%" — purely cosmetic, the underlying selection/purchase logic is 100% native.

**Row anatomy** (`Assets/InfluencerRise/Prefabs/UI/UI_BusinessRow.prefab`, left to right):
1. **Circular icon frame** (`IconFrame`, skin-system slot, 72×72) — also the **tap-to-produce** target: tapping it calls `BusinessDisplayer.Click()` → `BusinessHolder.TryStartProduction()`, for manually triggering production on businesses not yet automated by an owned Manager. Matches the reference's "tap the character to work" interaction; there is no longer a separate dedicated Produce button.
2. **Owned-count badge** (`IconFrame/OwnedBadge`) — a small pill overlapping the icon's bottom-left corner (anchored to that corner, `LayoutElement.ignoreLayout = true` so it doesn't participate in the row's horizontal flow), showing `BusinessDisplayer.amountText`. Layered on the icon, not a separate floating number in the row.
3. **Center column** (`CenterColumn`, flexible width): business name, then a **ticker/progress bar** (`Ticker`) — a non-interactive `Slider` driven by `BusinessHolder.GetProductionProgress()` (confirmed 0–1 value, already what the native `BusinessDisplayer` uses), with production-per-second and time-left text overlaid on top. A `FastFill`/"READY!" state (native `fastFillTreshold`) swaps in when time-to-produce is too short for the bar to read as meaningfully filling.
4. **One large BUY button** (`BuyButton`) spanning most of the remaining row width. Dynamic label reads "Buy {amount}x {SHORT NAME}" — the short name comes from `Metadata.iconString` (set per-Business, e.g. `"FIRST POST"`), a field the native `BusinessDisplayer.GetBuyButtonsString()` already reads via `showIconStringInBuyButton`. Purchases at the shared buy-amount selector's current amount via `BusinessDisplayer.BuyBusiness()`.
5. **Small "i" info button** (`InfoButton`), corner-anchored to the row's top-right (`ignoreLayout = true`, pinned via anchors, not manual positioning), a separate tap target from the buy button. Opens the detail popup for that row's specific business.

**Detail popup** (`MainCanvas/BusinessDetailBackdrop`): a full-screen dimmed backdrop (tap-to-close) containing a centered skinned panel. The `IOpenable`/`BusinessDetailPopup` component lives on the **backdrop** GameObject itself, not the inner panel — `IOpenable.Open()`/`Close()` toggle `GameObject.SetActive()` on whichever GameObject the component is attached to, so the component must sit on the object that needs to become visible as a whole. (Found this the hard way: putting it on the inner panel meant `Open()` activated a panel whose inactive parent still hid it entirely.) Shows: name, description (`Business.Metadata.description` — confirmed this field exists and is now authored with real flavor text for all 3 StarterFeed businesses), cost, production, speed, cost multiplier, and — if the business has an assigned Manager — that Manager's name/level/owned-status via a nested `ManagerDisplayer`. Built as `InfluencerRise.UI.BusinessDetailPopup : BusinessDetailDisplayer`, since the base class's `Redraw_Default()` reads the deprecated `Business.outputs` legacy field (always empty for content authored against `outputTables`, which is everything we've built) instead of the real `Business.Outputs` property — our subclass overrides this to fix it, plus adds the speed/cost-multiplier fields the base class doesn't expose at all.

**Top bar:** currency displays (Followers, Cash, Aura, Burnout) rebuilt as fixed-width (84×58) icon+label+value slots — skin-frame icon placeholder on top, a small caps-case name label ("FOLLOWERS", "CASH", "AURA", "BURNOUT"), and the value below, each with an explicit `LayoutElement` floor per the minimum-size-floor rule above. The three currency slots (not Burnout, which has no shop entry) are each followed by a small "+" button (present and styled, functionally unwired for now — real Shop-screen wiring is a separate future task). Player avatar and hamburger menu are still not built — deferred, not yet reached in the build order.

**Not yet built:** player avatar/profile icon, hamburger menu + sliding dropdown, sequential contextual tutorial, legal/consent gate + loading screen, "welcome back" offline-progress summary UI (the underlying `ProfitApplicationSummary` data is native, just not surfaced yet).

- **Bottom tab bar** for platform switching (already built, matches this pattern; also retrofitted to the skin system this pass).
- **Sequential contextual tutorial** — speech-bubble style callouts pointing at specific UI elements as they become relevant, not a static text wall.
- **Legal/consent gate + loading screen** before first gameplay, required for store compliance regardless of style preference.
- **"Welcome back" offline-progress summary** on relaunch (maps to `ProfitApplicationSummary`, already native).

## Competitive differentiation

Existing "influencer idle" titles play the theme fairly literally. Ours leans into satire as the actual hook, uses multi-platform Location structure for genuine empire-building depth, and adopts proven idle-game UX patterns (studied via AdVenture Communist) without reproducing their specific bespoke systems (Trades, mission-chest progress-gating) — see Roadmap for the explicit exclusion list and reasoning.