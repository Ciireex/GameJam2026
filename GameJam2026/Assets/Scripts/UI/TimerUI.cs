//using UnityEngine;
//using UnityEngine.UI;
//public class TimerUI : MonoBehaviour
//{
//    [SerializeField] private Image timerImage;

//    private void Update()
//    {
//        timerImage.fillAmount = GameManager.Instance.GetTimeLeftNomralized();
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private Color colorFull = Color.blue;
    [SerializeField] private Color colorMid = Color.yellow;
    [SerializeField] private Color colorLow = Color.red;

    [SerializeField] private float lowTimeThreshold = 10f;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        float normalizedTime = GameManager.Instance.GetTimeLeftNomralized();
        float timeLeft = GameManager.Instance.GetTimeLeft();

        timerImage.fillAmount = normalizedTime;

        Color fillColor;
        if (normalizedTime > 0.5f)
        {
            float t = (normalizedTime - 0.5f) / 0.5f;
            fillColor = Color.Lerp(colorMid, colorFull, t);
        }
        else
        {
            float t = normalizedTime / 0.5f;
            fillColor = Color.Lerp(colorLow, colorMid, t);
        }

        timerImage.color = fillColor;

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);

        Debug.Log($"Time Left: {minutes}:{seconds}");

        string formatted = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (formatted.Length > 5)
            formatted = formatted.Substring(0, 5);

        timerText.text = formatted;

        if (timeLeft <= lowTimeThreshold)
            timerText.color = colorLow;
        else
            timerText.color = Color.white;
    }
}
