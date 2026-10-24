using UnityEngine;

public class CherryController : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private float respawnDelay = 5f;
    [SerializeField] private float edgeMargin = 1.5f;
    [SerializeField] private LevelGenerator levelGenerator;

    [Header("Movement")]
    [SerializeField] private float tilesPerSecond = 5f;

    [Header("Rendering")]
    [SerializeField] private int sortingOrder = 100;

    private GameObject activeCherry;
    private Vector3 direction;
    private float distanceTraveled;
    private float totalTravelDistance;
    private float timer;
    private bool wasActive;

    private void Awake()
    {
        if (levelGenerator == null)
        {
            levelGenerator = FindAnyObjectByType<LevelGenerator>();
        }
    }

    private void Start()
    {
        timer = respawnDelay;
    }

    private void Update()
    {
        if (activeCherry == null)
        {
            if (wasActive)
            {
                timer = respawnDelay;
                wasActive = false;
            }

            timer -= Time.deltaTime;
            if(timer <= 0f)
            {
                Spawn();
            }
            return;
        }

        wasActive = true;
        Move();
    }
    
    private void Spawn()
    {
        if (levelGenerator == null || levelGenerator.Rows <= 0 || levelGenerator.Columns <= 0)
        {
            timer = respawnDelay;
            return;
        }

        float halfWidth = levelGenerator.Columns / 2f;
        float halfHeight = levelGenerator.Rows / 2f;

        Vector3 spawnPosition = RandomEdgePosition(halfWidth, halfHeight);

        activeCherry = Instantiate(cherryPrefab, spawnPosition, Quaternion.identity);
        activeCherry.AddComponent<Cherry>();

        SpriteRenderer cherryRenderer = activeCherry.GetComponent<SpriteRenderer>();
        cherryRenderer.sortingOrder = sortingOrder;
        cherryRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        direction = (Vector3.zero - spawnPosition).normalized;
        distanceTraveled = 0f;
        totalTravelDistance = spawnPosition.magnitude * 2f + edgeMargin;
    }

    private Vector3 RandomEdgePosition(float halfWidth, float halfHeight)
    {
        int edge = Random.Range(0, 4);

        switch (edge)
        {
            case 0:
                return new Vector3(Random.Range(-halfWidth, halfWidth), halfHeight + edgeMargin, 0f);
            case 1:
                return new Vector3(halfWidth + edgeMargin, Random.Range(-halfHeight, halfHeight), 0f);
            case 2:
                return new Vector3(Random.Range(-halfWidth, halfWidth), -halfHeight - edgeMargin, 0f);
            default:
                return new Vector3(-halfWidth - edgeMargin, Random.Range(-halfHeight, halfHeight), 0f);
        }
    }

    private void Move()
    {
        float step = Time.deltaTime * tilesPerSecond;
        distanceTraveled += step;
        activeCherry.transform.position += direction * step;

        if (distanceTraveled >= totalTravelDistance)
        {
            Destroy(activeCherry);
            activeCherry = null;
        }
    }
}
