# Demo catalog

The package contains 28 Unity scenes: 23 focused feature demos, a guided documentation demo, an Adventure Communist-style showcase, and three experimental scenes.

## Table of contents

- [Run a demo](#run-a-demo)
- [Focused feature demos](#focused-feature-demos)
- [Larger demos](#larger-demos)
- [Experimental scenes](#experimental-scenes)
- [Demo presentation notes](#demo-presentation-notes)

## Run a demo

1. Open the scene from the path in the catalog.
2. Inspect the scene's `ReadMe` asset and controller component.
3. Confirm the required managers exist on the scene manager GameObject.
4. Enter Play Mode.
5. Use the setup section in the in-scene readme to drive the intended interaction.

Focused feature demos live at `EasyIdleGame > Demos > FeatureDemos`. Open a scene from the catalog, then select its adjacent `ReadMe.asset` for setup notes and the resource prefix.

Each focused demo folder contains the scene, its `ReadMe.asset`, a small controller script demonstrating the public API, and prefixed definition assets under its own `Resources` folder. The prefixes keep demo assets distinct even though `Resources.LoadAll` sees assets project-wide.

## Focused feature demos

| Demo | Scene | What it demonstrates |
| --- | --- | --- |
| Basic Flow | `EasyIdleGame > Demos > FeatureDemos > 01_BasicFlow > BasicFlow.unity` | One business, manager, permanent upgrade, temporary boost, player XP, and prestige in a minimal loop |
| Boosts And Upgrades | `EasyIdleGame > Demos > FeatureDemos > BoostsAndUpgrades > BoostsAndUpgrades.unity` | Temporary boosts, permanent upgrades, duration merging, business-group targeting, and wallet-cap overrides |
| Business Chaining | `EasyIdleGame > Demos > FeatureDemos > BusinessChaining > BusinessChaining.unity` | One business output creates another business, which then produces currency |
| Business Group Targeting | `EasyIdleGame > Demos > FeatureDemos > BusinessGroupTargeting > BusinessGroupTargeting.unity` | Typed `BusinessGroup` targeting; empty target groups behave globally |
| Business Level Upgrades | `EasyIdleGame > Demos > FeatureDemos > BusinessLevelUpgrades > BusinessLevelUpgrades.unity` | `BusinessUpgradeLevel` metadata and limit overrides; `-1` represents unlimited |
| Custom Save And Autosave | `EasyIdleGame > Demos > FeatureDemos > CustomSaveAndAutosave > CustomSaveAndAutosave.unity` | A `SaveFile` subclass with project state, toggleable save-worthy event coalescing, load-guarded autosave, and offline reapplication |
| Daily Rewards | `EasyIdleGame > Demos > FeatureDemos > DailyRewards > DailyRewards.unity` | Current day, missed days, exact/repeating rewards, claiming, and countdown |
| Drop Table Rewards | `EasyIdleGame > Demos > FeatureDemos > DropTableRewards > DropTableRewards.unity` | Independent chance tables and weighted-random reward pools |
| Event Listener Showcase | `EasyIdleGame > Demos > FeatureDemos > EventListenerShowcase > EventListenerShowcase.unity` | A pinned chip per runtime `UnityEvent` of a small business/boost/manager loop; each flashes the instant its event fires, with a live payload feed |
| Location Travel | `EasyIdleGame > Demos > FeatureDemos > LocationTravel > LocationTravel.unity` | Locked/unlocked worlds, travel costs, starter rewards, and `TryTravelTo` |
| Locks And Buy Amounts | `EasyIdleGame > Demos > FeatureDemos > LocksAndBuyAmounts > LocksAndBuyAmounts.unity` | Fixed/percent/max purchases plus lower locks, upper locks, and shared caps |
| Managers And Automation | `EasyIdleGame > Demos > FeatureDemos > ManagersAndAutomation > ManagersAndAutomation.unity` | Automatic production/collection, passive modifiers, active abilities, and cooldowns |
| Merging Recipes | `EasyIdleGame > Demos > FeatureDemos > MergingRecipes > MergingRecipes.unity` | Business/currency inputs, drop-table outputs, recipe lookup, and merging |
| Offers And Time Skip | `EasyIdleGame > Demos > FeatureDemos > OffersAndTimeSkip > OffersAndTimeSkip.unity` | Weighted limited-time offers and `TimeSkipBoost` offline-style production |
| Offline Profit Application | `EasyIdleGame > Demos > FeatureDemos > OfflineProfitApplication > OfflineProfitApplication.unity` | Elapsed time to `ProfitData`, applying the economy result, and `ProfitApplicationSummary` |
| Player Progression | `EasyIdleGame > Demos > FeatureDemos > PlayerProgression > PlayerProgression.unity` | Player XP, currency totals, custom stats, business levels, and achievements |
| Prestige Resets | `EasyIdleGame > Demos > FeatureDemos > PrestigeResets > PrestigeResets.unity` | Requirements, rewards, reset flags, currency exclusions, and persistent upgrade blocks |
| Production And Collection | `EasyIdleGame > Demos > FeatureDemos > ProductionAndCollection > ProductionAndCollection.unity` | Manual/automatic production, stored outputs, claiming, and manager auto-collection |
| Production Costs | `EasyIdleGame > Demos > FeatureDemos > ProductionCosts > ProductionCosts.unity` | Input spending at round start, scaled and partial batches, and `consumeOnProduce` |
| Production Modifiers | `EasyIdleGame > Demos > FeatureDemos > ProductionModifiers > ProductionModifiers.unity` | An ordinary fixed-output factory compared with a separate toggleable modifier that adds Coins by owned amount in round, per-second, active, and offline calculations |
| Shop Reward Completion | `EasyIdleGame > Demos > FeatureDemos > ShopRewardCompletion > ShopRewardCompletion.unity` | External purchase callbacks, `CompleteExternalPurchase`, rewards, and consumable behavior |
| Timed Upgrade Queues | `EasyIdleGame > Demos > FeatureDemos > TimedUpgradeQueues > TimedUpgradeQueues.unity` | Delayed upgrades, parallel/sequential queues, pending state, and `SkipUpgradeWait` |
| Wrapper Variants | `EasyIdleGame > Demos > FeatureDemos > WrapperVariants > WrapperVariants.unity` | `BusinessWrapper` copies, scaling, synchronization, remapped references, and isolated holder state |

All scene locations in the table start at the relative package root, `EasyIdleGame`.

## Larger demos

### DocsDemo

Location: `EasyIdleGame > Demos > DocsDemo > DocsScene.unity`

Use this scene to see a multi-step setup with managers, ScriptableObject definitions, displayers, save/load, offline profit, prestige, and audio in one place.

### AdventureCommunist

Open the `.unity` scene below `EasyIdleGame > Demos > AdventureCommunist`. This is a presentation-heavy example that combines more assets and UI than the focused scenes. Use it for composition and display patterns, not as the shortest explanation of a single feature.

## Experimental scenes

The scenes below `EasyIdleGame > Demos > _ExperimentalScenes` are development-testing content, not supported samples. They are undocumented, may change or disappear in updates, and are not a reference for public features — use the focused feature demos instead.

## Demo presentation notes

The demos are designed for a portrait, mobile-style view; preview them at a `9:16` or taller aspect ratio. The focused feature demos use UI Toolkit. `DocsDemo` and `AdventureCommunist` use uGUI with TextMeshPro — import TMP essentials before opening them, and set the TMP outline to `0.42` where a material should match the provided look.

These settings affect only how the demos look, not the economy runtime.
