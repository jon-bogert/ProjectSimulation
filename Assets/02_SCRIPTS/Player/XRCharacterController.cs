using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(CapsuleCollider))]
public class XRCharacterController : MonoBehaviour
{
    [Header("Referenences")]
    [SerializeField] Transform _camera;
    [SerializeField] GroundCheck _groundCheck;

    //Components
    Rigidbody _rigidbody;
    CapsuleCollider _capsuleCollider;

    public GroundEvent onLand;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        if (_groundCheck == null)
            Debug.LogError("XRCharacterController -> Ground Check Component Reference was null");

        _groundCheck.onLand += OnLand;
    }

    private void FixedUpdate()
    {
        UpdateColliders();
    }

    private void OnDestroy()
    {
        if (_groundCheck != null)
            _groundCheck.onLand -= OnLand;
    }

    private void UpdateColliders()
    {
        _capsuleCollider.height = _camera.position.y - transform.position.y + _capsuleCollider.radius;

        //Set Offset of capsule collider
        Vector3 center = Vector3.up * (_capsuleCollider.height * 0.5f + (0.5f * _capsuleCollider.radius));
        center.x = _camera.position.x - transform.position.x;
        center.z = _camera.position.z - transform.position.z;
        _capsuleCollider.center = center;

        //Set Offset of Ground Check
        center.y = 0f;
        _groundCheck.transform.localPosition = center;
    }

    public void MoveWithVelocity(Vector2 velocity)
    {
        _rigidbody.velocity = new Vector3(
            velocity.x,
            _rigidbody.velocity.y,
            velocity.y);
    }

    public void SetJumpVelocity(float velocity)
    {
        _rigidbody.velocity = new Vector3(
            _rigidbody.velocity.x,
            velocity,
            _rigidbody.velocity.z);
    }

    private void OnLand()
    {
        onLand?.Invoke();
    }
}
