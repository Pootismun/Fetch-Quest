using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private const string Level1SceneName = "Level01Scene";
    private const string Level2SceneName = "InnovationScene";

    public void OnLevel1Clicked()
    {
        SceneManager.LoadScene(Level1SceneName);
    }

    public void OnLevel2Clicked()
    {
        SceneManager.LoadScene(Level2SceneName);
    }

    public void OnExitClicked()
    {
        SceneManager.LoadScene("StartScene");
    }
}
