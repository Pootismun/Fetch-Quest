using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MapFrame : MonoBehaviour
{
    private IEnumerator Start()
    {
        LevelGenerator generator = FindAnyObjectByType<LevelGenerator>();

        while (generator == null || generator.Columns == 0)
        {
            yield return null;
            generator = FindAnyObjectByType<LevelGenerator>();
        }

        SpriteRenderer frame = GetComponent<SpriteRenderer>();

        if (frame.sprite == null)
        {
            yield break;
        }

        float thickness = frame.sprite.border.x / frame.sprite.pixelsPerUnit;

        frame.drawMode = SpriteDrawMode.Sliced;
        frame.size = new Vector2(generator.Columns + 2f * thickness, generator.Rows + 2f * thickness);
        transform.position = Vector3.zero;
    }
}
