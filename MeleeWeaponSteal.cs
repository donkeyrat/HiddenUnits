using System.Collections.Generic;
using Landfall.TABS;
using UnityEngine;

namespace HiddenUnits
{
    public class MeleeWeaponSteal : CollisionWeaponEffect
    {
        public override void DoEffect(Transform hitTransform, Collision collision)
        {
            if (hitTransform.GetComponent<Rigidbody>() && hitTransform.root.GetComponent<Unit>() && hitTransform.root.GetComponent<Unit>().Team != GetComponent<Weapon>().connectedData.unit.Team && !hitTransform.root.GetComponent<Unit>().data.Dead)
            {
                GameObject stolenWeapon = null;
                var handType = HoldingHandler.HandType.Right;
                var enemy = hitTransform.root.GetComponent<Unit>();
                if (enemy.data.health > 600f || enemy.data.Dead)
                {
                    return;
                }
                var hold = enemy.GetComponentInChildren<HoldingHandler>();
                if (hold)
                {
                    enemy.GetComponentInChildren<WeaponHandler>().fistRefernce = null;
                    if (hold.rightObject)
                    {
                        stolenWeapon = hold.rightObject.gameObject;
                        handType = HoldingHandler.HandType.Right;
                        hold.LetGoOfWeapon(stolenWeapon);
                        Destroy(hold.rightObject);
                    }
                    else if (hold.leftObject)
                    {
                        stolenWeapon = hold.leftObject.gameObject;
                        handType = HoldingHandler.HandType.Left;
                        hold.LetGoOfWeapon(stolenWeapon);
                        Destroy(hold.leftObject);
                    }
                }
                if (stolenWeapon != null)
                {
                    stolenWeapon.transform.SetParent(null);
                    if (transform.root.GetComponentInChildren<HoldingHandler>())
                    {
                        transform.root.GetComponentInChildren<HoldingHandler>().LetGoOfWeapon(gameObject);
                        gameObject.AddComponent<RemoveAfterSeconds>().shrink = true;
                        var w = transform.root.GetComponent<Unit>().unitBlueprint.SetWeapon(transform.root.GetComponent<Unit>(), transform.root.GetComponent<Unit>().Team, stolenWeapon, new PropItemData(), handType, transform.root.GetComponent<Unit>().data.mainRig.rotation, new List<GameObject>());
                        transform.root.GetComponentInChildren<HoldingHandler>().leftHandActivity = hold.leftHandActivity;
                        if (w.GetComponent<ConfigurableJoint>())
                        {
                            foreach (var joint in w.GetComponentsInChildren<ConfigurableJoint>())
                            {
                                Destroy(joint);
                            }
                        }
                        Destroy(stolenWeapon);
                    }
                }
            }
        }
    }
}