using UnityEngine;

public class Example_StatefulObjectController : MonoBehaviour
{
    [SerializeField] private StatefulObject _statefulObject = null;

    [SerializeField] private string _stateName = null;

    // This is just an example.
    // Do not copy bad habits.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Cycle Through States
            _statefulObject.SetToNextState();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // 
            if (_statefulObject.HasState(_stateName) == false)
            {
                Debug.Log($"State not found {_stateName ?? "NULL"}");
                return;
            }

            // Try to set state to specific value (yes it uses strings...)
            _statefulObject.SetState(_stateName);

            // has an optional 'force' param if you want
            _statefulObject.SetState(_stateName, true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Set to random state
            _statefulObject.SetToRandomState();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // Set to default state
            _statefulObject.SetToDefaultState();
        }
    }
}
