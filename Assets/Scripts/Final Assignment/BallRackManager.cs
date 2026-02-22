using System.Collections.Generic;
using UnityEngine;

namespace Final_Assignment {
    public class BallRackManager : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Ball ballPrefab;
        
        [Header("Setup")]
        [SerializeField] private Transform rackOrigin;
        [SerializeField] private Vector3 rackSpacing = new(0.35f, 0f, 0f);
        [SerializeField] private int ballCount = 10;
        [SerializeField] private Transform activeBallSpawnPoint;

        public int AttemptsLeft => Mathf.Max(0, ballCount - _currentIndex);
        public int GoalsScored => _goals;

        private readonly List<Ball> _balls = new();
        private int _currentIndex;
        private int _goals;

        public Ball CurrentBall { get; private set; }

        private void Start() {
            SpawnRack();
            ActivateNextBall();
        }

        private void SpawnRack() {
            _balls.Clear();
            for (int i = 0; i < ballCount; i++) {
                Vector3 p = rackOrigin.position + rackSpacing * i;
                var b = Instantiate(ballPrefab, p, Quaternion.identity);
                b.SetActiveBall(false);
                _balls.Add(b);
            }
        }

        private void ResetCurrentBallToSpawn() {
            if (!CurrentBall) return;
            
            CurrentBall.CorrectPosition(activeBallSpawnPoint.position);
            CurrentBall.velocity = Vector3.zero;
            CurrentBall.angularVelocity = Vector3.zero;
        }

        public void ConsumeBallGoal() {
            _goals++;
            ConsumeAndAdvance();
        }

        public void ConsumeBallLost() {
            ConsumeAndAdvance();
        }

        private void ConsumeAndAdvance() {
            if (CurrentBall) {
                CurrentBall.SetActiveBall(false);
                CurrentBall = null;
            }

            ActivateNextBall();
        }

        private void ActivateNextBall() {
            // No attempts left
            if (_currentIndex >= _balls.Count) {
                gameManager.NoBallsLeft();
                return;
            }
            
            // Get the next ball and activate it
            CurrentBall = _balls[_currentIndex++];
            CurrentBall.SetActiveBall(true);
            
            ResetCurrentBallToSpawn();
        }
    }
}