using UnityEngine;
using UnityEngine.InputSystem;
using XephTools;

[RequireComponent (typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] float _moveSpeedGround = 3f;
    [SerializeField] float _moveSpeedAir = 2f;
    [Range(0f, 1f)]
    [SerializeField] float _frictionGround = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] float _frictionAir = 0.2f;
    [SerializeField] float _gravityAmount = -9.8f;
    [SerializeField] float _groundForce = 0.1f;
    [SerializeField] float _jumpVelocity = 5f;
    [SerializeField] float _slopeDetectionDistance = 0.2f;

    [Header("Settings")]
    [SerializeField] bool _useDisplayRelative = true;

    [Header("Inputs")]
    [SerializeField] InputActionReference _moveInput;
    [SerializeField] InputActionReference _jumpInput;

    [Header("References")]
    [SerializeField] Transform _camera;
    [SerializeField] PlayerHand _leftHand;
    [SerializeField] PlayerHand _rightHand;

    ClimbController _leftClimb;
    ClimbController _rightClimb;


    private CharacterController _charController;
    private StateMachine<PlayerMovement> _stateMachine;

    private Vector2 _moveAxis = Vector2.zero;
    private Vector3 _velocity = Vector3.zero;
    bool _jumpThisFrame = false;

    //
    internal Vector2 moveAxis { get { return _moveAxis; } }
    internal float moveSpeedGround { get { return _moveSpeedGround; } }
    internal float moveSpeedAir { get { return _moveSpeedAir; } }
    internal float frictionGround { get { return _frictionGround; } }
    internal float frictionAir { get { return _frictionAir; } }
    internal StateMachine<PlayerMovement> stateMachine { get {  return _stateMachine; } }
    internal bool jumpThisFrame {  get { return _jumpThisFrame; }  set { _jumpThisFrame = value; } } 
    internal PlayerHand leftHand { get { return _leftHand; } }
    internal PlayerHand rightHand { get { return _rightHand; } }
    internal ClimbController leftClimb { get { return _leftClimb; } }
    internal ClimbController rightClimb { get { return _rightClimb; } }

    private void Awake()
    {
        _charController = GetComponent<CharacterController>();
        _jumpInput.action.performed += OnJumpInput;

        _stateMachine = new StateMachine<PlayerMovement>(this);
        _stateMachine.AddState<PlayerGrounded>();
        _stateMachine.AddState<PlayerAirborn>();
        _stateMachine.AddState<PlayerClimbing>();
        _stateMachine.ChangeState((int)PlayerStates.Grounded);

        GameLoader.CallOnComplete(Init);
    }

    private void Init()
    {
        _leftClimb = _leftHand.GetComponent<ClimbController>();
        _rightClimb = _rightHand.GetComponent<ClimbController>();
    }

    private void Update()
    {
        _moveAxis = _moveInput.action.ReadValue<Vector2>();
        ColliderUpdate();
        _stateMachine.Update(Time.deltaTime);
        VRDebug.Monitor(1, ((PlayerStates)_stateMachine.currentState).ToString());

        ApplySlope();
        _charController.Move(_velocity * Time.deltaTime);
    }

    private void OnDestroy()
    {
        _jumpInput.action.performed -= OnJumpInput;
    }

    internal void Move(float speed)
    {
        Vector3 velocity = new Vector3(
            _moveAxis.x * speed,
            0,
            _moveAxis.y * speed);

        //Adjust for Facing Direction
        Vector3 forward = (_useDisplayRelative) ?
            _camera.forward :
            _leftHand.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = (_useDisplayRelative) ?
            _camera.right :
            _leftHand.transform.right;
        right.y = 0f;
        right.Normalize();

        velocity = right * velocity.x + forward * velocity.z;

        velocity.y += _velocity.y;

        //Apply
        _velocity = velocity;
    }

    internal void Deccelerate(float friction)
    {
        Vector2 hVel = new Vector2(_velocity.x, _velocity.z);
        if (hVel == Vector2.zero)
            return;

        hVel = Vector2.Lerp(hVel, Vector2.zero, friction);
        if (hVel.sqrMagnitude < float.Epsilon)
        {
            hVel = Vector2.zero;
        }
        _velocity = new Vector3(hVel.x, _velocity.y, hVel.y);
    }

    internal bool isGrounded
    {
        get
        {
            return _charController.isGrounded;
        }
    }
    internal void OnLand()
    {
        ResetYVelocity();
    }

    internal void ApplyGravity()
    {
        _velocity.y += _gravityAmount * Time.deltaTime;
    }

    internal void ApplyGroundForce()
    {
        if (_velocity.y <= 0f)
            _velocity.y = -_groundForce;
    }

    private void ApplySlope()
    {
        Vector3 v = new Vector3(_velocity.x, 0f, _velocity.z);
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit info, _slopeDetectionDistance))
        {
            Quaternion slopeRot = Quaternion.FromToRotation(Vector3.up, info.normal);
            v = slopeRot * v;

            if (v.y >= 0f)
                return;

            v.y += _velocity.y;
            _velocity = v;
        }
    }

    internal void ResetYVelocity()
    {
        _velocity.y = 0f;
    }

    internal void SetVelocity(Vector3 velocity)
    {
        _velocity = velocity;
    }

    internal void AbsoluteMove(Vector3 amt)
    {
        _velocity = Vector3.zero;
        _charController.Move(amt);
    }

    internal void DoJump()
    {
        _velocity.y = _jumpVelocity;
    }

    private void ColliderUpdate()
    {
        Vector3 position = _camera.localPosition;
        _charController.height = position.y;
        Vector3 newCenter = new Vector3(position.x, _charController.height * 0.5f, position.z);
        _charController.center = newCenter;
    }

    // ===== Input Events =====

    private void OnJumpInput(InputAction.CallbackContext ctx)
    {
        _jumpThisFrame = true;
    }
}