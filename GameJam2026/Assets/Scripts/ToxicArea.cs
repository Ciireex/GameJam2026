using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ToxicArea : MonoBehaviour
{
    [SerializeField] private float healthDrainSpeedMultiplier = 3f;
    private ColorCurves curves;
    private Volume volume;
    private AudioSource audioSource;
    [SerializeField] private AudioClip geigerSFX;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        GameObject volumeGO = GameObject.FindGameObjectWithTag("URP");
        volume = volumeGO.GetComponent<Volume>();
        volume.profile.TryGet(out curves);
        audioSource.clip = geigerSFX;
        Debug.Log(curves);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        audioSource.enabled = true;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (!GameManager.Instance.Player.IsMaskOn())
            {
                GameManager.Instance.Player.SetHealthDrainSpeedMultiplier(healthDrainSpeedMultiplier);
                if (curves != null)
                    curves.active = true;
                audioSource.outputAudioMixerGroup = null;
            }
            else {
                if (curves != null)
                    curves.active = false;
                if (audioSource.outputAudioMixerGroup == null)
                {
                    AudioMixerGroup mixerGroup = AudioManager.Instance.AudioMixer.FindMatchingGroups("SFX").FirstOrDefault();
                    if (mixerGroup != null)
                        audioSource.outputAudioMixerGroup = mixerGroup;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.Player.SetHealthDrainSpeedMultiplier(1f);
            if (curves != null)
                curves.active = false;
            audioSource.enabled = false;
        }
    }
}
