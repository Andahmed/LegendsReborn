# Unity Personal headless licensing for GitHub Actions

This project uses Unity **2022.3.62f3**. Unity Personal no longer supports the old manual `.alf/.ulf` activation route. Do **not** create or use a `UNITY_LICENSE` secret for this workflow.

## GameCI implementation pinned by this repository

- GitHub Action: [`game-ci/unity-builder@v6.0.0`](https://github.com/game-ci/unity-builder/releases/tag/v6.0.0), which is now a thin wrapper around `game-ci/cli`.
- CLI: [`game-ci/cli v0.1.73`](https://github.com/game-ci/cli/releases/tag/v0.1.73), pinned through the action's `cliVersion` input.
- Unity project version: `2022.3.62f3`, read from `ProjectSettings/ProjectVersion.txt`.
- The merged [game-ci/cli PR #246](https://github.com/game-ci/cli/pull/246) added the Personal licensing strategy. When only `UNITY_EMAIL` and `UNITY_PASSWORD` are supplied, the CLI selects that strategy and invokes the Unity licensing client with `--activate-all --include-personal --username ... --password ...`. The Personal seat is returned using `--return-ulf` / `game-ci return-license` cleanup, including an EXIT trap around build execution.

## 1. Add the two GitHub repository secrets

1. Open [Andahmed/LegendsReborn](https://github.com/Andahmed/LegendsReborn).
2. Select **Settings**.
3. In the left sidebar, open **Secrets and variables** → **Actions**.
4. Select **New repository secret**.
5. Add these two secrets separately:

| Secret name | Value |
| --- | --- |
| `UNITY_EMAIL` | Email address for the Unity ID |
| `UNITY_PASSWORD` | Password for that Unity ID |

6. Save each secret. Do not put either value in a commit, workflow file, issue, pull request, or chat.
7. No `UNITY_LICENSE` secret and no `.ulf` file are required by this Personal licensing route.

## 2. Run the Android APK workflow

1. Open the repository's **Actions** tab.
2. Select **Android APK**.
3. Select **Run workflow**.
4. Choose branch **main** and select **Run workflow** again.
5. Open the run and inspect the **Build Android APK with GameCI** and **Upload APK** steps.
6. Download the artifact named `SandsOfLegends-Android-APK` only when both steps actually ran and succeeded.

A green workflow whose GameCI build or Upload APK step is `skipped` is **not** a successful APK build.

## 3. Licensing limitations and upstream caveats

- **2FA/device verification is not supported** by headless Personal activation. If Unity challenges the account with 2FA or device verification, the CLI reports the activation failure; this workflow cannot complete that challenge.
- The Unity licensing-client flags `--activate-all`, `--include-personal`, and `--return-ulf` are **not documented by Unity** and may change without notice.
- The Personal seat is a consumed seat while checked out. GameCI CLI PR #246 adds return-license cleanup on process exit; avoid overlapping builds on the same Personal account.
- PR #246's author explicitly stated that the new Personal activation flow had **not been tested end-to-end against a live Unity account** at merge time. Its tests used a stub licensing client, including coverage for strategy selection, 2FA errors, seat exhaustion and seat return. The first real build in this repository is therefore the end-to-end verification.
- The upstream PR recommends a dedicated CI-only Unity account because sharing a Personal seat through CI credentials may be a grey area under Unity's terms. The repository owner has directed that the existing Ahmed Unity ID be used; confirm that this use is acceptable under the applicable Unity terms before enabling it.

References:
- [game-ci/unity-builder v6.0.0](https://github.com/game-ci/unity-builder/releases/tag/v6.0.0)
- [game-ci/cli v0.1.73](https://github.com/game-ci/cli/releases/tag/v0.1.73)
- [game-ci/cli PR #246](https://github.com/game-ci/cli/pull/246)
- [GameCI builder documentation](https://game.ci/docs/github/builder)
