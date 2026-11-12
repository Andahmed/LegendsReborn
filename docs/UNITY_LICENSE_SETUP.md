# Unity Personal license setup (browser / Unity Hub)

This repository uses Unity **2022.3.62f3**. The GameCI `unity-request-activation-file` action is deprecated and no longer supported. The supported Personal-license flow is to activate Unity locally in Unity Hub, then store the resulting `.ulf` file as a GitHub Actions secret.

If Unity Personal was already activated on this Unity ID, do not activate it again unless Unity Hub says the license is missing. Locate the existing `.ulf` file and continue at step 4.

## 1. Activate Unity Personal in Unity Hub

1. Open **Unity Hub** and sign in with the Unity ID that owns the Personal license.
2. Open **Preferences** (or **Settings**) → **Licenses**.
3. Select **Add** → **Get a free Personal license** and complete the activation flow.
4. Ensure Unity Hub shows the Personal license as active.

## 2. Locate the .ulf license file

The usual locations are:

- Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
- macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
- Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`

The license can be activated on one operating system and used by GitHub Actions on Ubuntu. Do not upload the file to the repository.

## 3. Add GitHub Actions repository secrets

1. Open the repository on GitHub.
2. Go to **Settings** → **Secrets and variables** → **Actions**.
3. Select **New repository secret** and add:

| Secret name | Value |
| --- | --- |
| `UNITY_LICENSE` | The complete contents of `Unity_lic.ulf`, including every line |
| `UNITY_EMAIL` | The email address for the Unity ID |
| `UNITY_PASSWORD` | The password for that Unity ID |

Do not commit the `.ulf` file or paste its contents into issues, pull requests, chat, or source files. Enter it only in the GitHub secret value field.

## 4. Verify the repository secret configuration

1. Open **Actions** → **Unity Activation**.
2. Select **Run workflow** on `main`.
3. Read the run summary. It reports whether all three secrets are present, without printing their values.

This check confirms only that the secrets exist. It does not validate the license with Unity.

## 5. Run the Android build

After all three secrets are saved, open **Actions** → **Android APK** → **Run workflow** on `main`. The Android workflow uses GameCI to validate the Unity license and build `Builds/SandsOfLegends.apk`.

If GameCI fails, inspect the complete job logs; a green workflow with the GameCI or Upload APK steps skipped is not a successful APK build.

Official GameCI instructions: https://game.ci/docs/github/activation
