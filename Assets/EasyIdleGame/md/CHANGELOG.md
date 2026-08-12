**Easy Idle Game Changelog**

This changelog highlights the most important additions, improvements, and fixes in each release.

**2.0.0**

Version 2.0.0 is a major package update focused on flexibility, extensibility, clearer workflows, and stronger support for production-ready idle games.

**Production and rewards**

- **[NEW]** Drop tables with independent and weighted-random reward strategies.
- **[NEW]** Drop tables can be used by businesses, achievements, daily rewards, prestige rewards, shop items, merge recipes, and level rewards.
- **[NEW]** Production round modes for live, snapshot, and concurrent production.
- **[NEW]** Production modifier pipeline for custom output calculations during live play, previews, per-second calculations, and offline progress.
- **[NEW]** Partial production when only part of a production cost can be paid.
- **[NEW]** Production inputs can be consumed when production starts or finishes.
- **[CHANGE]** Production previews now distinguish between expected results and actual randomized results.

**Businesses**

- **[NEW]** Business wrappers for creating variations of an existing business while keeping separate runtime state.
- **[NEW]** Typed business groups for effect targeting and shared ownership limits.
- **[NEW]** Upper locks and production-specific locks.
- **[NEW]** Separate current, total obtained, and purchased business amounts.
- **[NEW]** Manual production for a specified number of businesses.
- **[NEW]** Pause, resume, production-progress, and current-production-per-second helpers.
- **[CHANGE]** Business outputs now use reusable output tables.

**Currencies and item groups**

- **[NEW]** Typed groups for businesses, currencies, boosts, upgrades, managers, locations, achievements, and shop items.
- **[NEW]** Shared currency capacity through currency groups.
- **[NEW]** Shared business capacity through business groups.
- **[NEW]** Group-based targeting for locks, upgrades, boosts, shop offers, and prestige exclusions.

**Upgrades and boosts**

- **[NEW]** Context-aware upgrade effects with source, target, group, level, output, manual-production, and offline-production filters.
- **[NEW]** Upgrade effects for purchase costs, level-up costs, durations, cooldowns, production inputs, merge inputs, drop chances, drop weights, and production outputs.
- **[NEW]** Override upgrade modifiers and business ownership-limit overrides.
- **[NEW]** Runtime control for adding and removing active boosts.
- **[NEW]** Optional merging of repeated uses of the same boost.
- **[CHANGE]** Timed upgrades now have clearer sequential and parallel queue behavior.
- **[CHANGE]** Upgrade purchases and timed upgrade application are reported as separate events.

**Shop, offers, and locations**

- **[NEW]** Shop system with in-game purchases, advertisements, real-money purchase delegation, reward tables, locks, purchase limits, and consumable items.
- **[NEW]** Offers system for temporary weighted offers built from shop items.
- **[NEW]** Location system with unlocking, purchasing, travel, starter rewards, locks, and active-location tracking.

**Achievements, rewards, and player progression**

- **[NEW]** Tiered achievements with repeatable claims and scaled requirements.
- **[NEW]** Achievement groups and prestige reset controls.
- **[NEW]** Custom player statistics with save support.
- **[NEW]** Configurable daily-reward day length, calendar previews, next-day simulation, and missed-day claims.
- **[CHANGE]** Achievements, daily rewards, player-level rewards, and prestige rewards now use reward tables.
- **[CHANGE]** Player statistics now track total currencies earned, businesses obtained, boosts obtained, custom statistics, XP, and level rewards.

**Merging and prestige**

- **[NEW]** Merge recipes can use general business or currency inputs and drop-table outputs.
- **[NEW]** Merge duration, cooldown, metadata, audio, and context-aware upgrade effects.
- **[NEW]** Per-prestige reset options, typed-group exclusions, and reset summaries.
- **[NEW]** Prestige reset and preservation support for locations and achievements.
- **[CHANGE]** Prestige now reports the rewards applied and the items reset or preserved.

**Saving, offline progress, and economy events**

- **[NEW]** Source-aware economy changes distinguish purchases, production, rewards, advertisements, conversions, resets, and other actions.
- **[NEW]** Debounced save-worthy economy events for autosave and UI integrations.
- **[NEW]** Offline-profit application summaries report the amounts actually applied after limits and stockpiles.
- **[CHANGE]** Save data now covers the expanded location, shop, achievement, statistics, boost, reward, and ownership systems.
- **[CHANGE]** Legacy save paths and supported legacy asset fields are migrated automatically.

**Audio, editor, and demos**

- **[NEW]** Operation-specific audio hooks for purchases, unlocking, leveling, production, merging, boosts, achievements, and related economy actions.
- **[NEW]** Shared metadata for display names, icons, models, and descriptions.
- **[NEW]** More than 20 focused feature demos with in-Inspector setup guides.
- **[CHANGE]** Inspector validation, explanations, event reporting, and debugging workflows have been improved.
- **[CHANGE]** Documentation has been expanded with a feature guide, scripting reference, migration guide, breaking-change reference, troubleshooting guide, and demo catalog.

**Other changes**

- **[CHANGE]** Several APIs were renamed or redesigned for consistency and clearer failure handling.
- **[CHANGE]** Purchase and reward operations now provide more detailed success results and applied outputs.
- **[CHANGE]** Logging and validation messages provide more actionable context.
- **[CHANGE]** Overall performance, extensibility, and code consistency improvements.
- **[FIX]** Save-path handling, bulk external purchases, daily-reward validation, non-positive offline durations, merge cooldown modifiers, and lock dependency validation.
- **[FIX]** Additional bug fixes and stability improvements across the package.

> **Important:** Version 2.0.0 contains breaking API and behavior changes. Review the included Migration Guide and Breaking Changes document before upgrading an existing project.

**1.7.2**

This release primarily implements user-requested features.

**Businesses**

- **[NEW]** Randomized outputs.

**Upgrades**

- **[NEW]** Timed upgrades that take effect after a configurable delay.

**Inspector**

- **[NEW]** Explanations for selected features are now available directly in the Inspector.

**Other changes**

- **[FIX]** Bug fixes.

**1.7.1**

**Upgrades and boosts**

- **[NEW]** Player and business XP multiplier upgrades and boosts.
- **[NEW]** Idle production cap upgrade.

**Merging**

- **[NEW]** Businesses can now be merged.

**Other changes**

- **[CHANGE]** Improved logging system.
- **[FIX]** Bug fixes.

**1.7**

**Daily rewards**

- **[NEW]** Daily rewards system.

**Achievements**

- **[NEW]** Achievements system.

**Player stats**

- **[NEW]** Businesses and total businesses obtained are now included in player statistics.

**Currencies**

- **[NEW]** Maximum ownable amount constraint.

**Businesses**

- **[NEW]** Maximum ownable amount constraint.
- **[NEW]** Businesses can now be upgraded.

**Upgrades**

- **[NEW]** Maximum currency amount upgrade.

**Other changes**

- **[CHANGE]** Overall improvements.
- **[FIX]** Bug fixes.

**1.6.5**

**Boosts**

- **[NEW]** Auto-production boost.

**Other changes**

- **[NEW]** ManagerObject quality-of-life improvements.
- **[CHANGE]** Normalized functions across models.
- **[CHANGE]** Performance improvements.
- **[CHANGE]** Overall improvements.
- **[FIX]** Bug fixes.

**1.6**

**Businesses**

- **[NEW]** Immediate production option.
- **[NEW]** Reworked player and business XP awarded when a business is acquired.
- **[CHANGE]** Level reward system overhaul.

**Prestiges**

- **[NEW]** Upgrades, managers, and businesses can be excluded from a prestige reset.
- **[NEW]** Built-in rewards after prestige.

**Editor**

- **[NEW]** Inspector quality-of-life improvements.

**Player stats**

- **[NEW]** Level descriptions.
- **[NEW]** Standard level-up rewards.

**Displayers**

- **[NEW]** Quality-of-life features.

**Other changes**

- **[NEW]** ManagerObject quality-of-life improvements.
- **[FIX]** Bug fixes.

**1.5**

**Businesses**

- **[NEW]** Price multiplier.

**Upgrades**

- **[CHANGE]** Price multiplier performance improvements.

**Managers**

- **[NEW]** Price multiplier.
- **[NEW]** Native bulk purchasing for managers.

**Editor**

- **[CHANGE]** Improved Add Component menu.

**Displayers**

- **[NEW]** Quality-of-life features.
- **[CHANGE]** Overall UI improvements.

**Other changes**

- **[FIX]** Bug fixes.

**1.4.5**

**Editor**

- **[CHANGE]** New debug fields and a debug mode toggle.

**Other changes**

- **[NEW]** Assembly definitions for faster reload times.
- **[CHANGE]** Documentation overhaul.
- **[CHANGE]** Performance improvements.
- **[FIX]** Bug fixes.

**1.4**

**Businesses**

- **[NEW]** Manual output collection.

**Upgrades**

- **[NEW]** The same upgrade can now be purchased multiple times.
- **[NEW]** Price multiplier when purchasing multiple copies.

**Managers**

- **[NEW]** Manual level-up option.

**Player stats**

- **[NEW]** Manual level-up option.

**UI**

- **[NEW]** UI scripts for audio.
- **[CHANGE]** Overall improvements.

**Other changes**

- **[CHANGE]** Level reward improvements.
- **[CHANGE]** Model design overhaul.
- **[FIX]** Bug fixes.

**1.3**

**Boosts**

- **[NEW]** Generic multiplier boost for production, speed, and price at the same time.

**Audio**

- **[NEW]** Native audio support.

**Other changes**

- **[NEW]** Custom inspectors for easier debugging.
- **[NEW]** New prebuilt UI scripts.
- **[NEW]** Quality-of-life features in idle-game classes.
- **[CHANGE]** UI script lifecycle update.
- **[FIX]** Bug fixes.

**1.2**

**Managers**

- **[NEW]** Cooldown between activations.

**Other changes**

- **[CHANGE]** Faster and more precise idle-profit calculations.
- **[CHANGE]** UI script updates.
- **[CHANGE]** More user-friendly Inspector experience.
- **[FIX]** Bug fixes.

**1.1.5**

**Other changes**

- **[CHANGE]** UI script workflow update.
- **[FIX]** Bug fixes.

**1.1**

**UI**

- **[CHANGE]** Major UI overhaul.
- **[CHANGE]** Displayers moved from the `EasyIdleGame.Demos` namespace to the `EasyIdleGame.UI` namespace.
- **[CHANGE]** Workflow update: all redraw methods are now overridable.
- **[NEW]** UI scripts update in the Editor, allowing the game UI to be previewed without entering Play mode.

**Currencies**

- **[CHANGE]** Added descriptions.

**Upgrades**

- **[CHANGE]** Upgrades can be reset on prestige.

**Save and load system**

- **[CHANGE]** `SaveFile` is designed for inheritance, making custom save-file implementations easier.
- **[CHANGE]** `SaveAndLoadSystem` is now generic and supports custom save files.
- **[CHANGE]** Save and load behavior is easier to override.

**Other changes**

- **[CHANGE]** Manager scripts can now be added through the Add Component menu.
- **[CHANGE]** Performance improvements.
- **[FIX]** Bug fixes.

**1.0**

**Businesses**

- **[NEW]** Cost per production round.

**Boosts**

- **[NEW]** Business groups, allowing boosts to target specific groups of businesses.
- **[NEW]** Built-in price multiplier boost.
- **[CHANGE]** Internal logic improvements.

**Managers**

- **[NEW]** Activatable production, cost, and speed boosts.
- **[CHANGE]** Passive boost overhaul with production, cost, and speed effects.

**Global upgrades**

- **[NEW]** Global upgrades.

**Other changes**

- **[CHANGE]** Performance improvements.
- **[FIX]** Bug fixes.
