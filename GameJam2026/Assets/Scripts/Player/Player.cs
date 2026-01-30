using UnityEngine;
using System;
public class Player : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxHealthTime = 15f;
    [SerializeField] private HealthSlider healthSlider;

    public event EventHandler OnPlayerDeath;

    private float currentHealthTime;
    private bool isMaskOn = true;
    private bool countdownActive;


    private void Start()
    {
        currentHealthTime = maxHealthTime;
        healthSlider.SetMaxValue(maxHealthTime);
        GameInput.Instance.OnChangeMaskAction += GameInput_OnChangeMaskAction;
    }

    private void Update()
    {
        HandleMovment();
        HandleHealthCountdown();
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
}
