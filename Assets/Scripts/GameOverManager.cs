using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
   public static GameOverManager Instance { get; private set; }

    public const string HighScoreKey = "HighScore";
    public const string HighScoreTimeKey = "HighScoreTime";

    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private GameObject blockingImage;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private float returnDelay = 3f;

    private bool isGameOver;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!isGameOver && levelGenerator != null && levelGenerator.PelletsRemaining == 0)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StopTimer();
        }

        PacStudentController dog = FindAnyObjectByType<PacStudentController>();
        if (dog != null)
        {
            dog.StopAllSounds();
            dog.enabled = false;
        }

        CherryController cherryController = FindAnyObjectByType<CherryController>();
        if (cherryController != null)
        {
            cherryController.enabled = false;
        }

        foreach (GhostController ghost in FindObjectsByType<GhostController>())
        {
            ghost.enabled = false;
        }

        if (GhostScaredManager.Instance != null)
        {
            GhostScaredManager.Instance.enabled = false;
        }

        MusicController music = FindAnyObjectByType<MusicController>();
        if (music != null)
        {
            music.StopAllMusic();
        }

        blockingImage.SetActive(true);
        gameOverText.SetActive(true);

        TMP_Text label = gameOverText.GetComponent<TMP_Text>();
        if (label != null && levelGenerator != null && levelGenerator.PelletsRemaining == 0)
        {
            label.text = label.text + "\nYOU WIN!";
        }

        SaveHighScore();

        yield return new WaitForSecondsRealtime(returnDelay);

        SceneLoader.Load("StartScene");
    }

    private void SaveHighScore()
    {
        if (ScoreManager.Instance == null || GameTimer.Instance == null)
        {
            return;
        }

        int currentScore = ScoreManager.Instance.Score;
        float currentTime = GameTimer.Instance.ElapsedSeconds;

        int previousScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        float previousTime = PlayerPrefs.GetFloat(HighScoreTimeKey, float.MaxValue);

        bool beatenScore = currentScore > previousScore;
        bool sameScoreFaster = currentScore == previousScore && currentTime < previousTime;

        if (beatenScore || sameScoreFaster)
        {
            PlayerPrefs.SetInt(HighScoreKey, currentScore);
            PlayerPrefs.SetFloat(HighScoreTimeKey, currentTime);
            PlayerPrefs.Save();
        }
    }
}
