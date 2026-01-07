using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions {
    // Events
    public event UnityAction<InputAction.CallbackContext> Move = delegate { };
    
    private PlayerInputActions _inputActions;
    
    private void OnEnable() {
        if (_inputActions == null) {
            _inputActions = new PlayerInputActions();
            _inputActions.Player.SetCallbacks(this);
        }
        
        _inputActions.Player.Enable();
    }

    private void OnDisable() {
        _inputActions.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context) {
        Move?.Invoke(context);
    }

    public void OnLook(InputAction.CallbackContext context) {
        
    }

    public void OnAttack(InputAction.CallbackContext context) {
        
    }

    public void OnInteract(InputAction.CallbackContext context) {
        
    }

    public void OnCrouch(InputAction.CallbackContext context) {
        
    }

    public void OnJump(InputAction.CallbackContext context) {
        
    }

    public void OnPrevious(InputAction.CallbackContext context) {
        
    }

    public void OnNext(InputAction.CallbackContext context) {
        
    }

    public void OnSprint(InputAction.CallbackContext context) {
        
    }
}