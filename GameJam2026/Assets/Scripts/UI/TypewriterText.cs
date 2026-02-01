using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TypewriterText : MonoBehaviour
{
    [SerializeField] private GameObject controlBckg;
    [SerializeField] private GameObject controlsText;
    [SerializeField] private GameObject continueBtn;

    [SerializeField] private float fadeDuration = 1f;

    [TextArea(3, 10)]
    public string fullText;

    public float typingSpeed = 0.05f; // Time in seconds between each character

    private TMP_Text textComponent;


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


        ShowControls();
    }

    private void ShowControls()
    {
        if (controlBckg != null)
            StartCoroutine(FadeIn(controlBckg));

        if (controlsText != null)
            StartCoroutine(FadeIn(controlsText));

        if (continueBtn != null)
            StartCoroutine(FadeIn(continueBtn));
    }


    private IEnumerator FadeIn(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
            yield break;

        obj.SetActive(true);

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

}
