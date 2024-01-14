using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHand : MonoBehaviour
{
    public delegate void GrabEvent();

    public enum Hand { Dominant, NonDominant };
    [Header("Parameters")]
    [SerializeField] Hand _hand;
    [Header("Input")]
    [SerializeField] InputActionReference _grabInput;

    public GrabEvent grabbed;
    public GrabEvent released;

    Vector3 _prevPos = Vector3.zero;
    Vector3 _currPos = Vector3.zero;
    Vector3 _moveDelta = Vector3.zero;

    public Hand hand { get { return _hand; } }
    public Vector3 moveDelta { get { return _moveDelta; } }

    private void Awake()
    {
        _grabInput.action.performed += OnGrabInput;

        GameLoader.CallOnComplete(Init);
    }

    private void Init()
    {
        _prevPos = _currPos = transform.localPosition;
    }

    private void Update()
    {
        _prevPos = _currPos;
        _currPos = transform.localPosition;
        _moveDelta = _currPos - _prevPos;
    }

    private void OnDestroy()
    {
        _grabInput.action.performed -= OnGrabInput;
    }

    //Input Events
    void OnGrabInput(InputAction.CallbackContext ctx)
    {
        if (ctx.action.IsPressed())
        {
            grabbed?.Invoke();
            return;
        }
        released?.Invoke();
    }
}
