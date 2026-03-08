using UnityEngine;

namespace Final_Assignment {
    public class Ball : MonoBehaviour {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [Header("Refs")] 
        [SerializeField] private Renderer ballRenderer;
        
        [Header("Ball Settings")]
        public float radius = 0.11f;
        public float mass = 0.43f;
        public float restitution = 0.45f;
        public float groundFriction = 8f;
        public float airDrag = 0.15f;
        public float gravity = 22f;

        [Header("State")]
        public Vector3 velocity;
        public bool grounded;
        
        [Header("Spin")]
        public Vector3 angularVelocity;
        public float magnusStrength = 0.08f;
        public float spinDrag = 1.2f;
        public float maxSpin = 80f;
        
        [Header("Colors")]
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.red;
        
        private Vector3 _pos;
        public Vector3 Position => _pos;
        
        public bool IsActive { get; private set; }
        
        public bool Consumed { get; private set; }
        public void MarkConsumed() => Consumed = true;
        public void ClearConsumed() => Consumed = false;
        
        private MaterialPropertyBlock _mpb;

        private void Awake() {
            _pos = transform.position;
            _mpb = new MaterialPropertyBlock();
        }

        void LateUpdate() {
            if (!IsActive) return;

            float dt = Time.deltaTime;
            Simulate(dt);
            transform.position = _pos;
        }
        
        public void SetActiveBall(bool value) {
            IsActive = value;

            if (!value) {
                velocity = Vector3.zero;
                angularVelocity = Vector3.zero;
                SetColor(inactiveColor);
            }
            else {
                SetColor(activeColor);
            }
        }

        private void SetColor(Color color) {
            ballRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColor, color);
            ballRenderer.SetPropertyBlock(_mpb);
        }

        public void AddImpulse(Vector3 impulse) {
            velocity += impulse / Mathf.Max(0.0001f, mass);
        }

        private void Simulate(float dt) {
            // Gravity
            velocity += Vector3.down * (gravity * dt);
            
            // Magnus effect
            ApplyMagnus(dt);

            // Drag
            float drag = grounded ? 0f : airDrag;
            velocity *= Mathf.Clamp01(1f - drag * dt);

            // Integrate
            _pos += velocity * dt;

            // Ground collision (plane at y=0)
            ResolveGround(0f);
            
            // spin decay
            angularVelocity *= Mathf.Clamp01(1f - spinDrag * dt);
        }
        
        public void CorrectPosition(Vector3 p) {
            _pos = p;
        }

        private void ResolveGround(float groundY) {
            float bottom = _pos.y - radius;

            if (bottom < groundY) {
                // push out
                _pos.y = groundY + radius;

                // bounce only if moving downward
                if (velocity.y < 0f)
                    velocity.y = -velocity.y * restitution;

                grounded = true;

                // ground friction on horizontal velocity
                Vector3 vXZ = new Vector3(velocity.x, 0f, velocity.z);
                float speed = vXZ.magnitude;

                if (speed > 0.0001f) {
                    float drop = groundFriction * Time.deltaTime; // stable enough; or pass dt in
                    float newSpeed = Mathf.Max(0f, speed - drop);
                    vXZ = vXZ.normalized * newSpeed;

                    velocity.x = vXZ.x;
                    velocity.z = vXZ.z;
                }
            }
            else {
                grounded = false;
            }
        }
        
        private void ApplyMagnus(float dt) {
            // Only meaningful when moving
            float speed = velocity.magnitude;
            if (speed < 0.05f) return;

            // Optional: reduce effect near/along ground (you can keep it always-on if you want)
            // if (grounded) return;

            // ω × v gives a direction; scale by speed for stronger curve at higher speeds
            Vector3 magnusAccel = Vector3.Cross(angularVelocity, velocity) * magnusStrength;

            // Safety clamp so it doesn't explode
            const float maxAccel = 60f;
            if (magnusAccel.sqrMagnitude > maxAccel * maxAccel)
                magnusAccel = magnusAccel.normalized * maxAccel;

            velocity += magnusAccel * dt;

            // Clamp spin for stability
            if (angularVelocity.magnitude > maxSpin)
                angularVelocity = angularVelocity.normalized * maxSpin;
        }
    }
}
