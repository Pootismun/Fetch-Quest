using System.Collections;
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
    [SerializeField] private AudioClip wallBumpClip;
    [SerializeField] private float moveVolume = 1f;
    [SerializeField] private float eatPelletVolume = 1f;
    [SerializeField] private float wallBumpVolume = 1f;
    [SerializeField] private AudioClip powerPelletClip;
    [SerializeField] private float powerPelletVolume = 1f;
    [SerializeField] private AudioClip cherryClip;
    [SerializeField] private float cherryVolume = 1f;
    [SerializeField] private AudioClip ghostEatClip;
    [SerializeField] private float ghostEatVolume = 1f;

    [Header("Death")]
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private float deathVolume = 1f;
    [SerializeField] private float deathDuration = 3f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem dustEffect;
    [SerializeField] private ParticleSystem wallBumpEffect;
    [SerializeField] private ParticleSystem deathEffect;

    private static readonly Vector2Int Up = new Vector2Int(0, -1);
    private static readonly Vector2Int Down = new Vector2Int(0, 1);
    private static readonly Vector2Int Left = new Vector2Int(-1, 0);
    private static readonly Vector2Int Right = new Vector2Int(1, 0);
    private static readonly Vector2Int None = Vector2Int.zero;

    private Animator animator;
    private int normalSortingOrder;
    private AudioSource audioSource;
    private AudioSource eatAudioSource;
    private AudioSource wallBumpAudioSource;
    private AudioSource powerPelletAudioSource;
    private AudioSource cherryAudioSource;
    private AudioSource ghostEatAudioSource;
    private AudioSource deathAudioSource;

    private Vector2Int currentCell;
    private Vector2Int targetCell;
    private Vector2Int startCell;
    private bool isMoving;
    private float lerpT;

    private Vector2Int lastInput = None;
    private Vector2Int currentInput = None;
    private Vector2Int lastBumpDirection = None;
    private string playingAnimation = "";
    private bool initialized;

    private SpriteRenderer spriteRenderer;
    private GameObject teleportClone;
    private SpriteRenderer teleportCloneRenderer;
    private Vector3 teleportCloneFrom;
    private Vector3 teleportCloneTo;
    private bool pendingTeleport;
    private Vector2Int pendingTeleportCell;
    public float TilesPerSecond => tilesPerSecond;
    public bool IsDying { get; private set; }
    private const float DeadClipLength = 1.25f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        eatAudioSource = gameObject.AddComponent<AudioSource>();
        wallBumpAudioSource = gameObject.AddComponent<AudioSource>();
        powerPelletAudioSource = gameObject.AddComponent<AudioSource>();
        cherryAudioSource = gameObject.AddComponent<AudioSource>();
        ghostEatAudioSource = gameObject.AddComponent<AudioSource>();
        deathAudioSource = gameObject.AddComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalSortingOrder = spriteRenderer.sortingOrder;

        if (levelGenerator == null)
        {
            levelGenerator = FindAnyObjectByType<LevelGenerator>();
        }
    }

    private void Start()
    {
        animator.enabled = true;
        animator.SetBool("PreviewCycle", false);

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.clip = moveClip;
        audioSource.volume = moveVolume;

        eatAudioSource.playOnAwake = false;
        eatAudioSource.loop = false;
        eatAudioSource.clip = eatPelletMoveClip;
        eatAudioSource.volume = eatPelletVolume;

        wallBumpAudioSource.playOnAwake = false;
        wallBumpAudioSource.loop = false;
        wallBumpAudioSource.clip = wallBumpClip;
        wallBumpAudioSource.volume = wallBumpVolume;

        powerPelletAudioSource.playOnAwake = false;
        powerPelletAudioSource.loop = false;
        powerPelletAudioSource.clip = powerPelletClip;
        powerPelletAudioSource.volume = powerPelletVolume;

        cherryAudioSource.playOnAwake = false;
        cherryAudioSource.loop = false;
        cherryAudioSource.clip = cherryClip;
        cherryAudioSource.volume = cherryVolume;

        ghostEatAudioSource.playOnAwake = false;
        ghostEatAudioSource.loop = false;
        ghostEatAudioSource.clip = ghostEatClip;
        ghostEatAudioSource.volume = ghostEatVolume;

        deathAudioSource.playOnAwake = false;
        deathAudioSource.loop = false;
        deathAudioSource.clip = deathClip;
        deathAudioSource.volume = deathVolume;
    }

    public void ShowStartPose()
    {
        animator.enabled = true;
        animator.SetBool("PreviewCycle", false);
        PlayAnimation(Right);
        animator.speed = 0f;
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
        startCell = currentCell;

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
        Vector2Int attempted = lastInput != None ? lastInput : currentInput;

        if (lastInput != None && TryMoveOrTeleport(lastInput))
        {
            lastBumpDirection = None;
            return;
        }

        if (currentInput != None && TryMoveOrTeleport(currentInput))
        {
            lastBumpDirection = None;
            return;
        }

        StopMoving();

        if (attempted != None && lastBumpDirection == None)
        {
            Bump(attempted);
            lastBumpDirection = attempted;
        }
    }

    private bool TryMoveOrTeleport(Vector2Int direction)
    {
        Vector2Int nextCell = currentCell + direction;

        if (IsWalkable(nextCell))
        {
            currentInput = direction;
            BeginMove(nextCell);
            return true;
        }

        if (levelGenerator.TryGetTeleportDestination(currentCell, direction, out Vector2Int teleportCell))
        {
            currentInput = direction;
            StartTeleport(direction, teleportCell);
            return true;
        }

        return false;
    }

    private void StartTeleport(Vector2Int direction, Vector2Int teleportCell)
    {
        pendingTeleport = true;
        pendingTeleportCell = teleportCell;

        Vector2Int entryOutsideCell = teleportCell - direction;
        teleportCloneFrom = levelGenerator.CellToWorld(entryOutsideCell.y, entryOutsideCell.x);
        teleportCloneTo = levelGenerator.CellToWorld(teleportCell.y, teleportCell.x);

        teleportClone = new GameObject("TeleportClone");
        teleportCloneRenderer = teleportClone.AddComponent<SpriteRenderer>();
        teleportCloneRenderer.sprite = spriteRenderer.sprite;
        teleportCloneRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        teleportCloneRenderer.sortingOrder = spriteRenderer.sortingOrder;
        teleportCloneRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        teleportClone.transform.position = teleportCloneFrom;

        BeginMove(currentCell + direction);
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

        bool willEatPellet = levelGenerator.HasRegularPelletAt(nextCell.y, nextCell.x);
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

    private void Bump(Vector2Int direction)
    {
        if (wallBumpEffect != null)
        {
            Vector3 bumpPosition = levelGenerator.CellToWorld(currentCell.y, currentCell.x)
                + new Vector3(direction.x, -direction.y, 0f) * 0.5f;
            wallBumpEffect.transform.position = bumpPosition;
            wallBumpEffect.Play();
        }

        if (wallBumpClip != null)
        {
            wallBumpAudioSource.Play();
        }
    }
    private void AdvanceMove()
    {
        lerpT += Time.deltaTime * tilesPerSecond;

        while (lerpT >= 1f && isMoving)
        {
            if (pendingTeleport)
            {
                currentCell = pendingTeleportCell;
                transform.position = levelGenerator.CellToWorld(currentCell.y, currentCell.x);

                if (teleportClone != null)
                {
                    Destroy(teleportClone);
                    teleportClone = null;
                }
                pendingTeleport = false;
            }
            else
            {
                transform.position = levelGenerator.CellToWorld(targetCell.y, targetCell.x);
                currentCell = targetCell;
            }

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

            if (pendingTeleport && teleportCloneRenderer != null)
            {
                teleportCloneRenderer.sprite = spriteRenderer.sprite;
                teleportClone.transform.position = Vector3.Lerp(teleportCloneFrom, teleportCloneTo, lerpT);
            }
        }
    }
    public void StopAllSounds()
    {
        foreach (AudioSource source in GetComponents<AudioSource>())
        {
            source.Stop();
        }
    }

    public void Die()
    {
        if (IsDying)
        {
            return;
        }

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        IsDying = true;

        foreach (GhostController ghost in FindObjectsByType<GhostController>())
        {
            ghost.enabled = false;
        }

        StopMoving();
        StopAllSounds();
        lastInput = None;
        currentInput = None;
        lastBumpDirection = None;
        enabled = false;

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.LoseLife();
        }

        if (deathEffect != null)
        {
            deathEffect.transform.position = transform.position;
            deathEffect.Play();
        }

        if (deathClip != null)
        {
            deathAudioSource.Play();
        }

        float duration = deathDuration > 0f ? deathDuration : 3f;

        animator.speed = DeadClipLength / duration;
        playingAnimation = "Dead";
        animator.Play("Dead");
        spriteRenderer.sortingOrder = normalSortingOrder - 2;

        yield return new WaitForSeconds(duration);

        deathAudioSource.Stop();

        if (LivesManager.Instance != null && LivesManager.Instance.Lives <= 0)
        {
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.TriggerGameOver();
            }

            yield break;
        }

        if (teleportClone != null)
        {
            Destroy(teleportClone);
            teleportClone = null;
        }

        pendingTeleport = false;
        transform.position = levelGenerator.CellToWorld(startCell.y, startCell.x);
        currentCell = startCell;
        targetCell = startCell;
        lerpT = 0f;

        playingAnimation = "";
        PlayAnimation(Right);
        animator.speed = 0f;
        spriteRenderer.sortingOrder = normalSortingOrder;

        foreach (GhostController ghost in FindObjectsByType<GhostController>())
        {
            ghost.ResetToStart();
            ghost.enabled = true;
        }

        if (GhostScaredManager.Instance != null)
        {
            GhostScaredManager.Instance.RefreshMusic();
        }

        enabled = true;
        IsDying = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled)
        {
            return;
        }

        if (other.TryGetComponent(out Pellet pellet))
        {
            ScoreManager.Instance.AddScore(10);
            levelGenerator.MarkPelletEaten(pellet.Row, pellet.Column);
            Destroy(other.gameObject);
        }
        else if (other.TryGetComponent(out PowerPellet powerPellet))
        {
            ScoreManager.Instance.AddScore(50);
            levelGenerator.MarkPelletEaten(powerPellet.Row, powerPellet.Column);

            if (powerPelletClip != null)
            {
                powerPelletAudioSource.Play();
            }

            if (GhostScaredManager.Instance != null)
            {
                GhostScaredManager.Instance.StartScared();
            }

            Destroy(other.gameObject);
        }
        else if (other.TryGetComponent(out Cherry cherry))
        {
            ScoreManager.Instance.AddScore(100);

            if (cherryClip != null)
            {
                cherryAudioSource.Play();
            }

            Destroy(other.gameObject);
        }
        else
        {
            HandleGhostContact(other);
        }
     }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!enabled)
        {
            return;
        }

        HandleGhostContact(other);
    }

    private void HandleGhostContact(Collider2D other)
    {
        if (!other.TryGetComponent(out GhostStateController ghost))
        {
            return;
        }

        if (ghost.CurrentState == GhostState.Dead)
        {
            return;
        }

        if (!ghost.IsVulnerable)
        {
            Die();
            return;
        }

        ScoreManager.Instance.AddScore(300);

        if (ghostEatClip != null)
        {
            ghostEatAudioSource.Play();
        }

        ghost.SetState(GhostState.Dead);

        if (GhostScaredManager.Instance != null)
        {
            GhostScaredManager.Instance.RefreshMusic();
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
