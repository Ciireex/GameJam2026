using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Cinemachine;

public class ShutDownReactor : MonoBehaviour
{
    [Header("UI (Optional)")]
    [SerializeField] private GameObject popUp;

    [Header("Auto UI (if popUp is null)")]
    [SerializeField] private bool showAutoPrompt = true;
    [SerializeField] private string autoPromptText = "PULSA E";
    [SerializeField] private Vector2 autoPromptOffset = new Vector2(0f, -120f);
    [SerializeField] private int autoPromptFontSize = 40;

    [Header("Lights to shut down (assign your reactor lights here)")]
    [SerializeField] private Light2D[] reactorLights;

    [Tooltip("Si tus luces tienen scripts tipo EmergencyLight que cambian la intensidad, arrástralos aquí para desactivarlos al apagar.")]
    [SerializeField] private MonoBehaviour[] lightControllersToDisable;

    [Header("Shutdown Settings")]
    [SerializeField] private float shutdownDuration = 2.0f;

    [Header("After Shutdown")]
    [Tooltip("Índice de la escena del Main Menu en Build Settings.")]
    [SerializeField] private int mainMenuSceneIndex = 0;

    [Tooltip("Usar transición del SceneController (si existe).")]
    [SerializeField] private bool useTransition = true;

    //FinalAnimation
    [SerializeField] private CinemachineCamera vCam;
    [SerializeField] private float zoomDuration = 1f;  // how long the zoom takes
    [SerializeField] private float zoomFOV = .8f;        // target FOV for zoom
    private float originalFOV;


    private bool isPlayerColliding = false;
    private bool isShuttingDown = false;

    // No dependemos de la clase Player
    private GameObject cachedPlayerGO;
    private InputAction interactAction;

    private bool showPromptNow = false;

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
            cachedPlayerGO != null)
        {
            StartCoroutine(ShutdownSequence());
        }
    }

    private IEnumerator ShutdownSequence()
    {
        isShuttingDown = true;

        if (popUp != null)
            popUp.SetActive(false);

        showPromptNow = false;

        // Congelar player + invulnerable + máscara off (sin acoplar)
        cachedPlayerGO.SendMessage("SetControlFrozen", true, SendMessageOptions.DontRequireReceiver);
        cachedPlayerGO.SendMessage("SetInvulnerable", true, SendMessageOptions.DontRequireReceiver);
        cachedPlayerGO.SendMessage("ForceMaskOffVisualNoDrain", SendMessageOptions.DontRequireReceiver);

        cachedPlayerGO.SendMessage("LookAtCamera", SendMessageOptions.DontRequireReceiver);


        // Desactivar scripts de parpadeo
        if (lightControllersToDisable != null)
        {
            for (int i = 0; i < lightControllersToDisable.Length; i++)
            {
                if (lightControllersToDisable[i] != null)
                    lightControllersToDisable[i].enabled = false;
            }
        }

        // Guardar intensidades iniciales
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

        // Apagado progresivo
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

        // Asegurar 0 y ELIMINAR luces
        if (reactorLights != null)
        {
            for (int i = 0; i < reactorLights.Length; i++)
            {
                if (reactorLights[i] == null) continue;

                reactorLights[i].intensity = 0f;

                // Eliminar luz (más seguro que Destroy inmediato)
                reactorLights[i].gameObject.SetActive(false);
            }
        }

        yield return new WaitForSeconds(1f); // <-- wait  a second (adjust as needed)

        // Zoom a jugador
        yield return StartCoroutine(ZoomToPlayer());

        // Ir al Main Menu
        if (SceneController.Instance != null && useTransition)
        {
            SceneController.Instance.TransitionAndLoadScene(mainMenuSceneIndex, true, true);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneIndex);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        cachedPlayerGO = collision.gameObject;
        isPlayerColliding = true;

        if (!isShuttingDown)
        {
            if (popUp != null)
                popUp.SetActive(true);

            showPromptNow = (popUp == null && showAutoPrompt);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        isPlayerColliding = false;
        cachedPlayerGO = null;

        if (popUp != null)
            popUp.SetActive(false);

        showPromptNow = false;
    }

    // Texto simple "PULSA E" sin dependencias
    private void OnGUI()
    {
        if (!showPromptNow || isShuttingDown) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = autoPromptFontSize;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        float w = 600f;
        float h = 80f;

        Rect rect = new Rect(
            (Screen.width - w) * 0.5f + autoPromptOffset.x,
            (Screen.height - h) * 0.5f + autoPromptOffset.y,
            w,
            h
        );

        GUI.Label(rect, autoPromptText, style);
    }
    private IEnumerator ZoomToPlayer()
    {
        if (vCam == null)
            yield break;

        // Store original orthographic size
        originalFOV = vCam.Lens.OrthographicSize;

        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;

            // Smooth interpolation
            vCam.Lens.OrthographicSize = Mathf.Lerp(originalFOV, zoomFOV, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final size
        vCam.Lens.OrthographicSize = zoomFOV;
    }




}
