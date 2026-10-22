using System.Collections;
using TMPro;
using UnityEngine;

public class RoundStartManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private CountdownAnimator countdownAnimator;
    [SerializeField] private GameObject blockingImage;
    [SerializeField] private MusicController musicController;
    [SerializeField] private float stepDuration = 1f;
    [SerializeField] private float introDuration = 5f;

    private PacStudentController dog;
    private CherryController cherryController;
    private GhostController[] ghosts;

    private IEnumerator Start()
    {
        dog = FindAnyObjectByType<PacStudentController>();
        cherryController = FindAnyObjectByType<CherryController>();
        ghosts = FindObjectsByType<GhostController>();

        SetGameplayEnabled(false);
        countdownText.text = "";

        while (SceneLoader.IsTransitioning)
        {
            yield return null;
        }

        blockingImage.SetActive(true);
        countdownText.gameObject.SetActive(true);
        countdownText.text = "";

        yield return null;

        if (musicController != null)
        {
            musicController.PlayIntroMusic();
        }

        yield return new WaitForSeconds(introDuration);

        string[] steps = { "3", "2", "1", "EAT!" };

        for (int i = 0; i < steps.Length; i++)
        {
            yield return countdownAnimator.ShowStep(i, steps.Length, steps[i], stepDuration);
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
