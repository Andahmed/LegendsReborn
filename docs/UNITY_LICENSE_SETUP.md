# Unity Personal license setup (browser-only)

This repository uses Unity **2022.3.62f3**. The Unity Activation workflow creates the manual activation file; the repository owner completes the Personal-license activation in a browser.

## 1. Generate and download the .alf file

1. Open the repository on GitHub.
2. Select **Actions** → **Unity Activation**.
3. Select **Run workflow** on the `main` branch.
4. Open the completed run and download the **Unity-Activation-File** artifact.
5. Extract the downloaded ZIP. It contains the `.alf` manual activation file.

## 2. Activate Unity Personal

1. In a browser, open https://license.unity3d.com/manual.
2. Choose **Unity Personal** and follow the manual activation flow.
3. Upload the extracted `.alf` file when prompted.
4. Download the resulting `.ulf` license file.

## 3. Add GitHub Actions repository secrets

1. Open the GitHub repository **Settings**.
2. Go to **Secrets and variables** → **Actions**.
3. Select **New repository secret** and add each secret below:

| Secret name | Value |
| --- | --- |
| `UNITY_LICENSE` | The complete contents of the downloaded `.ulf` file, including all lines |
| `UNITY_EMAIL` | The Unity ID email used for this Personal license |
| `UNITY_PASSWORD` | The password for that Unity ID |

Do not commit the `.ulf` file or paste its contents into issues, pull requests, chat, or source files. Enter it only in the GitHub secret value field.

After all three repository secrets are saved, manually run **Actions** → **Android APK** → **Run workflow** on `main`. The workflow checks that all three secrets exist before starting GameCI.
