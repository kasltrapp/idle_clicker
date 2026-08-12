# Engine Capabilities — Bolt-On vs Custom Code

Check this before writing ANY new script. Most of this game is Inspector configuration on existing Easy Idle Game systems, not new code. Writing a custom system for something already covered here is a mistake — verify first.

## Zero-code (Inspector/ScriptableObject configuration only)

| Feature | Engine system |
|---|---|
| Platforms (TikTok/Substack/etc.) | `Location` — metadata, locks, unlock cost, starter rewards, `LocationsManager.TryTravelTo()` |
| Followers/Cash/Aura currencies | `Currency` — unlimited definitions, per-currency and group caps |
| Content creation (posts, streams, essays) | `Business` — cost, timing, output, chaining |
| Team members (Ghostwriter, Editor, etc.) | `Manager` — automation, passive/active boosts |
| Gear/lifestyle upgrades | `Upgrade` — production, speed, cost multipliers |
| Rebrand reset | `PrestigeManager` — full reset scoping, exclusions, per-call options |
| Milestones/achievements | `AchievementsManager` — tiered, repeatable, TotalAmount locks |
| Weekly boosts/sales | `OffersManager` — weighted timed offer pool from Shop Items |
| Save/load, offline profit | `SaveAndLoad`, `ProfitCalculator` — fully built in |

## Requires real integration work (unavoidable, budget time for these)

| Feature | What's needed |
|---|---|
| Real-money IAP | Unity IAP package + `ShopManager.OnAttemptRealMoneyPurchase` subscription + receipt verification + `CompleteExternalPurchase()` call. Engine provides the hook, not the store integration itself. |
| Rewarded ads | Ad mediation SDK + `ShopManager.OnAttemptAdPurchase` subscription, same completion contract as IAP. |

## Requires small custom script (confirmed gap, keep minimal)

| Feature | What's needed | Why |
|---|---|---|
| Burnout auto-decay/rise | One `MonoBehaviour` calling `PlayerStats.IncrementStat(Burnout, ±amount)` on a timer/production event | `CustomStat` storage, persistence, locks, and events are all built-in — but nothing ticks it automatically. This is the one piece of genuinely new gameplay logic in the whole design. |
| Burnout affecting production output | A `ProductionOutputModifierAsset` (documented, supported extension point — not from-scratch) | Standard package pattern for project-specific output scaling; not a new system, just a documented subclass. |

## Rule of thumb

If a feature sounds like "restrict/modify/automate purchasing, producing, or resetting," check for an existing manager method first (`TryBuyItems`, `TryTravelTo`, `TryPrestigeGame`, `CompleteExternalPurchase`, etc.) before writing anything new. Direct holder edits or parallel custom systems are almost always wrong — see CLAUDE.md Hard Rule #4.

## Known tooling limitation

Automated Play Mode timing verification does NOT work through the Unity MCP bridge — commands execute synchronously on Unity's main thread, which blocks the player loop from ticking during any wait. For any feature involving production timers, boost durations, or other real-time behavior: set up the test state via MCP, then hand off to the user to observe the result directly in the Editor. Do not attempt QueuePlayerLoopUpdate() polling loops or similar workarounds — already confirmed not to work in this environment.