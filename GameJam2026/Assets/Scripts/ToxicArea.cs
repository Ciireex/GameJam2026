using UnityEngine;

public class ToxicArea : MonoBehaviour
{
    [SerializeField] private float healthDrainSpeedMultiplier = 3f;
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!GameManager.Instance.Player.IsMaskOn())
                GameManager.Instance.Player.SetHealthDrainSpeedMultiplier(healthDrainSpeedMultiplier);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.Player.SetHealthDrainSpeedMultiplier(1f);
        }
    }
}
