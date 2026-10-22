using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private float loadDelay = 0.25f;
    private const string Level1SceneName = "Level01Scene";
    private const string Level2SceneName = "InnovationScene";

    public void OnLevel1Clicked()
    {
        StartCoroutine(LoadAfterDelay(Level1SceneName));
    }

    public void OnLevel2Clicked()
    {
        StartCoroutine(LoadAfterDelay(Level2SceneName));
    }

    public void OnExitClicked()
    {
        StartCoroutine(LoadAfterDelay("StartScene"));
    }
    private IEnumerator LoadAfterDelay(string sceneName)
    {
        yield return new WaitForSecondsRealtime(loadDelay);
        SceneLoader.Load(sceneName);
    }
}
