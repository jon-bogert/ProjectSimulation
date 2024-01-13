using UnityEngine;

public class ClimbController : MonoBehaviour
{
    PlayerHand _hand = null;
    Climbable _climbable = null;
    bool _isClimbing = false;

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
        _handVisual.triggerEntered += _OnTriggerEnter;
        _handVisual.triggerExited += _OnTriggerExit;
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
            _handVisual.triggerEntered -= _OnTriggerEnter;
            _handVisual.triggerExited -= _OnTriggerExit;
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

    private void OnGrab(Transform handTransform)
    {
        if (_climbable is null)
            return;

        _isClimbing = true;
        BeginClimb();
    }

    private void OnRelease(Transform handTransform)
    {
        if (!_isClimbing)
            return;

        _isClimbing = false;
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
        //check hand y-state
        Vector3 velocity = -_hand.moveDelta / Time.deltaTime;
        _playerMovement.SetVelocity(velocity);
        _playerMovement.stateMachine.ChangeState((int)PlayerStates.Airborn);
        _handVisual.ResetDestination();
    }

}
