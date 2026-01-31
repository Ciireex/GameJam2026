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

    // NEW: Spawn point arrastrable + respawn delay
    [Header("Spawn / Respawn")]
    [Tooltip("Arrastra aqu� el punto donde debe reaparecer el jugador.")]
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

    [Header("PlayerVisual")]
    // NEW: Solo se encoge el visual
    [SerializeField] private Transform playerVisual;

    private bool isFalling;
    private Vector3 originalVisualScale;

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

        // NEW: si no est� asignado, intenta encontrarlo por nombre
        if (playerVisual == null)
        {
            Transform t = transform.Find("PlayerVisual");
            if (t != null) playerVisual = t;
        }

        // NEW: guardar escala original del visual (no del root)
        if (playerVisual != null)
            originalVisualScale = playerVisual.localScale;

        currentHealthTime = maxHealthTime;

        healthSlider.SetMaxValue(maxHealthTime);
        healthSlider.SetValue(currentHealthTime);

        // Set light radius to whatever mask state we have
        UpdateLightRadius();

        GameInput.Instance.OnChangeMaskAction += GameInput_OnChangeMaskAction;
        GameManager.Instance.OnTimeIsUp += GameManager_OnTimeIsUp;

        // NEW: si no has arrastrado spawnPoint, por defecto usa la posici�n inicial del player
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

        // Light
        UpdateLightRadius();

        // Health Slide Shader
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
        StartCoroutine(ChangeLightRadiusCoroutine(targetOuterRadius, changeMaskAnimationDuration));
    }

    private IEnumerator ChangeLightRadiusCoroutine(float targetOuterRadius, float duration)
    {
        float elapsedTime = 0f;
        float startRadius = spotLight.pointLightOuterRadius;
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
        if (other.CompareTag("Spikes"))
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

        // NEW: respawn desde aqu�
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

        // Si tenemos escala original guardada del visual, �sala; si no, usa la actual
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

        // Reset visual (por si muri� encogido)
        if (playerVisual != null)
        {
            playerVisual.localScale = originalVisualScale;
        }

        // FORZAR M�SCARA ON AL RESPAWNEAR
        isMaskOn = true;

        // (Opcional) �Quieres que el contador se reinicie apagado?
        countdownActive = false;

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