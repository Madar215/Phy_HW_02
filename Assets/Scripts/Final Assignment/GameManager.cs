using UnityEngine;
using UnityEngine.Events;

namespace Final_Assignment {
    public class GameManager : MonoBehaviour {
        public event UnityAction OnGameOver;
        
        [Header("Gameplay")]
        [SerializeField] private float matchSeconds = 120f;
        
        private bool _isGameOver;
        private float _elapsed;

        private void Update() {
            if (_isGameOver) return;
            
            _elapsed += Time.deltaTime;
            if (_elapsed >= matchSeconds) {
                // TODO: Implement game over
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