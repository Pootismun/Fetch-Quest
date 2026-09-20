using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

//Editor Tool for map tile placement.

public static class LevelLayoutBuilder
{
    private const string LevelSprites = "Assets/Sprites/Level/";
    private const string PickupSprites = "Assets/Sprites/Pickups/";
    private const string PowerPelletController = "Assets/Animations/Controllers/PowerPelletAnimator.controller";

    // NOTE: Tile row 0, column 0 sits at x = -13.5, y = 14.
    // Each column should move 1 unit to the right and each row moves 1 unit down.
    private const float FirstColumnX = -13.5f;
    private const float FirstRowY = 14f;

    private const int WallOrder = 0;
    private const int PickupOrder = 1;

    // NOTE: Two characters per tile, 14 tiles per row, 15 rows.
    // ..: Empty tile
    // pp: Pellet
    // PP: Power Pellet
    // A: Outside Corner, B: Outside Wall, C: Inside Corner
    // D: Inside Wall, T: T Junction, G: Squirrel Exit Wall
    // Digit after a wall is how many times the piece is turned 90 deg.
    // Anticlockwise, so its Z rotation is digit x 90.
    private static readonly string[] TopLeftQuadrant =
    {
        "A0 B0 B0 B0 B0 B0 B0 B0 B0 B0 B0 B0 B0 T0",
        "B1 pp pp pp pp pp pp pp pp pp pp pp pp D1",
        "B1 pp C0 D0 D0 C3 pp C0 D0 D0 D0 C3 pp D1",
        "B1 PP D1 .. .. D1 pp D1 .. .. .. D1 pp D1",
        "B1 pp C1 D0 D0 C2 pp C1 D0 D0 D0 C2 pp C1",
        "B1 pp pp pp pp pp pp pp pp pp pp pp pp pp",
        "B1 pp C0 D0 D0 C3 pp C0 C3 pp C0 D0 D0 D0",
        "B1 pp C1 D0 D0 C2 pp D1 D1 pp C1 D0 D0 C3",
        "B1 pp pp pp pp pp pp D1 D1 pp pp pp pp D1",
        "A1 B0 B0 B0 B0 A3 pp D1 C1 D0 D0 C3 .. D1",
        ".. .. .. .. .. B1 pp D1 C0 D0 D0 C2 .. C1",
        ".. .. .. .. .. B1 pp D1 D1 .. .. .. .. ..",
        ".. .. .. .. .. B1 pp D1 D1 .. C0 D0 D0 G0",
        "B0 B0 B0 B0 B0 A2 pp C1 C2 .. D1 .. .. ..",
        ".. .. .. .. .. .. pp .. .. .. D1 .. .. .."
    };

    [MenuItem("Tools/Fetch Quest/Build Level Quadrant")]
    public static void Build()
    {
        try
        {
            GameObject level = GameObject.Find("Level01");
            if (level == null)
            {
                throw new Exception("There is no object called Level01 in the open scene. Open Level01Scene first.");
            }

            RemoveChild(level.transform, "Walls");
            RemoveChild(level.transform, "Pellets");
            RemoveChild(level.transform, "Quadrant_TopLeft");

            Transform quadrant = NewGroup("Quadrant_TopLeft", level.transform);
            Transform walls = NewGroup("Walls", quadrant);
            Transform pellets = NewGroup("Pellets", quadrant);

            int wallCount = 0;
            int pelletCount = 0;

            for (int row = 0; row < TopLeftQuadrant.Length; row++)
            {
                string[] tokens = TopLeftQuadrant[row].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length !=14)
                {
                    throw new Exception("Row " + row + " has " + tokens.Length + " tiles. It needs 14.");
                }

                for (int col = 0; col < tokens.Length; col++)
                {
                    string token = tokens[col];
                    Vector3 position = new Vector3(FirstColumnX + col, FirstRowY - row, 0f);

                    if (token == "..")
                    {
                        continue;
                    }

                    if (token == "pp")
                    {
                        CreateTile("Pellet_" + row + "_" + col, PickupSprites + "Pickup_Pellet.png", pellets, position, 0, PickupOrder);
                        pelletCount++;
                    }
                    else if (token == "PP")
                    {
                        GameObject power = CreateTile("PowerPellet_" + row + "_" + col, PickupSprites + "Pickup_PowerPellet_1.png", pellets, position, 0, PickupOrder);
                        Animator animator = power.AddComponent<Animator>();
                        animator.runtimeAnimatorController =
                            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PowerPelletController);
                        pelletCount++;
                    }
                    else
                    {
                        string pieceName;
                        string spriteFile;
                        WallInfo(token, row, col, out pieceName, out spriteFile);
                        int turns = token[1] - '0';
                        CreateTile(pieceName + "_" + row + "_" + col, LevelSprites + spriteFile, walls, position, turns, WallOrder);
                        wallCount++;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Selection.activeGameObject = quadrant.gameObject;
            Debug.Log("LevelLayoutBuilder: placed " + wallCount + " wall pieces and " + pelletCount + " pellets in Quadrant_TopLeft.");
        }
        catch (Exception e)
        {
            Debug.LogError("LevelLayoutBuilder failed: " + e.Message);
        }
    }

    private static void WallInfo(string token, int row, int col, out string pieceName, out string spriteFile)
    {
        if (token.Length !=2 || token[1] < '0' || token[1] > '3')
        {
            throw new Exception("Bad tile " + token + " at row " + row + ", coloumn " + col + ".");
        }

        switch (token[0])
        {
            case 'A': pieceName = "OutsideCorner"; spriteFile = "Wall_OutsideCorner.png"; break;
            case 'B': pieceName = "OutsideWall"; spriteFile = "Wall_OutsideWall.png"; break;
            case 'C': pieceName = "InsideCorner"; spriteFile = "Wall_InsideCorner.png"; break;
            case 'D': pieceName = "InsideWall"; spriteFile = "Wall_InsideWall.png"; break;
            case 'T': pieceName = "TJunction"; spriteFile = "Wall_TJunction.png"; break;
            case 'G': pieceName = "SquirrelExit"; spriteFile = "Wall_SquirrelExit.png"; break;
            default:
                throw new Exception("Unknown tile '" + token + "' at row " + row + ", column " + col + ".");
        }
    }

    private static GameObject CreateTile(string objectName, string spritePath, Transform parent, Vector3 position, int turns, int sortingOrder)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            throw new Exception("Sprite not found: " + spritePath);
        }

        var tile = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(tile, "Build level quadrant");
        tile.transform.SetParent(parent, false);
        tile.transform.localPosition = position;
        tile.transform.localRotation = Quaternion.Euler(0f, 0f, 90f * turns);

        SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = sortingOrder;
        return tile;
    }

    private static Transform NewGroup(string groupName, Transform parent)
    {
        var group = new GameObject(groupName);
        Undo.RegisterCreatedObjectUndo(group, "Build level quadrant");
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private static void RemoveChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            Undo.DestroyObjectImmediate(child.gameObject);
        }
    }
}

