# Compilation errors after import or update

Use this procedure when Unity cannot compile the project after Easy Idle Game is imported or updated.

1. Wait for Unity to finish importing assets before treating transient assembly-definition errors as final.
2. Confirm TextMeshPro essentials are imported and the `EasyIdleGame.UI` assembly can resolve TextMeshPro and uGUI.
3. Check [Breaking changes](BreakingChanges.md) (`BreakingChanges.md`) for removed APIs and the interface, input, output, and level-reward migrations.
4. Search project-owned code for obsolete string-group overloads, removed boost types, flattened `Outputs`, and direct calls to older `Input` signatures.
5. Confirm there is only one Easy Idle Game source tree. A relocated older copy plus a newly imported copy at the archive's original location can cause duplicate types, assemblies, resources, and mixed-version errors.
6. When upgrading a pristine package copy, replace the package as a unit. Do not delete individual source files named by old support notes from a current package.
7. Run the Unity Test Runner after compilation succeeds; **Tools > EasyIdleGame > ExecuteTests** is deprecated.

## Clean package reimport

If errors remain after a partial import or update, a clean package reimport can remove stale, duplicated, or mixed-version files:

1. Commit the Unity project or make a complete backup.
2. Move any project-owned code, assets, or package-source modifications outside the Easy Idle Game installation folder.
3. Delete the entire Easy Idle Game installation folder through Unity's Project window so its `.meta` file is removed with it.
4. Let Unity finish processing the deletion.
5. Import one fresh, complete package copy and wait for compilation to finish.

Deleting and reimporting is a last-resort repair for a clean package installation. Importing an update overwrites package files, so it is not a substitute for preserving local changes. If the package was deliberately relocated, reconcile or remove the relocated copy before importing so the project never contains two package versions.

Return to [Troubleshooting diagnostics and tests](07-troubleshooting.md#use-diagnostics-and-tests-first) (`07-troubleshooting.md` > `Use diagnostics and tests first`) after compilation succeeds.
