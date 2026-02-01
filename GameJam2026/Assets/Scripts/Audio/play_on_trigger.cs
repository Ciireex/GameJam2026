using UnityEngine;

public class PlayExternalAudioOnTrigger2D : MonoBehaviour
{
    public AudioSource externalAudioSource;   // Assigna aquí l'AudioSource extern
    public string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (externalAudioSource != null)
            {
                externalAudioSource.Play();
            }
            else
            {
                Debug.LogWarning("No hi ha cap AudioSource extern assignat!");
            }
        }
    }
}
