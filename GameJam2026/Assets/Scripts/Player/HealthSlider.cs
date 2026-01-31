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
    [SerializeField] private Material fillMaterial;
    [SerializeField] private Image healthStateIcon;
    [SerializeField] private Sprite safeSprite;
    [SerializeField] private Sprite dyingSprite;

    public void SetMaxValue(float max)
    {
        slider.maxValue = max;
        slider.value = max;
        fillMaterial.SetFloat("_maxValue", max);
    }

    public void SetValue(float value)
    {
        slider.value = value;
        fillMaterial.SetFloat("_value", value);
    }


    public void SetShaderShake(float value)
    {
        fillMaterial.SetFloat("_shakeStrength", value);
    }

    public void ChangeHealthStateIcon()
    {
        healthStateIcon.sprite = GameManager.Instance.Player.IsMaskOn() ? safeSprite : dyingSprite;
    }
}

