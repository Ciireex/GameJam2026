/*  This component exists once in the scene (singleton)
    and does not get destroyed when scene loads.
    It allows to transition from one scene to another a screen 
    animation with the method TransitionAndLoadScene.
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;
    public event Action OnSceneTransitionStart;
    public event Action OnSceneTransitionComplete;

    [Header("TransitionAnimation")]
    [SerializeField] private GameObject transitionGraphics;
    [SerializeField] private float transitionAnimationTime = 0.5f;
    [SerializeField] private float startPosition = -1920f;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void TransitionAndLoadScene(int sceneIndex)
    {
        StartCoroutine(DoSceneTransition(sceneIndex));
    }

    private IEnumerator DoSceneTransition(int sceneIndex)
    {
        OnSceneTransitionStart?.Invoke();
        yield return StartCoroutine(TransitionEnterAnimation());
        SceneManager.LoadScene(sceneIndex);
        yield return StartCoroutine(TransitionExitAnimation());
        OnSceneTransitionComplete?.Invoke();
    }

    private IEnumerator TransitionEnterAnimation()
    {
        // Do transition enter animation
        float elapsedTime = 0f;
        while (elapsedTime < transitionAnimationTime)
        {
            transitionGraphics.transform.localPosition = new Vector3(Mathf.Lerp(startPosition, 0f , elapsedTime/transitionAnimationTime), 0, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transitionGraphics.transform.localPosition = Vector3.zero;
    }
    private IEnumerator TransitionExitAnimation()
    {
        // Do transition exit animation
        float elapsedTime = 0f;
        while (elapsedTime < transitionAnimationTime)
        {
            transitionGraphics.transform.localPosition = new Vector3(Mathf.Lerp(0, -startPosition , elapsedTime/transitionAnimationTime), 0, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transitionGraphics.transform.localPosition = new Vector3(-startPosition, 0, 0);
    }
}
