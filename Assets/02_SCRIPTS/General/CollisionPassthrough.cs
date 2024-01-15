using UnityEngine;

public class CollisionPassthrough : MonoBehaviour
{
    public delegate void CollisionEvent(Collision other);
    public delegate void TriggerEvent(Collider other);

    public CollisionEvent collisionEntered;
    public CollisionEvent collisionExited;
    public TriggerEvent triggerEntered;
    public TriggerEvent triggerExited;

    private void OnCollisionEnter(Collision collision)
    {
        collisionEntered?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        collisionExited?.Invoke(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerEntered?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        triggerExited?.Invoke(other);
    }
}
