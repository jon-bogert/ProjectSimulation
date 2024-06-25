using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleHook : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] float _grappleSpeed = 1f;

    [Header("Physics")]
    [SerializeField] float _maxDistance = 10f;
    [SerializeField] float _castExtends = 1f;
    [SerializeField] LayerMask _grappleMask = 0;

    [Header("Inputs")]
    [SerializeField] InputActionReference _grappleInput;

    [Header("Refrences")]
    [SerializeField] Transform _shootPoint;
    [SerializeField] Transform _hook;
    [SerializeField] LineRenderer _cableRenderer;
    [SerializeField] GameObject _reticle;

    bool _isMoving = false;
    Transform _grappleTarget = null;
    PlayerMovement _movement;

    float _timer = 0f;
    float _endTime = 0f;
    float _invEndTime = 0f;
    Vector3 _startPos = Vector3.zero;

    private void Awake()
    {
        if (_grappleMask == 0)
            Debug.LogWarning("Grapple Hook mask is set to None");
    }

    private void Start()
    {
        _movement = GetComponentInParent<PlayerMovement>();
        if (_movement == null)
            Debug.LogError("Grapple Gun Couldn't find Player Movement Component in parent");
        _cableRenderer.gameObject.SetActive(false);
        _reticle.transform.SetParent(null);
    }

    private void OnEnable()
    {
        _reticle.SetActive(true);
    }

    private void Update()
    {
        if (_isMoving)
        {
            MoveUpdate();
            return;
        }

        CheckUpdate();
    }

    private void OnDisable()
    {
        _reticle.SetActive(false);
    }

    private void MoveUpdate()
    {
        if (_timer >= _endTime)
        {
            _timer = _endTime;
            ArriveAdjust();
            return;
        }

        if (_grappleInput.action.ReadValue<float>() <= 0.1)
        {
            _isMoving = false;
            _grappleTarget = null;
            _movement.movementOverride = false;
            _cableRenderer.gameObject.SetActive(false);
            return;
        }

        Vector3 pos = Vector3.Lerp(_startPos, _grappleTarget.position, _timer * _invEndTime);
        MovePlayerByTargetPoint(pos);

        _cableRenderer.SetPosition(0, _shootPoint.position);

        _timer += Time.deltaTime;
    }

    private void CheckUpdate()
    {
        RaycastHit[] hitInfo = Physics.RaycastAll(_shootPoint.position, _shootPoint.forward, _maxDistance, _grappleMask);
        if (hitInfo.Length <= 0)
        {
            _reticle.transform.position = _shootPoint.position + _shootPoint.forward * _maxDistance;
            _grappleTarget = null;
            return;
        }

        float dist = float.MaxValue;
        RaycastHit closest = hitInfo[0];
        foreach (RaycastHit hit in hitInfo)
            if (hit.distance < dist)
                closest = hit;


        GrapplePoint point = closest.collider.GetComponent<GrapplePoint>();
        if (point == null)
        {
            _reticle.transform.position = closest.point;
            _grappleTarget = null;
            return;
        }

        _reticle.transform.position = closest.transform.position;
        _grappleTarget = point.point;

        if (_grappleInput.action.ReadValue<float>() > 0.1f)
        {
            _isMoving = true;
            float distance = (_grappleTarget.position - _shootPoint.position).magnitude;

            _endTime = distance / _grappleSpeed;
            _invEndTime = 1f/ _endTime;
            _timer = 0f;
            _startPos = _shootPoint.position;
            _movement.movementOverride = true;
            _cableRenderer.gameObject.SetActive(true);
            _cableRenderer.SetPosition(1, _grappleTarget.position);
        }
    }

    private void ArriveAdjust()
    {
        _cableRenderer.gameObject.SetActive(false);

        if (_grappleInput.action.ReadValue<float>() <= 0.1f)
        {
            _isMoving = false;
            _grappleTarget = null;
            _movement.movementOverride = false;
            return;
        }

        MovePlayerByTargetPoint(_grappleTarget.position);
    }

    private void MovePlayerByTargetPoint(Vector3 point)
    {
        Vector3 shootPointOffset = _shootPoint.position - _movement.transform.position;
        Vector3 playerTargetPoint = point - shootPointOffset;
        Vector3 displacement = playerTargetPoint - _movement.transform.position;
        _movement.AbsoluteMove(displacement);
    }
}
