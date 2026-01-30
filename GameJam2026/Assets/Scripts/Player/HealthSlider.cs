using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private float maxValueInSeconds = 15f;
    public event Action OnHealthDepleted;
    private bool countdownActive = true;


    void Start()
    {
        slider.maxValue = maxValueInSeconds;
        slider.value = maxValueInSeconds;
    }

    void Update()
    {
        if (countdownActive)
            slider.value -= Time.deltaTime;
    }

    public void ActivateCountdown()
    {
        countdownActive = true;
    }

    public void DeactivateCountdown()
    {
        countdownActive = false;
    }

    public void OnHealthChanged()
    {
        if (slider.value <= slider.minValue)
        {
            Debug.Log("Player has died");
            OnHealthDepleted?.Invoke();
            countdownActive = false;
        }
    }
}
