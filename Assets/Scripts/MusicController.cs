using UnityEngine;

// Plays intro music during the round start countdown, then loops the level soundtrack (squirrel normal music).
// Note for marker: the squirrels are the "ghosts".
[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    [SerializeField] private AudioClip introMusic;
    [SerializeField] private AudioClip squirrelNormalMusic;
    [SerializeField] private AudioClip squirrelScaredMusic;

    private AudioSource introSource;
    private AudioSource loopSource;

    private float normalMusicTime;

    private void Awake()
    {
        introSource = GetComponent<AudioSource>();
        introSource.playOnAwake = false;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.volume = introSource.volume;
        loopSource.outputAudioMixerGroup = introSource.outputAudioMixerGroup;
    }

    public void PlayIntroMusic()
    {
        if (introMusic == null)
        {
            Debug.LogError("For MusicController, check that both music clips are assigned in the inspector!!!", this);
            return;
        }

        introSource.clip = introMusic;
        introSource.loop = false;
        introSource.Play();
    }

    public void StopIntroMusic()
    {
        introSource.Stop();
    }

    public void StopAllMusic()
    {
        introSource.Stop();
        loopSource.Stop();
    }

    public void PlayScaredMusic()
    {
        if (squirrelScaredMusic == null || loopSource.clip == squirrelScaredMusic)
        {
            return;
        }

        normalMusicTime = loopSource.time;

        loopSource.clip = squirrelScaredMusic;
        loopSource.loop = true;
        loopSource.time = 0f;
        loopSource.Play();
    }

    public void PlayNormalMusic()
    {
        if (squirrelNormalMusic == null || loopSource.clip == squirrelNormalMusic)
        {
            return;
        }

        loopSource.clip = squirrelNormalMusic;
        loopSource.loop = true;
        loopSource.time = normalMusicTime;
        loopSource.Play();
    }
}
