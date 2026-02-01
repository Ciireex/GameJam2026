using UnityEngine;
using UnityEngine.InputSystem;

public class Reactor : MonoBehaviour
{
    [SerializeField] private GameObject popUp;
    [SerializeField] private int sceneIndexToLoad;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openStateName = "Open";
    [SerializeField] private string openTriggerName = "OpenDoor";

    private bool isPlayerColliding = false;
    private bool isOpening = false;

    private InputAction interactAction;

    private GameObject cachedPlayerGO;


    private void Awake()
    {
        // Cachea la acci�n para no hacer FindAction cada frame
        interactAction = InputSystem.actions.FindAction("Player/Interact");
    }

    private void Update()
    {
        if (isOpening) return;

        if (interactAction != null &&
            interactAction.WasPressedThisDynamicUpdate() &&
            isPlayerColliding)
        {
            StartCoroutine(OpenThenLoad());
        }
    }

    private System.Collections.IEnumerator OpenThenLoad()
    {
        isOpening = true;

        cachedPlayerGO.SendMessage("SetControlFrozen", true, SendMessageOptions.DontRequireReceiver);
        cachedPlayerGO.SendMessage("SetInvulnerable", true, SendMessageOptions.DontRequireReceiver);
        cachedPlayerGO.SendMessage("ForceMaskOffVisualNoDrain", SendMessageOptions.DontRequireReceiver);

        popUp.SetActive(false);

        if (animator != null)
        {
            animator.ResetTrigger(openTriggerName);
            animator.SetTrigger(openTriggerName);

            // Espera a que entre en el estado Open
            yield return null;
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(openStateName))
                yield return null;

            // Espera a que termine la animaci�n (normalizedTime >= 1)
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }

        GameManager.Instance.SceneController.TransitionAndLoadScene(sceneIndexToLoad, true, true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            cachedPlayerGO = collision.gameObject;
            if (!isOpening) popUp.SetActive(true);
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
