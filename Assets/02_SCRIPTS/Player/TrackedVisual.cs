using UnityEngine;

public class TrackedVisual : MonoBehaviour
{
    public delegate void CollisionEvent(Collision other);
    public delegate void TriggerEvent(Collider other);

    [Range(0f, 1f)]
    [SerializeField] float smoothAmount = 0.25f;
    [SerializeField] Transform _homeTransform;

    public CollisionEvent collisionEntered;
    public CollisionEvent collisionExited;
    public TriggerEvent triggerEntered;
    public TriggerEvent triggerExited;

    private void Awake()
    {
        GameLoader.CallOnComplete(Init);
    }

    private void Init()
    {
        ResetDestination();
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
