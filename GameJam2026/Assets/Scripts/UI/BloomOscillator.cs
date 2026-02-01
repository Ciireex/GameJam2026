using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloomOscillator : MonoBehaviour
{
    [Header("Bloom Settings")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float minIntensity = 0.1f;
    [SerializeField] private float maxIntensity = 1f;

    private Bloom bloom;

    void Start()
    {
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume not assigned!");
            return;
        }

        // Try to get the Bloom component from the Volume
        if (!globalVolume.profile.TryGet<Bloom>(out bloom))
        {
            Debug.LogError("Bloom not found in Volume profile!");
        }
    }

    void Update()
    {
        if (bloom == null) return;

        // Oscillate between 0 and 1 using PingPong, then remap to min/max
        float t = Mathf.PingPong(Time.time * speed, 1f);
        bloom.intensity.value = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}
