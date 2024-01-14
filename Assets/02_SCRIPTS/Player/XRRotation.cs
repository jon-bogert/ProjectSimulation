using UnityEngine;
using UnityEngine.InputSystem;

public class XRRotation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] bool _useSnapTurn = true;

    [Header("Snap Turn")]
    [SerializeField] float _snapAmount = 30f;
    [SerializeField] float _snapResetValue = 0.8f;

    [Header("Smooth Turn")]
    [SerializeField] float _smoothMaxSpeed = 120f;

    [Header("Inputs")]
    [SerializeField] InputActionReference _turnInput;

    [Header("References")]
    [SerializeField] Transform _camera;

    bool _snapLatch = false;

    private void Update()
    {
        float axis = _turnInput.action.ReadValue<Vector2>().x;
        if (axis == 0f)
            return;

        if (_useSnapTurn)
            ApplySnap(axis);
        else
            ApplySmooth(axis);
    }

    private void ApplySnap(float axis)
    {
        if (_snapLatch && Mathf.Abs(axis) > _snapResetValue)
            return;

        if (_snapLatch)
        {
            _snapLatch = false;
            return;
        }

        if (Mathf.Abs(axis) <= _snapResetValue)
            return;

        _snapLatch = true;
        transform.RotateAround(_camera.transform.position, Vector3.up, (axis > 0) ? _snapAmount : -_snapAmount);
    }

    private void ApplySmooth(float axis)
    {
        transform.RotateAround(_camera.transform.position, Vector3.up, axis * _smoothMaxSpeed * Time.deltaTime);
    }
}
