using UnityEngine;

public class TargetObject : MonoBehaviour {
    [field: SerializeField] public float sizeX = 2f;
    [field: SerializeField] public float sizeY = 0.5f;
    [field: SerializeField] public float sizeZ = 2f;

    private void Awake() {
        transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
    }
}
