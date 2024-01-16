using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [SerializeField] Transform _point;

    public Transform point { get { return _point; } }
}
