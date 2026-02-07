using System.Collections.Generic;
using UnityEngine;

namespace HW_03 {
    public class SystemDefense : MonoBehaviour {
        [Header("Refs")]
        [SerializeField] private BowAndArrowController bow;
        [SerializeField] private TargetObject targetObj;
        [SerializeField] private Interceptor interceptor;

        [Header("Settings")] 
        [SerializeField] private int pointsAmountToCheck = 20;

        [SerializeField] private int[] stepMultipliers = { 1, 2, 5, 10 };

        // thresholds for classification (out of 4)
        [SerializeField] private int mightHitMin = 1;      // >=1 run predicts hit
        [SerializeField] private int probablyHitMin = 3;   // >=3 runs predict hit

        private bool _decided;

        private Vector3 _localHalf;

        private bool _interceptorFired;

        private void Update() {
            if (_decided) return;

            float baseStep = bow.GetTrajectoryStep();

            // We only decide after launch & on the way down
            if (!bow.IsFlying) return;
            if (bow.CurrentVelocity.y >= 0) return;

            int hitVotes = 0;
            // For every step we'll calculate the trajectory point
            foreach (int m in stepMultipliers) {
                float step = baseStep * m;

                var points = bow.ComputeTrajectory(
                    bow.arrow.position,
                    bow.CurrentVelocity,
                    bow.trajectoryMaxPoints,
                    step
                );

                if (TrajectoryLikelyHits(points))
                    hitVotes++;
            }

            if (hitVotes < mightHitMin) {
                // clearly miss
                _decided = true;
                return;
            }

            bool probablyHit = hitVotes >= probablyHitMin;
            bool mightHit = hitVotes >= mightHitMin;

            // Policy: choose what you want to intercept
            if (mightHit && !probablyHit) {
                Debug.Log($"Might hit (votes {hitVotes}/{stepMultipliers.Length}) - ignoring");
                _decided = true;
                return;
            }
        
            Debug.Log($"Probably or Will hit (votes {hitVotes}/{stepMultipliers.Length}) - firing");

            var gravity = bow.gravity;
            var drag = bow.airDrag;
            interceptor.Init(bow.arrow, gravity, drag);

            _decided = true;
        }
    
        private bool TrajectoryLikelyHits(List<Vector3> points) {
            if (points == null || points.Count < pointsAmountToCheck)
                return false;
            // Check from the last point in the list outwards
            for (int i = 0; i < pointsAmountToCheck; i++) {
                int j = points.Count - 1 - i;
                if (CheckIfHitTarget(points[j]))
                    return true;
            }

            return false;
        }

        private bool CheckIfHitTarget(Vector3 impactPos) {
            // Get the target Transform
            Transform targetObjTransform = targetObj.transform;
        
            // Invert the impact pos to local space
            Vector3 localPoint = targetObjTransform.InverseTransformPoint(impactPos);
        
            // Get the target's half size
            Vector3 worldHalf = new Vector3(targetObj.SizeX, targetObj.SizeY, targetObj.SizeZ) * 0.5f;
        
            // Get the target lossy scale
            Vector3 targetLossyScale = targetObjTransform.lossyScale;
            _localHalf = new Vector3(
                worldHalf.x / Mathf.Abs(targetLossyScale.x),
                worldHalf.y / Mathf.Abs(targetLossyScale.y),
                worldHalf.z / Mathf.Abs(targetLossyScale.z)
            );
        
            // Check if the impact pos is inside the target "collider"
            return Mathf.Abs(localPoint.x) <= _localHalf.x &&
                   Mathf.Abs(localPoint.y) <= _localHalf.y &&
                   Mathf.Abs(localPoint.z) <= _localHalf.z;
        }
    }
}
