using UnityEngine;
using UnityEngine.UI;
public class TimerUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;

    private void Update()
    {
        timerImage.fillAmount = GameManager.Instance.GetTimeLeftNomralized();
    }
}
