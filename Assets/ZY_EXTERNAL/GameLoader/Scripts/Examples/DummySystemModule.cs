using UnityEngine;
public interface IDummySystemModule
{
    string GetSystemName();
}

public class DummySystemModule : MonoBehaviour, IGameModule, IDummySystemModule
{

    [SerializeField] private GameObject _sysPrefab = null;

    public void Load()
    {
        GameObject.Instantiate(_sysPrefab);
        ServiceLocator.Register<IDummySystemModule>(this);
        Debug.Log(GetSystemName());
    }

    public string GetSystemName()
    {
        return this.GetType().ToString();
    }
}
