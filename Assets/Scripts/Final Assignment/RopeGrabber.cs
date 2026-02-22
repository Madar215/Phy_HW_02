using UnityEngine;

namespace Final_Assignment {
    public class RopeGrabber : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private VerletRope rope;
        [SerializeField] private PlayerController playerController;

        [Header("Grab Settings")]
        [SerializeField] private float grabRadius = 0.6f;
        [SerializeField] private float followStrength = 35f;
        [SerializeField] private float maxPullSpeed = 14f;

        private bool _grabbing;
        private int _grabIndex = -1;

        private void OnEnable() { 
            inputReader.GrabRope += ToggleGrab;
        }

        private void OnDisable() { 
            inputReader.GrabRope -= ToggleGrab;
        }

        private void Update() {
            if (!rope) return;

            if (_grabbing)
                ApplyGrabConstraint();
        }

        private void ToggleGrab() {
            if (_grabbing) Release();
            else TryGrabNearest();
        }

        private void TryGrabNearest() {
            int best = -1;
            float bestSq = grabRadius * grabRadius;

            Vector3 p = transform.position;

            for (int i = 1; i < rope.Count; i++) {
                Vector3 rp = rope.GetPoint(i);
                float sq = (rp - p).sqrMagnitude;
                if (sq < bestSq) {
                    bestSq = sq;
                    best = i;
                }
            }

            if (best != -1) {
                _grabbing = true;
                _grabIndex = best;
            }
        }

        private void Release() {
            _grabbing = false;
            _grabIndex = -1;
        }

        private void ApplyGrabConstraint() {
            Vector3 target = rope.GetPoint(_grabIndex);
            Vector3 delta = target - transform.position;

            Vector3 pullVel = delta * (followStrength * Time.deltaTime);

            if (pullVel.magnitude > maxPullSpeed)
                pullVel = pullVel.normalized * maxPullSpeed;

            if (playerController != null) playerController.AddExternalVelocity(pullVel);
            else transform.position += pullVel * Time.deltaTime;
        }
    }
}