using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PacStudentController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float tilesPerSecond = 5f;
    [SerializeField] private LevelGenerator levelGenerator;

    [Header("Audio")]
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip eatPelletMoveClip;
    [SerializeField] private float moveVolume = 1f;
    [SerializeField] private float eatPelletVolume = 1f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem dustEffect;

    private static readonly Vector2Int Up = new Vector2Int(0, -1);
    private static readonly Vector2Int Down = new Vector2Int(0, 1);
    private static readonly Vector2Int Left = new Vector2Int(-1, 0);
    private static readonly Vector2Int Right = new Vector2Int(1, 0);
    private static readonly Vector2Int None = Vector2Int.zero;

    private Animator animator;
    private AudioSource audioSource;
    private AudioSource eatAudioSource;

    private Vector2Int currentCell;
    private Vector2Int targetCell;
    private bool isMoving;
    private float lerpT;

    private Vector2Int lastInput = None;
    private Vector2Int currentInput = None;
    private string playingAnimation = "";
    private bool initialized;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        eatAudioSource = gameObject.AddComponent<AudioSource>();

        if (levelGenerator == null)
        {
            levelGenerator = FindAnyObjectByType<LevelGenerator>();
        }
    }

    private void Start()
    {
        animator.enabled = true;

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.clip = moveClip;
        audioSource.volume = moveVolume;

        eatAudioSource.playOnAwake = false;
        eatAudioSource.loop = false;
        eatAudioSource.clip = eatPelletMoveClip;
        eatAudioSource.volume = eatPelletVolume;
    }

    private void Initialize()
    {
        if (levelGenerator == null)
        {
            levelGenerator = FindAnyObjectByType<LevelGenerator>();
        }

        if (levelGenerator == null || levelGenerator.Rows <= 0 || levelGenerator.Columns <= 0)
        {
            return;
        }

        currentCell = levelGenerator.WorldToCell(transform.position);
        transform.position = levelGenerator.CellToWorld(currentCell.y, currentCell.x);
        targetCell = currentCell;

        PlayAnimation(Right);
        animator.speed = 0f;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            Initialize();
            if (!initialized)
            {
                return;
            }
        }

        ReadInput();

        if (isMoving)
        {
            AdvanceMove();
        }
        else
        {
            TryStartMove();
        }
    }

    private void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
        {
            lastInput = Up;
        }
        else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
        {
            lastInput = Down;
        }
        else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            lastInput = Left;
        }
        else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            lastInput = Right;
        }
    }

    private void TryStartMove()
    {
        if (lastInput != None && IsWalkable(currentCell + lastInput))
        {
            currentInput = lastInput;
            BeginMove(currentCell + currentInput);
        }
        else if (currentInput != None && IsWalkable(currentCell + currentInput))
        {
            BeginMove(currentCell + currentInput);
        }
        else
        {
            StopMoving();
        }
    }

    private void BeginMove(Vector2Int nextCell)
    {
        targetCell = nextCell;
        isMoving = true;
        lerpT = 0f;

        PlayAnimation(currentInput);
        animator.speed = 1f;

        if (dustEffect != null && !dustEffect.isPlaying)
        {
            dustEffect.Play();
        }

        if (moveClip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        bool willEatPellet = levelGenerator.HasPelletAt(nextCell.y, nextCell.x);
        if (willEatPellet && eatPelletMoveClip != null && !eatAudioSource.isPlaying)
        {
            eatAudioSource.Play();
        }
    }

    private void StopMoving()
    {
        isMoving = false;
        animator.speed = 0f;

        if (dustEffect != null && dustEffect.isPlaying)
        {
            dustEffect.Stop();
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void AdvanceMove()
    {
        lerpT += Time.deltaTime * tilesPerSecond;
        
        while (lerpT >= 1f && isMoving)
        {
            transform.position = levelGenerator.CellToWorld(targetCell.y, targetCell.x);
            currentCell = targetCell;
            isMoving = false;
            float overshoot = lerpT - 1f;

            TryStartMove();

            lerpT = isMoving ? overshoot : 0f;
        }

        if (isMoving)
        {
            Vector3 from = levelGenerator.CellToWorld(currentCell.y, currentCell.x);
            Vector3 to = levelGenerator.CellToWorld(targetCell.y, targetCell.x);
            transform.position = Vector3.Lerp(from, to, lerpT);
        }
    }

    private bool IsWalkable(Vector2Int cell)
    {
        return levelGenerator != null && levelGenerator.IsWalkable(cell.y, cell.x);
    }

    private void PlayAnimation(Vector2Int direction)
    {
        string name;
        if (direction == Up) name = "Walk_Up";
        else if (direction == Down) name = "Walk_Down";
        else if (direction == Left) name = "Walk_Left";
        else if (direction == Right) name = "Walk_Right";
        else return;

        if (name != playingAnimation)
        {
            playingAnimation = name;
            animator.Play(name);
        }
    }
}
