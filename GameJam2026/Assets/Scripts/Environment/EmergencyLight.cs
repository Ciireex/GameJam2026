using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class EmergencyLight : MonoBehaviour
{
    private Light2D light2D;

    [Header("Intensity Settings")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;

    [Header("Radius Settings")]
    public float minRadius = 2f;
    public float maxRadius = 4f;

    [Header("Flicker Speed")]
    public float intensitySpeed = 1f; // speed of intensity change
    public float radiusSpeed = 1f;    // speed of radius change

    private float intensityOffset;
    private float radiusOffset;

    void Start()
    {
        light2D = GetComponent<Light2D>();

        // Random offsets so each light is different
        intensityOffset = Random.Range(0f, 100f);
        radiusOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        // Smooth intensity using Perlin noise
        float tIntensity = Mathf.PerlinNoise(Time.time * intensitySpeed, intensityOffset);
        light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, tIntensity);

        // Smooth radius using Perlin noise
        float tRadius = Mathf.PerlinNoise(Time.time * radiusSpeed, radiusOffset);
        light2D.pointLightOuterRadius = Mathf.Lerp(minRadius, maxRadius, tRadius);
    }
}
