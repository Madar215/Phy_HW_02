using UnityEngine;
using UnityEngine.InputSystem;

namespace Final_Assignment {
    public class PlayerController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private InputReader input;

        [Header("Movement Settings")]
        [SerializeField] private float maxSpeed = 8f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 25f;
        [SerializeField] private float turnSpeed = 10f;

        private Vector3 _velocity;
        private Vector2 _moveInput;
        private Vector3 _externalVelocity;
        
        public Vector3 Velocity => _velocity;
        
        private void OnEnable() {
            input.Move += OnMove;
        }

        private void OnDisable() {
            input.Move -= OnMove;
        }

        private void OnMove(InputAction.CallbackContext ctx) {
            _moveInput = ctx.ReadValue<Vector2>();
        }

        private void FixedUpdate() {
            float dt = Time.deltaTime;

            Vector3 desiredDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);
        
            if (desiredDirection.magnitude > 1f)
                desiredDirection.Normalize();

            if (desiredDirection.sqrMagnitude > 0.001f) {
                // Acceleration
                _velocity += desiredDirection * (acceleration * dt);
            }
            else {
                // Deceleration
                float speed = _velocity.magnitude;
                speed -= deceleration * dt;
                speed = Mathf.Max(speed, 0f);
                _velocity = _velocity.normalized * speed;
            }

            // Clamp max speed
            if (_velocity.magnitude > maxSpeed)
                _velocity = _velocity.normalized * maxSpeed;

            // Apply movement
            _velocity += _externalVelocity;
            _externalVelocity = Vector3.zero;
            transform.position += _velocity * dt;

            // Rotate toward movement
            if (_velocity.sqrMagnitude > 0.001f) {
                Quaternion targetRot = Quaternion.LookRotation(_velocity.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    turnSpeed * dt
                );
            }
        }
        
        public void AddExternalVelocity(Vector3 v) {
            _externalVelocity += v;
        }
    }
}