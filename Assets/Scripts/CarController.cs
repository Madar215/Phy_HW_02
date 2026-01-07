using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody rb;

    [Header("Forces")] 
    [SerializeField] private float moveSpeed = 10f;
    
    private float _move;

    private void OnEnable() {
        inputReader.Drive += OnDrive;
    }

    private void OnDisable() {
        inputReader.Drive -= OnDrive;
    }

    private void FixedUpdate() {
        // Calculate the forward direction force and apply it to the car
        var targetDir = transform.right * (_move * moveSpeed * Time.fixedDeltaTime);
        rb.AddForce(targetDir, ForceMode.VelocityChange);
    }

    private void OnDrive(InputAction.CallbackContext context) {
        _move = context.ReadValue<float>();
    }
}
