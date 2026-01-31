using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnChangeMaskAction;
    public event EventHandler OnPauseAction;

    private InputSystem_Actions playerInputActions;

    private void Awake()
    {
        playerInputActions = new InputSystem_Actions();

        Instance = this;

        playerInputActions.Player.Enable();
        playerInputActions.Player.Mask.performed += ChangeMask_performed; // E key, changes mask
        playerInputActions.Player.Pause.performed += Pause_performed; // Esc key, pauses game

    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void ChangeMask_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnChangeMaskAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovmentVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }
}
