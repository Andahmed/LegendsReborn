# Work Log (newest first)

## 2026-11-12 - Log Boot and Home scene generation during CI builds
**Done:** MVPProjectInstaller now checks SaveScene results and emits explicit Unity log lines for generated Assets/Scenes/Boot.unity and Assets/Scenes/Home.unity. A failed save throws immediately instead of silently continuing.
**Not verified:** The logs have not yet been observed in a real Unity/GameCI build; this is pending the first build with Unity Personal credentials.


## 2026-11-12 - Unity Personal headless licensing for Android CI
**Done:** Switched Android APK CI to `game-ci/unity-builder@v6.0.0` with `game-ci/cli v0.1.73`; readiness now requires only `UNITY_EMAIL` and `UNITY_PASSWORD`. Removed the obsolete Unity Activation workflow and replaced .ulf instructions with the CLI Personal flow, GitHub GUI secret setup, 2FA/undocumented-flag/E2E caveats. Kept the requested Android paths filters.
**Not verified:** No live Unity Personal activation or Android APK build has yet been performed with these credentials. The upstream PR #246 explicitly had not completed live-account end-to-end testing.


## 2026-11-12 - Unity activation summary shell fix
**Done:** Removed shell command-substitution backticks from the missing-secret summary so the manual readiness workflow prints secret names and documentation path literally.
**Not verified:** The workflow must be run again after this correction; repository secrets are still missing and Unity license validity is unverified.

## 2026-11-12 - Unity Personal activation workflow correction
**Done:** Replaced the deprecated `unity-request-activation-file` action with a manual secret-readiness check. Updated the instructions to the currently supported Unity Hub Personal-license flow and documented the OS-specific `.ulf` locations.
**Not verified:** The actual Unity license remains unvalidated until GameCI runs the Android build with the repository secrets.

## 2026-11-12 - A3b Hero portrait and tint metadata
**Done:** Wired `HeroCardData.PortraitId` to the roster portrait resource lookup and `TintHex` to the portrait plate and hero-card accent. Missing portrait IDs safely omit the portrait; missing/invalid tints fall back to the existing teal accent.
**Not verified:** Unity Editor import, roster rendering and Android runtime presentation remain unverified. Engine-Free CI does not compile Unity UI code.

## 2026-11-12 - B3 Hero visual metadata (GPT Lane B)
- Added TintHex and PortraitId metadata for the 20 hero catalog entries. Engine-Free C# CI evidence is pending; no Unity verification.
## 2026-11-12 - A7 Boot to Home game flow
**Done:** Added a generated Boot scene with the persistent GameBootstrap, automatic Boot -> Home routing, HomeScreen binding after scene load, and Build Settings order Boot then Home. Battle remains the existing in-Home presentation; victory/defeat results return to Home or offer the next stage.
**Not verified:** لم يتم التحقق في Unity؛ generated scene contents, runtime sceneLoaded timing, Play Mode battle/result flow and Android build are pending CI/editor verification.
