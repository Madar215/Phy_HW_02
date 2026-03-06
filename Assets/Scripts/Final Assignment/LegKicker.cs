using UnityEngine;

namespace Final_Assignment {
    public class LegKicker : MonoBehaviour {
        public enum KickMode { GroundPass, Lob }

        [Header("Refs")]
        [SerializeField] private InputReader input;
        [SerializeField] private BallRackManager rack;
        [SerializeField] private PlayerController player;
        [SerializeField] private TrajectoryDebugger trajectory;

        [Header("Leg Collider")]
        [SerializeField] private Vector3 localCenter = new(0f, 0.5f, 0.7f);
        [SerializeField] private Vector3 halfExtents = new(0.25f, 0.35f, 0.25f);

        [Header("Kick Tuning")]
        [SerializeField] private float passImpulse = 3.5f;
        [SerializeField] private float lobImpulse = 4.0f;
        [SerializeField] private float lobUpFactor = 0.9f;
        [SerializeField] private float kickCooldown = 0.25f;
        
        [Header("Spin From Kicks")]
        [SerializeField] private float sideSpinAmount = 35f;
        [SerializeField] private float topSpinAmount = 25f;
        [SerializeField] private float backSpinAmount = 28f;

        private float _cd;

        private void OnEnable() {
            input.KickPass += OnKickPass;
            input.KickLob  += OnKickLob;
        }

        private void OnDisable() {
            input.KickPass -= OnKickPass;
            input.KickLob  -= OnKickLob;
        }
        
        private void Update() {
            _cd -= Time.deltaTime;
        }
        
        private void OnKickPass() {
            TriggerKick(KickMode.GroundPass);
        }

        private void OnKickLob() {
            TriggerKick(KickMode.Lob);
        }
        
        public void TriggerKick(KickMode mode) {
            Ball ball = rack ? rack.CurrentBall : null;
            if (_cd > 0f || !ball || !ball.IsActive) return;

            if (BallOverlapsLegBox()) {
                ApplyKick(mode);
                _cd = kickCooldown;
            }
        }

        private bool BallOverlapsLegBox() {
            Ball ball = rack ? rack.CurrentBall : null;
            if (!ball || !ball.IsActive) return false;
            
            // Convert ball center into leg local space
            Vector3 ballLocal = transform.InverseTransformPoint(ball.Position);
            Vector3 d = ballLocal - localCenter;

            // Closest point on AABB in leg local space
            Vector3 clamped = new Vector3(
                Mathf.Clamp(d.x, -halfExtents.x, halfExtents.x),
                Mathf.Clamp(d.y, -halfExtents.y, halfExtents.y),
                Mathf.Clamp(d.z, -halfExtents.z, halfExtents.z)
            );

            Vector3 closestLocal = localCenter + clamped;
            Vector3 closestWorld = transform.TransformPoint(closestLocal);

            float distSq = (ball.Position - closestWorld).sqrMagnitude;
            return distSq <= ball.radius * ball.radius;
        }

        private void ApplyKick(KickMode mode) {
            Ball ball = rack ? rack.CurrentBall : null;
            if (!ball || !ball.IsActive) return;
            
            Vector3 fwd = transform.forward;

            // Where did we hit
            Vector3 toBall = ball.Position - transform.position;
            toBall.y = 0f;
            Vector3 side = Vector3.Cross(Vector3.up, fwd).normalized;

            float sideFactor = Mathf.Clamp(Vector3.Dot(toBall.normalized, side), -1f, 1f);

            float baseImpulse = mode == KickMode.GroundPass ? passImpulse : lobImpulse;

            // Airborne gets slightly stronger impulse to reward timing
            float timingBoost = ball.grounded ? 1f : 1.15f;

            Vector3 impulseDir = fwd;

            if (mode == KickMode.Lob) {
                impulseDir = (fwd + Vector3.up * lobUpFactor).normalized;
            }

            // Small side variation based on contact point
            impulseDir = (impulseDir + side * (0.15f * sideFactor)).normalized;

            Vector3 impulse = impulseDir * (baseImpulse * timingBoost * ball.mass);

            ball.AddImpulse(impulse);
            
            ApplySpinFromContact(mode, sideFactor);
            
            trajectory.DrawFrom(ball, ball.Position, ball.velocity, ball.angularVelocity, seconds: 2f, stepOverride: 0.02f);
        }
        
        private void ApplySpinFromContact(KickMode mode, float sideFactor) {
            Ball ball = rack ? rack.CurrentBall : null;
            if (!ball || !ball.IsActive) return;
            
            // Player basis
            Vector3 right = transform.right;
            Vector3 up = Vector3.up;

            // Side spin
            Vector3 sideSpinAxis = up;

            // Topspin/ backspin
            Vector3 tbAxis = right;

            // Decide top vs back depending on mode
            float tb;
            if (mode == KickMode.GroundPass)
                tb = +topSpinAmount;     // driven/dipping
            else
                tb = -backSpinAmount;    // lob/chip with backspin

            // Side spin stronger for "outside-foot"
            float sideSpin = sideSpinAmount * Mathf.Clamp(sideFactor, -1f, 1f);

            // Slightly boost spin when ball is airborne
            float timingBoost = ball.grounded ? 1f : 1.2f;

            Vector3 w =
                sideSpinAxis * (sideSpin * timingBoost) +
                tbAxis * (tb * timingBoost);

            // Add spin so repeated touches can accumulate a bit
            ball.angularVelocity += w;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(localCenter, halfExtents * 2f);
        }
    }
}
