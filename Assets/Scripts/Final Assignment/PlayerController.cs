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
        
        [Header("Joints")]
        [SerializeField] private Transform pelvisJoint;
        [SerializeField] private Transform torsoJoint;
        [SerializeField] private Transform headJoint;
        [SerializeField] private Transform leftLegJoint;
        [SerializeField] private Transform rightLegJoint;
        [SerializeField] private float torsoLength = 0.8f;
        [SerializeField] private float headLength = 0.4f;
        [SerializeField] private float legLength = 1.0f;
        [SerializeField] private float legsDistant = 0.4f;
        [SerializeField] private int constraintIterations = 6;
        
        [Header("Ground Settings")]
        [SerializeField] private float standingHeightAboveGround = 1f;
        [SerializeField] private float gravity = 22f;
        [SerializeField] private float groundY;
        [SerializeField] private float jumpSpeed = 8f;
        
        // Input
        private Vector2 _moveInput;
        private bool _jumpPressed;
        
        // Ground
        private bool _isGrounded;
        
        // Forces
        private Vector3 _velocity;
        private Vector3 _externalVelocity;
        
        // Tackle
        private bool _isTackled;
        private float _tackleTimer;
        
        // Joints
        private Vector3 _pelvis, _torso, _head, _leftLeg, _rightLeg;
        private Vector3 _prevPelvis, _prevTorso, _prevHead, _prevLeftLeg, _prevRightLeg;
        
        // Cached variables
        private float _dt;
        
        // Properties
        public Vector3 Velocity => _velocity;
        public Vector3 PelvisPosition => _pelvis;
        public float StandingHeightAboveGround => standingHeightAboveGround;
        public bool IsGrounded => _isGrounded;
        
        private void OnEnable() {
            input.Move += OnMove;
            input.Jump += OnJump;
        }

        private void OnDisable() {
            input.Move -= OnMove;
            input.Jump -= OnJump;
        }
        
        private void OnMove(InputAction.CallbackContext ctx) {
            _moveInput = ctx.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext ctx) {
            if(ctx.started) _jumpPressed = true;
        }

        private void Update() {
            if (!_isTackled) return;

            _tackleTimer -= Time.deltaTime;
            if (_tackleTimer <= 0f) {
                _isTackled = false;
                
                ResetJoints();

                ResetPrev();
                ApplyToTransforms();

                // Also good: clear motion so you don't “slide” from old velocity
                _velocity = Vector3.zero;
            }
        }

        private void FixedUpdate() {
            _dt = Time.fixedDeltaTime;

            if (!_isTackled) {
                ApplyMovement();

                _pelvis = transform.position;
                _torso = _pelvis + Vector3.up * torsoLength;
                _head = _torso + Vector3.up * headLength;
                _leftLeg = _pelvis + Vector3.left * legsDistant - Vector3.up * legLength;
                _rightLeg = _pelvis + Vector3.right * legsDistant - Vector3.up * legLength;

                ResetPrev();
                ApplyToTransforms();
                return;
            }

            SimulateTackle();
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
        
        private void SimulateTackle() {
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
        }
        
        private void ResetJoints() {
            Vector3 p = _pelvis;

            // Keep player on the ground/standing height
            p.y = groundY + standingHeightAboveGround;
            transform.position = p;
                
            _pelvis = transform.position;
            _torso = _pelvis + Vector3.up * torsoLength;
            _head = _torso + Vector3.up * headLength;
            _leftLeg = _pelvis + Vector3.left * 0.2f - Vector3.up * legLength;
            _rightLeg = _pelvis + Vector3.right * 0.2f - Vector3.up * legLength;
        }

        private void ApplyMovement() {
            // Desired movement
            Vector3 desiredDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);
            desiredDirection.Normalize();
            
            // Desired horizontal velocity
            Vector3 horizontalVelocity = new Vector3(_velocity.x, 0f, _velocity.z);
            
            // Accelerate or decelerate the player
            if (desiredDirection.sqrMagnitude > 0.001f) {
                horizontalVelocity += desiredDirection * (acceleration * _dt);
            }
            else {
                float speed = horizontalVelocity.magnitude;
                speed -= deceleration * _dt;
                speed = Mathf.Max(speed, 0f);
                horizontalVelocity = speed > 0f ? horizontalVelocity.normalized * speed : Vector3.zero;
            }
            
            // clamp velocity to a max speed
            if (horizontalVelocity.magnitude > maxSpeed)
                horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            
            // Apply it to the velocity
            _velocity.x = horizontalVelocity.x;
            _velocity.z = horizontalVelocity.z;

            // Apply external forces first (rope or trampoline)
            _velocity += _externalVelocity;
            _externalVelocity = Vector3.zero;

            float groundHeight = groundY + standingHeightAboveGround;

            // Ground check before jump
            _isGrounded = transform.position.y <= groundHeight + 0.01f;

            // Jump
            if (_jumpPressed && _isGrounded && !_isTackled) {
                _velocity.y = jumpSpeed;
                _isGrounded = false;
            }
            _jumpPressed = false;

            // Gravity
            if (!_isGrounded || _velocity.y > 0f) {
                _velocity.y -= gravity * _dt;
            }

            // Apply movement
            transform.position += _velocity * _dt;

            // Ground collision
            Vector3 pos = transform.position;
            if (pos.y < groundHeight) {
                pos.y = groundHeight;
                transform.position = pos;

                if (_velocity.y < 0f)
                    _velocity.y = 0f;

                _isGrounded = true;
            }

            // Rotate from horizontal movement
            if (horizontalVelocity.sqrMagnitude > 0.001f) {
                Quaternion targetRot = Quaternion.LookRotation(horizontalVelocity.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    turnSpeed * _dt
                );
            }
        }

        private void SimulatePoint(ref Vector3 pos, ref Vector3 prev, float dt) {
            Vector3 vel = pos - prev;
            prev = pos;
            pos += vel;
            pos += Vector3.down * (gravity * dt * dt);
        }
        
        private static void SolveDistance(ref Vector3 a, ref Vector3 b, float length) {
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