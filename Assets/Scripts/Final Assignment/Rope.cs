using UnityEngine;

namespace Final_Assignment {
    public class Rope : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private LineRenderer line;
        
        [Header("Rope Setup")]
        [SerializeField] private Transform anchor;
        [SerializeField] private int segmentCount = 20;
        [SerializeField] private float segmentLength = 0.35f;

        [Header("Simulation")]
        [SerializeField] private float gravity = 22f;
        [SerializeField] private float damping = 0.002f;
        [SerializeField] private int constraintIterations = 15;

        [Header("Collision")]
        [SerializeField] private float groundY;

        private Vector3[] _pos;
        private Vector3[] _prevPos;

        public int Count => _pos.Length;
        public Vector3 GetPoint(int i) => _pos[i];

        private void Awake() {
            Init();
        }

        private void Init() {
            _pos = new Vector3[segmentCount];
            _prevPos = new Vector3[segmentCount];

            Vector3 start = anchor.position;

            for (int i = 0; i < segmentCount; i++) {
                Vector3 p = start + Vector3.down * (i * segmentLength);
                _pos[i] = p;
                _prevPos[i] = p;
            }
        }

        private void Update() {
            Simulate(Time.deltaTime);
            DrawDebug();
        }
        
        private void LateUpdate() {
            UpdateVisual();
        }

        private void Simulate(float dt) {
            // Verlet integration
            for (int i = 1; i < _pos.Length; i++) {
                Vector3 current = _pos[i];
                Vector3 velocity = (current - _prevPos[i]) * (1f - damping);

                _prevPos[i] = current;

                Vector3 acceleration = Vector3.down * gravity;

                _pos[i] = current + velocity + acceleration * (dt * dt);
            }

            // Constraints solver
            for (int iteration = 0; iteration < constraintIterations; iteration++) {
                // Anchor fixed
                _pos[0] = anchor.position;

                for (int i = 0; i < _pos.Length - 1; i++) {
                    SolveDistanceConstraint(i, i + 1);
                }

                // Ground collision
                for (int i = 1; i < _pos.Length; i++) {
                    if (_pos[i].y < groundY)
                        _pos[i].y = groundY;
                }
            }
        }

        private void SolveDistanceConstraint(int i, int j) {
            Vector3 p1 = _pos[i];
            Vector3 p2 = _pos[j];

            Vector3 delta = p2 - p1;
            float dist = delta.magnitude;
            if (dist < 0.0001f) return;

            float error = dist - segmentLength;
            Vector3 correction = delta.normalized * error;

            if (i == 0) {
                // First point fixed (anchor)
                _pos[j] -= correction;
            }
            else {
                _pos[i] += correction * 0.5f;
                _pos[j] -= correction * 0.5f;
            }
        }
        
        private void UpdateVisual() {
            if (!line) return;

            line.positionCount = _pos.Length;

            for (int i = 0; i < _pos.Length; i++) {
                line.SetPosition(i, _pos[i]);
            }
        }

        private void DrawDebug() {
            for (int i = 0; i < _pos.Length - 1; i++) {
                Debug.DrawLine(_pos[i], _pos[i + 1], Color.yellow);
            }
        }
    }
}