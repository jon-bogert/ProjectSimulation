using UnityEngine;
using XephTools;

public enum Ability
{
    None = -1,
    DataLink = 0,
    GrappleHook = 1,
    Drone = 2,
    ImpulseCannon = 3,
    PlasmaCutter = 4,
    Health = 5,
    // ^-- Above are on Gadget Wheel
    FallDampers = 6,
    Boosters = 7,
    DroneDataCollection = 8
}
public class PlayerAbilities : MonoBehaviour, ISavable
{
    public delegate void UnlockEvent(Ability ability);

    [SerializeField] bool dataLink = false;
    [SerializeField] bool grappleHook = false;
    [SerializeField] bool drone = false;
    [SerializeField] bool impulseCannon = false;
    [SerializeField] bool plasmaCutter = false;
    [SerializeField] bool health = false;
    [SerializeField] bool fallDampers = false;
    [SerializeField] bool boosters = false;
    [SerializeField] bool droneDataCollection = false;

    public UnlockEvent abilityUnlocked;

    public string SaveID { get => "playerAbilities";}

    public void Load(SaveData data)
    {
        data.TrySet("dataLink", ref dataLink);
        data.TrySet("grappleHook", ref grappleHook);
        data.TrySet("drone", ref drone);
        data.TrySet("impulseCannon", ref impulseCannon);
        data.TrySet("plasmaCutter", ref plasmaCutter);
        data.TrySet("health", ref health);
        data.TrySet("fallDampers", ref fallDampers);
        data.TrySet("boosters", ref boosters);
        data.TrySet("droneDataCollection", ref droneDataCollection);
    }

    public void Save(SaveData data)
    {
        data.AddOrChange(DataType.Bool, "dataLink", dataLink);
        data.AddOrChange(DataType.Bool, "grappleHook", grappleHook);
        data.AddOrChange(DataType.Bool, "drone", drone);
        data.AddOrChange(DataType.Bool, "impulseCannon", impulseCannon);
        data.AddOrChange(DataType.Bool, "plasmaCutter", plasmaCutter);
        data.AddOrChange(DataType.Bool, "health", health);
        data.AddOrChange(DataType.Bool, "fallDampers", fallDampers);
        data.AddOrChange(DataType.Bool, "boosters", boosters);
        data.AddOrChange(DataType.Bool, "droneDataCollection", droneDataCollection);
    }

    private void Awake()
    {
        GameLoader.CallOnComplete(Init);
    }

    private void Init()
    {
        ServiceLocator.Get<GlobalRefs>().playerAbilities = this;
        ServiceLocator.Get<SaveManager>().Register(this);
    }

    private void OnDestroy()
    {
        GlobalRefs gr = ServiceLocator.Get<GlobalRefs>();
        if (gr != null && gr.playerAbilities == this)
            gr.playerAbilities = null;

        ServiceLocator.Get<SaveManager>().Unregister(this);
    }

    public bool IsUnlocked(Ability ability)
    {
        switch (ability)
        {
            case Ability.None:
                Debug.LogWarning("You should not be checking if 'None' is unlocked...");
                return false;
            case Ability.DataLink:              return dataLink;
            case Ability.GrappleHook:           return grappleHook;
            case Ability.Drone:                 return drone;
            case Ability.ImpulseCannon:         return impulseCannon;
            case Ability.PlasmaCutter:          return plasmaCutter;
            case Ability.Health:                return health;
            case Ability.FallDampers:           return fallDampers;
            case Ability.Boosters:              return boosters;
            case Ability.DroneDataCollection:   return droneDataCollection;
            default:
                Debug.LogWarning("PlayerAbilities.IsUnlocked: Ability not implemented");
                return false;
        }
    }

    public void UnlockAbility(Ability ability)
    {
        switch (ability)
        {
            case Ability.None:
                Debug.LogWarning("You should not be trying to unlock 'None'...");
                return;
            case Ability.DataLink:
                dataLink = true;
                break;
            case Ability.GrappleHook:
                grappleHook = true;
                break;
            case Ability.Drone:
                drone = true;
                break;
            case Ability.ImpulseCannon:
                impulseCannon = true;
                break;
            case Ability.PlasmaCutter:
                plasmaCutter = true;
                break;
            case Ability.Health:
                health = true;
                break;
            case Ability.FallDampers:
                fallDampers = true;
                break;
            case Ability.Boosters:
                boosters = true;
                break;
            case Ability.DroneDataCollection:
                droneDataCollection = true;
                break;
            default:
                Debug.LogWarning("PlayerAbilities.UnlockAbility: Ability not implemented");
                return;
        }

        abilityUnlocked?.Invoke(ability);

    }
}
