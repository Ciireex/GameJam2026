using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AcidSplash : MonoBehaviour
{
    [SerializeField] private float healthDrainSpeedMultiplier = 3f;
    private ColorCurves curves;
    private Volume volume;
    private void Start()
    {
        GameObject volumeGO = GameObject.FindGameObjectWithTag("URP");
        volume = volumeGO.GetComponent<Volume>();
        volume.profile.TryGet(out curves);
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.Player.SetHealthDrainSpeedMultiplier(healthDrainSpeedMultiplier);
            if (curves != null)
                curves.active = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.Player.SetHealthDrainSpeedMultiplier(1f);
            if (curves != null)
                curves.active = false;
        }
    }
}
