using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringWheel : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody rb;

    [Header("Forces")] 
    [SerializeField] private float steerForce = 10f;
    [SerializeField] private float maxSteerAngle = 30f;
    
    [Header("Wheels")]
    [SerializeField] private ConfigurableJoint rightPivot;
    [SerializeField] private ConfigurableJoint leftPivot;
    
    private float _steer;
    
    private void OnEnable() {
        inputReader.Steer += OnSteer;
    }

    private void OnDisable() {
        inputReader.Steer -= OnSteer;
    }

    private void Start() {
        // Set the angle limit for the front wheels
        var limit = rightPivot.angularYLimit;
        limit.limit = maxSteerAngle;
        rightPivot.angularYLimit = limit;
        
        leftPivot.angularYLimit = limit;
        limit.limit = maxSteerAngle;
        rightPivot.angularYLimit = limit;
    }

    private void FixedUpdate() {
        // Rotate the steering wheels
        rb.AddTorque(transform.right * (_steer * steerForce * Time.fixedDeltaTime));
        
        // Apply steer to the front wheels
        ApplySteer();
    }

    private void ApplySteer() {
        var targetAngle = maxSteerAngle * _steer;
        
        rightPivot.targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
        leftPivot.targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
    }

    private void OnSteer(InputAction.CallbackContext ctx) {
        _steer = ctx.ReadValue<float>();
    }
}
