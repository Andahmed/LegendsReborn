using UnityEngine;

/// <summary>
/// Locks the mobile game to landscape before the first scene is loaded.
/// Keep Player Settings aligned with Landscape Left as well.
/// </summary>
public static class LandscapeOrientation
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
}
