using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour {
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody rb;

    [Header("Forces")] 
    [SerializeField] private float moveSpeed = 10f;

    private Vector2 _move;
    private float _moveX;
    private float _moveY;

    private void OnEnable() {
        inputReader.Move += OnMove;
    }

    private void OnDisable() {
        inputReader.Move -= OnMove;
    }

    private void FixedUpdate() {
        var movement = new Vector3(_moveX, 0f, _moveY) * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + movement);
    }

    private void OnMove(InputAction.CallbackContext context) {
        _move = context.ReadValue<Vector2>();
        _moveX = _move.x;
        _moveY = _move.y;
    }
}
