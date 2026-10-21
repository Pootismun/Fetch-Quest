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
    private Vector2Int facing = new Vector2Int(0, -1);

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
        PlayCurrentAnimation();
    }

    public void SetDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.zero || direction == facing)
        {
            return;
        }

        facing = direction;

        if (currentState == GhostState.Normal || currentState == GhostState.Scared)
        {
            PlayCurrentAnimation();
        }
    }

    private void PlayCurrentAnimation()
    {
        switch (currentState)
        {
            case GhostState.Normal:
                animator.Play("Walk_" + FacingName());
                break;
            case GhostState.Scared:
                animator.Play("Scared_" + FacingName());
                break;
            case GhostState.Recovering:
                animator.Play("Recovering");
                break;
            case GhostState.Dead:
                animator.Play("Dead");
                break;
        }
    }

    private string FacingName()
    {
        if (facing == new Vector2Int(0, -1)) return "Up";
        if (facing == new Vector2Int(1, 0)) return "Right";
        if (facing == new Vector2Int(0, 1)) return "Down";
        return "Left";
    }
}
