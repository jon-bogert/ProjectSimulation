using UnityEngine;

public class Climbable : MonoBehaviour
{
    [Header("Parameters")]
    
    [Header("References")]
    [SerializeField] Transform[] _gripPoints = new Transform[0];
    [SerializeField] Transform _topPoint = null;

    public bool isTop { get { return _topPoint != null; } }
}
