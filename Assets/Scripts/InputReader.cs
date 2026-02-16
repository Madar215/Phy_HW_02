using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject, PlayerInputActions.ICarActions, PlayerInputActions.IPlayerActions {
    // Car events
    public event UnityAction<InputAction.CallbackContext> Drive = delegate { };
    public event UnityAction<InputAction.CallbackContext> Steer = delegate { };
    public event UnityAction KickPass = delegate { };
    public event UnityAction KickLob = delegate { };
    
    // Player events
    public event UnityAction<InputAction.CallbackContext> Move = delegate { };
    
    private PlayerInputActions _inputActions;
    
    private void OnEnable() {
        if (_inputActions == null) {
            _inputActions = new PlayerInputActions();
            _inputActions.Car.SetCallbacks(this);
            _inputActions.Player.SetCallbacks(this);
        }
        
        _inputActions.Player.Enable();
    }

    private void OnDisable() {
        _inputActions.Car.Disable();
        _inputActions.Player.Disable();
    }

    public void OnDrive(InputAction.CallbackContext context) {
        Drive?.Invoke(context);
    }

    public void OnSteer(InputAction.CallbackContext context) {
        Steer?.Invoke(context);
    }

    public void OnMove(InputAction.CallbackContext context) {
        Move?.Invoke(context);
    }

    public void OnKickPass(InputAction.CallbackContext c) {
        if (!c.started) return;
        KickPass?.Invoke();
    }

    public void OnKickLob(InputAction.CallbackContext c) {
        if (!c.started) return;
        KickLob?.Invoke();
    }
}