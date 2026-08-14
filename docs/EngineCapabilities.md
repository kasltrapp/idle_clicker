# Engine Capabilities — Bolt-On vs Custom Code

Check this before writing ANY new script.

## Zero-code (Inspector/ScriptableObject configuration only) — confirmed by direct code audit

| Feature | Notes |
|---|---|
| Platforms, Currencies, Businesses, Managers, Upgrades, Prestige reset scoping, Achievements, Save/load, offline profit | Fully native, all built and verified this project |
| Level-up rewards | `PlayerStats.levelupData` (`LevelDataHolder`) — exact-level (`levelRewards`) and repeating/divisor-based (`eachLevelReward`, `scaleEachLevelReward`) rewards via the standard Reward→DropTable→Output system. **Not yet configured for CloutCoin** — pending task. |
| Manager auto-leveling from duplicates | `AddCount()` auto-fires on purchase/grant (`ManagerHolder.AddManagerCount()`). Copies-per-level curve is a fixed global geometric formula (×2/level) — NOT independently configurable per Manager asset without subclassing. Rarity differentiation comes from pull odds + per-level power, not a custom curve. |
| Manager platform-wide/global passive effects | `Manager.passiveBoosts` (`List<UpgradeBlock>`) is the *identical* type used by `Upgrade.upgrades` — same `UpgradeBlockFilter`, same `targetBusinessGroups` mechanism. Fully proven this project (6 Rare Managers built this way). |
| **Weighted Manager-output DropTable ("Capsule" mechanic)** | **CONFIRMED WORKING, not just structurally sound** — proven twice: an isolated POC (157/43 split against an 80/20 weight, within 1 std dev) and the real production Capsule build (24-manager weighted pool, statistically verified zero-Legendary-selection over 10,000 trials on ad/free tiers). No caveat remains — this is a fully proven mechanic. |

## Confirmed NOT supported — needs genuinely new code/systems

| Feature | Confirmed via | Status |
|---|---|---|
| Manager rarity/tier | Full code read, package-wide grep, zero native matches. | ✅ Built — custom `Rarity` enum + `ManagerRarity` companion ScriptableObject (tag + Capsule-weight lookup only; cannot and does not feed native leveling math). |
| Manager group-based targeting (e.g. "all Rare/Legendary managers") | `UpgradeBlockFilter` has `targetManager` (single) but no `targetManagerGroups` — confirmed via full filter-field read. | Worked around for The Nepo Baby via 8 individually-targeted entries — **manual maintenance required**, see EconomySchema.md. |
| CustomStat (Burnout) as an Output/Upgrade/passiveBoost target | `Output.cs` has exactly 4 grant fields, none for CustomStats. `UpgradeType`'s 14 values have no stat-multiplier option. Confirmed by direct code read multiple times (Aura audit, Rare Manager audit). | ✅ Built — `BurnoutController.cs`, 4 custom responsibilities: decay tick, Rebrand reset, Spa Day shop-relief, Wellness Guru decay-rate modifier. This is the ONE piece of core custom gameplay logic in the whole project. |
| `ShopItem` cooldown/claim-interval | `ShopItem.cs` has only `maxPurchases` (lifetime cap), no recurring-interval field. Confirmed via full class read. | ✅ Built — `FreeCapsuleCooldown.cs`, same category as `BurnoutController.cs`. FreeCapsule's free/no-cost purchase path has no cancellable pre-purchase event (unlike ad/IAP), so the gate owns the entry point instead: a "Claim" UI must call `TryClaimFreeCapsule()` rather than `ShopManager.AttemptPurchase` directly. Timestamp persisted via PlayerPrefs, not SaveFile.cs (SaveFile.cs is native-package code outside `Assets/InfluencerRise/`, see Hard Rule #7). |
| Manager max level / duplicate overflow / salvage | `Manager.MaxLevel = BigNumber.MaxValue` by default, no override. No absorb/error/salvage path exists because the precondition (a real cap) doesn't occur under default config. | Deliberately not built for launch — infinite leveling is arguably better UX than a cap. Revisit only with real player data. |
| Player Cosmetics / avatar system | Package-wide grep for avatar/cosmetic/skin terms — zero matches. No 5th Output type. | Deferred to V1.1. |

## Package gotchas found during UI build (useful for future UI work)

- `IOpenable.Open()`/`Close()` toggle the GameObject the component is *on* — must be placed on the actual panel that should show/hide, not a wrapper/backdrop, or nothing visibly happens.
- `BusinessDetailDisplayer`'s base `Redraw_Default()` reads the deprecated `Business.outputs` legacy field (always empty for content built this project) instead of the real `Business.Outputs` property — subclassed and fixed.
- Manual `AssetDatabase`-created ScriptableObjects are not auto-dirtied by direct post-creation field writes — always call `EditorUtility.SetDirty()` (confirmed as a real, silent-failure-causing gap during the Broadcast batch).
- `Unity_ManageAsset` Move operations frequently report a false-negative failure even on success — always verify via fresh filesystem/GUID check.

## Rule of thumb

Check for an existing manager method or `UpgradeBlock`/`Output` field before writing anything new. Direct holder edits or parallel custom systems are almost always wrong.