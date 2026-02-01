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

    public void XFadeToMusicTrack(string clipName, string mixerGroup, float duration = 1f)
    {
        musicAudioSource.volume = 1f;
        StartCoroutine(Crossfade(musicAudioSource, clipName, mixerGroup, mixer, duration));
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
                if (musicAudioSource.clip != null && track.name == musicAudioSource.clip.name)
                    break;
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

    /*private IEnumerator XFadeTrack(AudioSource mainSource, string clipName, string targetMixerGroupName, bool keepPlaybackPosition, float duration = 1f)
    {
        foreach (AudioClip track in tracks)
        {
            if (track.name == clipName)
            {
                if (musicAudioSource.clip != null && track.name == musicAudioSource.clip.name)
                    break;
                GameObject secondTrack = new("xfade");
                AudioSource secondTrackSource = secondTrack.AddComponent<AudioSource>();
                secondTrackSource.clip = track;
                secondTrackSource.volume = 0f;
                secondTrackSource.outputAudioMixerGroup = mixer.FindMatchingGroups(targetMixerGroupName).FirstOrDefault();
                if (keepPlaybackPosition)
                    secondTrackSource.time = mainSource.time;
                secondTrackSource.Play();

                StartCoroutine(FadeVolumeToValue(mainSource, 0, duration));
                StartCoroutine(FadeVolumeToValue(secondTrackSource, 1f, duration));

                yield return new WaitForSeconds(duration);
                mainSource.Stop();
                mainSource.clip = track;
                mainSource.outputAudioMixerGroup = secondTrackSource.outputAudioMixerGroup;
                if (keepPlaybackPosition)
                    mainSource.time = secondTrackSource.time;
                mainSource.volume = 1f;
                mainSource.Play();
                Destroy(secondTrack);
                break;
            }
        }
    }*/

    public IEnumerator Crossfade(
    AudioSource mainSource,
    string clipName,
    string targetMixerGroupName,
    AudioMixer mixer,
    float duration)
    {
        foreach (AudioClip clip in tracks)
        {
            if (clip.name == clipName)
            {
                
                // Find mixer group by name
                AudioMixerGroup targetGroup = mixer
                    .FindMatchingGroups(targetMixerGroupName)
                    .FirstOrDefault();

                if (targetGroup == null)
                {
                    Debug.LogError($"Mixer group not found: {targetMixerGroupName}");
                    yield break;
                }

                // Create temp AudioSource
                GameObject tempObj = new GameObject("TempCrossfadeSource");
                AudioSource tempSource = tempObj.AddComponent<AudioSource>();

                // Configure temp source
                tempSource.clip = clip;
                tempSource.outputAudioMixerGroup = targetGroup;
                tempSource.loop = mainSource.loop;
                tempSource.spatialBlend = mainSource.spatialBlend;
                tempSource.playOnAwake = false;
                tempSource.volume = 0f;

                // Match playback position (sample-accurate is better)
                int startSample = mainSource.timeSamples;
                tempSource.timeSamples = Mathf.Min(startSample, clip.samples - 1);

                tempSource.Play();

                float t = 0f;
                float startVolume = mainSource.volume;

                while (t < duration)
                {
                    t += Time.deltaTime;
                    float n = t / duration;

                    // Equal-power fade
                    mainSource.volume = Mathf.Cos(n * Mathf.PI * 0.5f) * startVolume;
                    tempSource.volume = Mathf.Sin(n * Mathf.PI * 0.5f) * startVolume;

                    yield return null;
                }

                // Swap back into main source
                mainSource.Stop();
                mainSource.clip = clip;
                mainSource.outputAudioMixerGroup = targetGroup;
                mainSource.timeSamples = tempSource.timeSamples;
                mainSource.volume = startVolume;
                mainSource.Play();

                Destroy(tempObj);
            }
        }
    }




    public void PlaySFX(string sfxName, float volume = 1f, float pitch = 1f)
    {
        GameObject sfxGO = new(sfxName);
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
