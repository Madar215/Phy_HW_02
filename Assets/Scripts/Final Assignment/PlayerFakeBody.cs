using UnityEngine;

namespace Final_Assignment {
    public class PlayerFakeBody : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private LineRenderer line;
        
        [Header("Lengths")]
        [SerializeField] private float torsoLength = 0.8f;
        [SerializeField] private float headLength = 0.4f;
        [SerializeField] private float legLength = 1.0f;

        [Header("Physics")]
        [SerializeField] private float gravity = 22f;
        [SerializeField] private int constraintIterations = 8;
        
        [Header("Settings")]
        [SerializeField] private float groundY;
        [SerializeField] private float jointRadius = 0.08f;
        
        // Body Points
        private Vector3 _pelvis;
        private Vector3 _torso;
        private Vector3 _head;
        private Vector3 _leftLeg;
        private Vector3 _rightLeg;

        private Vector3 _prevPelvis;
        private Vector3 _prevTorso;
        private Vector3 _prevHead;
        private Vector3 _prevLeftLeg;
        private Vector3 _prevRightLeg;

        private bool _isFalling;
        private float _fallTimer;

        public Vector3 PelvisPosition => _pelvis;

        private void Start() {
            _pelvis = transform.position;
            _torso = _pelvis + Vector3.up * torsoLength;
            _head = _torso + Vector3.up * headLength;
            _leftLeg = _pelvis + Vector3.left * 0.2f - Vector3.up * legLength;
            _rightLeg = _pelvis + Vector3.right * 0.2f - Vector3.up * legLength;

            _prevPelvis = _pelvis;
            _prevTorso = _torso;
            _prevHead = _head;
            _prevLeftLeg = _leftLeg;
            _prevRightLeg = _rightLeg;
        }

        private void FixedUpdate() {
            if (_isFalling) {
                _fallTimer -= Time.fixedDeltaTime;
                if (_fallTimer <= 0f) {
                    _isFalling = false;

                    // snap player back to pelvis and re-enable motor
                    transform.position = _pelvis;
                    var temp = transform.position;
                    temp.y = 1f;
                    transform.position = temp;
                    if (playerController) playerController.enabled = true;
                    return;
                }
            }
            
            if (!_isFalling) {
                _pelvis = transform.position;
                _torso  = _pelvis + Vector3.up * torsoLength;
                _head   = _torso + Vector3.up * headLength;
                _leftLeg  = _pelvis + Vector3.left * 0.2f - Vector3.up * legLength;
                _rightLeg = _pelvis + Vector3.right * 0.2f - Vector3.up * legLength;

                _prevPelvis = _pelvis;
                _prevTorso = _torso;
                _prevHead = _head;
                _prevLeftLeg = _leftLeg;
                _prevRightLeg = _rightLeg;
                return;
            }

            float dt = Time.fixedDeltaTime;

            SimulatePoint(ref _pelvis, ref _prevPelvis, dt);
            SimulatePoint(ref _torso, ref _prevTorso, dt);
            SimulatePoint(ref _head, ref _prevHead, dt);
            SimulatePoint(ref _leftLeg, ref _prevLeftLeg, dt);
            SimulatePoint(ref _rightLeg, ref _prevRightLeg, dt);

            for (int i = 0; i < constraintIterations; i++) {
                SolveDistance(ref _pelvis, ref _torso, torsoLength);
                SolveDistance(ref _torso, ref _head, headLength);
                SolveDistance(ref _pelvis, ref _leftLeg, legLength);
                SolveDistance(ref _pelvis, ref _rightLeg, legLength);
            }
            
            ApplyGroundCollision(ref _pelvis, ref _prevPelvis);
            ApplyGroundCollision(ref _torso, ref _prevTorso);
            ApplyGroundCollision(ref _head, ref _prevHead);
            ApplyGroundCollision(ref _leftLeg, ref _prevLeftLeg);
            ApplyGroundCollision(ref _rightLeg, ref _prevRightLeg);

            DrawDebug();
        }

        private void LateUpdate() {
            if (!line) return;

            // pelvis->torso->head is one chain, legs are separate lines,
            // simplest: draw a single polyline: leftLeg->pelvis->rightLeg and pelvis->torso->head
            line.positionCount = 5;
            line.SetPosition(0, _leftLeg);
            line.SetPosition(1, _pelvis);
            line.SetPosition(2, _rightLeg);
            line.SetPosition(3, _torso);
            line.SetPosition(4, _head);
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
        
        private void ApplyGroundCollision(ref Vector3 pos, ref Vector3 prev) {
            if (pos.y < groundY + jointRadius) {
                pos.y = groundY + jointRadius;

                // remove downward velocity
                if (prev.y < pos.y)
                    prev.y = pos.y;
            }
        }

        private void DrawDebug() {
            Debug.DrawLine(_pelvis, _torso, Color.green);
            Debug.DrawLine(_torso, _head, Color.green);
            Debug.DrawLine(_pelvis, _leftLeg, Color.green);
            Debug.DrawLine(_pelvis, _rightLeg, Color.green);
        }
    }
}