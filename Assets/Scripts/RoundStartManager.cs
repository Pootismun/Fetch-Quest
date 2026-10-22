using System.Collections;
using TMPro;
using UnityEngine;

public class RoundStartManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject blockingImage;
    [SerializeField] private MusicController musicController;
    [SerializeField] private float stepDuration = 1f;

    private PacStudentController dog;
    private CherryController cherryController;
    private GhostController[] ghosts;

    private IEnumerator Start()
    {
        dog = FindAnyObjectByType<PacStudentController>();
        cherryController = FindAnyObjectByType<CherryController>();
        ghosts = FindObjectsByType<GhostController>();

        SetGameplayEnabled(false);

        while (SceneLoader.IsTransitioning)
        {
            yield return null;
        }

        blockingImage.SetActive(true);
        countdownText.gameObject.SetActive(true);
        countdownText.text = "3";

        yield return null;

        if (musicController != null)
        {
            musicController.PlayIntroMusic();
        }

        string[] steps = { "3", "2", "1", "GO!" };

        foreach (string step in steps)
        {
            countdownText.text = step;
            yield return new WaitForSeconds(stepDuration);
        }

        countdownText.gameObject.SetActive(false);
        blockingImage.SetActive(false);

        if (musicController != null)
        {
            musicController.StopIntroMusic();
            musicController.PlayNormalMusic();
        }

        SetGameplayEnabled(true);

        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StartTimer();
        }
    }

    private void SetGameplayEnabled(bool isEnabled)
    {
        if (dog != null)
        {
            dog.enabled = isEnabled;
        }

        if (cherryController != null)
        {
            cherryController.enabled = isEnabled;
        }

        if (ghosts != null)
        {
            foreach (GhostController ghost in ghosts)
            {
                ghost.enabled = isEnabled;
            }
        }
    }
}
