using System.Collections.Generic;
using TGCore.Library;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits;

public class AddStealToWeapons : MonoBehaviour
{
    public float chance = 1f;
    public float minimumHealthToSteal = 600f;
    public UnityEvent stealEvent;
    
    private void Start()
    {
        var weapons = new List<GameObject>();
        
        var holdingHandler = transform.root.GetComponentInChildren<HoldingHandler>();
        var holdingHandlerMulti = transform.root.GetComponentInChildren<HoldingHandlerMulti>();

        if (holdingHandler)
        {
            if (holdingHandler.rightObject) weapons.Add(holdingHandler.rightObject.gameObject);
            if (holdingHandler.leftObject) weapons.Add(holdingHandler.leftObject.gameObject);
        }
        else if (holdingHandlerMulti)
        {
            weapons.AddRange(holdingHandlerMulti.spawnedWeapons);
        }

        foreach (var weapon in weapons)
        {
            var steal = weapon.AddComponent<MeleeWeaponSteal>();
            steal.chance = chance;
            steal.minimumHealthToSteal = minimumHealthToSteal;
            steal.stealEvent = stealEvent;
        }
    }
}