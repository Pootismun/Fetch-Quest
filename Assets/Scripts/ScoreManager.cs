using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI scoreText;

    private int score;
    public int Score => score;

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        UpdateScoreText();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("D6");
        }
    }
}
