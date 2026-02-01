using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ShutdownReactor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject popUp;

    [Header("Lights to shut down (assign your reactor lights here)")]
    [SerializeField] private Light2D[] reactorLights;

    [Tooltip("Si tus luces tienen scripts tipo EmergencyLight que cambian la intensidad, arrástralos aquí para desactivarlos al apagar.")]
    [SerializeField] private MonoBehaviour[] lightControllersToDisable;

    [Header("Shutdown Settings")]
    [SerializeField] private float shutdownDuration = 2.0f;

    private bool isPlayerColliding = false;
    private bool isShuttingDown = false;

    private Player cachedPlayer;

    private InputAction interactAction;

    private void Awake()
    {
        interactAction = InputSystem.actions.FindAction("Player/Interact");
    }

    private void Update()
    {
        if (isShuttingDown) return;

        if (interactAction != null &&
            interactAction.WasPressedThisDynamicUpdate() &&
            isPlayerColliding &&
            cachedPlayer != null)
        {
            StartCoroutine(ShutdownSequence());
        }
    }

    private IEnumerator ShutdownSequence()
    {
        isShuttingDown = true;

        if (popUp != null)
            popUp.SetActive(false);

        // 1) Congelar player + quitar máscara visual + no perder vida
        cachedPlayer.SetControlFrozen(true);
        cachedPlayer.SetInvulnerable(true);
        cachedPlayer.ForceMaskOffVisualNoDrain();

        // 2) Si hay scripts que “pulsan” la luz, los apagamos para que no peleen con el lerp
        if (lightControllersToDisable != null)
        {
            for (int i = 0; i < lightControllersToDisable.Length; i++)
            {
                if (lightControllersToDisable[i] != null)
                    lightControllersToDisable[i].enabled = false;
            }
        }

        // 3) Guardar intensidades iniciales
        float[] startInt = null;
        if (reactorLights != null && reactorLights.Length > 0)
        {
            startInt = new float[reactorLights.Length];
            for (int i = 0; i < reactorLights.Length; i++)
            {
                if (reactorLights[i] != null)
                    startInt[i] = reactorLights[i].intensity;
            }
        }

        // 4) Apagado progresivo
        float t = 0f;
        shutdownDuration = Mathf.Max(0.01f, shutdownDuration);

        while (t < shutdownDuration)
        {
            float a = t / shutdownDuration;

            if (reactorLights != null)
            {
                for (int i = 0; i < reactorLights.Length; i++)
                {
                    if (reactorLights[i] == null) continue;
                    float si = (startInt != null) ? startInt[i] : reactorLights[i].intensity;
                    reactorLights[i].intensity = Mathf.Lerp(si, 0f, a);
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        // 5) Asegurar 0
        if (reactorLights != null)
        {
            for (int i = 0; i < reactorLights.Length; i++)
            {
                if (reactorLights[i] != null)
                    reactorLights[i].intensity = 0f;
            }
        }

        // Aquí podrías: activar algo, abrir puerta, marcar objetivo, etc.
        // De momento lo dejamos apagado y el player sigue congelado + sin máscara (como pediste).
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        cachedPlayer = collision.GetComponent<Player>();
        isPlayerColliding = true;

        if (!isShuttingDown && popUp != null)
            popUp.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        isPlayerColliding = false;
        cachedPlayer = null;

        if (popUp != null)
            popUp.SetActive(false);
    }
}
