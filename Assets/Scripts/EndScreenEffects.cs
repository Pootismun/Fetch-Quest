using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class EndScreenEffects : MonoBehaviour
{
    public static EndScreenEffects Instance { get; private set; }

    [Header("Victory: Squirrels Falling")]
    [SerializeField] private float startDelay = 0.6f;
    [SerializeField] private float delayBetweenSquirrels = 0.35f;
    [SerializeField] private float hopHeight = 1.8f;
    [SerializeField] private float hopTime = 0.4f;
    [SerializeField] private float fallGravity = 40f;
    [SerializeField] private int fallSortingOrder = 20;

    [Header("Victory: Jumping Words")]
    [SerializeField] private TextWave messageWave;
    [SerializeField] private FireworksUI fireworks;

    [Header("Victory: Dog Celebration")]
    [SerializeField] private float dogStartDelay = 0.2f;
    [SerializeField] private int dogHops = 2;
    [SerializeField] private float dogHopHeight = 1.4f;
    [SerializeField] private float dogHopTime = 0.35f;
    [SerializeField] private float dogLeapStartSpeed = 8f;
    [SerializeField] private float dogLeapAcceleration = 45f;
    [SerializeField] private float dogLeapSpin = 540f;

    [Header("Victory: Dog Mascot")]
    [SerializeField] private RectTransform victoryDog;
    [SerializeField] private float mascotDelay = 1.5f;
    [SerializeField] private float mascotDropDistance = 700f;
    [SerializeField] private float mascotDropTime = 0.7f;
    [SerializeField] private float mascotHopHeight = 60f;
    [SerializeField] private float mascotHopPeriod = 0.55f;

    [Header("Defeat: Fight Cloud")]
    [SerializeField] private SpriteRenderer fightCloud;
    [SerializeField] private Sprite[] cloudFrames;
    [SerializeField] private float defeatStartDelay = 0.8f;
    [SerializeField] private float cloudFrameTime = 0.08f;
    [SerializeField] private float cloudShake = 0.08f;
    [SerializeField] private float cloudPopTime = 0.3f;
    [SerializeField] private float cloudBaseScale = 1.5f;
    [SerializeField] private int cloudSortingOrder = 20;
    [SerializeField] private float rushSpeed = 16f;
    [SerializeField] private float gatherRadius = 0.8f;
    [SerializeField] private float gatherPause = 0.3f;
    [SerializeField] private float cloudHoldTime = 1.2f;
    [SerializeField] private float cloudFallGravity = 30f;
    [SerializeField] private RectTransform defeatDog;
    [SerializeField] private float defeatDogDelay = 0.2f;
    [SerializeField] private float defeatDogDropDistance = 700f;
    [SerializeField] private float defeatDogDropTime = 0.5f;
    [SerializeField] private float defeatDogSquashTime = 0.3f;

    [Header("Defeat: Message")]
    [SerializeField] private float defeatWaveSpeed = 2f;
    [SerializeField] private Color defeatMessageColor = new Color(0.8f, 0.2f, 0.2f, 1f);

    [Header("Sound")]
    [SerializeField] private AudioClip squeakClip;
    [SerializeField] private float squeakVolume = 1f;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private float victoryVolume = 0.9f;
    [SerializeField] private AudioClip cloudClip;
    [SerializeField] private float cloudVolume = 1f;
    [SerializeField] private AudioClip defeatClip;
    [SerializeField] private float defeatVolume = 0.9f;

    private AudioSource audioSource;
    private bool dogGone;
    private Vector3 cloudCentre;
    private bool cloudShaking;
    private int squirrelsRushing;

    private void Awake()
    {
        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void PlayVictory()
    {
        if (fireworks != null)
        {
            fireworks.Play();
        }

        if (messageWave != null)
        {
            messageWave.enabled = true;
        }

        PacStudentController dog = FindAnyObjectByType<PacStudentController>();

        dogGone = dog == null;

        if (dog != null)
        {
            StartCoroutine(DogCelebration(dog));
        }

        if (victoryDog != null)
        {
            StartCoroutine(MascotRoutine());
        }

        GhostStateController[] squirrels = FindObjectsByType<GhostStateController>();

        if (victoryClip != null)
        {
            audioSource.PlayOneShot(victoryClip, victoryVolume);
        }

        for (int i = 0; i < squirrels.Length; i++)
        {
            squirrels[i].SetAnimationPaused(false);
            squirrels[i].SetState(GhostState.Dead);
            StartCoroutine(SquirrelFall(squirrels[i], startDelay + i * delayBetweenSquirrels));
        }
    }

    public IEnumerator FadeOutSounds(float duration)
    {
        if (fireworks != null)
        {
            fireworks.Stop();
        }

        float startVolume = audioSource.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float scale = 1f - Mathf.Clamp01(t / duration);

            audioSource.volume = startVolume * scale;

            if (fireworks != null)
            {
                fireworks.SetSoundScale(scale);
            }

            yield return null;
        }

        audioSource.volume = 0f;

        if (fireworks != null)
        {
            fireworks.SetSoundScale(0f);
        }
    }

    public void PlayDefeat()
    {
        if (messageWave != null)
        {
            messageWave.SetSpeed(defeatWaveSpeed);

            TMP_Text message = messageWave.GetComponent<TMP_Text>();
            message.color = defeatMessageColor;

            messageWave.enabled = true;
        }

        StartCoroutine(DefeatRoutine());
    }

    private IEnumerator DefeatRoutine()
    {
        if (fightCloud == null || cloudFrames == null || cloudFrames.Length == 0)
        {
            yield break;
        }

        PacStudentController dog = FindAnyObjectByType<PacStudentController>();
        GhostStateController[] squirrels = FindObjectsByType<GhostStateController>();

        if (dog != null)
        {
            dog.ShowDeadPose();
        }

        LevelGenerator level = FindAnyObjectByType<LevelGenerator>();

        Vector3 gatherPoint = dog != null ? dog.transform.position : Vector3.zero;
        gatherPoint.z = 0f;
        cloudCentre = gatherPoint;

        yield return new WaitForSecondsRealtime(defeatStartDelay);

        squirrelsRushing = 0;

        for (int i = 0; i < squirrels.Length; i++)
        {
            float angle = (45f + 90f * i) * Mathf.Deg2Rad;
            Vector3 spot = gatherPoint + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * gatherRadius;

            squirrels[i].SetState(GhostState.Normal);
            squirrels[i].SetAnimationPaused(false);
            List<Vector3> path = level != null ? FindRushPath(level, squirrels[i].transform.position, gatherPoint) : null;
            StartCoroutine(RushToDog(squirrels[i], spot, path));
        }

        while (squirrelsRushing > 0)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(gatherPause);

        fightCloud.sprite = cloudFrames[0];
        fightCloud.sortingOrder = cloudSortingOrder;
        fightCloud.transform.position = cloudCentre;
        fightCloud.transform.localScale = Vector3.zero;
        fightCloud.gameObject.SetActive(true);

        if (dog != null)
        {
            dog.gameObject.SetActive(false);
        }

        foreach (GhostStateController squirrel in squirrels)
        {
            squirrel.gameObject.SetActive(false);
        }

        cloudShaking = true;
        StartCoroutine(CloudLoop());

        if (cloudClip != null)
        {
            audioSource.PlayOneShot(cloudClip, cloudVolume);
        }

        float t = 0f;

        while (t < cloudPopTime)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float x = Mathf.Clamp01(t / cloudPopTime);
            fightCloud.transform.localScale = Vector3.one * cloudBaseScale * EaseOutBack(x);
            yield return null;
        }

        fightCloud.transform.localScale = Vector3.one * cloudBaseScale;

        yield return new WaitForSecondsRealtime(cloudHoldTime);

        Camera cam = Camera.main;
        float bottom = cam != null ? cam.transform.position.y - cam.orthographicSize - 3f : -30f;

        Vector3 pos = cloudCentre;
        float velocity = 0f;

        while (pos.y > bottom)
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            velocity += cloudFallGravity * dt;
            pos.y -= velocity * dt;
            cloudCentre = pos;
            yield return null;
        }

        cloudShaking = false;

        fightCloud.gameObject.SetActive(false);

        if (defeatDog != null)
        {
            yield return new WaitForSecondsRealtime(defeatDogDelay);
            yield return DefeatDogDrop();
        }
    }

    private IEnumerator DefeatDogDrop()
    {
        Vector2 rest = defeatDog.anchoredPosition;

        defeatDog.gameObject.SetActive(true);
        defeatDog.localScale = Vector3.one;

        float t = 0f;

        while (t < defeatDogDropTime)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float x = Mathf.Clamp01(t / defeatDogDropTime);
            defeatDog.anchoredPosition = rest + Vector2.up * Mathf.Lerp(defeatDogDropDistance, 0f, x * x);
            yield return null;
        }

        defeatDog.anchoredPosition = rest;

        if (defeatClip != null)
        {
            audioSource.PlayOneShot(defeatClip, defeatVolume);
        }

        t = 0f;

        while (t < defeatDogSquashTime)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float x = Mathf.Clamp01(t / defeatDogSquashTime);
            float squash = 1f - x;
            defeatDog.localScale = new Vector3(1f + 0.15f * squash, 1f - 0.25f * squash, 1f);
            yield return null;
        }

        defeatDog.localScale = Vector3.one;
    }

    private IEnumerator RushToDog(GhostStateController squirrel, Vector3 spot, List<Vector3> path)
    {  
        squirrelsRushing++;

        Transform body = squirrel.transform;
        float z = body.position.z;
        spot.z = z;

        List<Vector3> waypoints = new List<Vector3>();

        if (path != null)
        {
            foreach (Vector3 point in path)
            {
                Vector3 waypoint = point;
                waypoint.z = z;
                waypoints.Add(waypoint);
            }
        }

        waypoints.Add(spot);

        foreach (Vector3 waypoint in waypoints)
        {
            while (Vector3.Distance(body.position, waypoint) > 0.01f)
            {
                Vector3 direction = waypoint - body.position;

                if (direction.sqrMagnitude > 0.0001f)
                {
                    Vector2Int facing;

                    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                    {
                        facing = direction.x > 0f ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
                    }
                    else
                    {
                        facing = direction.y > 0f ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
                    }

                    squirrel.SetDirection(facing);
                }

                body.position = Vector3.MoveTowards(body.position, waypoint, rushSpeed * Mathf.Min(Time.unscaledDeltaTime, 0.05f));
                yield return null;
            }
        }

        squirrelsRushing--;
    }

    private bool CanRushThrough(LevelGenerator level, int row, int column)
    {
        if (level.IsWalkable(row, column))
        {
            return true;
        }

        bool isDoor = (row == LevelGenerator.HouseTopDoorRow || row == LevelGenerator.HouseBottomDoorRow)
            && column >= LevelGenerator.HouseDoorLeftColumn
            && column <= LevelGenerator.HouseDoorRightColumn;

        return isDoor;
    }

    private List<Vector3> FindRushPath(LevelGenerator level, Vector3 from, Vector3 to)
    {
        int rows = level.Rows;
        int columns = level.Columns;

        Vector2Int start = level.WorldToCell(from);
        Vector2Int goal = level.WorldToCell(to);

        if (start.y < 0 || start.y >= rows || start.x < 0 || start.x >= columns)
        {
            return null;
        }

        bool[,] visited = new bool[rows, columns];
        Vector2Int[,] parent = new Vector2Int[rows, columns];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Vector2Int[] steps = { new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };

        visited[start.y, start.x] = true;
        queue.Enqueue(start);

        bool found = false;

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();

            if (cell == goal)
            {
                found = true;
                break;
            }

            foreach (Vector2Int step in steps)
            {
                Vector2Int next = cell + step;

                if (next.y < 0 || next.y >= rows || next.x < 0 || next.x >= columns)
                {
                    continue;
                }

                if (visited[next.y, next.x] || !CanRushThrough(level, next.y, next.x))
                {
                    continue;
                }

                visited[next.y, next.x] = true;
                parent[next.y, next.x] = cell;
                queue.Enqueue(next);
            }
        }

        if (!found)
        {
            return null;
        }

        List<Vector3> path = new List<Vector3>();
        Vector2Int walk = goal;

        while (walk != start)
        {
            path.Add(level.CellToWorld(walk.y, walk.x));
            walk = parent[walk.y, walk.x];
        }

        path.Reverse();
        return path;
    }

    private IEnumerator CloudLoop()
    {
        float timer = 0f;
        int frame = 0;

        while (cloudShaking)
        {
            timer += Mathf.Min(Time.unscaledDeltaTime, 0.05f);

            if (timer >= cloudFrameTime)
            {
                timer = 0f;
                frame = (frame + 1) % cloudFrames.Length;
                fightCloud.sprite = cloudFrames[frame];
            }

            Vector2 jitter = Random.insideUnitCircle * cloudShake;
            fightCloud.transform.position = cloudCentre + new Vector3(jitter.x, jitter.y, 0f);
            yield return null;
        }
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    private IEnumerator MascotRoutine()
    {
        while (!dogGone)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(mascotDelay);

        Vector2 rest = victoryDog.anchoredPosition;
        victoryDog.gameObject.SetActive(true);

        float t = 0f;

        while (t < mascotDropTime)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float x = Mathf.Clamp01(t / mascotDropTime);
            float e = EaseOutBounce(x);
            victoryDog.anchoredPosition = rest + Vector2.up * Mathf.LerpUnclamped(mascotDropDistance, 0f, e);
            yield return null;
        }

        victoryDog.anchoredPosition = rest;

        const float squashPortion = 0.18f;
        float clock = 0f;

        while (true)
        {
            clock += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float u = (clock % mascotHopPeriod) / mascotHopPeriod;

            float height;
            float scaleX;
            float scaleY;

            if (u < squashPortion)
            {
                float s = Mathf.Sin(u / squashPortion * Mathf.PI);
                height = 0f;
                scaleX = 1f + 0.12f * s;
                scaleY = 1f - 0.18f * s;
            }
            else
            {
                float a = (u - squashPortion) / (1f - squashPortion);
                float s = Mathf.Sin(a * Mathf.PI);
                height = s * mascotHopHeight;
                scaleX = 1f - 0.06f * s;
                scaleY = 1f + 0.10f * s;
            }

            victoryDog.anchoredPosition = rest + Vector2.up * height;
            victoryDog.localScale = new Vector3(scaleX, scaleY, 1f);
            yield return null;
        }
    }

    private static float EaseOutBounce(float x)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (x < 1f / d1)
        {
            return n1 * x * x;
        }

        if (x < 2f / d1)
        {
            x -= 1.5f / d1;
            return n1 * x * x + 0.75f;
        }

        if (x < 2.5f / d1)
        {
            x -= 2.25f / d1;
            return n1 * x * x + 0.9375f;
        }

        x -= 2.625f / d1;
        return n1 * x * x + 0.984375f;
    }
    private IEnumerator DogCelebration(PacStudentController dog)
    {
        dog.ShowVictoryPose();

        yield return new WaitForSecondsRealtime(dogStartDelay);

        Transform body = dog.transform;
        SpriteRenderer sprite = dog.GetComponentInChildren<SpriteRenderer>();

        if (sprite != null)
        {
            sprite.sortingOrder = fallSortingOrder + 1;
            sprite.maskInteraction = SpriteMaskInteraction.None;
        }

        Vector3 start = body.position;

        for (int hop = 0; hop < dogHops; hop++)
        {
            float t = 0f;

            while (t < dogHopTime)
            {
                t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                float x = Mathf.Clamp01(t / dogHopTime);
                body.position = start + Vector3.up * Mathf.Sin(x * Mathf.PI) * dogHopHeight;
                yield return null;
            }
        }

        body.position = start;

        Camera cam = Camera.main;
        float top = cam != null ? cam.transform.position.y + cam.orthographicSize + 2f : 30f;

        Vector3 pos = body.position;
        float velocity = dogLeapStartSpeed;

        while (pos.y < top)
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            velocity += dogLeapAcceleration * dt;
            pos.y += velocity * dt;
            body.position = pos;
            body.Rotate(0f, 0f, dogLeapSpin * dt);
            yield return null;
        }

        dogGone = true;

        if (sprite != null)
        {
            sprite.enabled = false;
        }
        else
        {
            dog.gameObject.SetActive(false);
        }
    }

    private IEnumerator SquirrelFall(GhostStateController squirrel, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (squeakClip != null)
        {
            audioSource.PlayOneShot(squeakClip, squeakVolume);
        }

        Transform body = squirrel.transform;
        SpriteRenderer sprite = squirrel.GetComponentInChildren<SpriteRenderer>();

        if (sprite != null)
        {
            sprite.sortingOrder = fallSortingOrder;
        }

        Vector3 start = body.position;
        float t = 0f;

        while (t < hopTime)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float x = Mathf.Clamp01(t / hopTime);
            float height = Mathf.Sin(x * Mathf.PI * 0.5f) * hopHeight;
            body.position = start + Vector3.up * height;
            yield return null;
        }

        Camera cam = Camera.main;
        float bottom = cam != null ? cam.transform.position.y - cam.orthographicSize - 2f : -30f;

        Vector3 pos = body.position;
        float velocity = 0f;

        while (pos.y > bottom)
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            velocity += fallGravity * dt;
            pos.y -= velocity * dt;
            body.position = pos;
            yield return null;
        }

        squirrel.gameObject.SetActive(false);
    }
}
