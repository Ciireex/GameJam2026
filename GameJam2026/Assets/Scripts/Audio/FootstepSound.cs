using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    void PlayFootstepSound()
    {
        GetComponent<AudioSource>().Play();
    }
}
