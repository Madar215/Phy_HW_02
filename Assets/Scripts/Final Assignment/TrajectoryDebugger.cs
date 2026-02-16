using UnityEngine;

namespace Final_Assignment {
    public class TrajectoryDebugger : MonoBehaviour {
        [Header("Refs (optional)")]
        [SerializeField] private ManualBall ball; // optional convenience reference

        [Header("Prediction Settings")]
        [SerializeField] private float predictSeconds = 2.0f;
        [SerializeField] private float step = 0.02f; // smaller = smoother, more lines

        [Header("Ground Settings")]
        [SerializeField] private float groundY = 0f;

        [Header("Draw Settings")]
        [SerializeField] private bool drawAlways = false;  // if true: draws every frame from ball state
        [SerializeField] private float drawDuration = 0f;  // Debug.DrawLine duration; 0 = one frame

        private bool _requestedOnce;

        /// <summary>
        /// Call this right after a kick to draw the next 1-2 seconds.
        /// </summary>
        public void RequestDrawOnce() {
            _requestedOnce = true;
        }

        /// <summary>
        /// Draw from explicit parameters (recommended to call right after kick).
        /// </summary>
        public void DrawFrom(Vector3 startPos, Vector3 startVel, Vector3 startAngularVel,
            float? seconds = null, float? stepOverride = null) {
            float T = seconds ?? predictSeconds;
            float dt = stepOverride ?? step;

            SimAndDraw(startPos, startVel, startAngularVel, T, dt);
        }

        private void Update() {
            if (drawAlways && ball) {
                DrawFrom(ball.Position, ball.velocity, ball.angularVelocity);
            }
            else if (_requestedOnce && ball) {
                _requestedOnce = false;
                DrawFrom(ball.Position, ball.velocity, ball.angularVelocity);
            }
        }

        private void SimAndDraw(Vector3 startPos, Vector3 startVel, Vector3 startW, float seconds, float dt) {
            // copy state so prediction doesn't affect real game state
            Vector3 pos = startPos;
            Vector3 vel = startVel;
            Vector3 w = startW;

            // pull parameters from ball if available, else use defaults
            float gravity = ball ? ball.gravity : 22f;
            float airDrag = ball ? ball.airDrag : 0.15f;
            float restitution = ball ? ball.restitution : 0.45f;
            float groundFriction = ball ? ball.groundFriction : 8f;
            float magnusStrength = ball ? ball.magnusStrength : 0.08f;
            float spinDrag = ball ? ball.spinDrag : 1.2f;
            float radius = ball ? ball.radius : 0.11f;

            int steps = Mathf.Max(1, Mathf.CeilToInt(seconds / dt));

            Vector3 prev = pos;

            // Use a fixed dt simulation for determinism.
            for (int i = 0; i < steps; i++) {
                // --- forces / accelerations ---
                // gravity
                vel += Vector3.down * (gravity * dt);

                // magnus (curve): a ∝ ω × v
                if (vel.sqrMagnitude > 0.0025f){ // speed > ~0.05
                    Vector3 magnusAccel = Vector3.Cross(w, vel) * magnusStrength;
                    vel += magnusAccel * dt;
                }

                // air drag
                vel *= Mathf.Clamp01(1f - airDrag * dt);

                // integrate
                pos += vel * dt;

                // ground collision (plane)
                float bottom = pos.y - radius;
                if (bottom < groundY) {
                    // push out
                    pos.y = groundY + radius;

                    // bounce only if falling
                    if (vel.y < 0f)
                        vel.y = -vel.y * restitution;

                    // ground friction on horizontal
                    Vector3 vXZ = new Vector3(vel.x, 0f, vel.z);
                    float speed = vXZ.magnitude;
                    if (speed > 0.0001f) {
                        float newSpeed = Mathf.Max(0f, speed - groundFriction * dt);
                        vXZ = vXZ.normalized * newSpeed;
                        vel.x = vXZ.x;
                        vel.z = vXZ.z;
                    }
                }

                // spin decay
                w *= Mathf.Clamp01(1f - spinDrag * dt);

                // draw segment
                Debug.DrawLine(prev, pos, Color.white, drawDuration);
                prev = pos;
            }
        }
    }
}
