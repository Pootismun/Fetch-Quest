using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI timerText;

    private float elapsedSeconds;
    private bool running;

    public float ElapsedSeconds => elapsedSeconds;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateTimerText();
    }

    public void StartTimer()
    {
        running = true;
    }

    public void StopTimer()
    {
        running = false;
    }

    private void Update()
    {
        if (!running)
        {
            return;
        }

        elapsedSeconds += Time.deltaTime;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int minutes = (int)(elapsedSeconds / 60f);
        int seconds = (int)(elapsedSeconds % 60f);
        int hundredths = (int)((elapsedSeconds * 100f) % 100f);

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + hundredths.ToString("00");
    }
}
