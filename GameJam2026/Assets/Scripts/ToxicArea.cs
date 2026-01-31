using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ToxicArea : MonoBehaviour
{
    [SerializeField] private float healthDrainSpeedMultiplier = 3f;
    private ColorCurves curves;
    private Volume volume;
    private void Start()
    {
        GameObject volumeGO = GameObject.FindGameObjectWithTag("URP");
        volume = volumeGO.GetComponent<Volume>();
        volume.profile.TryGet(out curves);
        Debug.Log(curves);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!GameManager.Instance.Player.IsMaskOn())
            {
                GameManager.Instance.Player.SetHealthDrainSpeedMultiplier(healthDrainSpeedMultiplier);
                if (curves != null)
                    curves.active = true;
            }
            else {
                if (curves != null)
                    curves.active = false;
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
        }
    }
}
