using TMPro;
using UnityEngine;

namespace Final_Assignment {
    public class UIManager : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BallRackManager rack;
        
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI ballsLeftText;
        [SerializeField] private TextMeshProUGUI goalsText;

        private void OnEnable() {
            rack.OnGoal += SetGoalsText;
            rack.OnBallUsed += SetBallsRemaining;
        }

        private void OnDisable() {
            rack.OnGoal -= SetGoalsText;
            rack.OnBallUsed -= SetBallsRemaining;
        }

        private void Update() {
            timeText.text = Mathf.RoundToInt(gameManager.Elapsed).ToString();
        }

        private void SetBallsRemaining(int newValue) {
            ballsLeftText.text = newValue.ToString();
        }

        private void SetGoalsText(int newValue) {
            goalsText.text = newValue.ToString();
        }
    }
}