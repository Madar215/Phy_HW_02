using UnityEngine;

namespace Final_Assignment {
    public class VerletRope : MonoBehaviour {
        [Header("Rope Setup")]
        public Transform anchor;
        public int segmentCount = 18;
        public float segmentLength = 0.35f;

        [Header("Simulation")]
        public float gravity = 22f;
        public float damping = 0.01f;
        public int constraintIterations = 10;

        [Header("Debug")]
        public bool drawDebug = true;

        private Vector3[] _pos;
        private Vector3[] _prev;

        public int Count => _pos?.Length ?? 0;
        public Vector3 GetPoint(int i) => _pos[i];
        public void SetPoint(int i, Vector3 p) => _pos[i] = p;

        private void Awake() {
            Init();
        }

        private void Init() {
            _pos = new Vector3[segmentCount];
            _prev = new Vector3[segmentCount];

            Vector3 start = anchor ? anchor.position : transform.position;

            for (int i = 0; i < segmentCount; i++)
            {
                Vector3 p = start + Vector3.down * (i * segmentLength);
                _pos[i] = p;
                _prev[i] = p;
            }
        }

        private void FixedUpdate() {
            float dt = Time.deltaTime;
            Simulate(dt);
            if (drawDebug) Draw();
        }

        private void Simulate(float dt) {
            if (_pos == null || _pos.Length == 0) return;
            if (anchor) _pos[0] = anchor.position;

            // verlet integrate
            for (int i = 1; i < _pos.Length; i++) {
                Vector3 current = _pos[i];
                Vector3 vel = (current - _prev[i]) * (1f - damping);

                _prev[i] = current;

                // gravity
                Vector3 accel = Vector3.down * gravity;

                _pos[i] = current + vel + accel * (dt * dt);
            }

            // constraints
            for (int it = 0; it < constraintIterations; it++) {
                if (anchor) _pos[0] = anchor.position;

                for (int i = 0; i < _pos.Length - 1; i++) {
                    Vector3 p1 = _pos[i];
                    Vector3 p2 = _pos[i + 1];

                    Vector3 delta = p2 - p1;
                    float dist = delta.magnitude;
                    if (dist < 0.0001f) continue;

                    float diff = (dist - segmentLength) / dist;

                    if (i == 0) {
                        // p1 fixed
                        _pos[i + 1] = p2 - delta * diff;
                    }
                    else {
                        // split correction
                        _pos[i]     = p1 + delta * (diff * 0.5f);
                        _pos[i + 1] = p2 - delta * (diff * 0.5f);
                    }
                }
            }
        }

        private void Draw() {
            for (int i = 0; i < _pos.Length - 1; i++) {
                Debug.DrawLine(_pos[i], _pos[i + 1], Color.yellow);
            }
        }
    }
}