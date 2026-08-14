# Engine Capabilities — Bolt-On vs Custom Code

Check this before writing ANY new script. Verify first.

## Zero-code (Inspector/ScriptableObject configuration only)

| Feature | Engine system |
|---|---|
| Platforms | `Location` — metadata, locks, unlock cost, starter rewards, `TryTravelTo()` |
| Currencies | `Currency` — unlimited definitions, per-currency/group caps |
| Content creation | `Business` — cost, timing, output, chaining |
| Team members | `Manager` — automation, passive/active boosts |
| Gear/lifestyle upgrades | `Upgrade` — production, speed, cost multipliers |
| Rebrand reset | `PrestigeManager` — reset scoping, exclusions, per-call options |
| Milestones | `AchievementsManager` — tiered, repeatable, TotalAmount locks |
| Save/load, offline profit | `SaveAndLoad`, `ProfitCalculator` |
| **Level-up rewards** — CONFIRMED via audit | `PlayerStats.levelupData` (`LevelDataHolder`): exact-level (`levelRewards`) and repeating/divisor-based (`eachLevelReward`, optional `scaleEachLevelReward`) rewards, using the standard `Reward`→`DropTable`→`Output` system. |
| **Manager auto-leveling from duplicates** — CONFIRMED via audit | `IUpgradable.AddCount()` is called automatically in `ManagerHolder.AddManagerCount()` on purchase. `AutoUpgradeForFree`/`AutoUpgradeToFirstLevelFree` default true on Manager. No custom code needed — unlike Business, which requires manual `TryUpgrade()`. |

## Confirmed via audit — Manager Capsule system

| Feature | Finding |
|---|---|
| Weighted `DropTable` granting a Manager (`Output.managerOutput`) | **CONFIRMED statistically, not just structurally.** `Output.cs`'s four grant fields (business/currency/boost/manager) are treated identically everywhere, including `DropTable.GetWeightedOutputs()` and `GenerateDrops()`. `DropTable.GenerateDrops()` is a pure synchronous method — no Play Mode needed to test it. Ran a 20,000-trial in-memory `DropTable` (3 real Manager outputs, weights 10/5/1) directly via `RunCommand` in Edit Mode: observed distribution 62.6%/31.1%/6.3% against expected 62.5%/31.25%/6.25% — all outcomes reachable, matches weights within 2%. Separately ran a 10,000-trial zero-weight exclusion test: a `weight: 0` entry was selected **0 times**, confirming zero-weight exclusion works as the Manager Capsule Legendary-tier constraint depends on. (Note: a prior commit's message claimed this was "confirmed," but the actual diff showed the "unproven" caveat being added, not resolved — this doc was stale until this re-verification. Don't trust a commit message over the doc's own content.) |
| `ShopItem` + Weighted `DropTable` ("Capsule" pattern) | **CONFIRMED — built and shipped.** `ShopItem.rewardTables` has no restriction on `DropTable.strategy`; same type-agnostic `GenerateDrops()` path as above. 5 production Capsule Shop Items now built on this mechanism (`FreeCapsule`, `WatchAdCapsule`, `CloutCoinCapsuleLow`, `CloutCoinCapsuleHigh`, `IAPCapsule` — see `MonetizationPlan.md`/`EconomySchema.md`). |
| `ShopItem` cooldown/claim-interval field | **CONFIRMED NOT to exist.** Full read of `ShopItem.cs`: only limiting field is `maxPurchases` (lifetime total cap, 0 = infinite) — no recurring-interval field. `FreeCapsule`'s "long cooldown timer" design intent is therefore unimplemented pending a small custom script (same category as `BurnoutController.cs`). |

## Confirmed native, one important caveat

| Feature | Finding |
|---|---|
| Manager duplicate-leveling | Fully native (`AddCount()` auto-fires on purchase/grant). **Caveat confirmed via audit**: the copies-per-level curve is a fixed global formula (geometric, ×2 per level) — NOT independently configurable per Manager asset without subclassing and overriding `UpgradeLevelCopiesCountMultiplier` in code. Rarity differentiation must come from pull odds + per-level power (`businessSpeedMultiplier`), not a custom leveling curve. |
| Manager max level / duplicate overflow | No native cap (`MaxLevel = BigNumber.MaxValue` by default). No salvage/absorb mechanism exists because the precondition doesn't occur under default config. Building a cap is possible but explicitly out of scope for launch. |

## Requires real integration work (unavoidable, Phase 4)

| Feature | What's needed |
|---|---|
| Real-money IAP | Unity IAP package + `OnAttemptRealMoneyPurchase` + receipt verification + `CompleteExternalPurchase()`. |
| Rewarded ads | Ad mediation SDK + `OnAttemptAdPurchase`, same completion contract. |

## Confirmed NOT supported — needs genuinely new code/systems

| Feature | Confirmed via | What's needed |
|---|---|---|
| Manager rarity/tier (Common/Rare/Epic/Mythic) | Full read of `Manager.cs`, `ManagerGroup.cs`, `ItemGroup.cs`; package-wide grep for `rarity\|Rarity` — zero matches. | New field — enum on a custom Manager subclass, or a companion `ManagerRarity` ScriptableObject referenced from Manager. |
| Cosmetic/avatar system | Package-wide grep for `avatar\|Avatar\|cosmetic\|Cosmetic\|skin\|Skin` — zero matches. `Output.cs` has exactly 4 grant fields, no 5th cosmetic type. | New definition-asset type + new Output target field, or a parallel unlock-tracking system. Deferred to V1.1. |
| Burnout auto-rise/decay/prestige-reset/shop-relief | `PlayerStats.customStats` has no auto-ticker, no `resetCustomStats` flag on PrestigeManager, `Output` has no CustomStat target. | ✅ Already built — `BurnoutController.cs` (decay done, rise deferred to Phase 3, reset + shop-relief done). |
| TherapySessionUpgrade (Burnout-reducing Upgrade) | `Upgrade`/`UpgradeBlock` only affects production/speed/cost multipliers, no CustomStat target. | Deferred — fold into Burnout script's responsibilities when built out further. |
| Player Missions (separate quest-tracker from Achievements/Prestige) | Not audited directly (design decision, not engine gap) — **explicitly cut from scope**, see Roadmap. | N/A — not building this. |
| Trades (resource-conversion economy) | Not audited (design decision) — **explicitly cut from scope**, see Roadmap. | N/A — not building this. |

## Known tooling limitations

See CLAUDE.md's "Known tooling limitations" section (Play Mode timing, camera capture) — kept there since it's a workflow constant referenced across all work, not economy-specific.

## Rule of thumb

If a feature sounds like "restrict/modify/automate purchasing, producing, or resetting," check for an existing manager method first before writing anything new.