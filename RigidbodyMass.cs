using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyMass : MonoBehaviour
{
    public void Start()
    {
        allRigs = GetComponentsInChildren<Rigidbody>();
    }

    public void ChangeMass()
    {
        foreach (var rig in allRigs)
        {
            rig.mass *= massMultiplier;
        }
    }

    public void ResetMass()
    {
        foreach (var rig in allRigs)
        {
            rig.mass /= massMultiplier;
        }
    }

    public Rigidbody[] allRigs;

    public float massMultiplier;
}
