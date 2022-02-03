using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, InputMaster.IGameplayActions {
    public InputMaster gameInput;

    public event UnityAction moveEvent = delegate { };
    public event UnityAction jumpStartEvent = delegate { };
    public event UnityAction jumpStopEvent = delegate { };
    public event UnityAction dodgeEvent = delegate { };

    public event UnityAction attackEvent = delegate { };

    [SerializeField] public Vector2 movementInputs;
    [SerializeField] public float horizontalInput;
    [SerializeField] public float verticalInput;
    [SerializeField] public float jumping;
    [SerializeField] public float dodging;

    [SerializeField] public float attacking;

    private void OnEnable() {
        if (gameInput == null) {
            gameInput = new InputMaster();
            gameInput.Gameplay.SetCallbacks(this);
        }

        EnableGameplayInput();
    }

    private void OnDisable() {
        DisableAllInput();
    }

    public void OnMovement(InputAction.CallbackContext value) {
        movementInputs = value.ReadValue<Vector2>();
        horizontalInput = movementInputs.x;
        verticalInput = movementInputs.y;
        moveEvent.Invoke();
    }

    public void OnJump(InputAction.CallbackContext value) {
        if (value.action.triggered) {
            jumping = 1.0f;
            jumpStartEvent.Invoke();
        }
        if (value.phase == InputActionPhase.Canceled) {
            jumping = 0.0f;
            jumpStopEvent.Invoke();
        }
    }

    public void OnDodge(InputAction.CallbackContext value) {
        if (value.phase == InputActionPhase.Performed)
            dodgeEvent.Invoke();
    }

    public void OnAttack(InputAction.CallbackContext value) {
        if (value.phase == InputActionPhase.Performed)
            attackEvent.Invoke();
    }

    private void EnableGameplayInput()
    {
        gameInput.Gameplay.Enable();
        //gameInput.Menus.Disable();
    }

    private void EnableMenuInput()
    {
        gameInput.Gameplay.Disable();
        //gameInput.Menus.Enable();
    }

    private void DisableAllInput()
    {
        gameInput.Gameplay.Disable();
        //gameInput.Menus.Disable();
    }
}
