using UnityEngine;

public interface IDummyMonoSystem
{
    string GetSystemName();
}

public class DummyMonoSystem : MonoBehaviour, IDummyMonoSystem
{
    private void Awake()
    {
        GameLoader.CallOnComplete(Initialize);
    }

    private void Initialize()
    {
        Debug.Log($"Initializing {GetSystemName()}");
    }

    public string GetSystemName()
    {
        return this.GetType().ToString();
    }
}
