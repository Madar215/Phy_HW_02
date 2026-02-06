using UnityEngine;

public class Interceptor : MonoBehaviour {
    [Header("Forces")] 
    [SerializeField] private float speed = 10f;

    private Vector3 _velocity;
    private Vector3 _gravity;
    private float _drag;

    private bool _isActive;
    private Transform _target;
    
    public void Init(Transform target, Vector3 gravity, float drag) {
        // Activate
        _isActive = true;
        
        // Set the target's transform to intercept
        _target = target;
        
        // Set velocity
        Vector3 dir = (_target.position - transform.position).normalized;
        _velocity = dir * speed;
        
        // Set gravity and drag
        _gravity = gravity;
        _drag = drag;
    }

    private void Update() {
        if(!_isActive) return;
        
        float dt = Time.deltaTime;
        
        ApplyDrag(ref _velocity, _drag, dt);
        // Calculate velocity and apply to position
        Vector3 dir = (_target.position - transform.position).normalized;
        _velocity = dir * speed;
        _velocity +=  _gravity * dt;
        transform.position += _velocity * dt;

        // “Kill” when we reach the planned intercept time (or close enough)
        if (Vector3.Distance(transform.position, _target.position) < 0.25f) {
            Debug.Log("[Interceptor] Intercept executed.");
        }
    }

    private static void ApplyDrag(ref Vector3 v, float drag, float dt) {
        if (drag <= 0f) return;
        
        float scale = Mathf.Clamp01(1f - drag * dt);
        v *= scale;
    }
}