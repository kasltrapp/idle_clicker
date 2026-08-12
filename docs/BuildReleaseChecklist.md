# Build & Release Checklist

Work through this before any store submission build. Do NOT do these steps during active development — only right before a release build.

## Demo removal

- [ ] Delete `Assets/EasyIdleGame/Demos/` entirely (or move outside `Assets/` if you want to keep it for reference on a separate branch).
- [ ] Reason: every asset under any folder named `Resources` ships in the final build automatically via `Resources.LoadAll`, regardless of scene references. Demo Resources folders add unnecessary size and confusion to a shipped app.
- [ ] Confirm no InfluencerRise script/asset references anything inside Demos before deleting (search project first).
- [ ] Rebuild and confirm no compile errors after removal.

## Build Settings

- [ ] Scenes In Build contains only `Assets/InfluencerRise/Scenes/Main` (and any other real project scenes — no demo scenes).
- [ ] Correct platform selected (iOS / Android) with correct signing configured.
- [ ] Player Settings: correct bundle ID, version number, icon, splash screen set.
- [ ] If Google Sign-In is implemented on Android, confirm Sign in with Apple (or equivalent) is also implemented on iOS — required by App Store Review Guideline 4.8, not optional.

## IAP / Ads

- [ ] Real IAP product IDs (not placeholders) filled into MonetizationPlan.md and matched exactly in App Store Connect / Play Console.
- [ ] Sandbox/test purchases verified working on a real device before submission.
- [ ] Ad SDK configured with real (not test) ad unit IDs for release build.

## Compliance

- [ ] Privacy policy URL live and linked in store listing.
- [ ] Age rating questionnaire completed accurately.
- [ ] App Tracking Transparency prompt tested on iOS if applicable.

## Final checks

- [ ] Fresh install → play → force-quit → relaunch → confirm save/load and offline profit work correctly.
- [ ] Test on lowest-spec device you're targeting, not just your dev machine.
- [ ] Git tag the release commit (e.g. `v1.0.0`) after confirming the build is submission-ready.