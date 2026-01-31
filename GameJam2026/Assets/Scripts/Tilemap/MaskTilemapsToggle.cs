using System;
using System.Collections;
using UnityEngine;

public class MaskTilemapsToggle : MonoBehaviour
{
    [Header("Assign these in Inspector")]
    [SerializeField] private GameObject tilemapDisappearShow;
    [SerializeField] private GameObject tilemapDisappearHide;

    [Header("Initial state")]
    [Tooltip("True = empieza con máscara puesta (Hide activo). False = empieza sin máscara (Show activo).")]
    [SerializeField] private bool maskStartsOn = true;

    private bool isMaskOn;
    private bool subscribed;
    private Coroutine subscribeRoutine;

    private void Awake()
    {
        isMaskOn = maskStartsOn;
        ApplyState();
    }

    private void OnEnable()
    {
        subscribeRoutine = StartCoroutine(WaitAndSubscribe());
    }

    private void OnDisable()
    {
        if (subscribeRoutine != null)
        {
            StopCoroutine(subscribeRoutine);
            subscribeRoutine = null;
        }

        Unsubscribe();
    }

    private IEnumerator WaitAndSubscribe()
    {
        // Espera hasta que exista el GameInput (por orden de Awake/OnEnable en Unity)
        while (GameInput.Instance == null)
            yield return null;

        Subscribe();
    }

    private void Subscribe()
    {
        if (subscribed) return;

        GameInput.Instance.OnChangeMaskAction += OnMaskChanged;
        subscribed = true;

        Debug.Log("MaskTilemapsToggle suscrito a OnChangeMaskAction");
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;
        if (GameInput.Instance != null)
            GameInput.Instance.OnChangeMaskAction -= OnMaskChanged;

        subscribed = false;
    }

    private void OnMaskChanged(object sender, EventArgs e)
    {
        isMaskOn = !isMaskOn;
        ApplyState();

        Debug.Log("MASK TOGGLED -> isMaskOn=" + isMaskOn);
    }

    private void ApplyState()
    {
        // Cuando se quita la máscara Show activo
        bool maskOff = !isMaskOn;

        if (tilemapDisappearShow != null)
            tilemapDisappearShow.SetActive(maskOff);

        if (tilemapDisappearHide != null)
            tilemapDisappearHide.SetActive(isMaskOn);
    }
}
