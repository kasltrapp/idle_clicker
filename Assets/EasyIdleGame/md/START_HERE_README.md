# OPEN THIS FILE FIRST — Easy Idle Game documentation

**Known limitation:** clicking a link between guides works in the generated
PDFs when opened in Chrome, Edge, or Adobe Acrobat/Reader — this was tested
directly against the generated files. It does **not** work in Firefox's
built-in PDF viewer (pdf.js): Firefox resolves a PDF's relative links
against its own internal viewer URL instead of the file's real location on
disk, which breaks relative links in any PDF regardless of the link-action
type used to build them. If cross-guide navigation matters, open these PDFs
in Chrome, Edge, or Acrobat/Reader instead of Firefox.

> **Start here.** This is the main index for all Easy Idle Game documentation included with the package.

The maintained guides are under `EasyIdleGame > Documentation > md`. Field-level help lives directly in the Unity Inspector through tooltips, comment areas, and validation warnings. The scripting reference and current source describe the public API, including interfaces, `Try...()` entry points, events, and key enums.

## Documentation contents

- [Overview and architecture](../Documentation/md/00-overview.md) (`EasyIdleGame > Documentation > md > 00-overview.md`) — package structure, runtime layers, required managers, conventions, and a guide map.
- [Create your first idle game](../Documentation/md/01-create-your-first-idle-game.md) (`EasyIdleGame > Documentation > md > 01-create-your-first-idle-game.md`) — the newcomer tutorial and shortest complete playable loop.
- [Core economy systems](../Documentation/md/02-core-economy-systems.md) (`EasyIdleGame > Documentation > md > 02-core-economy-systems.md`) — currencies, businesses, production, rewards, purchase amounts, groups, capacities, locks, wrappers, and merging.
- [Feature guides](../Documentation/md/03-feature-guides.md) (`EasyIdleGame > Documentation > md > 03-feature-guides.md`) — managers, upgrades, boosts, locations, achievements, daily rewards, shops, offers, production modifiers, audio, and editor helpers.
- [Persistence, offline profit, and prestige](../Documentation/md/04-persistence-offline-prestige.md) (`EasyIdleGame > Documentation > md > 04-persistence-offline-prestige.md`) — saved identity, custom saves, offline calculations, timers, and reset behavior.
- [Scripting reference](../Documentation/md/05-scripting-reference.md) (`EasyIdleGame > Documentation > md > 05-scripting-reference.md`) — runtime architecture, manager and holder APIs, interfaces, events, extension points, validation, logging, and important enums.
- [Demo catalog](../Documentation/md/06-demo-catalog.md) (`EasyIdleGame > Documentation > md > 06-demo-catalog.md`) — every supported demo scene, its package-relative path, and the behavior it demonstrates.
- [Troubleshooting](../Documentation/md/07-troubleshooting.md) (`EasyIdleGame > Documentation > md > 07-troubleshooting.md`) — import, configuration, persistence, production, reward, UI, and runtime diagnostics.
- [UI and displayers](../Documentation/md/08-ui-and-displayers.md) (`EasyIdleGame > Documentation > md > 08-ui-and-displayers.md`) — supplied uGUI components, UI Toolkit presenters, redraw behavior, subclassing, and event-driven refresh.
- [Compilation errors after import or update](../Documentation/md/CompilationErrors.md) (`EasyIdleGame > Documentation > md > CompilationErrors.md`) — compiler recovery, dependency checks, and clean package reimport procedure.
- [Changelog](CHANGELOG.md) (`EasyIdleGame > md > CHANGELOG.md`) — version history and feature changes.
- [Breaking changes](../Documentation/md/BreakingChanges.md) (`EasyIdleGame > Documentation > md > BreakingChanges.md`) — incompatible API and behavior changes, serialization notes, and migration risks.
- [Migration guide](MigrationGuide.md) (`EasyIdleGame > md > MigrationGuide.md`) — practical steps for updating an existing project to Easy Idle Game 2.0.

## Quick routes

- **New to the package:** read [Create your first idle game](../Documentation/md/01-create-your-first-idle-game.md) (`EasyIdleGame > Documentation > md > 01-create-your-first-idle-game.md`), then open `EasyIdleGame > Demos > FeatureDemos > 01_BasicFlow > BasicFlow.unity`.
- **Looking for a feature:** use [Feature guides](../Documentation/md/03-feature-guides.md) (`EasyIdleGame > Documentation > md > 03-feature-guides.md`) and the [Demo catalog](../Documentation/md/06-demo-catalog.md) (`EasyIdleGame > Documentation > md > 06-demo-catalog.md`).
- **Writing custom code:** use the [Scripting reference](../Documentation/md/05-scripting-reference.md) (`EasyIdleGame > Documentation > md > 05-scripting-reference.md`) and current XML summaries in your IDE.
- **Updating an existing project:** read the [Migration guide](MigrationGuide.md) (`EasyIdleGame > md > MigrationGuide.md`) and [Breaking changes](../Documentation/md/BreakingChanges.md) (`EasyIdleGame > Documentation > md > BreakingChanges.md`) before importing.
- **Something is not working:** start with [Troubleshooting](../Documentation/md/07-troubleshooting.md) (`EasyIdleGame > Documentation > md > 07-troubleshooting.md`) or [Compilation errors after import or update](../Documentation/md/CompilationErrors.md) (`EasyIdleGame > Documentation > md > CompilationErrors.md`).

## Contact and support

Use either contact channel for **questions, setup help, issue and bug reports, or feature requests**:

- **Email:** `kocnar.ja@gmail.com`
- **Discord:** `https://discord.gg/bzZA9FFGKt`

When reporting an issue, include:

- The Easy Idle Game version and Unity version.
- Whether this is a new installation or an update from an older package version.
- Clear reproduction steps and the expected versus actual behavior.
- The complete Console error or stack trace, when available.
- Relevant screenshots, videos, asset settings, or a minimal reproduction project when they help explain the problem.

Before reporting an update or compilation problem, check [Troubleshooting](../Documentation/md/07-troubleshooting.md) (`EasyIdleGame > Documentation > md > 07-troubleshooting.md`), [Compilation errors after import or update](../Documentation/md/CompilationErrors.md) (`EasyIdleGame > Documentation > md > CompilationErrors.md`), and [Breaking changes](../Documentation/md/BreakingChanges.md) (`EasyIdleGame > Documentation > md > BreakingChanges.md`).

Feature requests should describe the intended player/developer workflow and why the existing extension points do not cover it.

## About the PDF guides

Every guide above ships two ways: as its `.md` source (in an `md/` folder,
e.g. `EasyIdleGame > Documentation > md > 00-overview.md`) and as a
generated `.pdf` sitting next to that `md/` folder (e.g.
`EasyIdleGame > Documentation > 00-overview.pdf`). The `.md` files are the
source of truth — edit those, not the PDFs. Cross-guide links inside the
`.md` sources point at sibling `.md` files; the generated PDFs get those
same links rewritten to point at the sibling `.pdf` instead, so clicking a
reference in a PDF opens another PDF rather than a markdown file.

To regenerate every PDF after editing any guide, run this from the Unity
project root (one-time setup: `npm install`, which reads the `package.json`
at the project root for the `md-to-pdf` and `pdf-lib` dev dependencies):

```
node Assets/generate-docs-pdf.js
```
