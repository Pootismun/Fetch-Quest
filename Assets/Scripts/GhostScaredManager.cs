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
    private PopInOnEnable timerPop;

    public GhostState GlobalState
    {
        get
        {
            if (!scaredActive)
            {
                return GhostState.Normal;
            }

            return recoveringTriggered ? GhostState.Recovering : GhostState.Scared;
        }
    }

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

        RefreshMusic();

        ShowTimer();

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

        RefreshMusic();

        HideTimer();
    }

    public void RefreshMusic()
    {
        if (!enabled || musicController == null)
        {
            return;
        }

        if (AnyGhostDead())
        {
            musicController.PlayDeadMusic();
        }
        else if (scaredActive)
        {
            musicController.PlayScaredMusic();
        }
        else
        {
            musicController.PlayNormalMusic();
        }
    }

    private bool AnyGhostDead()
    {
        foreach (GhostStateController ghost in ghosts)
        {
            if (ghost.CurrentState == GhostState.Dead)
            {
                return true;
            }
        }

        return false;
    }

    private void ShowTimer()
    {
        if (timerText == null)
        {
            return;
        }

        if (timerPop == null)
        {
            timerPop = timerText.GetComponent<PopInOnEnable>();
        }

        if (timerPop != null)
        {
            timerPop.Show();
        }
        else
        {
            timerText.gameObject.SetActive(true);
        }
    }

    private void HideTimer()
    {
        if (timerText == null)
        {
            return;
        }

        if (timerPop == null)
        {
            timerPop = timerText.GetComponent<PopInOnEnable>();
        }

        if (timerPop != null)
        {
            timerPop.Hide();
        }
        else
        {
            timerText.gameObject.SetActive(false);
        }
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }
}
