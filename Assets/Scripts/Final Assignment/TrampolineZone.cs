using UnityEngine;

namespace Final_Assignment {
    public class TrampolineZone : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private BallRackManager rack;

        [Header("Zone (manual AABB in local space)")]
        [SerializeField] private Vector3 halfExtents = new(2f, 0.2f, 2f);

        [Header("Bounce Tuning")]
        [SerializeField] private float trampolineRestitution = 1.35f; // strong elastic
        [SerializeField] private float normalBoost = 8f;              // extra upward pop
        [SerializeField] private float tangentialPreserve = 0.95f;    // keep some sideways
        [SerializeField] private float cooldown = 0.08f;

        private float _cd;

        private void FixedUpdate() {
            _cd -= Time.deltaTime;

            var ball = rack ? rack.CurrentBall : null;
            if (!ball || !ball.IsActive) return;

            Vector3 local = transform.InverseTransformPoint(ball.Position);

            // Only if within XZ
            if (Mathf.Abs(local.x) > halfExtents.x + ball.radius) return;
            if (Mathf.Abs(local.z) > halfExtents.z + ball.radius) return;

            // Check if ball bottom is below trampoline top (local y = +halfExtents.y)
            float topY = halfExtents.y;
            float bottom = local.y - ball.radius;

            if (bottom < topY && _cd <= 0f) {
                // Snap ball to surface
                local.y = topY + ball.radius;
                ball.CorrectPosition(transform.TransformPoint(local));

                // Bounce: reflect along up normal
                Vector3 v = ball.velocity;

                // Only bounce if moving downward into it
                if (v.y < 0f) {
                    float incoming = -v.y; // how hard we hit vertically

                    Vector3 tang = new Vector3(v.x, 0f, v.z) * tangentialPreserve;
                    float outY = incoming * trampolineRestitution + (incoming * 0.15f) + normalBoost;

                    ball.velocity = tang + Vector3.up * outY;
                }

                _cd = cooldown;
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        }
    }
}