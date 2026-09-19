using System.Collections;
using UnityEngine;

// Plays intro music, then loops the level soundtrack (squirrel normal music).
// Note for marker: the squirrels are the "ghosts".
[RequireComponent(typeof(AudioSource))]
public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private AudioClip introMusic;
    [SerializeField] private AudioClip squirrelNormalMusic;
    [SerializeField] private float maxIntroSeconds = 3f;

    private AudioSource musicSource;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (introMusic == null || squirrelNormalMusic == null)
        {
            Debug.LogError("For MusicController, check that both music clips are assigned in the inspector!!!", this);
            return;
        }

        StartCoroutine(PlayIntroThenNormalMusic());
    }

    private IEnumerator PlayIntroThenNormalMusic()
    {
        // Play the intro music once.
        musicSource.Stop();
        musicSource.loop = false;
        musicSource.clip = introMusic;
        musicSource.Play();

        // Wait until the intro clip ends or maxIntroSeconds passes, whichever is earliest.
        float waitTime = Mathf.Min(introMusic.length, maxIntroSeconds);
        yield return new WaitForSeconds(waitTime);

        // Switch to the squirrel normal music and loop it.
        musicSource.Stop();
        musicSource.clip = squirrelNormalMusic;
        musicSource.loop = true;
        musicSource.Play();
    }
}
