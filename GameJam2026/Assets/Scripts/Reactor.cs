using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Reactor : MonoBehaviour
{
    [SerializeField] private GameObject popUp;
    [SerializeField] private int sceneIndexToLoad;
    private bool isPlayerColliding = false;


    private void Update()
    {
        if (InputSystem.actions.FindAction("Player/Interact").WasPressedThisDynamicUpdate() && isPlayerColliding)
        {
            GameManager.Instance.SceneController.TransitionAndLoadScene(sceneIndexToLoad);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            popUp.SetActive(true);
            isPlayerColliding = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            popUp.SetActive(false);
            isPlayerColliding = false;
        }
    }
}
