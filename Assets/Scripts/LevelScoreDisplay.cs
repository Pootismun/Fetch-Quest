using TMPro;
using UnityEngine;

public class LevelScoreDisplay : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level01Scene";
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;

    private void Start()
    {
        string scoreKey = GameOverManager.ScoreKeyFor(sceneName);
        string timeKey = GameOverManager.TimeKeyFor(sceneName);

        if (!PlayerPrefs.HasKey(scoreKey))
        {
            scoreText.text = "000000";
            timeText.text = "00:00:00";
            return;
        }

        int score = PlayerPrefs.GetInt(scoreKey, 0);
        float time = PlayerPrefs.GetFloat(timeKey, 0f);

        if (time >= float.MaxValue)
        {
            time = 0f;
        }

        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        int hundredths = (int)((time * 100f) % 100f);

        scoreText.text = score.ToString("D6");
        timeText.text = minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + hundredths.ToString("00");
    }
}
