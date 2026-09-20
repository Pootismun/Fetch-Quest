using UnityEngine;

// Dog (Player) movement.
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class DogController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private AudioClip moveClip;

    [SerializeField] private bool startAtBottomCentre = true;
    private const float FirstColumnX = -13.5f;
    private const float FirstRowY = 14f;

    private const int StartRow = 23;
    private const int LeadInColumn = 6;

    private const int LoopStartRow = 3;
    private const int LoopStartColumn = 1;

    private static readonly int[,] LoopCorners = { { 1, 1 }, { 1, 6 }, { 5, 6 }, { 5, 1 } };

    private const string PreviewParameter = "PreviewCycle";

    private Animator animator;
    private AudioSource audioSource;
    private Vector3[] corners;

    private Vector3[] leadIn;
    private int leadInIndex;
    private int nextCorner;

    private Vector3 segmentStart;
    private Vector3 segmentEnd;
    private float segmentStartTime;
    private float segmentDuration;
    private string currentDirection = "";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        corners = new Vector3[LoopCorners.GetLength(0)];
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = TileCentre(LoopCorners[i, 0], LoopCorners[i, 1]);
        }
    }

    private void Start()
    {
        animator.SetBool(PreviewParameter, false);

        audioSource.playOnAwake = false;
        audioSource.clip = moveClip;
        audioSource.loop = true;
        if (moveClip != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("DogController, assign the moving sound in the inspector.", this);
        }

        if (startAtBottomCentre)
        {
            transform.position = new Vector3(0f, FirstRowY - StartRow, 0f);
            leadIn = new[] { TileCentre(StartRow, LeadInColumn), corners[2] };
            nextCorner = 3;
        }
        else
        {
            transform.position = TileCentre(LoopStartRow, LoopStartColumn);
            leadIn = new Vector3[0];
            nextCorner = 0;
        }

        leadInIndex = 0;
        StartSegment(transform.position, Time.time);
    }

    private void Update()
    {
        float now = Time.time;

        while (now >= segmentStartTime + segmentDuration)
        {
            StartSegment(segmentEnd, segmentStartTime + segmentDuration);
        }

        float t = (now - segmentStartTime) / segmentDuration;
        transform.position = Vector3.Lerp(segmentStart, segmentEnd, t);
    }
    private void StartSegment(Vector3 from, float startTime)
    {
        segmentStart = from;
        segmentEnd = NextTarget();
        segmentStartTime = startTime;
        segmentDuration = Vector3.Distance(segmentStart, segmentEnd) / Mathf.Max(moveSpeed, 0.01f);

        SetDirection(segmentEnd - segmentStart);
    }
    private Vector3 NextTarget()
    {
        if (leadInIndex < leadIn.Length)
        {
            Vector3 waypoint = leadIn[leadInIndex];
            leadInIndex++;
            return waypoint;
        }

        Vector3 corner = corners[nextCorner];
        nextCorner = (nextCorner + 1) % corners.Length;
        return corner;
    }

    private void SetDirection(Vector3 delta)
    {
        string direction;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            direction = delta.x > 0f ? "Right" : "Left";
        }
        else
        {
            direction = delta.y > 0f ? "Up" : "Down";
        }

        if (direction != currentDirection)
        {
            currentDirection = direction;
            animator.Play("Walk_" + direction);
        }
    }

    private static Vector3 TileCentre(int row, int column)
    {
        return new Vector3(FirstColumnX + column, FirstRowY - row, 0f);
    }
}
