using System.Collections.Generic;
using UnityEngine;

public class SystemDefense : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private BowAndArrowController bow;
    [SerializeField] private TargetObject targetObj;
    [SerializeField] private Interceptor interceptor;

    [Header("Settings")] 
    [SerializeField] private int pointsAmountToCheck = 3;

    private Vector3 _localHalf;

    private bool _interceptorFired;

    private void Update() {
        // This make sure we only shot once
        if (_interceptorFired) return;
        
        // If the arrow wasn't even shot yet, return
        if (!bow.IsFlying) return;
        
        Transform target = bow.arrow;
        float step = bow.GetTrajectoryStep();
        
        // We want to intercept the arrow only on its way down - negative Y velocity
        if (bow.CurrentVelocity.y >= 0) return;
        
        // Compute trajectory
        List<Vector3> pointsTest = bow.ComputeTrajectory(bow.arrow.position,
            bow.CurrentVelocity,
            bow.trajectoryMaxPoints,
            step);
        
        if (pointsTest.Count < pointsAmountToCheck) return;
        for (int i = 0; i < pointsAmountToCheck; i++) {
            int j = pointsTest.Count - 1 - i;

            if (CheckIfHitTarget(pointsTest[j])) {
                var gravity = bow.gravity;
                var drag = bow.airDrag;
                // Intercept
                interceptor.Init(target, gravity, drag);
                
                _interceptorFired = true;
                break;
            }
        }
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
