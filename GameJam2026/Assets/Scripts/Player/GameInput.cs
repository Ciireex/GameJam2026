using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnChangeMaskAction;
    public event EventHandler OnPauseAction;

    private InputSystem_Actions playerInputActions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerInputActions = new InputSystem_Actions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Mask.performed += ChangeMask_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;
    }

    private void OnDestroy()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Player.Mask.performed -= ChangeMask_performed;
            playerInputActions.Player.Pause.performed -= Pause_performed;
            playerInputActions.Disable();
        }
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void ChangeMask_performed(InputAction.CallbackContext obj)
    {
        OnChangeMaskAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovmentVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }
}
