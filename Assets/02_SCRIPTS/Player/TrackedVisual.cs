using UnityEngine;

public class TrackedVisual : MonoBehaviour
{
    public delegate void CollisionEvent(Collision other);
    public delegate void TriggerEvent(Collider other);
    public delegate void BasicEvent();
    public delegate void AdjustEvent(Vector3 visualPosition, Vector3 homePosition);

    [SerializeField] Transform _homeTransform;
    [SerializeField] float _maxHomeDistance = 0.5f;

    public CollisionEvent collisionEntered;
    public CollisionEvent collisionExited;
    public TriggerEvent triggerEntered;
    public TriggerEvent triggerExited;
    public BasicEvent distanceReseted;
    public AdjustEvent displacementAdjust;

    private void Awake()
    {
        GameLoader.CallOnComplete(Init);
    }

    private void Init()
    {
        ResetDestination();
    }

    private void Update()
    {
        if (transform.parent == _homeTransform)
            return;

        Vector3 displacement = transform.position - _homeTransform.position;
        if (displacement.sqrMagnitude > (_maxHomeDistance * _maxHomeDistance))
        {
            ResetDestination();
            distanceReseted?.Invoke();
        }
    }
    private void LateUpdate()
    {
        if (transform.parent == _homeTransform)
            return;
        
        displacementAdjust?.Invoke(transform.position, _homeTransform.position);
    }

    public void SetDestination(Transform destination)
    {
        transform.SetParent(destination);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void ResetDestination()
    {
        transform.SetParent(_homeTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        collisionEntered.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        collisionExited.Invoke(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerEntered.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        triggerExited?.Invoke(other);
    }
}
