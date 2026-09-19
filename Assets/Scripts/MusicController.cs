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

    // Small delay so both clips are scheduled before they need to start.
    private const double StartDelay = 0.05;
    private AudioSource introSource;
    private AudioSource loopSource;

    private void Awake()
    {
        introSource = GetComponent<AudioSource>();

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.volume = introSource.volume;
        loopSource.outputAudioMixerGroup = introSource.outputAudioMixerGroup;
    }

    private void Start()
    {
        if (introMusic == null || squirrelNormalMusic == null)
        {
            Debug.LogError("For MusicController, check that both music clips are assigned in the inspector!!!", this);
            return;
        }

        // Intro length in seconds.
        double introClipLength = (double)introMusic.samples / introMusic.frequency;

        // Intro plays until clip ends or maxIntroSeconds passes, whichever is earliest.
        double introPlayTime = System.Math.Min(introClipLength, maxIntroSeconds);

        // Both start times use the audio clock so the switch between audios has no gap.
        double introStart = AudioSettings.dspTime + StartDelay;
        double loopStart = introStart + introPlayTime;

        introSource.clip = introMusic;
        introSource.loop = false;
        introSource.PlayScheduled(introStart);

        if (introClipLength > maxIntroSeconds)
        {
            introSource.SetScheduledEndTime(loopStart);
        }

        loopSource.clip = squirrelNormalMusic;
        loopSource.loop = true;
        loopSource.PlayScheduled(loopStart);
    }
}
