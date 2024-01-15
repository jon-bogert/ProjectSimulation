using UnityEngine;

public class SelectionWheel : MonoBehaviour
{
    [SerializeField] CollisionPassthrough[] _slices;
    [SerializeField] Transform _hand;
    [SerializeField] Transform _handTrackingPoint; // Managed by this script
    [SerializeField] LineRenderer _line;
    [SerializeField] float _lineMaxLength = 0.1f;

    int _sliceIndex = -1;
    bool _active = false;

    private void Awake()
    {
        foreach (var slice  in _slices)
        {
            slice.transform.parent.gameObject.SetActive(false);
        }
        _line.gameObject.SetActive(false);
        _handTrackingPoint.gameObject.SetActive(false);

        GameLoader.CallOnComplete(Init);
    }

    private void Init()
    {
        if (_slices.Length != 6)
        {
            Debug.LogError("Slice Array is not the correct length, Should be: 6");
            return;
        }

        _slices[0].triggerEntered   += Enter0;
        _slices[0].triggerExited    += Exit0;
        _slices[1].triggerEntered   += Enter1;
        _slices[1].triggerExited    += Exit1;
        _slices[2].triggerEntered   += Enter2;
        _slices[2].triggerExited    += Exit2;
        _slices[3].triggerEntered   += Enter3;
        _slices[3].triggerExited    += Exit3;
        _slices[4].triggerEntered   += Enter4;
        _slices[4].triggerExited    += Exit4;
        _slices[5].triggerEntered   += Enter5;
        _slices[5].triggerExited    += Exit5;
    }

    private void Update()
    {
        if (!_active)
            return;

        _handTrackingPoint.position = _hand.position;
        _handTrackingPoint.localPosition = new Vector3(
            _handTrackingPoint.localPosition.x,
            _handTrackingPoint.localPosition.y,
            0f);
        if (_handTrackingPoint.localPosition.sqrMagnitude > _lineMaxLength * _lineMaxLength)
        {
            _handTrackingPoint.localPosition = _handTrackingPoint.localPosition.normalized;
            _handTrackingPoint.localPosition = _handTrackingPoint.localPosition * _lineMaxLength;
        }

        _line.SetPosition(0, transform.position);
        _line.SetPosition(1, _handTrackingPoint.position);

    }

    private void OnDestroy()
    {
        if (_slices[0] != null)
        {
            _slices[0].triggerEntered -= Enter0;
            _slices[0].triggerExited -= Exit0;
        }
        if (_slices[1] != null)
        {
            _slices[1].triggerEntered -= Enter1;
            _slices[1].triggerExited -= Exit1;
        }
        if (_slices[2] != null)
        {
            _slices[2].triggerEntered -= Enter2;
            _slices[2].triggerExited -= Exit2;
        }
        if (_slices[3] != null)
        {
            _slices[3].triggerEntered -= Enter3;
            _slices[3].triggerExited -= Exit3;
        }
        if (_slices[4] != null)
        {
            _slices[4].triggerEntered -= Enter4;
            _slices[4].triggerExited -= Exit4;
        }
        if (_slices[5] != null)
        {
            _slices[5].triggerEntered -= Enter5;
            _slices[5].triggerExited -= Exit5;
        }
    }

    public void Activate(Vector3 position, Transform lookAt)
    {
        foreach (var slice in _slices)
        {
            slice.transform.parent.gameObject.SetActive(true);
        }
        _line.gameObject.SetActive(true);
        _handTrackingPoint.gameObject.SetActive(true);

        _active = true;
        transform.position = position;
        Vector3 lookFinal = new Vector3(
            lookAt.position.x,
            transform.position.y,
            lookAt.position.z);
        transform.LookAt(lookAt);
    }

    /// <summary>
    /// Returns the current collision slice index
    /// </summary>
    /// <returns></returns>
    public int Deactivate()
    {
        int result = _sliceIndex;
        _sliceIndex = -1;

        foreach (var slice in _slices)
        {
            slice.transform.parent.gameObject.SetActive(false);
        }
        _line.gameObject.SetActive(false);
        _handTrackingPoint.gameObject.SetActive(false);

        return result;
    }

    // Collisions
    private void Enter0(Collider collider)
    {
        _sliceIndex = 0;
    }

    private void Exit0(Collider collider)
    {
        if (_sliceIndex == 0)
            _sliceIndex = -1;
    }

    private void Enter1(Collider collider)
    {
        _sliceIndex = 1;
    }

    private void Exit1(Collider collider)
    {
        if (_sliceIndex == 1)
            _sliceIndex = -1;
    }

    private void Enter2(Collider collider)
    {
        _sliceIndex = 2;
    }

    private void Exit2(Collider collider)
    {
        if (_sliceIndex == 2)
            _sliceIndex = -1;
    }

    private void Enter3(Collider collider)
    {
        _sliceIndex = 3;
    }

    private void Exit3(Collider collider)
    {
        if (_sliceIndex == 3)
            _sliceIndex = -1;
    }

    private void Enter4(Collider collider)
    {
        _sliceIndex = 4;
    }

    private void Exit4(Collider collider)
    {
        if (_sliceIndex == 4)
            _sliceIndex = -1;
    }

    private void Enter5(Collider collider)
    {
        _sliceIndex = 5;
    }

    private void Exit5(Collider collider)
    {
        if (_sliceIndex == 5)
            _sliceIndex = -1;
    }
}
