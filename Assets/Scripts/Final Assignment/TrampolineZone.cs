using UnityEngine;

namespace Final_Assignment {
    public class TrampolineZone : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private BallRackManager rack;
        [SerializeField] private PlayerController player;

        [Header("Zone")]
        [SerializeField] private Vector3 halfExtents = new(2f, 0.2f, 2f);

        [Header("Bounce Tuning")]
        [SerializeField] private float trampolineRestitution = 1.35f;
        [SerializeField] private float normalBoost = 8f;
        [SerializeField] private float tangentialPreserve = 0.95f;
        [SerializeField] private float cooldown = 0.08f;

        [Header("Player Bounce")]
        [SerializeField] private float playerFootTolerance = 0.1f;
        [SerializeField] private float playerExtraBoost = 2f;
        [SerializeField] private float maxBounceHeight = 10f;

        private float _ballCd;
        private float _playerCd;

        private void FixedUpdate() {
            float dt = Time.fixedDeltaTime;
            _ballCd -= dt;
            _playerCd -= dt;

            HandleBall();
            HandlePlayer();
        }

        private void HandleBall() {
            var ball = rack ? rack.CurrentBall : null;
            if (!ball || !ball.IsActive) return;

            Vector3 local = transform.InverseTransformPoint(ball.Position);

            if (Mathf.Abs(local.x) > halfExtents.x + ball.radius) return;
            if (Mathf.Abs(local.z) > halfExtents.z + ball.radius) return;

            float topY = halfExtents.y;
            float bottom = local.y - ball.radius;

            if (bottom > topY || _ballCd > 0f) return;

            Vector3 v = ball.velocity;
            if (v.y >= 0f) return;

            local.y = topY + ball.radius;
            ball.CorrectPosition(transform.TransformPoint(local));

            float incoming = -v.y;
            Vector3 tang = new Vector3(v.x, 0f, v.z) * tangentialPreserve;
            float outY = incoming * trampolineRestitution + (incoming * 0.15f) + normalBoost;

            ball.velocity = tang + Vector3.up * outY;
            _ballCd = cooldown;
        }

        private void HandlePlayer() {
            if (!player) return;

            Vector3 playerPos = player.transform.position;
            Vector3 localRoot = transform.InverseTransformPoint(playerPos);

            // XZ overlap
            if (Mathf.Abs(localRoot.x) > halfExtents.x) return;
            if (Mathf.Abs(localRoot.z) > halfExtents.z) return;

            float topY = transform.position.y + halfExtents.y;

            // Player "feet" height in world space
            float playerFeetY = player.transform.position.y - player.StandingHeightAboveGround;

            // Must be near the trampoline top
            bool touchingTop = playerFeetY <= topY + playerFootTolerance;
            if (!touchingTop || _playerCd > 0f) return;

            Vector3 v = player.Velocity;

            // Bounce only if moving down or grounded on it
            if (v.y > 0f && !player.IsGrounded) return;
            
            // Calculate the desired jump "force"
            float incoming = Mathf.Max(0f, -v.y);
            Vector3 tang = new Vector3(v.x, 0f, v.z) * tangentialPreserve;
            float outY = incoming * trampolineRestitution + normalBoost + playerExtraBoost;
            
            // Clamp it to a maximum height
            outY = Mathf.Clamp(outY, 0f, maxBounceHeight);

            // Snap root so feet sit just above trampoline
            Vector3 p = player.transform.position;
            p.y = topY + player.StandingHeightAboveGround + 0.02f;
            player.transform.position = p;

            // Replace current velocity with bounced version
            Vector3 targetVelocity = tang + Vector3.up * outY;
            player.AddExternalVelocity(targetVelocity - v);

            _playerCd = cooldown;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        }
    }
}