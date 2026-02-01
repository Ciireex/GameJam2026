using UnityEngine;

public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Spike collided with: " + collision.name);
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Spike collided with: " + collision.name);
            GameManager.Instance.Player.Kill();
        }
    }
}
