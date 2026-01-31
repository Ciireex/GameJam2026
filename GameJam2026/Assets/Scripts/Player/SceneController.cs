using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    public event Action OnSceneTransitionStart;
    public event Action OnSceneTransitionComplete;

    [Header("TransitionAnimation")]
    [SerializeField] private GameObject transitionGraphics;
    [SerializeField] private float transitionAnimationTime = 0.8f;
    [SerializeField] private float startPosition = -1920f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void TransitionAndLoadScene(int sceneIndex, bool doFadeIn, bool doFadeout)
    {
        // Si alguien intenta usarlo mientras se est� destruyendo o duplicando, lo evitamos.
        if (this == null) return;

        if (!gameObject.activeInHierarchy)
            return;

        if (isTransitioning)
            return;

        StartCoroutine(DoSceneTransition(sceneIndex, doFadeIn, doFadeout));
    }

    private IEnumerator DoSceneTransition(int sceneIndex, bool doFadeIn, bool doFadeOut)
    {
        isTransitioning = true;

        OnSceneTransitionStart?.Invoke();

        // Si el objeto gr�fico no existe, cargamos directo para no romper.
        if (transitionGraphics != null && doFadeIn)
            yield return StartCoroutine(TransitionEnterAnimation());

        SceneManager.LoadScene(sceneIndex);

        if (transitionGraphics != null && doFadeOut)
            yield return StartCoroutine(TransitionExitAnimation());

        OnSceneTransitionComplete?.Invoke();

        isTransitioning = false;
    }

    private IEnumerator TransitionEnterAnimation()
    {
        float elapsedTime = 0f;

        while (elapsedTime < transitionAnimationTime)
        {
            if (transitionGraphics == null) yield break;

            transitionGraphics.transform.localPosition =
                new Vector3(Mathf.Lerp(startPosition, 0f, elapsedTime / transitionAnimationTime), 0, 0);

            elapsedTime += Time.unscaledDeltaTime; // recomendable si pausas con timeScale
            yield return null;
        }

        if (transitionGraphics != null)
            transitionGraphics.transform.localPosition = Vector3.zero;
    }

    private IEnumerator TransitionExitAnimation()
    {
        float elapsedTime = 0f;

        while (elapsedTime < transitionAnimationTime)
        {
            if (transitionGraphics == null) yield break;

            transitionGraphics.transform.localPosition =
                new Vector3(Mathf.Lerp(0f, -startPosition, elapsedTime / (transitionAnimationTime)), 0, 0);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (transitionGraphics != null)
            transitionGraphics.transform.localPosition = new Vector3(-startPosition, 0, 0);
    }
}
