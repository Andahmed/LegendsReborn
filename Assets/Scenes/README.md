# Scenes

The project starts with one generated Home scene for the MVP slice.

1. Open Unity 2022.3.62f3.
2. Run **Sands → Setup MVP Home Scene**. The installer creates Home.unity, adds GameBootstrap and HomeScreen, then updates Build Settings.
3. Press Play after the art installers finish importing their FBX assets.

The Android build menu also ensures Home.unity is registered before invoking BuildPlayer. Boot and dedicated Battle scenes can be split out after the first end-to-end Play Mode pass.
