using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxHealthTime = 15f;
    [SerializeField] private HealthSlider healthSlider;
    [SerializeField] private float healthDrainSpeedMultiplier = 1f;

    // NEW: Spawn point arrastrable + respawn delay
    [Header("Spawn / Respawn")]
    [Tooltip("Arrastra aquí el punto donde debe reaparecer el jugador.")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 1f;

    [Header("Fall")]
    [SerializeField] private float fallDuration = 0.6f;
    [SerializeField] private float fallEndScale = 0.05f;

    // NEW: Solo se encoge el visual
    [SerializeField] private Transform playerVisual;

    private bool isFalling;
    private Vector3 originalVisualScale;

    [Header("Mask / Shadow")]
    [SerializeField] private Renderer shadowRenderer;
    [SerializeField] private float darknessLerpSpeed = 5f;
    private Material shadowMaterial;
    private const string DARKNESS_PARAM = "_DarknessStrength";
    private float currentDarkness;
    [SerializeField] private float maskOnDarkness = 250f;
    [SerializeField] private float maskOffDarkness = 5f;

    private bool isDead;

    public event EventHandler OnPlayerDeath;

    private float currentHealthTime;
    private bool isMaskOn = true;
    private bool countdownActive;

    // Movement (Rigidbody2D)
    private Rigidbody2D rb;
    private Vector2 moveInput;

    // NEW: para no lanzar respawn dos veces
    private bool isRespawning;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // NEW: si no está asignado, intenta encontrarlo por nombre
        if (playerVisual == null)
        {
            Transform t = transform.Find("PlayerVisual");
            if (t != null) playerVisual = t;
        }

        // NEW: guardar escala original del visual (no del root)
        if (playerVisual != null)
            originalVisualScale = playerVisual.localScale;

        currentDarkness = maskOnDarkness;
        currentHealthTime = maxHealthTime;

        healthSlider.SetMaxValue(maxHealthTime);
        healthSlider.SetValue(currentHealthTime);

        GameInput.Instance.OnChangeMaskAction += GameInput_OnChangeMaskAction;

        GameManager.Instance.OnTimeIsUp += GameManager_OnTimeIsUp;

        shadowMaterial = shadowRenderer.material;
        shadowMaterial.SetFloat(DARKNESS_PARAM, currentDarkness);

        // NEW: si no has arrastrado spawnPoint, por defecto usa la posición inicial del player
        if (spawnPoint == null)
        {
            GameObject sp = new GameObject("SpawnPoint_Runtime");
            sp.transform.position = transform.position;
            sp.transform.rotation = transform.rotation;
            spawnPoint = sp.transform;
        }
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
        UpdateShadowDarkness();
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
        ToggleMask();
        ToggleCountdown();
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

    private void UpdateShadowDarkness()
    {
        float targetValue = isMaskOn ? maskOnDarkness : maskOffDarkness;

        currentDarkness = Mathf.Lerp(
            currentDarkness,
            targetValue,
            Time.deltaTime * darknessLerpSpeed
        );

        shadowMaterial.SetFloat(DARKNESS_PARAM, currentDarkness);
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
    }

    public void Kill()
    {
        if (isDead)
            return;

        isDead = true;
        countdownActive = false;

        Debug.Log("Player has died");

        OnPlayerDeath?.Invoke(this, EventArgs.Empty);

        // NEW: respawn desde aquí
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

        // NEW: encoger solo el visual
        Transform visual = playerVisual != null ? playerVisual : transform;

        Vector3 startScale = visual.localScale;
        Vector3 endScale;

        // Si tenemos escala original guardada del visual, úsala; si no, usa la actual
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

        // (Opcional) ¿Quieres que el contador se reinicie apagado?
        countdownActive = false;

        // Reset sombras a "modo máscara"
        currentDarkness = maskOnDarkness;
        shadowMaterial.SetFloat(DARKNESS_PARAM, currentDarkness);

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
