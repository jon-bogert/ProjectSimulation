using UnityEngine;

public delegate void GroundEvent();

[RequireComponent(typeof(BoxCollider))]
public class GroundCheck : MonoBehaviour
{
    public GroundEvent onLand;
    bool _isGrounded = false;
    private void OnCollisionEnter(Collision collision)
    {
        onLand?.Invoke();
        Debug.Log("ON_LAND");
    }
}
