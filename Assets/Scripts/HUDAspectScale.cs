using UnityEngine;

public class HUDAspectScale : MonoBehaviour
{
    [SerializeField] private float narrowAspect = 1.33f;
    [SerializeField] private float wideAspect = 1.78f;
    [SerializeField] private float narrowScale = 1f;
    [SerializeField] private float wideScale = 1.6f;

    private void LateUpdate()
    {
        float aspect = (float)Screen.width / Screen.height;
        float t = Mathf.InverseLerp(narrowAspect, wideAspect, aspect);
        float scale = Mathf.Lerp(narrowScale, wideScale, t);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
