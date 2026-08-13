# Build & Release Checklist

Work through this before any store submission build. Not a during-development task list.

## Demo removal

- [ ] Delete `Assets/EasyIdleGame/Demos/` entirely (or move outside `Assets/`).
- [ ] Reason: everything under any `Resources` folder ships automatically via `Resources.LoadAll`, regardless of scene references.
- [ ] Confirm no InfluencerRise script/asset references anything inside Demos before deleting.
- [ ] Rebuild and confirm no compile errors after removal.

## Build Settings

- [ ] Scenes In Build contains only real project scenes — no demo scenes.
- [ ] Correct platform, signing, bundle ID, version, icon, splash screen.

## IAP / Ads

- [ ] Real IAP product IDs filled into MonetizationPlan.md, matched exactly in store consoles.
- [ ] Sandbox purchases verified on a real device.
- [ ] Ad SDK configured with real (not test) ad unit IDs.

## Account/Login

- [ ] If Google Sign-In is implemented on Android, confirm Sign in with Apple (or equivalent) is also implemented on iOS — required by App Store Review Guideline 4.8, not optional.

## Compliance

- [ ] Privacy policy URL live and linked in store listing.
- [ ] Age rating questionnaire completed accurately.
- [ ] App Tracking Transparency prompt tested on iOS if applicable.

## Final checks

- [ ] Fresh install → play → force-quit → relaunch → confirm save/load and offline profit work correctly.
- [ ] Test on lowest-spec target device, not just dev machine.
- [ ] Git tag the release commit after confirming submission-ready.