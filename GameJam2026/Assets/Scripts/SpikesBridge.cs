using UnityEngine;
using System.Collections;

public class SpikesBridge : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float startDelay = 15f;
    [SerializeField] private float spikeInterval = 0.5f;  // Time between spikes
    [SerializeField] private float spikeLifetime = 5f;
    [SerializeField] private GameObject[] spikes; // Assign all spikes in Inspector

    private void Start()
    {
        foreach (var spike in spikes)
        {
            if (spike != null)
                spike.SetActive(false);
        }

        StartCoroutine(TriggerSpikesRoutine());
    }

    private IEnumerator TriggerSpikesRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (true) // Infinite loop
        {
            for (int i = 0; i < spikes.Length; i++)
            {
                ActivateSpike(spikes[i]);
                yield return new WaitForSeconds(spikeInterval);
            }

            // Optional wait before repeating
            yield return new WaitForSeconds(2f);
        }
    }


    private void ActivateSpike(GameObject spike)
    {
        spike.SetActive(true);
        StartCoroutine(DeactivateSpikeAfterTime(spike));
    }

    private IEnumerator DeactivateSpikeAfterTime(GameObject spike)
    {
        yield return new WaitForSeconds(spikeLifetime);
        spike.SetActive(false);
    }


}
