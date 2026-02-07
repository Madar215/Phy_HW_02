using UnityEngine;

namespace HW_03 {
    public class TargetObject : MonoBehaviour {
        [field: SerializeField] public float SizeX { get; private set; } = 2f;
        [field: SerializeField] public float SizeY { get; set; } = 0.5f;
        [field: SerializeField] public float SizeZ { get; set; } = 2f;

        private void Awake() {
            transform.localScale = new Vector3(SizeX, SizeY, SizeZ);
        }
    }
}
