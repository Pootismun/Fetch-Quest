using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
   public static GameOverManager Instance { get; private set; }

    public const string HighScoreKey = "HighScore";
    public const string HighScoreTimeKey = "HighScoreTime";

    public static string ScoreKeyFor(string sceneName)
    {
        return HighScoreKey + "_" + sceneName;
    }

    public static string TimeKeyFor(string sceneName)
    {
        return HighScoreTimeKey + "_" + sceneName;
    }

    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private GameObject blockingImage;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private GameObject newHighScoreText;
    [SerializeField] private GameObject noHighScoreText;
    [SerializeField] private GameObject tryAgainText;
    [SerializeField] private float returnDelay = 8f;

    private bool forceWin;
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
        bool won = forceWin || (levelGenerator != null && levelGenerator.PelletsRemaining == 0);

        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StopTimer();
        }

        PacStudentController dog = FindAnyObjectByType<PacStudentController>();
        if (dog != null)
        {
            dog.StopMovementSounds();
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

        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        float finalTime = GameTimer.Instance != null ? GameTimer.Instance.ElapsedSeconds : 0f;

        bool newHighScore = SaveHighScore();

        if (messageText != null)
        {
            messageText.text = won ? "You won! All the treats were saved!" : "The squirrels ate all your treats!";
        }

        if (statsText != null)
        {
            int minutes = (int)(finalTime / 60f);
            int seconds = (int)(finalTime % 60f);
            int hundredths = (int)((finalTime * 100f) % 100f);

            statsText.text = "SCORE: " + finalScore.ToString("D6") + "\nTIME: " + minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + hundredths.ToString("00");
        }

        if (newHighScoreText != null)
        {
            newHighScoreText.SetActive(won && newHighScore);
        }

        if (noHighScoreText != null)
        {
            noHighScoreText.SetActive(won && !newHighScore);
        }

        if (tryAgainText != null)
        {
            tryAgainText.SetActive(!won);
        }

        EndPanelEntrance entrance = endPanel.GetComponent<EndPanelEntrance>();

        if (entrance != null)
        {
            entrance.SetVictory(won);
        }

        endPanel.SetActive(true);

        if (EndScreenEffects.Instance != null)
        {
            if (won)
            {
                EndScreenEffects.Instance.PlayVictory();
            }
            else
            {
                EndScreenEffects.Instance.PlayDefeat();
            }
        }

        float fadeTime = 0.5f;

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, returnDelay - fadeTime));

        if (EndScreenEffects.Instance != null)
        {
            yield return EndScreenEffects.Instance.FadeOutSounds(fadeTime);
        }
        else
        {
            yield return new WaitForSecondsRealtime(fadeTime);
        }

        SceneLoader.Load("StartScene");
    }

    [ContextMenu("Test Win Screen")]
    private void TestWinScreen()
    {
        forceWin = true;
        TriggerGameOver();
    }

    [ContextMenu("Test Lose Screen")]
    private void TestLoseScreen()
    {
        forceWin = false;
        TriggerGameOver();
    }

    private bool SaveHighScore()
    {
        if (ScoreManager.Instance == null || GameTimer.Instance == null)
        {
            return false;
        }

        int currentScore = ScoreManager.Instance.Score;
        float currentTime = GameTimer.Instance.ElapsedSeconds;

        string sceneName = SceneManager.GetActiveScene().name;
        string scoreKey = ScoreKeyFor(sceneName);
        string timeKey = TimeKeyFor(sceneName);

        if (currentScore <= 0)
        {
            return false;
        }

        int previousScore = PlayerPrefs.GetInt(scoreKey, 0);
        float previousTime = PlayerPrefs.GetFloat(timeKey, float.MaxValue);

        bool beatenScore = currentScore > previousScore;
        bool sameScoreFaster = currentScore == previousScore && currentTime < previousTime;

        if (beatenScore || sameScoreFaster)
        {
            PlayerPrefs.SetInt(scoreKey, currentScore);
            PlayerPrefs.SetFloat(timeKey, currentTime);
            PlayerPrefs.Save();
            return true;
        }

        return false;
    }
}
