using UnityEngine;

public class SelectionWheelShader : MonoBehaviour
{
    Material _material;

    private void Awake()
    {
        GameLoader.CallOnComplete(Init);
    }

    private void Init()
    {
        _material = GetComponent<MeshRenderer>().material;
    }

    private void OnTriggerEnter(Collider other)
    {
        _material.SetInt("_Selected", 1);
    }
    private void OnTriggerExit(Collider other)
    {
        _material.SetInt("_Selected", 0);
    }
}
