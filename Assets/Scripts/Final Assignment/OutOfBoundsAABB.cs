using UnityEngine;

namespace Final_Assignment {
    public class OutOfBoundsAABB : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private BallRackManager rack;

        [Header("Bounds")]
        [SerializeField] private Vector3 halfExtents = new(18f, 5f, 28f);

        private void FixedUpdate() {
            var ball = rack ? rack.CurrentBall : null;
            if (!ball || !ball.IsActive) return;

            Vector3 local = transform.InverseTransformPoint(ball.Position);

            bool outX = Mathf.Abs(local.x) > halfExtents.x;
            bool outZ = Mathf.Abs(local.z) > halfExtents.z;
            bool outY = local.y < -halfExtents.y || local.y > halfExtents.y;

            if (outX || outZ || outY) {
                rack.ConsumeBallLost();
            }
        }

        
        private void OnDrawGizmos() {
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        }
    }
}