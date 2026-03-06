using UnityEngine;
using UnityEngine.Events;

namespace Final_Assignment {
    public class GameManager : MonoBehaviour {
        public event UnityAction OnGameOver;
        
        [Header("Gameplay")]
        [SerializeField] private float matchSeconds = 120f;
        
        private bool _isGameOver;
        public float Elapsed { get; private set; }

        private void Start() {
            Elapsed = matchSeconds;
        }

        private void Update() {
            if (_isGameOver) return;
            
            Elapsed -= Time.deltaTime;
            if (Elapsed <= 0f) {
                _isGameOver = true;
                OnGameOver?.Invoke();
            }
        }

        public void NoBallsLeft() {
            _isGameOver = true;
            OnGameOver?.Invoke();
        }
    }
}