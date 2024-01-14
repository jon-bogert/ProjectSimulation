using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Transform lookTransform;

    private void Update()
    {
        transform.LookAt(lookTransform);
    }
}
