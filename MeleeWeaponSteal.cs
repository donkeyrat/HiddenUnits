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
            var enemy = hitTransform.root.GetComponent<Unit>();
            if (collision.rigidbody && enemy && enemy.Team != GetComponent<Weapon>().connectedData.unit.Team && !enemy.data.Dead)
            {
                GameObject stolenWeapon = null;
                var handType = HoldingHandler.HandType.Right;
                if (enemy.data.health > minimumHealthToSteal || enemy.data.Dead || Random.value > chance)
                {
                    return;
                }
                stealEvent.Invoke();
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
                    if (OwnUnit.holdingHandler && OwnUnit.WeaponHandler)
                    {
                        OwnUnit.WeaponHandler.fistRefernce = null;
                        OwnUnit.holdingHandler.LetGoOfWeapon(gameObject);
                        gameObject.AddComponent<RemoveAfterSeconds>().shrink = true;
                        var w = OwnUnit.unitBlueprint.SetWeapon(OwnUnit, OwnUnit.Team, stolenWeapon, new PropItemData(), handType, OwnUnit.data.mainRig.rotation, new List<GameObject>());
                        OwnUnit.holdingHandler.leftHandActivity = hold.leftHandActivity;
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