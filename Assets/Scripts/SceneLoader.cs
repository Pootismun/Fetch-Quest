using UnityEngine.SceneManagement;

public class SceneLoader
{
    private const string LoadingSceneName = "LoadingScene";

    public static string TargetScene { get; private set; } = "StartScene";

    public static void Load(string sceneName)
    {
        TargetScene = sceneName;
        SceneManager.LoadScene(LoadingSceneName);
    }
}
