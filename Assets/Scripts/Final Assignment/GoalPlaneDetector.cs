using UnityEngine;

namespace Final_Assignment {
    public class GoalPlaneDetector : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private BallRackManager rack;

        [Header("Goal Plane (manual)")]
        [Tooltip("A transform whose forward defines the goal plane normal.")]
        [SerializeField] private Transform goalPlane;
        [Tooltip("Width of the goal mouth in world units.")]
        [SerializeField] private float goalWidth = 3f;
        [Tooltip("Height under crossbar in world units.")]
        [SerializeField] private float goalHeight = 2f;

        // Track last frame side
        private bool _initialized;
        private float _prevSignedDist;

        private void FixedUpdate() {
            var ball = rack ? rack.CurrentBall : null;
            if (!ball || !ball.IsActive || !goalPlane) return;

            Vector3 p = ball.Position;

            // Signed distance to plane
            Vector3 n = goalPlane.forward.normalized;
            float d = Vector3.Dot(p - goalPlane.position, n);

            if (!_initialized) {
                _initialized = true;
                _prevSignedDist = d;
                return;
            }

            // We count a goal when the ball fully crosses from front->back:
            // i.e. distance goes from > radius to < -radius
            float r = ball.radius;
            bool crossed = (_prevSignedDist > r) && (d < -r);

            if (crossed) {
                // Check within goalmouth rectangle (in goalPlane local space)
                Vector3 local = goalPlane.InverseTransformPoint(p);

                bool insideWidth = Mathf.Abs(local.x) <= goalWidth * 0.5f;
                bool insideHeight = (local.y >= 0f) && (local.y <= goalHeight);

                if (insideWidth && insideHeight) {
                    rack.ConsumeBallGoal();
                    _initialized = false;
                    return;
                }
            }

            _prevSignedDist = d;
        }

        private void OnDrawGizmosSelected() {
            if (!goalPlane) return;
            Gizmos.matrix = goalPlane.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, goalHeight * 0.5f, 0f), new Vector3(goalWidth, goalHeight, 0.02f));
        }
    }
}