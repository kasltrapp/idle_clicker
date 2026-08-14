# Naming Conventions

## Folder placement

- Currencies → `Resources/Currencies/`
- Businesses → `Resources/Businesses/`
- Business Groups → `Resources/BusinessGroups/`
- Managers → `Resources/Managers/`
- Manager Rarities → `Resources/ManagerRarities/`
- Locations → `Resources/Locations/`
- Upgrades → `Resources/Upgrades/`
- Boosts → `Resources/Boosts/`
- Shop Items → `Resources/Shop/`
- Achievements → `Resources/Achievements/`
- PlayerStats/CustomStats → `Resources/PlayerStats/`

Scripts → `Assets/InfluencerRise/Scripts/`, subfoldered by system (`Scripts/Burnout/`, `Scripts/UI/`, `Scripts/Managers/`).
Scenes → `Assets/InfluencerRise/Scenes/`.
Art → `Assets/InfluencerRise/Art/`, subfoldered by type (`Art/UI/Skins/`, `Art/Icons/Businesses/`, `Art/Icons/Managers/`).
UI Prefabs → `Assets/InfluencerRise/Prefabs/UI/`.

## Asset naming

- PascalCase, no spaces. Asset name = exact name in `EconomySchema.md`. Display names go in `Metadata`.

## Script naming

- C# class name matches file name exactly. PascalCase, suffix by role.

## Custom enums

- `Rarity.cs`: `Common`, `Rare`, `Legendary` (3 tiers, locked design language — do not reintroduce "Epic" or "Mythic"). `Legendary` is pinned to int value 3 for save-compatibility reasons — do not renumber.

## Scene naming

`Main` — primary gameplay scene.

## Prefab naming

Prefix by system: `UI_BusinessRow`, `UI_ManagerRow` (not yet built), `Skin_PanelPlaceholder`, `Skin_IconFramePlaceholder`.

## Git commit messages

Short imperative present tense.