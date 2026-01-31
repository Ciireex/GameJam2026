using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxHealthTime = 15f;
    [SerializeField] private HealthSlider healthSlider;
    [SerializeField] private float healthDrainSpeedMultiplier = 1f;

    [Header("Spawn / Respawn")]
    [Tooltip("Arrastra aquí el punto donde debe reaparecer el jugador.")]
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

    // NEW: para no acumular coroutines de luz
    private Coroutine lightRoutine;

    // NEW: evita que el jugador pueda spamear máscara durante la intro
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

        // Asegura luz en estado normal ANTES de la intro (para evitar saltos raros)
        if (spotLight != null)
            spotLight.pointLightOuterRadius = maskOffRadius;

        // Intro: Wipe + animación de luz (abre, espera 1s y vuelve a cerrar)
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        introPlaying = true;

        // 1) Wipe in
        if (wipeEffect != null)
            wipeEffect.AnimateIn();

        // Espera a que acabe el wipe (ajusta wipeInDuration en inspector)
        if (wipeInDuration > 0f)
            yield return new WaitForSeconds(wipeInDuration);

        // 2) "Como si te quitaras la máscara": abre la luz
        yield return AnimateLightTo(maskOffRadius, changeMaskAnimationDuration);

        // 3) Delay dramático antes de volver a máscara puesta
        if (delayBeforeMaskBack > 0f)
            yield return new WaitForSeconds(delayBeforeMaskBack);

        // 4) Vuelve a estado normal con máscara puesta
        isMaskOn = true;
        yield return AnimateLightTo(maskOnRadius, changeMaskAnimationDuration);

        // Asegura estado final
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
        // Bloquea el cambio durante la intro para evitar estados raros
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

        // NEW: para no acumular coroutines
        if (lightRoutine != null)
            StopCoroutine(lightRoutine);

        lightRoutine = StartCoroutine(ChangeLightRadiusCoroutine(targetOuterRadius, changeMaskAnimationDuration));
    }

    // NEW: helper para "esperar" la animación de luz en la intro
    private IEnumerator AnimateLightTo(float targetOuterRadius, float duration)
    {
        // Si hay una anim previa en curso, la paramos
        if (lightRoutine != null)
        {
            StopCoroutine(lightRoutine);
            lightRoutine = null;
        }

        // Ejecuta y espera
        yield return ChangeLightRadiusCoroutine(targetOuterRadius, duration);
    }

    private IEnumerator ChangeLightRadiusCoroutine(float targetOuterRadius, float duration)
    {
        if (spotLight == null)
            yield break;

        float elapsedTime = 0f;
        float startRadius = spotLight.pointLightOuterRadius;

        // Evita división por 0
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

        yield return new WaitForSeconds(respawnDelay);

        // Teleport al punto de spawn
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            if (rb != null) rb.position = spawnPoint.position;
        }

        // Reset vida + UI
        currentHealthTime = maxHealthTime;
        healthSlider.SetMaxValue(maxHealthTime);
        healthSlider.SetValue(currentHealthTime);

        // Reset visual (por si murió encogido)
        if (playerVisual != null)
        {
            playerVisual.localScale = originalVisualScale;
        }

        // FORZAR MÁSCARA ON AL RESPAWNEAR
        isMaskOn = true;

        // Reinicia contador apagado
        countdownActive = false;

        // Asegura luz normal tras respawn
        if (spotLight != null)
            spotLight.pointLightOuterRadius = maskOnRadius;

        // Volver a estado vivo
        isDead = false;
        isFalling = false;

        // Reactivar movimiento/colisiones
        if (rb != null) rb.simulated = true;
        if (col != null) col.enabled = true;

        isRespawning = false;

        GameManager.Instance.StartTimer();
    }

    public void SetDrainMultiplier(float multiplier)
    {
        healthDrainSpeedMultiplier = multiplier;
    }
}
