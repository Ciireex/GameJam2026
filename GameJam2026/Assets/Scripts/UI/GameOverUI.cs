using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.Player.OnPlayerDeath += Player_OnPlayerDeath;
    }

    private void Player_OnPlayerDeath(object sender, System.EventArgs e)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }
}
