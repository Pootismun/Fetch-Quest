using UnityEngine;
using UnityEngine.UI;

public class LoadingTileGrid : MonoBehaviour
{
    [SerializeField] private Sprite image;
    [SerializeField] private int columns = 16;
    [SerializeField] private int rows = 9;

    private RawImage[,] tiles;

    public int Columns => columns;
    public int Rows => rows;

    private void Awake()
    {
        Build();
    }

    private void Build()
    {
        tiles = new RawImage[rows, columns];
        Texture texture = image.texture;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                GameObject tile = new GameObject("Tile_" + r + "_" + c, typeof(RectTransform), typeof(RawImage));
                tile.transform.SetParent(transform, false);

                float left = c / (float)columns;
                float right = (c + 1) / (float)columns;
                float bottom = 1f - (r + 1) / (float)rows;
                float top = 1f - r / (float)rows;

                RectTransform rect = tile.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(left, bottom);
                rect.anchorMax = new Vector2(right, top);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                RawImage raw = tile.GetComponent<RawImage>();
                raw.texture = texture;
                raw.uvRect = new Rect(left, bottom, 1f / columns, 1f / rows);

                tiles[r, c] = raw;
            }
        }
    }

    public void SetTileVisible(int row, int column, bool visible)
    {
        tiles[row, column].enabled = visible;
    }

    public void SetAllVisible(bool visible)
    {
        foreach (RawImage tile in tiles)
        {
            tile.enabled = visible;
        }
    }
}
