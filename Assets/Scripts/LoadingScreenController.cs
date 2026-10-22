using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private TextWave wave;
    [SerializeField] private float minimumShowTime = 2f;

    private void Start()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadTarget());
    }

    private IEnumerator LoadTarget()
    {
        while (wave != null && !wave.IsResting)
        {
            yield return null;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneLoader.TargetScene);

        if (operation == null)
        {
            yield break;
        }

        operation.allowSceneActivation = false;

        float elapsed = 0f;

        while (operation.progress < 0.9f || elapsed < minimumShowTime || (wave != null && !wave.IsResting))
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            yield return null;
        }

        operation.allowSceneActivation = true;
    }
}

