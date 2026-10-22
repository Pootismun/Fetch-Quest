using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private const string LoadingSceneName = "LoadingScene";

    public static string TargetScene { get; private set; } = "StartScene";
    public static string PreviousScene { get; private set; } = "";
    public static bool IsTransitioning { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        TargetScene = "StartScene";
        PreviousScene = "";
        IsTransitioning = false;
    }

    public static void Load(string sceneName)
    {
        if (IsTransitioning)
        {
            return;
        }

        IsTransitioning = true;
        TargetScene = sceneName;
        PreviousScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(LoadingSceneName, LoadSceneMode.Additive);
    }

    public static void Finish()
    {
        IsTransitioning = false;
    }
}
