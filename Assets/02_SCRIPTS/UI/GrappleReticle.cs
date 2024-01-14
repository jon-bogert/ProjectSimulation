using UnityEngine;

public class GrappleReticle : MonoBehaviour
{
    [SerializeField] GameObject outterRing;
    [SerializeField] GameObject innerRing;
    [SerializeField] float outterSpeed = 45f;
    [SerializeField] float innerSpeed = 45f;

    private void Start()
    {
        if (!outterRing || !innerRing)
        {
            Debug.LogWarning("GrappleReticle: Rings gameObjects must be assigned in inspector");
        }
    }

    private void Update()
    {
        outterRing.transform.Rotate(Vector3.forward, outterSpeed * Time.deltaTime);
        innerRing.transform.Rotate(Vector3.forward, -innerSpeed * Time.deltaTime);
    }
}
