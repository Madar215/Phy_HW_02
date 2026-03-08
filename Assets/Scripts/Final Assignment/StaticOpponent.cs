using UnityEngine;

namespace Final_Assignment {
    public class StaticOpponent : MonoBehaviour {
        [Header("Body AABB")]
        [SerializeField] private Vector3 halfExtents = new(0.5f, 1f, 0.5f);

        [Header("Ball Reflection")]
        [SerializeField] private float restitution = 0.6f;

        [Header("Tackle")]
        [SerializeField] private float tackleRadius = 1.2f;

        public void CheckBallCollision(Ball ball) {
            Vector3 localBall = transform.InverseTransformPoint(ball.Position);

            Vector3 closest = new Vector3(
                Mathf.Clamp(localBall.x, -halfExtents.x, halfExtents.x),
                Mathf.Clamp(localBall.y, -halfExtents.y, halfExtents.y),
                Mathf.Clamp(localBall.z, -halfExtents.z, halfExtents.z)
            );

            Vector3 closestWorld = transform.TransformPoint(closest);

            Vector3 delta = ball.Position - closestWorld;
            float distSq = delta.sqrMagnitude;

            if (distSq <= ball.radius * ball.radius) {
                Vector3 normal = delta.normalized;

                // push out
                ball.CorrectPosition(closestWorld + normal * ball.radius);

                // reflect velocity
                ball.velocity = Vector3.Reflect(ball.velocity, normal) * restitution;
            }
        }

        public bool ShouldTackle(Vector3 playerPos, out float sqrDistXZ) {
            Vector3 d = playerPos - transform.position;
            d.y = 0f;
            sqrDistXZ = d.sqrMagnitude;
            return sqrDistXZ <= tackleRadius * tackleRadius;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
            
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, tackleRadius);
        }
    }
}