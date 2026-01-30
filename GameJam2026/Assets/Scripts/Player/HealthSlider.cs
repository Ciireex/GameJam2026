//using System;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.UI;

//public class HealthSlider : MonoBehaviour
//{
//    [SerializeField] private Slider slider;
//    [SerializeField] private float maxValueInSeconds = 15f;
//    [SerializeField] private bool countdownActive = false;
//    public event Action OnHealthDepleted;



//    void Start()
//    {
//        slider.maxValue = maxValueInSeconds;
//        slider.value = maxValueInSeconds;
//        GameInput.Instance.OnChangeMaskAction += GameInput_OnChangeMaskAction;

//    }

//    private void GameInput_OnChangeMaskAction(object sender, EventArgs e)
//    {
//        ToggleCountdown();
//    }

//    void Update()
//    {
//        if (countdownActive)
//            slider.value -= Time.deltaTime;
//    }

//    public void ToggleCountdown()
//    {
//        countdownActive = !countdownActive;
//    }


//    public void OnHealthChanged()
//    {
//        if (slider.value <= slider.minValue)
//        {
//            Debug.Log("Player has died");
//            OnHealthDepleted?.Invoke();
//            countdownActive = false;
//        }
//    }
//}
using UnityEngine;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public void SetMaxValue(float max)
    {
        slider.maxValue = max;
        slider.value = max;
    }

    public void SetValue(float value)
    {
        slider.value = value;
    }
}

