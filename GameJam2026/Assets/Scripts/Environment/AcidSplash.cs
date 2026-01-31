using UnityEngine;

public class AcidSplash : MonoBehaviour
{
    [SerializeField] private float healthDrainSpeedMultiplier = 3f;
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.Player.SetHealthDrainSpeedMultiplier(healthDrainSpeedMultiplier);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.Player.SetHealthDrainSpeedMultiplier(1f);
        }
    }
}
