using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GhostStateController))]
public class GhostController : MonoBehaviour
{
    [SerializeField] private int ghostNumber = 1;
    [SerializeField] private LevelGenerator levelGenerator;

    private const float NormalSpeedMultiplier = 0.9f;
    private const float ScaredSpeedMultiplier = 0.5f;

    private const float OutwardBias = 0.5f;

    private static readonly Vector2Int Up = new Vector2Int(0, -1);
    private static readonly Vector2Int Down = new Vector2Int(0, 1);
    private static readonly Vector2Int Left = new Vector2Int(-1, 0);
    private static readonly Vector2Int Right = new Vector2Int(1, 0);
    private static readonly Vector2Int None = Vector2Int.zero;
    private static readonly Vector2Int[] AllDirections = { Up, Right, Down, Left };

    private GhostStateController stateController;
    private PacStudentController dog;
    private float dogTilesPerSecond = 5f;

    private Vector2Int homeCell;
    private Vector2Int currentCell;
    private Vector2Int targetCell;
    private Vector2Int lastDirection = None;
    private float lerpT;
    private bool isMoving;
    private bool initialized;

    private bool leavingHouse;
    private readonly Queue<Vector2Int> exitPath = new Queue<Vector2Int>();

    //Ghost 4 only
    private bool followingWall;
    private GhostState previousState = GhostState.Normal;

    public int GhostNumber => ghostNumber;
    public Vector2Int CurrentCell => currentCell;
    public Vector2Int LastDirection => lastDirection;

    private void Awake()
    {
        stateController = GetComponent<GhostStateController>();
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

        dog = FindAnyObjectByType<PacStudentController>();
        if (dog != null)
        {
            dogTilesPerSecond = dog.TilesPerSecond;
        }

        currentCell = levelGenerator.WorldToCell(transform.position);
        transform.position = levelGenerator.CellToWorld(currentCell.y, currentCell.x);
        targetCell = currentCell;
        homeCell = currentCell;

        BuildExitPath();
        leavingHouse = exitPath.Count > 0;

        initialized = true;
    }

    private void BuildExitPath()
    {
        exitPath.Clear();

        if (!levelGenerator.IsInsideGhostHouse(currentCell.y, currentCell.x))
        {
            return;
        }

        bool exitTop = ghostNumber == 1 || ghostNumber == 3;
        int laneRow = exitTop ? LevelGenerator.HouseTopRow : LevelGenerator.HouseBottomRow;
        int doorRow = exitTop ? LevelGenerator.HouseTopDoorRow : LevelGenerator.HouseBottomDoorRow;
        int outsideRow = exitTop ? doorRow - 1 : doorRow + 1;
        int doorColumn = currentCell.x <= LevelGenerator.HouseDoorLeftColumn
            ? LevelGenerator.HouseDoorLeftColumn
            : LevelGenerator.HouseDoorRightColumn;

        int row = currentCell.y;
        int column = currentCell.x;

        while (row != laneRow)
        {
            row += laneRow > row ? 1 : -1;
            exitPath.Enqueue(new Vector2Int(column, row));
        }

        while (column != doorColumn)
        {
            column += doorColumn > column ? 1 : -1;
            exitPath.Enqueue(new Vector2Int(column, row));
        }

        exitPath.Enqueue(new Vector2Int(column, doorRow));
        exitPath.Enqueue(new Vector2Int(column, outsideRow));
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

        GhostState state = stateController.CurrentState;

        if (state == GhostState.Dead)
        {
            if (previousState != GhostState.Dead)
            {
                EnterDead();
            }

            previousState = state;
            MoveDead();
            return;
        }

        if (state == GhostState.Scared && previousState == GhostState.Normal)
        {
            ReverseNow();
        }

        previousState = state;

        if (!isMoving)
        {
            StartNextMove();
        }

        if (isMoving)
        {
            AdvanceMove();
        }
    }

    private void EnterDead()
    {
        isMoving = false;
        leavingHouse = false;
        followingWall = false;
        exitPath.Clear();
    }

    private void MoveDead()
    {
        Vector3 homePosition = levelGenerator.CellToWorld(homeCell.y, homeCell.x);

        transform.position = Vector3.MoveTowards(
            transform.position,
            homePosition,
            CurrentSpeed() * Time.deltaTime);

        if (Vector3.Distance(transform.position, homePosition) <= 0.001f)
        {
            Respawn(homePosition);
        }
    }

    private void Respawn(Vector3 homePosition)
    {
        transform.position = homePosition;
        currentCell = homeCell;
        targetCell = homeCell;
        lastDirection = None;
        lerpT = 0f;
        isMoving = false;
        followingWall = false;

        BuildExitPath();
        leavingHouse = exitPath.Count > 0;

        if (GhostScaredManager.Instance != null)
        {
            stateController.SetState(GhostScaredManager.Instance.GlobalState);
            GhostScaredManager.Instance.RefreshMusic();
        }
        else
        {
            stateController.SetState(GhostState.Normal);
        }
    }

    public void ResetToStart()
    {
        if (!initialized)
        {
            return;
        }

        transform.position = levelGenerator.CellToWorld(homeCell.y, homeCell.x);
        currentCell = homeCell;
        targetCell = homeCell;
        lastDirection = None;
        lerpT = 0f;
        isMoving = false;
        followingWall = false;
        previousState = GhostState.Normal;

        BuildExitPath();
        leavingHouse = exitPath.Count > 0;

        stateController.SetState(GhostState.Normal);
    }

    // I know reversing isn't allowed according to the brief but it doesn't make sense
    // for the squirrels to not run away when the power pellet is eaten. ! Note for Marker
    private void ReverseNow()
    {
        if (!isMoving || leavingHouse)
        {
            return;
        }

        if (dog != null)
        {
            Vector3 dogPosition = dog.transform.position;
            Vector3 ahead = levelGenerator.CellToWorld(targetCell.y, targetCell.x);
            Vector3 behind = levelGenerator.CellToWorld(currentCell.y, currentCell.x);

            if (Vector3.Distance(behind, dogPosition) <= Vector3.Distance(ahead, dogPosition))
            {
                return;
            }
        }

        Vector2Int oldCurrent = currentCell;
        currentCell = targetCell;
        targetCell = oldCurrent;

        lerpT = 1f - lerpT;
        lastDirection = targetCell - currentCell;
        stateController.SetDirection(lastDirection);
    }

    private float CurrentSpeed()
    {
        float normalSpeed = dogTilesPerSecond * NormalSpeedMultiplier;

        if (stateController.IsVulnerable || stateController.CurrentState == GhostState.Dead)
        {
            return normalSpeed * ScaredSpeedMultiplier;
        }

        return normalSpeed;
    }

    private void StartNextMove()
    {
        if (leavingHouse)
        {
            if (exitPath.Count > 0)
            {
                BeginMove(exitPath.Dequeue());
                return;
            }

            leavingHouse = false;
        }

        Vector2Int direction = ChooseDirection();
        if (direction == None)
        {
            return;
        }

        BeginMove(currentCell + direction);
    }

    private void BeginMove(Vector2Int nextCell)
    {
        targetCell = nextCell;
        lastDirection = nextCell - currentCell;
        stateController.SetDirection(lastDirection);
        lerpT = 0f;
        isMoving = true;
    }

    private void AdvanceMove()
    {
        lerpT += Time.deltaTime * CurrentSpeed();

        while (lerpT >= 1f && isMoving)
        {
            currentCell = targetCell;
            transform.position = levelGenerator.CellToWorld(currentCell.y, currentCell.x);
            isMoving = false;

            float overshoot = lerpT - 1f;

            StartNextMove();

            lerpT = isMoving ? overshoot : 0f;
        }

        if (isMoving)
        {
            Vector3 from = levelGenerator.CellToWorld(currentCell.y, currentCell.x);
            Vector3 to = levelGenerator.CellToWorld(targetCell.y, targetCell.x);
            transform.position = Vector3.Lerp(from, to, lerpT);
        }
    }

    private Vector2Int ChooseDirection()
    {
        List<Vector2Int> options = GetValidDirections();

        if (options.Count == 0)
        {
            return None;
        }

        if (stateController.IsVulnerable)
        {
            followingWall = false;
            return ChooseByDistanceToDog(options, true);
        }

        switch (ghostNumber)
        {
            case 1:
                return ChooseByDistanceToDog(options, true);
            case 2:
                return ChooseByDistanceToDog(options, false);
            case 4:
                return ChooseClockwise(options);
            default:
                return ChooseRandom(options);
        }
    }

    private Vector2Int ChooseRandom(List<Vector2Int> options)
    {
        return options[Random.Range(0, options.Count)];
    }

    private Vector2Int ChooseByDistanceToDog(List<Vector2Int> options, bool wantFurther)
    {
        if (dog == null)
        {
            return ChooseRandom(options);
        }

        Vector3 dogPosition = dog.transform.position;
        Vector3 here = levelGenerator.CellToWorld(currentCell.y, currentCell.x);
        float currentDistance = Vector3.Distance(here, dogPosition);

        List<Vector2Int> matching = new List<Vector2Int>();

        foreach (Vector2Int direction in options)
        {
            Vector2Int next = currentCell + direction;
            Vector3 nextPosition = levelGenerator.CellToWorld(next.y, next.x);
            float nextDistance = Vector3.Distance(nextPosition, dogPosition);

            bool fits = wantFurther ? nextDistance >= currentDistance : nextDistance <= currentDistance;
            if (fits)
            {
                matching.Add(direction);
            }
        }

        if (matching.Count == 0)
        {
            return ChooseRandom(options);
        }

        return ChooseRandom(matching);
    }

    private Vector2Int ChooseClockwise(List<Vector2Int> options)
    {
        if (!followingWall && lastDirection != None)
        {
            Vector2Int leftCell = currentCell + TurnLeft(lastDirection);
            if (levelGenerator.IsOuterWall(leftCell.y, leftCell.x))
            {
                followingWall = true;
            }
        }

        if (followingWall)
        {
            Vector2Int[] preference = { TurnLeft(lastDirection), lastDirection, TurnRight(lastDirection) };

            foreach (Vector2Int direction in preference)
            {
                if (options.Contains(direction))
                {
                    return direction;
                }
            }

            return options[0];
        }

        return ChooseTowardOutside(options);
    }

    private Vector2Int ChooseTowardOutside(List<Vector2Int> options)
    {
        Vector2 center = new Vector2((levelGenerator.Columns - 1) / 2f, (levelGenerator.Rows - 1) / 2f);
        Vector2 offset = new Vector2(currentCell.x, currentCell.y) - center;
        Vector2 outward = offset.normalized;

        // Clockwise on screen, where Y increases downwards in grid coordinates.
        Vector2 clockwise = new Vector2(-outward.y, outward.x);

        Vector2Int best = options[0];
        float bestScore = float.MinValue;

        foreach (Vector2Int direction in options)
        {
            Vector2 step = direction;
            float score = Vector2.Dot(step, clockwise) + OutwardBias * Vector2.Dot(step, outward);

            if (score > bestScore)
            {
                bestScore = score;
                best = direction;
            }
        }

        return best;
    }

    private static Vector2Int TurnLeft(Vector2Int direction)
    {
        return new Vector2Int(direction.y, -direction.x);
    }

    private static Vector2Int TurnRight(Vector2Int direction)
    {
        return new Vector2Int(-direction.y, direction.x);
    }

    // Valid directions are walkable for ghosts and are not a back-step.
    // A back-step is only allowed when there is no other choice (a dead end).
    private List<Vector2Int> GetValidDirections()
    {
        List<Vector2Int> options = new List<Vector2Int>();

        foreach (Vector2Int direction in AllDirections)
        {
            if (direction == -lastDirection)
            {
                continue;
            }

            Vector2Int cell = currentCell + direction;
            if (levelGenerator.IsGhostWalkable(cell.y, cell.x))
            {
                options.Add(direction);
            }
        }

        if (options.Count == 0 && lastDirection != None)
        {
            Vector2Int reverse = -lastDirection;
            Vector2Int cell = currentCell + reverse;
            if (levelGenerator.IsGhostWalkable(cell.y, cell.x))
            {
                options.Add(reverse);
            }
        }

        return options;
    }
}
