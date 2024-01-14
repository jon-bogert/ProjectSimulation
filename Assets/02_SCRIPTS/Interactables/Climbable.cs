using UnityEngine;

public class Climbable : MonoBehaviour
{
    [Header("Parameters")]
    
    [Header("References")]
    [SerializeField] Transform[] _gripPoints = new Transform[0];
    [SerializeField] Transform _topPoint = null;

    public bool isTop { get { return _topPoint != null; } }
    public Vector3 topPosition
    {
        get
        {
            if (_topPoint is null)
            {
                Debug.LogError("Top Point was null");
                return Vector3.zero;
            }
            return _topPoint.position;
        }
    }

    public Transform GetClosestGrip(Vector3 position)
    {
        if (_gripPoints.Length == 0)
        {
            Debug.LogError("Climbable.GetClosestGrip -> Array of grip points was empty");
            return null;
        }

        Transform closest = _gripPoints[0];
        float sqrDist = (closest.position - position).sqrMagnitude;
        for (int i = 1;  i < _gripPoints.Length; ++i)
        {
            float newSqrDist = (_gripPoints[i].position - position).sqrMagnitude;
            if (newSqrDist < sqrDist)
            {
                sqrDist = newSqrDist;
                closest = _gripPoints[i];
            }
        }
        return closest;
    }

    private void OnDrawGizmos()
    {

        if (_topPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(_topPoint.position, new Vector3(0.25f, 0, 0.25f));
        }
    }
}
