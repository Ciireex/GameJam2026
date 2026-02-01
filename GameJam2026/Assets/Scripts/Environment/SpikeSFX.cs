using UnityEngine;

public class SpikeSFX : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip sfx;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    void Start()
    {
        audioSource.clip = sfx;
        audioSource.pitch = Random.Range(0.8f, 1f);
        audioSource.Play();
        Destroy(gameObject, sfx.length);
    }
}
