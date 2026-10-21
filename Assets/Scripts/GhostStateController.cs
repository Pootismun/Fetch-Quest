using UnityEngine;

public enum GhostState
{
    Normal,
    Scared,
    Recovering,
    Dead
}

[RequireComponent(typeof(Animator))]
public class GhostStateController : MonoBehaviour
{
    private Animator animator;
    private GhostState currentState = GhostState.Normal;

    public GhostState CurrentState => currentState;
    public bool IsVulnerable => currentState == GhostState.Scared || currentState == GhostState.Recovering;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        SetState(GhostState.Normal);
    }

    public void SetState(GhostState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GhostState.Normal:
                animator.Play("Walk_Up");
                break;
            case GhostState.Scared:
                animator.Play("Scared_Up");
                break;
            case GhostState.Recovering:
                animator.Play("Recovering");
                break;
            case GhostState.Dead:
                animator.Play("Dead");
                break;
        }
    }
}
