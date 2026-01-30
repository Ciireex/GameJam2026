using UnityEngine;
using System;
public class Player : MonoBehaviour
{
    public event EventHandler OnPlayerDeath;


    // Logic parameters for player movement and health
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxHealthTime = 15f;
    [SerializeField] private HealthSlider healthSlider;

    // Visual effects for mask on/off
    [SerializeField] private SpriteRenderer shadowVisual;
    [SerializeField] private float maskDarkness = 100f;
    [SerializeField] private float noMaskDarkness = 10f;
    [SerializeField] private float darknessTransitionSpeed = 5f;


    private float currentHealthTime;
    private bool isMaskOn = true;
    private bool countdownActive;

    private Material shadowMaterial;
    private float targetDarkness;

    private void Start()
    {
        shadowMaterial = shadowVisual.material;
        shadowMaterial.SetFloat("DarknessStrength", noMaskDarkness);
        targetDarkness = noMaskDarkness;

        currentHealthTime = maxHealthTime;
        healthSlider.SetMaxValue(maxHealthTime);
        GameInput.Instance.OnChangeMaskAction += GameInput_OnChangeMaskAction;
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

        currentHealthTime -= Time.deltaTime;
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

        targetDarkness = IsMaskOn() ? maskDarkness : noMaskDarkness;

        Debug.Log("Mask is " + IsMaskOn());
    }

    private bool IsMaskOn()
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
        float current = shadowMaterial.GetFloat("DarknessStrength");
        float newValue = Mathf.Lerp(
            current,
            targetDarkness,
            Time.deltaTime * darknessTransitionSpeed
        );

        shadowMaterial.SetFloat("DarknessStrength", newValue);
    }

}
