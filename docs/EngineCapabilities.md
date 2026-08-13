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

## Native with caveat — code path is unrestricted, but unproven by any working example in the package

| Feature | Finding |
|---|---|
| Weighted `DropTable` granting a Manager (`Output.managerOutput`) | `Output.cs`'s four grant fields (business/currency/boost/manager) are treated identically everywhere, including `DropTable.GetWeightedOutputs()` and `GenerateDrops()` — fully type-agnostic by direct code read. **But no demo in the package combines `DropStrategy.WeightedRandom` with a populated `managerOutput`.** Build a narrow proof-of-concept and verify statistically (both outcomes reachable) before building the full system on top of it. |
| `ShopItem` + Weighted `DropTable` ("Capsule" pattern) | `ShopItem.rewardTables` has no restriction on `DropTable.strategy`. Confirmed structurally via `ShopManager.CompleteExternalPurchase()` → `RewardCalculator`/`DropTable.GenerateDrops`, same type-agnostic path. **No demo exercises this exact combination either** — same "prove it before building on it" caveat applies. |

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