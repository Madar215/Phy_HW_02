using UnityEngine;

public class CollisionManager : MonoBehaviour {
    [SerializeField] private float breakMagnitude = 20f;
    [SerializeField] private ConfigurableJoint connectedJoint;
    
    private void OnCollisionEnter(Collision other) {
        var colMagnitude = other.relativeVelocity.magnitude;

        if (!(colMagnitude > breakMagnitude)) return;
        if (!connectedJoint) return;
                
        Destroy(connectedJoint);
        connectedJoint = null;
    }
}
