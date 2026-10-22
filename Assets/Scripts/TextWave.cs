using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextWave : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 25f;
    [SerializeField] private float hopDuration = 0.35f;
    [SerializeField] private float delayPerCharacter = 0.07f;
    [SerializeField] private float restDuration = 0.5f;

    private TMP_Text text;
    private float clock;

    public bool IsResting { get; private set; }

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        clock = 0f;
    }

    private void LateUpdate()
    {
        text.ForceMeshUpdate();
        TMP_TextInfo info = text.textInfo;

        float hopsFinishAt = Mathf.Max(0, info.characterCount - 1) * delayPerCharacter + hopDuration;
        float cycleLength = hopsFinishAt + restDuration;
        clock += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        float phase = clock % cycleLength;

        IsResting = phase >= hopsFinishAt;

        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo character = info.characterInfo[i];

            if (!character.isVisible)
            {
                continue;
            }

            float t = phase - i * delayPerCharacter;
            float hop = 0f;

            if (t > 0f && t < hopDuration)
            {
                float x = t / hopDuration;
                hop = Mathf.Abs(Mathf.Sin(x * Mathf.PI * 2f)) * (1f - x);

            }

            Vector3 offset = new Vector3(0f, hop * jumpHeight, 0f);

            Vector3[] vertices = info.meshInfo[character.materialReferenceIndex].vertices;

            for (int v = 0; v < 4; v++)
            {
            vertices[character.vertexIndex + v] += offset;
            }
        }

        for (int m = 0; m<info.meshInfo.Length; m++)
        {
            info.meshInfo[m].mesh.vertices = info.meshInfo[m].vertices;
            text.UpdateGeometry(info.meshInfo[m].mesh, m);
        }
    }
}
