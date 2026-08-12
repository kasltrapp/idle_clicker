# UI and displayers

`EasyIdleGame.UI` supplies optional presentation components for uGUI and helper presenters for UI Toolkit. They are examples, not a required architecture: any UI can bind directly to the public manager and holder APIs described in the [scripting reference](05-scripting-reference.md) (`05-scripting-reference.md`). Per-field options are documented in each component's Inspector.

## Table of contents

- [Supplied uGUI displayers](#supplied-ugui-displayers)
- [The BaseDisplayer redraw contract](#the-basedisplayer-redraw-contract)
- [Subclass a displayer](#subclass-a-displayer)
- [UI Toolkit presenters](#ui-toolkit-presenters)
- [Refresh from events](#refresh-from-events)
- [UI helpers and contracts](#ui-helpers-and-contracts)

## Supplied uGUI displayers

Assign the definition asset in the Inspector; the holder is resolved at runtime. Displayers require TextMeshPro essentials.

| Area | Components |
| --- | --- |
| Currencies | `CurrencyDisplayer` |
| Businesses and production | `BusinessDisplayer`, `UpgradableBusinessDisplayer`, `UpgradableBusinessUpgradeDisplayer`, `CurrentProductionDisplayer` |
| Managers | `ManagerDisplayer` |
| Boosts | `BoostDisplayer`, `ActiveBoostDisplayer` |
| Upgrades | `UpgradeDisplayer`, `UpgradesSummaryDisplayer` |
| Buy amount selection | `BuyAmountButtonDisplayer` |
| Progression | `PlayerLevelDisplayer`, `PlayerStatsDisplayer`, `AchievementDisplayer` |
| Daily rewards | `DailyRewardListDisplayer`, `RewardListDayDisplayer` |
| Prestige | `PrestigeDisplayer` |
| Openable panels | `GenericOpenableDisplayer`, `BusinessDetailDisplayer`, `LevelUpDisplayer`, `ProfitDisplayer` — open/close through the `IOpenable` contract |

`BaseListDisplayer`, `BusinessListDisplayer`, `CurrencyListDisplayer`, and `BoostListDisplayer` are obsolete; use Unity layout components instead.

## The BaseDisplayer redraw contract

Every displayer derives from `BaseDisplayer`, which separates:

- `Redraw_Default_Empty()` — placeholder-only editor output.
- `Redraw_Default()` — editor output from assigned references.
- `Redraw()` — runtime state.

`autoRedraw` (default on) redraws every frame. Disable it and redraw from manager events in large lists. **Tools > EasyIdleGame > RedrawAll** redraws loaded displayers in the editor.

## Subclass a displayer

```csharp
public sealed class MyBusinessDisplayer : BusinessDisplayer
{
    public override void Redraw()
    {
        base.Redraw();
        // Project-specific presentation.
    }
}
```

Call the base `Redraw`, `Redraw_Default`, or `Redraw_Default_Empty` when overriding. Keep economy mutation in managers and holders; UI code presents state and forwards intent.

## UI Toolkit presenters

`UIToolkitPresenters.cs` provides `UIToolkitListRowPresenter` plus static presenters for businesses, upgrades, boosts, shop items, and prestige. This is helper code used by the focused feature demos, not a production UI framework — bind UI Toolkit directly to the manager APIs when the helpers do not fit.

## Refresh from events

Query UI elements defensively, subscribe after managers and holders exist, and remove listeners on disable or destroy. Refresh labels from amount, unlock, purchase, claim, active-state, and location events; poll only truly time-varying values such as progress bars and countdowns. Use `OnHolderAdded` when definitions can gain holders after your screen is enabled. The event table is in the [scripting reference](05-scripting-reference.md#important-events) (`05-scripting-reference.md`).

## UI helpers and contracts

- `UIUpdater` — static text/image/color update helpers for TMP and uGUI.
- `IOpenable` — open/close contract for panel displayers.
- `IMetadataDisplayer` — binds `Metadata` display name/icon safely.
- `AudioButton` — standard button click feedback; audio setup is in [Configure audio and editor helpers](03-feature-guides.md#configure-audio-and-editor-helpers) (`03-feature-guides.md`).
- `DemoManager` and `SimpleDemoManager` — demo-scene controllers; reference material, not production components.

The same manager APIs and events support full UI Toolkit or uGUI screens at production scale, not just the demo helpers above.
