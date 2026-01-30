using UnityEngine;
using System;
public class Player : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxHealthTime = 15f;
    [SerializeField] private HealthSlider healthSlider;
    [SerializeField] private float healthDrainSpeedMultiplier = 1f;

    [SerializeField] private Renderer shadowRenderer;
    [SerializeField] private float darknessLerpSpeed = 5f;
    private Material shadowMaterial;
    private const string DARKNESS_PARAM = "_DarknessStrength";
    private float currentDarkness;
    private float maskOnDarkness = 100f;
    private float maskOffDarkness = 5f;

    public event EventHandler OnPlayerDeath;

    private float currentHealthTime;
    private bool isMaskOn = true;
    private bool countdownActive;


    private void Start()
    {
        currentDarkness = maskOnDarkness;
        currentHealthTime = maxHealthTime;
        healthSlider.SetMaxValue(maxHealthTime);
        GameInput.Instance.OnChangeMaskAction += GameInput_OnChangeMaskAction;

        shadowMaterial = shadowRenderer.material;
    }

    private void Update()
    {
        HandleMovment();
        HandleHealthCountdown();
        UpdateShadowDarkness();
    }

    private void HandleMovment() 
    {
        Vector2 inputVector = GameInput.Instance.GetMovmentVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, inputVector.y, 0f);

        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void HandleHealthCountdown()
    {
        if (!countdownActive)
            return;

        currentHealthTime -= Time.deltaTime * healthDrainSpeedMultiplier;
        healthSlider.SetValue(currentHealthTime);

        if (currentHealthTime <= 0f)
        {
            currentHealthTime = 0f;
            countdownActive = false;
            Debug.Log("Player has died");
            OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnChangeMaskAction(object sender, System.EventArgs e)
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
        float targetValue = isMaskOn ? maskOffDarkness : maskOnDarkness;

        currentDarkness = Mathf.Lerp(currentDarkness, targetValue, Time.deltaTime * darknessLerpSpeed);

        shadowMaterial.SetFloat(DARKNESS_PARAM, currentDarkness);
    }

    public void SetHealthDrainSpeedMultiplier(float value)
    {
        healthDrainSpeedMultiplier = value;
    }
}
