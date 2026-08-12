# Naming Conventions

## Folder placement (non-negotiable — see CLAUDE.md Hard Rule #2)

All persisted definition assets go under `Assets/InfluencerRise/Resources/<category>/`:
- Currencies → `Resources/Currencies/`
- Businesses → `Resources/Businesses/`
- Business Groups → `Resources/BusinessGroups/`
- Managers → `Resources/Managers/`
- Locations → `Resources/Locations/`
- Upgrades → `Resources/Upgrades/`
- Boosts → `Resources/Boosts/`
- Shop Items → `Resources/Shop/`

Scripts go in `Assets/InfluencerRise/Scripts/`, subfoldered by system (e.g. `Scripts/Burnout/`, `Scripts/UI/`).
Scenes go in `Assets/InfluencerRise/Scenes/`.
Art goes in `Assets/InfluencerRise/Art/`, subfoldered by type (`Art/Icons/`, `Art/Sprites/`, `Art/UI/`).

## Asset naming

- **PascalCase, no spaces, no special characters** — e.g. `SelfieSession`, not `Selfie Session` or `selfie_session`.
- Asset name = the exact name listed in `EconomySchema.md`. Never deviate, never abbreviate differently than the schema.
- Display names (what the player sees) go in the asset's `Metadata` field, not the asset name itself — the asset name is a save identifier, the Metadata display name is presentation. These can differ (e.g. asset name `SelfieSession`, Metadata display name "Selfie Session").

## Script naming

- C# class name matches file name exactly (Unity requirement).
- PascalCase, descriptive, suffix by role: `BurnoutDecayController.cs`, `ShopIAPHandler.cs`.
- One class per file unless a small private helper class is genuinely only used by that file.

## Scene naming

- `Main` — primary gameplay scene.
- Future scenes (if split later) follow `PascalCase` matching their purpose, e.g. `Boot`, `Loading`.

## Prefab naming

- Prefix with the system it belongs to: `UI_CurrencyDisplay`, `FX_ViralMomentBoost`.

## Git commit messages

- Short imperative present tense: "Add Followers and Cash currency assets," not "Added" or "Adding."
- Reference the schema section when relevant: "Add StarterFeed businesses per EconomySchema.md."