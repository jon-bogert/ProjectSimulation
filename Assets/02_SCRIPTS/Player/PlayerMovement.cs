using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof(XRCharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] float _moveSpeed = 3f;
    [SerializeField] float _jumpVelocity = 5f;
    [Header("Inputs")]
    [SerializeField] InputActionReference _moveInput;
    [SerializeField] InputActionReference _jumpInput;

    XRCharacterController _charController;

    Vector2 _moveAxis = Vector2.zero;
    bool _canJump = true;

    // ===== Unity Events =====
    private void Awake()
    {
        _charController = GetComponent<XRCharacterController>();

        _jumpInput.action.performed += OnJumpInput;
        _charController.onLand += OnLand;
    }

    private void FixedUpdate()
    {
        _moveAxis = _moveInput.action.ReadValue<Vector2>();
        if (_moveAxis.sqrMagnitude > 0f)
        {
            _charController.MoveWithVelocity(_moveAxis * _moveSpeed);
        }
    }

    private void OnDestroy()
    {
        _jumpInput.action.performed -= OnJumpInput;
        if (_charController != null)
            _charController.onLand -= OnLand;
    }

    // ===== Member Functions =====

    private void OnLand()
    {
        _canJump = true;
    }

    private void CallJump()
    {
        _charController.SetJumpVelocity(_jumpVelocity);
    }

    // ===== Input Events =====
    private void OnJumpInput(InputAction.CallbackContext ctx)
    {
        if (!_canJump)
            return;
        _canJump = false;

        CallJump();
    }



}
