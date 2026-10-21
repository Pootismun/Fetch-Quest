using TMPro;
using UnityEngine;

public class GhostScaredManager : MonoBehaviour
{
    public static GhostScaredManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private MusicController musicController;
    [SerializeField] private float scaredDuration = 10f;
    [SerializeField] private float recoveringThreshold = 3f;

    private GhostStateController[] ghosts;
    private float timeRemaining;
    private bool scaredActive;
    private bool recoveringTriggered;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ghosts = FindObjectsByType<GhostStateController>();

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    public void StartScared()
    { 
        scaredActive = true;
        recoveringTriggered = false;
        timeRemaining = scaredDuration;

        foreach (GhostStateController ghost in ghosts)
        {
            if (ghost.CurrentState != GhostState.Dead)
            {
                ghost.SetState(GhostState.Scared);
            }
        }

        if (musicController != null)
        {
            musicController.PlayScaredMusic();
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }

        UpdateTimerText();
    }

    private void Update()
    {
        if (!scaredActive)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (!recoveringTriggered && timeRemaining <= recoveringThreshold)
        {
            recoveringTriggered = true;

            foreach (GhostStateController ghost in ghosts)
            {
                if (ghost.CurrentState == GhostState.Scared)
                {
                    ghost.SetState(GhostState.Recovering);
                }
            }
        }

        if (timeRemaining <= 0f)
        {
            EndScared();
            return;
        }
        UpdateTimerText();
    }

    private void EndScared()
    {
        scaredActive = false;

        foreach (GhostStateController ghost in ghosts)
        {
            if (ghost.CurrentState == GhostState.Scared || ghost.CurrentState == GhostState.Recovering)
            {
                ghost.SetState(GhostState.Normal);
            }
        }

        if (musicController != null)
        {
            musicController.PlayNormalMusic();
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text = "SCARED: " + Mathf.CeilToInt(timeRemaining).ToString();
        }
    }
}
