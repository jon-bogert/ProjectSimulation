using UnityEngine;
using UnityEngine.InputSystem;
using XephTools;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] Ability _currentGadget = Ability.None;

    [Header("Gadget Instances")]
    [SerializeField] GameObject _handVisual;        // TODO CHANGE
    [SerializeField] GameObject _dataLink;          // TODO CHANGE
    [SerializeField] GameObject _grappleHook;       // TODO CHANGE
    [SerializeField] GameObject _drone;             // TODO CHANGE
    [SerializeField] GameObject _impulseCannon;     // TODO CHANGE
    [SerializeField] GameObject _plasmaCutter;      // TODO CHANGE
    [SerializeField] GameObject _health;            // TODO CHANGE

    [Header("References")]
    [SerializeField] Transform _camera;
    [SerializeField] SelectionWheel _gadgetWheel;

    [Header("Inputs")]
    [SerializeField] InputActionReference _menuInput;

    Ability _lastGadget = Ability.None;

    private void Awake()
    {
        _menuInput.action.performed += OnMenuInput;

        GameLoader.CallOnComplete(Init);
    }

    private void Init()
    {
        DisableAllVisuals();
        EnableNewVisual();
    }

    private void OnDestroy()
    {
        _menuInput.action.performed -= OnMenuInput;
    }

    private void UpdateGadget(Ability gadget)
    {
        DisableOldVisual();

        if (gadget == Ability.None && _currentGadget != Ability.None)
            _lastGadget = _currentGadget;
        
        if (_currentGadget == Ability.None && gadget == Ability.None)
            _currentGadget = _lastGadget;
        else
            _currentGadget = gadget;

        EnableNewVisual();
    }

    private void EnableNewVisual()
    {
        PlayerAbilities abilities = ServiceLocator.Get<GlobalRefs>().playerAbilities;
        VRDebug.Monitor(3, _currentGadget);
        switch (_currentGadget)
        {
            case Ability.None:
                _handVisual.SetActive(true);
                break;
            case Ability.DataLink:
                if (_dataLink != null && abilities.IsUnlocked(Ability.DataLink))
                    _dataLink.SetActive(true);
                break;
            case Ability.GrappleHook:
                if (_grappleHook != null && abilities.IsUnlocked(Ability.GrappleHook))
                    _grappleHook.SetActive(true);
                break;
            case Ability.Drone:
                if (_drone != null && abilities.IsUnlocked(Ability.Drone))
                    _drone.SetActive(true); // TODO - Swap Cameras
                break;
            case Ability.ImpulseCannon:
                if (_impulseCannon != null && abilities.IsUnlocked(Ability.ImpulseCannon))
                    _impulseCannon.SetActive(true);
                break;
            case Ability.PlasmaCutter:
                if (_plasmaCutter != null && abilities.IsUnlocked(Ability.PlasmaCutter))
                    _plasmaCutter.SetActive(true);
                break;
            case Ability.Health:
                if (_health != null && abilities.IsUnlocked(Ability.Health))
                    _health.SetActive(true);
                break;
            default:
                Debug.LogWarning("GadgetManager.EnableNewVisual Ability not implemented");
                break;
        }
    }

    private void DisableOldVisual()
    {
        PlayerAbilities abilities = ServiceLocator.Get<GlobalRefs>().playerAbilities;
        switch (_currentGadget)
        {
            case Ability.None:
                _handVisual.SetActive(false);
                break;
            case Ability.DataLink:
                if (_dataLink != null)
                    _dataLink.SetActive(false);
                break;
            case Ability.GrappleHook:
                if (_grappleHook != null)
                    _grappleHook.SetActive(false);
                break;
            case Ability.Drone:
                if (_drone != null)
                    _drone.SetActive(false); // TODO - Swap Cameras
                break;
            case Ability.ImpulseCannon:
                if (_impulseCannon != null)
                    _impulseCannon.SetActive(false);
                break;
            case Ability.PlasmaCutter:
                if (_plasmaCutter != null)
                    _plasmaCutter.SetActive(false);
                break;
            case Ability.Health:
                if (_health != null)
                    _health.SetActive(false);
                break;
            default:
                Debug.LogWarning("GadgetManager.DisableOldVisual Ability not implemented");
                break;
        }
    }

    private void DisableAllVisuals()
    {
        if (_handVisual != null) 
            _handVisual.SetActive(false);
        if (_dataLink != null) 
            _dataLink.SetActive(false);
        if (_grappleHook != null) 
            _grappleHook.SetActive(false);
        if (_drone != null) 
            _drone.SetActive(false);
        if (_impulseCannon != null) 
            _impulseCannon.SetActive(false);
        if (_plasmaCutter != null) 
            _plasmaCutter.SetActive(false);
        if (_health != null) 
            _health.SetActive(false);
    }

    // Input Events
    private void OnMenuInput(InputAction.CallbackContext ctx)
    {
        if (ctx.action.IsPressed())
        {
            _gadgetWheel.Activate(transform.position, _camera);
            return;
        }
        int setTo = _gadgetWheel.Deactivate();
        Debug.Log("Gadget set to: " + (Ability)setTo + " " + setTo);
        UpdateGadget((Ability)setTo);
    }
}
