using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource musicAudioSource;
    public AudioSource MusicAudioSource => musicAudioSource;
    [SerializeField] private AudioMixer mixer;
    public AudioMixer AudioMixer => mixer;
    [SerializeField] private AudioClip[] tracks;
    [SerializeField] private AudioClip[] sfx;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeMusicTrack(string clipName, string mixerGroupName, bool keepPlaybackPosition, bool fadeOut)
    {
        StartCoroutine(ChangeTrackCoroutine(musicAudioSource, clipName, mixerGroupName, keepPlaybackPosition, fadeOut));
    }

    public IEnumerator FadeVolumeToValue(AudioSource source, float endValue, float time)
    {
        float elapsedTime = 0f;
        float startValue = musicAudioSource.volume;
        while (elapsedTime < time)
        {
            source.volume = Mathf.Lerp(startValue, endValue, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
            source.volume = endValue;
    }

    public IEnumerator ChangeTrackCoroutine(AudioSource source, string clipName, string mixerGroupName, bool keepPlaybackPosition, bool fadeOut)
    {
        float playbackPosition = 0f;
        foreach (AudioClip track in tracks)
        {
            if (track.name == clipName)
            {
                if (fadeOut)
                    yield return StartCoroutine(FadeVolumeToValue(source, 0, 0.5f));
                source.Stop();
                if (keepPlaybackPosition && source.clip != null)
                    playbackPosition = source.time;
                source.volume = 1f;
                source.clip = track;
                source.outputAudioMixerGroup = mixer.FindMatchingGroups(mixerGroupName).FirstOrDefault();
                if (keepPlaybackPosition)
                    source.time = playbackPosition;
                source.Play(); 
                break;
            }
        }
    }

    public IEnumerator XFadeTrack(AudioSource mainSource, string clipName, string targetMixerGroupName, float duration = 1f)
    {
        foreach (AudioClip track in tracks)
        {
            if (track.name == clipName)
            {
                GameObject secondTrack = Instantiate(new GameObject("xfade"));
                AudioSource secondTrackSource = secondTrack.AddComponent<AudioSource>();
                secondTrackSource.clip = track;
                secondTrackSource.volume = 0f;
                secondTrackSource.outputAudioMixerGroup = mixer.FindMatchingGroups(targetMixerGroupName).FirstOrDefault();

                StartCoroutine(FadeVolumeToValue(mainSource, 0, duration));
                StartCoroutine(FadeVolumeToValue(secondTrackSource, 1f, duration));

                yield return new WaitForSeconds(duration);
                mainSource.Stop();
                mainSource.clip = track;
                mainSource.outputAudioMixerGroup = secondTrackSource.outputAudioMixerGroup;
                mainSource.time = secondTrackSource.time;
                mainSource.volume = 1f;
                mainSource.Play();
                Destroy(secondTrack);
                break;
            }
        }
    }

    public void PlaySFX(string sfxName, float volume = 1f, float pitch = 1f)
    {
        GameObject sfxGO = Instantiate(new GameObject(sfxName));
        AudioSource sfxAudioSource = sfxGO.AddComponent<AudioSource>();
        foreach (AudioClip clip in sfx)
        {
            if (clip.name == sfxName)
            {
                sfxAudioSource.clip = clip;
                sfxAudioSource.volume = volume;
                sfxAudioSource.pitch = pitch;
                sfxAudioSource.loop = false;
                sfxAudioSource.Play();
            }
        }
        if (sfxAudioSource.clip)
        {
            Destroy(sfxAudioSource.gameObject, sfxAudioSource.clip.length);
        }
        else
        {
            Destroy(sfxAudioSource.gameObject);
        }
    }
}
