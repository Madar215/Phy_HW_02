using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions {
    // Events
    public event UnityAction<InputAction.CallbackContext> Drive = delegate { };
    public event UnityAction<InputAction.CallbackContext> Steer = delegate { };
    
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

    public void OnDrive(InputAction.CallbackContext context) {
        Drive?.Invoke(context);
    }

    public void OnSteer(InputAction.CallbackContext context) {
        Steer?.Invoke(context);
    }

}