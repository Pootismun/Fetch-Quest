using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HighScoreDisplay : MonoBehaviour
{
    private void Start()
    {
        int highScore = PlayerPrefs.GetInt(GameOverManager.HighScoreKey, 0);
        float highScoreTime = PlayerPrefs.GetFloat(GameOverManager.HighScoreTimeKey, 0f);

        if (highScoreTime >= float.MaxValue)
        {
            highScoreTime = 0f;
        }

        int minutes = (int)(highScoreTime / 60f);
        int seconds = (int)(highScoreTime % 60f);
        int hundredths = (int)((highScoreTime * 100f) % 100f);

        string timeText = minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + hundredths.ToString("00");

        GetComponent<TextMeshProUGUI>().text = "HIGH SCORE: " + highScore.ToString("D6") + "  BEST TIME: " + timeText;
    }
}
