using UnityEngine;

namespace Final_Assignment {
    public class OpponentManager : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private BallRackManager rack;
        [SerializeField] private StaticOpponent[] opponents;
        [SerializeField] private PlayerFakeBody playerBody;
        [SerializeField] private PlayerController playerController;
        
        [Header("Settings")]
        [SerializeField] private float tackleCooldown = 0.8f;
        
        private Ball _ball;

        private float _tackleCd;

        private void FixedUpdate() {
            _tackleCd -= Time.fixedDeltaTime;

            _ball = rack.CurrentBall;
            if (_ball && _ball.IsActive) {
                foreach (var opponent in opponents)
                    opponent.CheckBallCollision(_ball);
            }

            if (_tackleCd > 0f) return;

            Vector3 p = playerBody.PelvisPosition;
            foreach (var opponent in opponents) {
                if (opponent.ShouldTackle(p, out _)) {
                    playerController.TriggerTackle(opponent.transform.position);
                    _tackleCd = tackleCooldown;
                    break;
                }
            }
        }
    }
}