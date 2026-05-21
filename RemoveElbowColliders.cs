using UnityEngine;

namespace HiddenUnits;

public class RemoveElbowColliders : MonoBehaviour
{
    
    private void Start()
    {
        var holdable = GetComponent<Holdable>();
        if (holdable)
        {
            holdable.AddWasGrabbedAction(RemoveColliders);
        }
    }

    private void RemoveColliders()
    {
        var joint = GetComponent<ConfigurableJoint>();
        if (joint)
        {
            foreach (var col in joint.connectedBody.GetComponentsInChildren<Collider>())
            {
                col.gameObject.layer = 13;
            }
        }
    }
}