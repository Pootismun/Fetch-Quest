using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject powerPelletPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite outsideCorner;
    [SerializeField] private Sprite outsideWall;
    [SerializeField] private Sprite insideCorner;
    [SerializeField] private Sprite insideWall;
    [SerializeField] private Sprite tJunction;
    [SerializeField] private Sprite ghostExitWall;
    [SerializeField] private Sprite pellet;
    [SerializeField] private Sprite floor;
    [SerializeField] private Sprite bonusItem;

    [Header("Bonus Item")]
    [Tooltip("The level is centred on (0, 0) and one unit is one tile.")]
    [SerializeField] private Vector2 bonusPosition = new Vector2(0f, -3f);

    // 0: Empty, 1: Outside Corner, 2: Outside Wall, 3: Inside Corner
    // 4: Inside Wall, 5: Pellet, 6: Power Pellet, 7: T Junction, 8: Ghost Exit Wall
    private int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };
    //Linking
    private const int Up = 1;
    private const int Right = 2;
    private const int Down = 4;
    private const int Left = 8;

    private static readonly int[] Directions = { Up, Right, Down, Left };
    private static readonly int[] RowStep = { -1, 0, 1, 0 };
    private static readonly int[] ColumnStep = { 0, 1, 0, -1 };

    private const float CameraMargin = 2f;

    private int rows;   // Size of full level
    private int columns;
    private int[,] map; // Full level after mirroring
    private int[,] links; // Which directions each wall piece links to
    private bool[,] linksKnown;

    private void Start()
    {
        if (!HasEverythingAssigned())
        {
            return;
        }

        GameObject manualLevel = GameObject.Find("Level01");
        if (manualLevel != null)
        {
            Destroy(manualLevel);
        }

        BuildFullMap();
        WorkOutLinks();
        SpawnLevel();
        FitCamera();
    }

    // Mirroring quadrants
    private void BuildFullMap()
    {
        int quarterRows = levelMap.GetLength(0);
        int quarterColumns = levelMap.GetLength(1);

        rows = quarterRows * 2 - 1;
        columns = quarterColumns * 2;

        map = new int[rows, columns];
        for (int r = 0; r < rows; r++)
        {
            int sourceRow = r < quarterRows ? r : rows - 1 - r;
            for (int c = 0; c < columns; c++)
            {
                int sourceColumn = c < quarterColumns ? c : columns - 1 - c;
                map[r, c] = levelMap[sourceRow, sourceColumn];
            }
        }
    }

    // Wall Connection Calculation
    private void WorkOutLinks()
    {
        links = new int[rows, columns];
        linksKnown = new bool[rows, columns];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int type = map[r, c];
                if (type == 2 || type == 4 || type == 8)
                {
                    int sideways = (IsWall(r, c - 1) ? 1 : 0) + (IsWall(r, c + 1) ? 1 : 0);
                    int vertical = (IsWall(r - 1, c) ? 1 : 0) + (IsWall(r + 1, c) ? 1 : 0);
                    links[r, c] = vertical > sideways ? Up | Down : Left | Right;
                    linksKnown[r, c] = true;
                }
            }
        }
        // Corner & Junction Calculation
        bool progress = true;
        while (progress)
        {
            progress = false;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (!NeedsLinks(r, c) || linksKnown[r, c])
                    {
                        continue;
                    }

                    int needed = map[r, c] == 7 ? 3 : 2;
                    int forced;
                    int possible;
                    LookAround(r, c, out forced, out possible);

                    if (CountBits(forced) == needed)
                    {
                        links[r, c] = forced;
                    }
                    else if (CountBits(possible) == needed)
                    {
                        links[r, c] = possible;
                    }
                    else
                    {
                        continue;
                    }

                    linksKnown[r, c] = true;
                    progress = true;
                }
            }
        }

        // Removes odd map pieces
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (!NeedsLinks(r, c) || linksKnown[r, c])
                {
                    continue;
                }

                int needed = map[r, c] == 7 ? 3 : 2;
                int forced;
                int possible;
                LookAround(r, c, out forced, out possible);

                int chosen = forced;
                foreach (int direction in Directions)
                {
                    if (CountBits(chosen) >= needed)
                    {
                        break;
                    }
                    if ((possible & direction) != 0)
                    {
                        chosen |= direction;
                    }
                }
                links[r, c] = chosen;
                linksKnown[r, c] = true;
            }
        }
    }

    private void LookAround(int r, int c, out int forced, out int possible)
    {
        forced = 0;
        possible = 0;

        for (int i = 0; i < 4; i++)
        {
            int neighbourRow = r + RowStep[i];
            int neighbourColumn = c + ColumnStep[i];
            if (!IsWall(neighbourRow, neighbourColumn))
            {
                continue;
            }

            int direction = Directions[i];
            if (linksKnown[neighbourRow, neighbourColumn])
            {
                if ((links[neighbourRow, neighbourColumn] & Opposite(direction)) != 0)
                {
                    forced |= direction;
                    possible |= direction;
                }
            }
            else
            {
                possible |= direction;
            }
        }
    }

    private void SpawnLevel()
    {
        GameObject root = new GameObject("GeneratedLevel");
        Transform walls = NewGroup("Walls", root.transform);
        Transform pellets = NewGroup("Pellets", root.transform);

        GameObject floorObject = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, root.transform);

        floorObject.name = "Floor";
        SpriteRenderer floorRenderer = floorObject.GetComponent<SpriteRenderer>();
        floorRenderer.sprite = floor;
        floorRenderer.drawMode = SpriteDrawMode.Tiled;
        floorRenderer.size = new Vector2(columns, rows);
        floorRenderer.sortingOrder = -10;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int type = map[r, c];
                Vector3 position = new Vector3(c - (columns - 1) / 2f, (rows - 1) / 2f - r, 0f);

                if (type == 5)
                {
                    PlacePiece(pellet, position, 0f, 1, pellets, r, c);
                }
                else if (type == 6)
                {
                    GameObject power = Instantiate(powerPelletPrefab, position, Quaternion.identity, pellets);
                    power.name = "PowerPellet_" + r + "_" + c;
                }
                else if (IsWall(r, c))
                {
                    PlacePiece(SpriteFor(type), position, RotationFor(type, links[r, c]), 0, walls, r, c);
                }
            }
        }

        PlaceBonusItem(root.transform);
    }

    private void PlaceBonusItem(Transform levelRoot)
    {
        int row = Mathf.RoundToInt((rows - 1) / 2f - bonusPosition.y);
        int column = Mathf.RoundToInt(bonusPosition.x + (columns - 1) / 2f);
        if (IsWall(row, column))
        {
            return;
        }

        Transform bonus = NewGroup("Bonus", levelRoot);
        GameObject bonusObject = Instantiate(tilePrefab, new Vector3(bonusPosition.x, bonusPosition.y, 0f), Quaternion.identity, bonus);
        bonusObject.name = bonusItem.name;

        SpriteRenderer bonusRenderer = bonusObject.GetComponent<SpriteRenderer>();
        bonusRenderer.sprite = bonusItem;
        bonusRenderer.sortingOrder = 1;
    }

    private void PlacePiece(Sprite sprite, Vector3 position, float angle, int sortingOrder, Transform parent, int r, int c)
    {
        GameObject piece = Instantiate(tilePrefab, position, Quaternion.Euler(0f, 0f, angle), parent);
        piece.name = sprite.name + "_" + r + "_" + c;

        SpriteRenderer spriteRenderer = piece.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = sortingOrder;
    }

    private Sprite SpriteFor(int type)
    {
        switch (type)
        {
            case 1: return outsideCorner;
            case 2: return outsideWall;
            case 3: return insideCorner;
            case 4: return insideWall;
            case 7: return tJunction;
            default: return ghostExitWall;
        }
    }

    // Sprite Rotation
    private float RotationFor(int type, int pieceLinks)
    {
        if (type == 2 || type == 4 || type == 8)
        {
            return pieceLinks == (Left | Right) ? 0f : 90f;
        }

        if (type == 1 || type == 3)
        {
            if (pieceLinks == (Right | Down)) return 0f;
            if (pieceLinks == (Up | Right)) return 90f;
            if (pieceLinks == (Left | Up)) return 180f;
            if (pieceLinks == (Down | Left)) return 270f;
            return 0f;
        }

        if (pieceLinks == (Left | Right | Down)) return 0f;
        if (pieceLinks == (Down | Up | Right)) return 90f;
        if (pieceLinks == (Right | Left | Up)) return 180f;
        if (pieceLinks == (Up | Down | Left)) return 270f;
        return 0f;
    }

    // Camera Control

    private void FitCamera()
    {
        Camera levelCamera = Camera.main;
        if (levelCamera == null)
        {
            return;
        }

        levelCamera.orthographic = true;
        levelCamera.transform.position = new Vector3(0f, 0f, -10f);

        // Fit to Screen
        float halfHeight = rows / 2f + CameraMargin;
        float halfWidth = columns / 2f + CameraMargin;
        levelCamera.orthographicSize = Mathf.Max(halfHeight, halfWidth / levelCamera.aspect);
    }

    // Small Helpers
    private bool IsWall(int r, int c)
    {
        if (r < 0 || r >= rows || c < 0 || c >= columns)
        {
            return false;
        }

        int type = map[r, c];
        return type == 1 || type == 2 || type == 3 || type == 4 || type == 7 || type == 8;
    }

    private bool NeedsLinks(int r, int c)
    {
        int type = map[r, c];
        return type == 1 || type == 3 || type == 7;
    }

    private static int Opposite(int direction)
    {
        return ((direction << 2) | (direction >> 2)) & 15;
    }

    private static int CountBits(int value)
    {
        int count = 0;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }
        return count;
    }

    private static Transform NewGroup(string groupName, Transform parent)
    {
        GameObject group = new GameObject(groupName);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private bool HasEverythingAssigned()
    {
        if (tilePrefab == null || powerPelletPrefab == null || outsideCorner == null || outsideWall == null || insideCorner == null || insideWall == null || tJunction == null || ghostExitWall == null || pellet == null || floor == null || bonusItem == null)
        {
            Debug.LogError("LevelGenerator, assign every prefab and sprite in the inspector.", this);
            return false;
        }
        return true;
    }
}
