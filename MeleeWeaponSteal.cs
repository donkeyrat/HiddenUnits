using System.Collections.Generic;
using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class MeleeWeaponSteal : CollisionWeaponEffect
    {
        private Unit OwnUnit;
        
        public float chance = 1f;
        public float minimumHealthToSteal = 600f;
        public UnityEvent stealEvent;

        private void Start()
        {
            OwnUnit = transform.root.GetComponent<Unit>();
        }
        
        public override void DoEffect(Transform hitTransform, Collision collision)
        {
            if (hitTransform.GetComponent<Rigidbody>() && hitTransform.root.GetComponent<Unit>() && hitTransform.root.GetComponent<Unit>().Team != GetComponent<Weapon>().connectedData.unit.Team && !hitTransform.root.GetComponent<Unit>().data.Dead)
            {
                
                GameObject stolenWeapon = null;
                var handType = HoldingHandler.HandType.Right;
                var enemy = hitTransform.root.GetComponent<Unit>();
                if (enemy.data.health > minimumHealthToSteal || enemy.data.Dead || Random.value > chance)
                {
                    return;
                }

                if (enemy.holdingHandler && enemy.WeaponHandler)
                {
                    enemy.WeaponHandler.fistRefernce = null;
                    if (enemy.holdingHandler.rightObject)
                    {
                        stolenWeapon = enemy.holdingHandler.rightObject.gameObject;
                        handType = HoldingHandler.HandType.Right;
                        enemy.holdingHandler.LetGoOfWeapon(stolenWeapon);
                        //Destroy(hold.rightObject);
                    }
                    else if (enemy.holdingHandler.leftObject)
                    {
                        stolenWeapon = enemy.holdingHandler.leftObject.gameObject;
                        handType = HoldingHandler.HandType.Left;
                        enemy.holdingHandler.LetGoOfWeapon(stolenWeapon);
                        //Destroy(hold.leftObject);
                    }
                }
                if (stolenWeapon != null)
                {
                    stolenWeapon.transform.SetParent(null);
                    if (OwnUnit.holdingHandler && OwnUnit.WeaponHandler)
                    {
                        OwnUnit.WeaponHandler.fistRefernce = null;
                        OwnUnit.holdingHandler.LetGoOfWeapon(gameObject);
                        gameObject.AddComponent<RemoveAfterSeconds>().shrink = true;
                        
                        var newWeapon = OwnUnit.unitBlueprint.SetWeapon(OwnUnit, OwnUnit.Team, stolenWeapon, new PropItemData(), handType, OwnUnit.data.mainRig.rotation, []);
                        OwnUnit.holdingHandler.leftHandActivity = enemy.holdingHandler.leftHandActivity;
                        if (newWeapon.GetComponent<ConfigurableJoint>())
                        {
                            foreach (var joint in newWeapon.GetComponentsInChildren<ConfigurableJoint>())
                            {
                                Destroy(joint);
                            }
                        }
                        Destroy(stolenWeapon);
                        stealEvent.Invoke();
                    }
                }
            }
        }
    }
}