using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TypewriterText : MonoBehaviour
{
    [SerializeField] private GameObject controlBckg;
    [SerializeField] private GameObject controlsText;
    [SerializeField] private GameObject continueBtn;
    [TextArea(3, 10)]
    public string fullText;

    public float typingSpeed = 0.05f; // Time in seconds between each character

    private TMP_Text textComponent;

    private bool isTyping = false;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char c in fullText)
        {
            textComponent.text += c;

            // Base speed
            float delay = typingSpeed;

            // Extra pauses for readability
            if (c == '.' || c == '!' || c == '?')
                delay *= 8f;
            else if (c == ',')
                delay *= 4f;
            else if (c == '\n')
                delay *= 10f;

            yield return new WaitForSeconds(delay);
        }

        isTyping = false;

        ShowControls();
    }

    private void ShowControls()
    {
        if (controlBckg != null)
            controlBckg.SetActive(true);
        if (controlsText != null)
            controlsText.SetActive(true);
        if (continueBtn != null)
            continueBtn.SetActive(true);
    }

}
