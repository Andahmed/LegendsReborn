# Work Log (newest first)

## 2026-11-11 - Approved hero portrait import pipeline
**Done**
- Connected the existing five-portrait runtime loader to the editor asset installation flow.
- The CC0 asset installer now copies `hero_warrior.png`, `hero_paladin.png`, `hero_berserker.png`, `hero_healer.png` and `hero_sharpshooter.png` from `ArtSource/Heroes/images/` into `Assets/Resources/HeroPortraits/`.
- Missing portrait source files are reported in the installer summary instead of being silently ignored.
- Updated README with the exact Unity menu and portrait source/destination paths.

**Verification**
- Static source inspection confirms the Heroes screen already loads `Resources/HeroPortraits/hero_<id>`.
- Static source inspection confirms the importer copies the five approved filenames to that matching Resources path.
- The actual PNG files are not currently present at the expected source path on this GitHub branch, so image bytes have not yet been imported or visually checked.
- Unity Editor import, sprite rendering, Play Mode and Android build remain unverified.

## 2026-11-11 - Replace generated castle geometry with bundled CC0 FBX assembly
**Done**
- Reworked Assets/Scripts/UI/Screens/HomeWorldStage.cs to assemble the home courtyard and castle from the repository's Quaternius CC0 FBX modules: Floor_Modular, Arch_Door, Wall_Modular, Decorative_Wall, Column, Column2, Banner, Banner_wall, Arch, Stairs_Modular and Torch.
- Removed the procedural/primitive castle path from the runtime scene. Missing FBX assets now emit explicit errors; no cube or capsule castle fallback is created.
- Removed the setup-time dependency on the procedural castle mesh generator.
- Updated the Quaternius installer so it checks bundled assets before prompting for a source folder and no longer searches or names another project checkout.
- Updated README and HANDOFF to describe the actual FBX-based assembly and the correct installer menu.

**Verified**
- GitHub Actions Server Tests completed successfully: 5 tests ran, all passed.
- Static checks on the PR branch confirm HomeWorldStage has no primitive cube/capsule creation and no procedural castle generator reference.
- Static checks confirm the setup menu no longer calls the procedural generator and the asset importer no longer searches another repository.

**Not verified**
- Unity Editor compilation, FBX import, materials, pivots, camera framing, Play Mode, mobile layout and Android APK were not run in this environment.
- Android APK workflow completed successfully but skipped the actual build because UNITY_LICENSE, UNITY_EMAIL and UNITY_PASSWORD are not configured.
- FBX module names and files are listed by the repository asset-readiness audit; the final placement still requires visual review in Unity.

**Next**
1. Open Unity 2022.3.62f3 and run the Home scene setup.
2. Run Install MVP Art only if the audit reports missing model files.
3. Inspect the castle from the mobile camera and adjust module scale/position based on actual imported bounds.
4. Run Unity EditMode/PlayMode tests and Android build after local Unity setup.

## 2026-11-11 - Foundations pass (Ahmed + Claude)
**Done**
- Added the gameplay foundation, runtime UI, art pipeline, server skeleton and project documentation.
- Engine-free code and tests were reported as passing under mono in the original work log; this is not a Unity Editor verification.

**Not verified**
- Project compilation, scene generation, Play Mode, Android APK and device tests remain unverified.
