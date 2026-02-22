using UnityEngine;

namespace Final_Assignment {
    public class OutOfBoundsAABB : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private BallRackManager rack;

        [Header("Bounds")]
        [SerializeField] private Vector3 halfExtents = new(18f, 5f, 28f);
        
        private Ball _ball;

        private void FixedUpdate() {
            if(!rack) return;
            
            _ball = rack.CurrentBall;
            if (!_ball || !_ball.IsActive) return;

            Vector3 local = transform.InverseTransformPoint(_ball.Position);

            bool outX = Mathf.Abs(local.x) > halfExtents.x;
            bool outZ = Mathf.Abs(local.z) > halfExtents.z;
            bool outY = local.y < -halfExtents.y || local.y > halfExtents.y;

            if (outX || outZ || outY) {
                if (_ball.Consumed) return;
                _ball.MarkConsumed();
                rack.ConsumeBallLost();
            }
        }
        
        private void OnDrawGizmos() {
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        }
    }
}