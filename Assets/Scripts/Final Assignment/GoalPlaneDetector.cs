using UnityEngine;

namespace Final_Assignment {
    public class GoalPlaneDetector : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private BallRackManager rack;

        [Header("Goal Plane")]
        [SerializeField] private Transform goalPlane;
        [SerializeField] private float goalWidth = 3f;
        [SerializeField] private float goalHeight = 2f;

        // Track last frame side
        private bool _initialized;
        private float _prevSignedDist;

        private Ball _ball;

        private Ball _prevBall;

        private void FixedUpdate() {
            if (!rack) return;

            _ball = rack.CurrentBall;
            if (!_ball || !_ball.IsActive) return;

            if (_ball != _prevBall) {
                _prevBall = _ball;
                _initialized = false;
            }

            Vector3 n = -goalPlane.forward;
            float signedDist = Vector3.Dot(_ball.Position - goalPlane.position, n);

            if (!_initialized) {
                _initialized = true;
                _prevSignedDist = signedDist;
                return;
            }

            bool crossedPlane = _prevSignedDist > 0f && signedDist < 0f;
            if (crossedPlane && IsInsideGoalmouth(_ball.Position)) {
                if (!_ball.Consumed) {
                    _ball.MarkConsumed();
                    rack.ConsumeBallGoal();
                    _initialized = false;
                }
            }

            _prevSignedDist = signedDist;
        }

        private bool IsInsideGoalmouth(Vector3 worldPos) {
            Vector3 local = goalPlane.InverseTransformPoint(worldPos);
            return Mathf.Abs(local.x) <= goalWidth * 0.5f
                   && local.y >= 0f
                   && local.y <= goalHeight;
        }

        private void OnDrawGizmosSelected() {
            if (!goalPlane) return;
            Gizmos.matrix = goalPlane.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, goalHeight * 0.5f, 0f), new Vector3(goalWidth, goalHeight, 0.02f));
        }
    }
}