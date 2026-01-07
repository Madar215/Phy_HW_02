using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour {
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody rb;

    [Header("Forces")] 
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float wheelSpinTorque = 20f;

    [SerializeField] private Rigidbody frontWheelRight;
    [SerializeField] private Rigidbody frontWheelLeft;
    [SerializeField] private Rigidbody rearWheelRight;
    [SerializeField] private Rigidbody rearWheelLeft;
    
    private float _move;

    private void OnEnable() {
        inputReader.Drive += OnDrive;
    }

    private void OnDisable() {
        inputReader.Drive -= OnDrive;
    }

    private void FixedUpdate() {
        rb.AddForce(new Vector3(_move, 0f, 0f) * (moveSpeed * Time.fixedDeltaTime), ForceMode.VelocityChange);
    }

    private void OnDrive(InputAction.CallbackContext context) {
        _move = context.ReadValue<float>();
    }
}
