using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxHealthTime = 15f;
    [SerializeField] private HealthSlider healthSlider;
    [SerializeField] private float healthDrainSpeedMultiplier = 1f;

    [Header("Spawn / Respawn")]
    [Tooltip("DEPRECATED: Ya no se usa. El respawn recarga la escena con SceneController.")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 1f;

    [Header("Fall")]
    [SerializeField] private float fallDuration = 0.6f;
    [SerializeField] private float fallEndScale = 0.05f;

    [Header("Lighting")]
    [SerializeField] private Light2D spotLight;
    [Range(0, 30)][SerializeField] private float maskOnRadius = 1f;
    [Range(0, 30)][SerializeField] private float maskOffRadius = 15f;
    [SerializeField] private float changeMaskAnimationDuration = 0.5f;

    [Header("Start Intro (Wipe + Light)")]
    [Tooltip("Duración aproximada del wipe AnimateIn() para esperar antes de animar la luz.")]
    [SerializeField] private float wipeInDuration = 0.6f;

    [Header("Death Outro (Wipe Out)")]
    [Tooltip("Tiempo que tarda el wipe AnimateOut() antes de recargar la escena.")]
    [SerializeField] private float wipeOutDuration = 0.6f;

    [Header("Intro Timing")]
    [Tooltip("Delay entre la luz abierta (máscara quitada) y volver a máscara puesta.")]
    [SerializeField] private float delayBeforeMaskBack = 1f;

    [Header("PlayerVisual")]
    [SerializeField] private Transform playerVisual;

    [Header("Wipe transition")]
    public WipeController wipeEffect;

    private bool isFalling;
    private Vector3 originalVisualScale;

    private bool isDead;
    public event EventHandler OnPlayerDeath;

    private float currentHealthTime;
    private bool isMaskOn = true;
    private bool countdownActive;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private bool isRespawning;

    // para no acumular coroutines de luz
    private Coroutine lightRoutine;

    // evita que el jugador pueda spamear máscara durante la intro
    private bool introPlaying;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerVisual == null)
        {
            Transform t = transform.Find("PlayerVisual");
            if (t != null) playerVisual = t;
        }

        if (playerVisual != null)
            originalVisualScale = playerVisual.localScale;

        currentHealthTime = maxHealthTime;

        healthSlider.SetMaxValue(maxHealthTime);
        healthSlider.SetValue(currentHealthTime);

        GameInput.Instance.OnChangeMaskAction += GameInput_OnChangeMaskAction;
        GameManager.Instance.OnTimeIsUp += GameManager_OnTimeIsUp;

        if (spawnPoint == null)
        {
            GameObject sp = new GameObject("SpawnPoint_Runtime");
            sp.transform.position = transform.position;
            sp.transform.rotation = transform.rotation;
            spawnPoint = sp.transform;
        }

        // Estado inicial: máscara puesta (normal) y sin contador.
        isMaskOn = true;
        countdownActive = false;

        // Asegura luz en estado inicial ANTES de la intro
        if (spotLight != null)
            spotLight.pointLightOuterRadius = maskOffRadius;

        // Intro: Wipe + animación de luz (abre, espera y vuelve a cerrar)
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        introPlaying = true;

        // 1) Wipe in
        if (wipeEffect != null)
            wipeEffect.AnimateIn();

        if (wipeInDuration > 0f)
            yield return new WaitForSeconds(wipeInDuration);

        // 2) Abre luz
        yield return AnimateLightTo(maskOffRadius, changeMaskAnimationDuration);

        // 3) Delay
        if (delayBeforeMaskBack > 0f)
            yield return new WaitForSeconds(delayBeforeMaskBack);

        // 4) Vuelve a máscara puesta
        isMaskOn = true;
        yield return AnimateLightTo(maskOnRadius, changeMaskAnimationDuration);

        if (spotLight != null)
            spotLight.pointLightOuterRadius = maskOnRadius;

        introPlaying = false;
    }

    private void Update()
    {
        if (!isDead && !isFalling)
        {
            moveInput = GameInput.Instance.GetMovmentVectorNormalized();
        }
        else
        {
            moveInput = Vector2.zero;
        }

        HandleHealthCountdown();
    }

    private void FixedUpdate()
    {
        if (isDead || isFalling)
            return;

        Vector2 newPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    private void HandleHealthCountdown()
    {
        if (!countdownActive || isDead)
            return;

        currentHealthTime -= Time.deltaTime * healthDrainSpeedMultiplier;
        healthSlider.SetValue(currentHealthTime);

        if (currentHealthTime <= 0f)
        {
            currentHealthTime = 0f;
            Kill();
        }
    }

    private void GameManager_OnTimeIsUp(object sender, EventArgs e)
    {
        Debug.Log("uaaaaaa");
        Kill();
    }

    private void GameInput_OnChangeMaskAction(object sender, EventArgs e)
    {
        if (introPlaying || isDead || isFalling)
            return;

        ToggleMask();
        ToggleCountdown();

        UpdateLightRadius();

        if (IsMaskOn())
            healthSlider.SetShaderShake(0.1f);
        else
            healthSlider.SetShaderShake(1f);

        Debug.Log("Mask is " + IsMaskOn());
    }

    public bool IsMaskOn()
    {
        return isMaskOn;
    }

    public void ToggleMask()
    {
        isMaskOn = !isMaskOn;
    }

    private void ToggleCountdown()
    {
        countdownActive = !countdownActive;
    }

    private void UpdateLightRadius()
    {
        float targetOuterRadius = IsMaskOn() ? maskOnRadius : maskOffRadius;

        if (lightRoutine != null)
            StopCoroutine(lightRoutine);

        lightRoutine = StartCoroutine(ChangeLightRadiusCoroutine(targetOuterRadius, changeMaskAnimationDuration));
    }

    private IEnumerator AnimateLightTo(float targetOuterRadius, float duration)
    {
        if (lightRoutine != null)
        {
            StopCoroutine(lightRoutine);
            lightRoutine = null;
        }

        yield return ChangeLightRadiusCoroutine(targetOuterRadius, duration);
    }

    private IEnumerator ChangeLightRadiusCoroutine(float targetOuterRadius, float duration)
    {
        if (spotLight == null)
            yield break;

        float elapsedTime = 0f;
        float startRadius = spotLight.pointLightOuterRadius;

        duration = Mathf.Max(0.0001f, duration);

        while (elapsedTime < duration)
        {
            spotLight.pointLightOuterRadius = Mathf.Lerp(startRadius, targetOuterRadius, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spotLight.pointLightOuterRadius = targetOuterRadius;
    }

    public void SetHealthDrainSpeedMultiplier(float value)
    {
        healthDrainSpeedMultiplier = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("FallingTilemap"))
        {
            FallAndDie();
        }
        if (other.CompareTag("Spike"))
        {
            Kill();
        }
    }

    public void Kill()
    {
        if (isDead)
            return;

        isDead = true;
        countdownActive = false;

        Debug.Log("Player has died");

        OnPlayerDeath?.Invoke(this, EventArgs.Empty);

        if (!isRespawning)
            StartCoroutine(RespawnRoutine());
    }

    public void FallAndDie()
    {
        if (isDead || isFalling)
            return;

        StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        isFalling = true;
        countdownActive = false;

        Transform visual = playerVisual != null ? playerVisual : transform;

        Vector3 startScale = visual.localScale;
        Vector3 endScale;

        if (playerVisual != null)
            endScale = originalVisualScale * fallEndScale;
        else
            endScale = startScale * fallEndScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, fallDuration);
            visual.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        visual.localScale = endScale;

        isFalling = false;
        Kill();
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // Desactivar movimiento/colisiones durante el respawn
        if (rb != null) rb.simulated = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 1) Dispara el wipe OUT al morir
        if (wipeEffect != null)
            wipeEffect.AnimateOut();

        // 2) Espera el tiempo del wipe out (o respawnDelay, lo que quieras usar)
        float waitTime = Mathf.Max(respawnDelay, wipeOutDuration);
        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        // 3) Recarga escena actual por índice (con SceneController si existe)
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (SceneController.Instance != null)
        {
            SceneController.Instance.TransitionAndLoadScene(sceneIndex);
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }

        // No hace falta reactivar nada: se recarga la escena
    }

    public void SetDrainMultiplier(float multiplier)
    {
        healthDrainSpeedMultiplier = multiplier;
    }
}
