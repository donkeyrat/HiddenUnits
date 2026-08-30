using TGCore.Library;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits;

public class EvilBees : MonoBehaviour
{
    public bool enableEvilBees;
    public UnityEvent evilBeesEvent;
    public float evilDamage = 30f;
    public float evilForce = 5000f;
    
    private void Start()
    {
        enableEvilBees = HUMain.EvilBeesEnabled;
        if (enableEvilBees)
        {
            evilBeesEvent.Invoke();
            GetComponentInChildren<Explosion>().damage = evilDamage;
            GetComponentInChildren<FlyTowardsTarget>().forwardForce = evilForce;
        }
    }
}