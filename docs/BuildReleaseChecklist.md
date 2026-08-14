# Build & Release Checklist

Work through before any store submission build — not a during-development task list.

## Demo removal
- [ ] Delete `Assets/EasyIdleGame/Demos/` entirely.
- [ ] Confirm no InfluencerRise script/asset references anything inside Demos first.
- [ ] Rebuild, confirm no compile errors.

## Build Settings
- [ ] Scenes In Build = real project scenes only.
- [ ] Correct platform, signing, bundle ID, version, icon, splash.

## IAP / Ads
- [ ] Real IAP product IDs filled in, matched exactly in store consoles (all 5 Capsule tiers + original packs).
- [ ] Sandbox purchases verified on a real device.
- [ ] Real ad unit IDs (not test) for release.
- [ ] **Re-verify the zero-Legendary-via-ad constraint one final time before shipping** — see MonetizationPlan.md.

## Account/Login
- [ ] If Google Sign-In on Android, confirm Sign in with Apple on iOS too.

## Compliance
- [ ] Privacy policy live and linked. Age rating accurate. ATT tested on iOS if applicable.

## Final checks
- [ ] Fresh install → play → force-quit → relaunch → confirm save/load and offline profit.
- [ ] Test on lowest-spec target device.
- [ ] Confirm FreeCapsule's cooldown gap has been closed before shipping (see EngineCapabilities.md / MonetizationPlan.md).
- [ ] Git tag the release commit.