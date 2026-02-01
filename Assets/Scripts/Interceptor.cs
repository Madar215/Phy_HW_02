using UnityEngine;

public class Interceptor : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private BowAndArrowController bowAndArrowController;
    [SerializeField] private TargetObject targetObj;

    private Vector3 _localHalf;

    private void Update() {
        if (bowAndArrowController.GetPredictedImpact(out Vector3 impactPos, out float impactSpeed)) {
            if (CheckIfHitTarget(impactPos)) {
                // TODO: Implement interception method
                Debug.Log("Hit target");
            }
        }
    }

    private bool CheckIfHitTarget(Vector3 impactPos) {
        // Get the target Transform
        Transform targetObjTransform = targetObj.transform;
        // Invert the impact pos to local space
        Vector3 localPoint = targetObjTransform.InverseTransformPoint(impactPos);
        // Get the target's half size
        Vector3 worldHalf = new Vector3(targetObj.sizeX, targetObj.sizeY, targetObj.sizeZ) * 0.5f;
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
