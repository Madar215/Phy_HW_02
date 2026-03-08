using UnityEngine;

namespace Final_Assignment {
    public class TrajectoryDebugger : MonoBehaviour {
        [Header("Prediction Settings")]
        [SerializeField] private float predictSeconds = 2.0f;
        [SerializeField] private float step = 0.02f;

        [Header("Ground Settings")]
        [SerializeField] private float groundY;

        [Header("Draw Settings")] 
        [SerializeField] private float drawDuration;
        [SerializeField] private Color drawColor = Color.yellow;
        
        public void DrawFrom(Ball ball, Vector3 startPos, Vector3 startVel, Vector3 startAngularVel,
            float? seconds = null, float? stepOverride = null) {
            float T = seconds ?? predictSeconds;
            float dt = stepOverride ?? step;

            SimAndDraw(ball, startPos, startVel, startAngularVel, T, dt);
        }

        private void SimAndDraw(Ball ball, Vector3 startPos, Vector3 startVel, Vector3 startW, float seconds, float dt) {
            // Copy state so prediction doesn't affect real game state
            Vector3 pos = startPos;
            Vector3 vel = startVel;
            Vector3 w = startW;

            // Pull parameters from ball if available, else use defaults
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
                // Gravity
                vel += Vector3.down * (gravity * dt);

                // Magnus curve
                if (vel.sqrMagnitude > 0.0025f){ // speed > ~0.05
                    Vector3 magnusAccel = Vector3.Cross(w, vel) * magnusStrength;
                    vel += magnusAccel * dt;
                }

                // Air drag
                vel *= Mathf.Clamp01(1f - airDrag * dt);

                // Integrate
                pos += vel * dt;

                // Ground collision (plane)
                float bottom = pos.y - radius;
                if (bottom < groundY) {
                    // Push out
                    pos.y = groundY + radius;

                    // Bounce only if falling
                    if (vel.y < 0f)
                        vel.y = -vel.y * restitution;

                    // Ground friction on horizontal
                    Vector3 vXZ = new Vector3(vel.x, 0f, vel.z);
                    float speed = vXZ.magnitude;
                    if (speed > 0.0001f) {
                        float newSpeed = Mathf.Max(0f, speed - groundFriction * dt);
                        vXZ = vXZ.normalized * newSpeed;
                        vel.x = vXZ.x;
                        vel.z = vXZ.z;
                    }
                }

                // Spin decay
                w *= Mathf.Clamp01(1f - spinDrag * dt);

                // Draw segment
                Debug.DrawLine(prev, pos, drawColor, drawDuration);
                prev = pos;
            }
        }
    }
}
