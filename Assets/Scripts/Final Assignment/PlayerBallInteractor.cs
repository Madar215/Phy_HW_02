using UnityEngine;

namespace Final_Assignment {
    public class PlayerBallInteractor : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private BallRackManager rack;
        [SerializeField] private PlayerController player;

        [Header("Player Body Collider")]
        [SerializeField] private float playerRadius = 0.35f;

        [Header("Body Push Tuning")] 
        [SerializeField] private float pushStrength = 6f;
        [SerializeField] private float maxPushSpeed = 3f;
        
        private Ball _ball;

        private void Update() {
            _ball = rack ? rack.CurrentBall : null;
            if (!_ball || !_ball.IsActive) return;

            ResolveBodyPush();
        }

        private void ResolveBodyPush() {
            if(!_ball || !_ball.IsActive) return;
            
            // 2D circle collision
            Vector2 p = new Vector2(transform.position.x, transform.position.z);
            Vector2 b = new Vector2(_ball.Position.x, _ball.Position.z);

            float r = playerRadius + _ball.radius;
            Vector2 delta = b - p;
            float dist = delta.magnitude;

            if (dist >= r || dist < 0.0001f)
                return;

            Vector2 n = delta / dist;
            float penetration = r - dist;

            // Positional correction: move ball out
            Vector3 ballPos = _ball.Position;
            ballPos.x += n.x * penetration;
            ballPos.z += n.y * penetration;
            
            _ball.CorrectPosition(ballPos);
            
            Vector3 v = player.Velocity;
            Vector2 v2 = new Vector2(v.x, v.z);

            float alongNormal = Vector2.Dot(v2, n);
            if (alongNormal > 0f){ // only push if moving into the ball
                float pushV = Mathf.Min(alongNormal * pushStrength, maxPushSpeed);
                Vector3 impulse = new Vector3(n.x, 0f, n.y) * (pushV * _ball.mass);
                _ball.AddImpulse(impulse);
            }
        }
    }
}