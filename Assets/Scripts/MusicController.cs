using UnityEngine;

// Plays intro music during the round start countdown, then loops the level soundtrack (squirrel normal music).
// Note for marker: the squirrels are the "ghosts".
[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip introMusic;
    [SerializeField] private AudioClip squirrelNormalMusic;
    [SerializeField] private AudioClip squirrelScaredMusic;
    [SerializeField] private AudioClip squirrelDeadMusic;

    [Header("Volumes (applied when the track starts)")]
    [SerializeField, Range(0f, 1f)] private float introVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float normalVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float scaredVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float deadVolume = 1f;

    private AudioSource introSource;
    private AudioSource loopSource;

    private float masterVolume;

    private float normalMusicTime;

    private void Awake()
    {
        introSource = GetComponent<AudioSource>();
        introSource.playOnAwake = false;
        masterVolume = introSource.volume;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.volume = masterVolume;
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
        introSource.volume = masterVolume * introVolume;
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
        PlayLoop(squirrelScaredMusic, 0f, scaredVolume);
    }

    public void PlayDeadMusic()
    {
        PlayLoop(squirrelDeadMusic, 0f, deadVolume);
    }

    public void PlayNormalMusic()
    {
        PlayLoop(squirrelNormalMusic, normalMusicTime, normalVolume);
    }

    private void PlayLoop(AudioClip clip, float startTime, float trackVolume)
    {
        if (clip == null || loopSource.clip == clip)
        {
            return;
        }

        // Remember where the normal track was before something else takes over.
        if (loopSource.clip != null && loopSource.clip == squirrelNormalMusic)
        {
            normalMusicTime = loopSource.time;
        }

        loopSource.clip = clip;
        loopSource.loop = true;
        loopSource.volume = masterVolume * trackVolume;
        loopSource.time = startTime;
        loopSource.Play();
    }
}
