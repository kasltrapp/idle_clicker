# Naming Conventions

## Folder placement

All persisted definition assets go under `Assets/InfluencerRise/Resources/<category>/`:
- Currencies → `Resources/Currencies/`
- Businesses → `Resources/Businesses/`
- Business Groups → `Resources/BusinessGroups/`
- Managers → `Resources/Managers/`
- Locations → `Resources/Locations/`
- Upgrades → `Resources/Upgrades/`
- Boosts → `Resources/Boosts/`
- Shop Items → `Resources/Shop/`
- Achievements → `Resources/Achievements/`
- PlayerStats/CustomStats → `Resources/PlayerStats/`

Scripts → `Assets/InfluencerRise/Scripts/`, subfoldered by system (e.g. `Scripts/Burnout/`, `Scripts/UI/`).
Scenes → `Assets/InfluencerRise/Scenes/`.
Art → `Assets/InfluencerRise/Art/`, subfoldered by type.
UI Prefabs → `Assets/InfluencerRise/Prefabs/UI/`.

## Asset naming

- PascalCase, no spaces, no special characters.
- Asset name = exact name in `EconomySchema.md`. Never deviate.
- Display names go in `Metadata`, not the asset name — the asset name is a save identifier.

## Script naming

- C# class name matches file name exactly.
- PascalCase, descriptive, suffix by role (e.g. `BurnoutController.cs`).

## Scene naming

- `Main` — primary gameplay scene.

## Prefab naming

- Prefix with the system it belongs to: `UI_BusinessRow`, `UI_ManagerRow`, `FX_ViralMomentBoost`.

## Git commit messages

- Short imperative present tense: "Add Followers and Cash currency assets."