using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringWheel : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody rb;

    [Header("Forces")] 
    [SerializeField] private float steerForce = 10f;
    
    private float _steer;
    
    private void OnEnable() {
        inputReader.Steer += OnSteer;
    }

    private void OnDisable() {
        inputReader.Steer -= OnSteer;
    }

    private void FixedUpdate() {
        rb.AddTorque(transform.right * (_steer * steerForce * Time.fixedDeltaTime));
    }

    private void OnSteer(InputAction.CallbackContext ctx) {
        _steer = ctx.ReadValue<float>();
    }
}
