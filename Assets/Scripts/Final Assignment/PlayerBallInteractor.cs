using UnityEngine;

namespace Final_Assignment {
    public class PlayerBallInteractor : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private ManualBall ball;
        [SerializeField] private PlayerController player;

        [Header("Player Body Collider (manual)")]
        [SerializeField] private float playerRadius = 0.35f;

        [Header("Body Push Tuning")]
        [SerializeField] private float pushStrength = 6f;      // keep small (non-exploit)
        [SerializeField] private float maxPushSpeed = 3f;      // cap body pushing effect

        private void Update() {
            if (!ball) return;

            ResolveBodyPush();
        }

        private void ResolveBodyPush() {
            // 2D (XZ) circle collision
            Vector2 p = new Vector2(transform.position.x, transform.position.z);
            Vector2 b = new Vector2(ball.Position.x, ball.Position.z);

            float r = playerRadius + ball.radius;
            Vector2 delta = b - p;
            float dist = delta.magnitude;

            if (dist >= r || dist < 0.0001f)
                return;

            Vector2 n = delta / dist;
            float penetration = r - dist;

            // Positional correction: move ball out (NOT player)
            Vector3 ballPos = ball.Position;
            ballPos.x += n.x * penetration;
            ballPos.z += n.y * penetration;
            
            ball.CorrectPosition(ballPos);
            
            Vector3 v = player.Velocity;
            Vector2 v2 = new Vector2(v.x, v.z);

            float alongNormal = Vector2.Dot(v2, n);
            if (alongNormal > 0f){ // only push if moving into the ball
                float pushV = Mathf.Min(alongNormal * pushStrength, maxPushSpeed);
                Vector3 impulse = new Vector3(n.x, 0f, n.y) * (pushV * ball.mass);
                ball.AddImpulse(impulse);
            }

        }
    }
}