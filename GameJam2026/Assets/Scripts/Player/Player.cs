
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
    [Range(0, 30)][SerializeField] private float maskOnRadius = 1.5f;
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

    [Header("Acid")]
    [SerializeField] private float acidDrainMultiplier = 2f;

    private bool isOnAcid;

    [Header("PlayerVisual")]
    [SerializeField] private Transform playerVisual;

    [Header("Wipe transition")]
    public WipeController wipeEffect;

    // =========================
    // NEW: ANIMACIONES (BlendTrees)
    // =========================
    [Header("Animations (BlendTrees)")]
    [Tooltip("Animator que controla las animaciones (normalmente está en PlayerVisual).")]
    [SerializeField] private Animator animator;

    [Tooltip("Umbral para considerar movimiento (evita parpadeo idle/walk).")]
    [SerializeField] private float animMoveThreshold = 0.01f;

    // Recordar última dirección para que el idle mire bien
    private Vector2 lastAnimDir = Vector2.down;

    // NEW: SpriteRenderer para flip (más robusto que localScale)
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    // =========================

    [Header("Death Animation")]
    [SerializeField] private float deathRotateAngle = -90f;
    [SerializeField] private float deathRotateDuration = 0.25f;

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

        // NEW: intenta encontrar animator si no está asignado
        if (animator == null)
        {
            if (playerVisual != null) animator = playerVisual.GetComponent<Animator>();
            if (animator == null) animator = GetComponent<Animator>();
        }

        // NEW: intenta encontrar el SpriteRenderer si no está asignado
        if (playerSpriteRenderer == null)
        {
            if (playerVisual != null) playerSpriteRenderer = playerVisual.GetComponentInChildren<SpriteRenderer>();
            if (playerSpriteRenderer == null) playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

        // NEW: inicializa parámetros animator al arrancar (para que idle tenga dirección)
        UpdateAnimatorParams(Vector2.zero);

        // Intro: Wipe + animación de luz (abre, espera y vuelve a cerrar)
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        introPlaying = true;

        // 1) Wipe in
        if (wipeEffect != null)
        {
            wipeEffect.AnimateIn();
            Debug.Log("Wipe in started");
        }

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
        if (!isDead && !isFalling && !introPlaying)
        {
            moveInput = GameInput.Instance.GetMovmentVectorNormalized();
        }
        else
        {
            moveInput = Vector2.zero;
        }

        // NEW: actualizar parámetros del Animator cada frame
        UpdateAnimatorParams(moveInput);

        HandleHealthCountdown();
    }

    private void FixedUpdate()
    {
        if (isDead || isFalling)
            return;

        Vector2 newPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        if (!isMaskOn)
        {
            // Si la máscara está puesta, el jugador se mueve a la mitad de velocidad
            newPos = rb.position + moveInput * (moveSpeed * 0.7f) * Time.fixedDeltaTime;
        }
        rb.MovePosition(newPos);
    }

    // =========================
    // NEW: LÓGICA DE ANIMACIÓN PARA BLEND TREES
    // Usa parámetros: Speed, DirX, DirY, MaskOn
    // Convierte input normalizado a 4 direcciones puras para que encaje con (0,±1)(±1,0)
    // =========================
    private void UpdateAnimatorParams(Vector2 input)
    {
        if (animator == null) return;

        float speed = input.magnitude;
        animator.SetFloat("Speed", speed);

        if (speed > animMoveThreshold)
        {
            Vector2 dir;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                dir = new Vector2(Mathf.Sign(input.x), 0f);
            else
                dir = new Vector2(0f, Mathf.Sign(input.y));

            lastAnimDir = dir;

            animator.SetFloat("DirX", dir.x);
            animator.SetFloat("DirY", dir.y);
        }
        else
        {
            animator.SetFloat("DirX", lastAnimDir.x);
            animator.SetFloat("DirY", lastAnimDir.y);
        }

        animator.SetBool("MaskOn", isMaskOn);

        // FLIP izquierda/derecha usando SpriteRenderer (más robusto que localScale)
        if (playerSpriteRenderer != null)
        {
            if (lastAnimDir.x < 0f) playerSpriteRenderer.flipX = true;
            else if (lastAnimDir.x > 0f) playerSpriteRenderer.flipX = false;
        }
    }

    private Color subtleGreen = new Color(0.3f, 1f, 0.3f);
    private void UpdatePlayerColor()
    {
        if (playerSpriteRenderer == null)
            return;

        float healthPercent = Mathf.Clamp01(currentHealthTime / maxHealthTime);

        // Stronger green, but not full pure green


        // Interpolate between white and green depending on health
        Color targetColor = Color.Lerp(subtleGreen, Color.white, healthPercent);

        playerSpriteRenderer.color = targetColor;
    }


    private void HandleHealthCountdown()
    {
        if (isDead)
            return;

        float drain = 0f;

        // Daño normal por máscara
        if (countdownActive)
            drain += healthDrainSpeedMultiplier;

        // Daño extra por ácido (siempre)
        if (isOnAcid)
            drain += healthDrainSpeedMultiplier * acidDrainMultiplier;

        if (drain <= 0f)
            return;

        currentHealthTime -= Time.deltaTime * drain;
        healthSlider.SetValue(currentHealthTime);

        UpdatePlayerColor();

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

        if (healthSlider)
            healthSlider.ChangeHealthStateIcon();

        // NEW: refresca el bool MaskOn al momento
        UpdateAnimatorParams(moveInput);

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
        else if (other.CompareTag("Spike"))
        {
            Kill();
        }
        else if (other.CompareTag("Acid"))
        {
            isOnAcid = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Acid"))
        {
            isOnAcid = false;
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

        StartCoroutine(DeathRotateRoutine());

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
            SceneController.Instance.TransitionAndLoadScene(sceneIndex, false, false);
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

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
            GameInput.Instance.OnChangeMaskAction -= GameInput_OnChangeMaskAction;

        if (GameManager.Instance != null)
            GameManager.Instance.OnTimeIsUp -= GameManager_OnTimeIsUp;
    }

    private IEnumerator DeathRotateRoutine()
    {
        Transform visual = playerVisual != null ? playerVisual : transform;

        Quaternion startRot = visual.rotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, deathRotateAngle);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, deathRotateDuration);
            visual.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        visual.rotation = endRot;
    }


}