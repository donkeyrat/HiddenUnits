using System.Collections.Generic;
using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits;

public class MeleeWeaponDisarm : CollisionWeaponEffect
{
    public float minimumHealthToDisarm = 600f;
    public UnityEvent disarmEvent;

    public override void DoEffect(Transform hitTransform, Collision collision)
    {
        if (hitTransform.GetComponent<Rigidbody>() && hitTransform.root.GetComponent<Unit>() &&
            hitTransform.root.GetComponent<Unit>().Team != GetComponent<Weapon>().connectedData.unit.Team &&
            !hitTransform.root.GetComponent<Unit>().data.Dead)
        {
            var enemy = hitTransform.root.GetComponent<Unit>();
            if (enemy.data.health > minimumHealthToDisarm || enemy.data.Dead)
            {
                return;
            }

            var hasDisarmed = false;
            var hold = enemy.GetComponentInChildren<HoldingHandler>();
            if (hold)
            {
                if (hold.rightObject)
                {
                    hold.rightObject.gameObject.AddComponent<RemoveAfterSeconds>().shrink = true;
                    hold.LetGoOfWeapon(hold.rightObject.gameObject);
                    //Destroy(hold.rightObject);
                    hasDisarmed = true;
                }
                else if (hold.leftObject)
                {
                    hold.leftObject.gameObject.AddComponent<RemoveAfterSeconds>().shrink = true;
                    hold.LetGoOfWeapon(hold.leftObject.gameObject);
                    //Destroy(hold.leftObject);
                    hasDisarmed = true;
                }
            }

            if (hasDisarmed)
            {
                disarmEvent.Invoke();
            }
        }
    }
}