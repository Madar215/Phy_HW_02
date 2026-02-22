using UnityEngine;
using UnityEngine.InputSystem;

namespace Final_Assignment {
    public class PlayerController : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private InputReader input;

        [Header("Movement Settings")]
        [SerializeField] private float maxSpeed = 8f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 25f;
        [SerializeField] private float turnSpeed = 10f;

        [Header("Tackle")]
        [SerializeField] private float tackleDuration = 1.2f;
        
        [Header("Fake Joints")]
        [SerializeField] private Transform pelvisJoint;
        [SerializeField] private Transform torsoJoint;
        [SerializeField] private Transform headJoint;
        [SerializeField] private Transform leftLegJoint;
        [SerializeField] private Transform rightLegJoint;

        [SerializeField] private float torsoLength = 0.8f;
        [SerializeField] private float headLength = 0.4f;
        [SerializeField] private float legLength = 1.0f;

        [SerializeField] private float gravity = 22f;
        [SerializeField] private int constraintIterations = 6;
        
        [SerializeField] private float groundY;
        
        // Velocity
        private Vector3 _velocity;
        private Vector2 _moveInput;
        private Vector3 _externalVelocity;
        
        // Tackle
        private bool _isTackled;
        private float _tackleTimer;
        
        // Joints
        private Vector3 _pelvis, _torso, _head, _leftLeg, _rightLeg;
        private Vector3 _prevPelvis, _prevTorso, _prevHead, _prevLeftLeg, _prevRightLeg;
        
        // Cached variables
        private float _dt;

        public bool IsTackled => _isTackled;
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
            if (!_isTackled) {
                _pelvis = transform.position;
                _torso = _pelvis + Vector3.up * torsoLength;
                _head = _torso + Vector3.up * headLength;
                _leftLeg = _pelvis + Vector3.left * 0.2f - Vector3.up * legLength;
                _rightLeg = _pelvis + Vector3.right * 0.2f - Vector3.up * legLength;

                ResetPrev();

                ApplyToTransforms();
                return;
            }
            
            _dt = Time.fixedDeltaTime;

            SimulatePoint(ref _pelvis, ref _prevPelvis, _dt);
            SimulatePoint(ref _torso, ref _prevTorso, _dt);
            SimulatePoint(ref _head, ref _prevHead, _dt);
            SimulatePoint(ref _leftLeg, ref _prevLeftLeg, _dt);
            SimulatePoint(ref _rightLeg, ref _prevRightLeg, _dt);

            for (int i = 0; i < constraintIterations; i++) {
                SolveDistance(ref _pelvis, ref _torso, torsoLength);
                SolveDistance(ref _torso, ref _head, headLength);
                SolveDistance(ref _pelvis, ref _leftLeg, legLength);
                SolveDistance(ref _pelvis, ref _rightLeg, legLength);
            }

            ApplyGround(ref _pelvis, ref _prevPelvis);
            ApplyGround(ref _torso, ref _prevTorso);
            ApplyGround(ref _head, ref _prevHead);
            ApplyGround(ref _leftLeg, ref _prevLeftLeg);
            ApplyGround(ref _rightLeg, ref _prevRightLeg);

            ApplyToTransforms();

            Vector3 desiredDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);
        
            if (desiredDirection.magnitude > 1f)
                desiredDirection.Normalize();

            if (desiredDirection.sqrMagnitude > 0.001f) {
                // Acceleration
                _velocity += desiredDirection * (acceleration * _dt);
            }
            else {
                // Deceleration
                float speed = _velocity.magnitude;
                speed -= deceleration * _dt;
                speed = Mathf.Max(speed, 0f);
                _velocity = _velocity.normalized * speed;
            }

            // Clamp max speed
            if (_velocity.magnitude > maxSpeed)
                _velocity = _velocity.normalized * maxSpeed;

            // Apply movement
            _velocity += _externalVelocity;
            _externalVelocity = Vector3.zero;
            transform.position += _velocity * _dt;

            // Rotate toward movement
            if (_velocity.sqrMagnitude > 0.001f) {
                Quaternion targetRot = Quaternion.LookRotation(_velocity.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    turnSpeed * _dt
                );
            }
        }
        
        public void TriggerTackle(Vector3 attackerPos) {
            if (_isTackled) return;

            _isTackled = true;
            _tackleTimer = tackleDuration;

            _velocity = Vector3.zero;

            _prevPelvis = _pelvis;
            _prevTorso = _torso;
            _prevHead = _head;
            _prevLeftLeg = _leftLeg;
            _prevRightLeg = _rightLeg;

            Vector3 shove = (_pelvis - attackerPos).normalized;
            _prevPelvis -= shove * 0.4f;
        }
        
        public void AddExternalVelocity(Vector3 v) {
            _externalVelocity += v;
        }
        
        private void SimulatePoint(ref Vector3 pos, ref Vector3 prev, float dt) {
            Vector3 vel = pos - prev;
            prev = pos;
            pos += vel;
            pos += Vector3.down * (gravity * dt * dt);
        }
        
        private void SolveDistance(ref Vector3 a, ref Vector3 b, float length) {
            Vector3 delta = b - a;
            float dist = delta.magnitude;
            if (dist < 0.0001f) return;

            float diff = (dist - length) / dist;
            a += delta * (diff * 0.5f);
            b -= delta * (diff * 0.5f);
        }

        private void ApplyGround(ref Vector3 pos, ref Vector3 prev) {
            if (pos.y < groundY) {
                pos.y = groundY;
                prev.y = pos.y;
            }
        }
        
        private void ApplyToTransforms() {
            pelvisJoint.position = _pelvis;
            torsoJoint.position = _torso;
            headJoint.position = _head;
            leftLegJoint.position = _leftLeg;
            rightLegJoint.position = _rightLeg;
        }
        
        private void ResetPrev() {
            _prevPelvis = _pelvis;
            _prevTorso = _torso;
            _prevHead = _head;
            _prevLeftLeg = _leftLeg;
            _prevRightLeg = _rightLeg;
        }
    }
}