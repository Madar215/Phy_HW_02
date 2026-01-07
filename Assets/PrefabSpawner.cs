using UnityEngine;
using UnityEngine.InputSystem;

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnPoint;

    void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            Spawn();
        }
    }

    void Spawn()
    {
        if (!prefab) return;

        Transform t = spawnPoint ? spawnPoint : transform;
        Instantiate(prefab, t.position, t.rotation);
    }
}
