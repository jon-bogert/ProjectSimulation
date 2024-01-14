using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Transformation;
using XephTools;

public class ClimbController : MonoBehaviour
{
    PlayerHand _hand = null;
    Climbable _climbable = null;
    bool _isClimbing = false;

    [Header("Parameters")]
    [Range(0, 1)]
    [SerializeField] float _topDetectionThreshold = 0.5f;
    [SerializeField] float _topVelocityOverride = 3f;
    [Range(0, 1)]
    [SerializeField] float _displacementSmoothing = 0.6f;

    [Header("References")]
    [SerializeField] PlayerMovement _playerMovement;
    [SerializeField] TrackedVisual _handVisual;

    public bool isClimbing { get { return _isClimbing; } }
    public Vector3 moveDelta { get { return _hand.moveDelta; } }

    private void Awake()
    {
        _hand = GetComponent<PlayerHand>();

        GameLoader.CallOnComplete(Init);
    }

    private void Init()
    {
        _hand.grabbed += OnGrab;
        _hand.released += OnRelease;
        _handVisual.distanceReseted += OnRelease;
        _handVisual.triggerEntered += _OnTriggerEnter;
        _handVisual.triggerExited += _OnTriggerExit;
        _handVisual.displacementAdjust += AdjustVisualOffset;
    }

    private void OnDestroy()
    {
        if (_hand != null)
        {
            _hand.grabbed -= OnGrab;
            _hand.released -= OnRelease;
        }
        if (_handVisual != null)
        {
            _handVisual.distanceReseted -= OnRelease;
            _handVisual.triggerEntered -= _OnTriggerEnter;
            _handVisual.triggerExited -= _OnTriggerExit;
            _handVisual.displacementAdjust -= AdjustVisualOffset;
        }
    }

    private void _OnTriggerEnter(Collider other)
    {
        Climbable tmp = other.gameObject.GetComponent<Climbable>();
        if (tmp is null)
            return;

        if (_climbable is not null)
        {
            Debug.LogWarning("There is already a Climbable Assigned to this hand");
            return;
        }

        _climbable = tmp;
    }

    private void _OnTriggerExit(Collider other)
    {
        Climbable tmp = other.gameObject.GetComponent<Climbable>();
        if (tmp is null)
            return;

        if (tmp != _climbable)
        {
            Debug.LogWarning("This is not the same Climbable!");
            return;
        }

        _climbable = null;
    }

    private void OnGrab()
    {
        if (_climbable is null)
            return;

        _isClimbing = true;
        BeginClimb();
    }

    private void OnRelease()
    {
        if (!_isClimbing)
            return;

        _isClimbing = false;
        _handVisual.ResetDestination();
    }

    private void BeginClimb()
    {
        Transform gripPoint = _climbable.GetClosestGrip(transform.position);
        _handVisual.SetDestination(gripPoint);

        if (_playerMovement.stateMachine.currentState == (int)PlayerStates.Climbing)
            return;
        
        _playerMovement.stateMachine.ChangeState((int)PlayerStates.Climbing);
    }

    public void EndClimb()
    {
        Vector3 velocity = -_hand.moveDelta / Time.deltaTime;
        if (velocity.sqrMagnitude < (_topVelocityOverride * _topVelocityOverride))
        {
            //check hand y-state
            if (_climbable.isTop)
            {
                if (TopCheck())
                {
                    _playerMovement.stateMachine.ChangeState((int)PlayerStates.Grounded);
                    return;
                }
            }
        }

        _playerMovement.SetVelocity(velocity);
        _playerMovement.stateMachine.ChangeState((int)PlayerStates.Airborn);
    }

    private bool TopCheck()
    {
        float handPercent = (_playerMovement.localHeadPosition.y - transform.localPosition.y) / _playerMovement.localHeadPosition.y;
        if (handPercent >= 0.5f)
        {
            Vector3 teleportPoint = _climbable.topPosition;
            _playerMovement.Teleport(teleportPoint);
            return true;
        }
        return false;
    }

    private void AdjustVisualOffset(Vector3 visualPosition, Vector3 handPosition)
    {
        if (!isClimbing)
            return;

        Vector3 offset = visualPosition - handPosition;
        _playerMovement.AbsoluteMove(offset * _displacementSmoothing);
    }
}
