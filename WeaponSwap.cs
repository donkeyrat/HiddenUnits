using UnityEngine;
using Landfall.TABS;
using System.Collections.Generic;
using UnityEngine.Events;

namespace HiddenUnits {
    
    public class WeaponSwap : MonoBehaviour {

        public void Start()
        {
            unit = transform.root.GetComponent<Unit>();
        }
        
        public void Swap() 
        {
            if (hasSwapped || transform.root.GetComponent<HoldingHandlerMulti>())
            {
                return;
            }
            
            bool left = false;
            bool right = false;
            
            if (unit.WeaponHandler) { unit.WeaponHandler.fistRefernce = null; }
            
            if (weaponToSwap == SwapType.Right || weaponToSwap == SwapType.Both) {
                
                if (unit.holdingHandler) {
                    
                    if (unit.holdingHandler.rightObject) {
                        var dropped = unit.holdingHandler.rightObject.gameObject;
                        unit.holdingHandler.LetGoOfWeapon(dropped);
                        Destroy(dropped);
                    }

                    if (weaponR)
                    {
                        var weaponRSpawned = unit.unitBlueprint.SetWeapon(unit, unit.Team, weaponR, new PropItemData(), HoldingHandler.HandType.Right, unit.data.mainRig.rotation, new List<GameObject>()).gameObject;
                        weaponRSpawned.GetComponent<Rigidbody>().mass *= unit.unitBlueprint.massMultiplier;
                        right = true;
                    }
                }
            }
            if (weaponToSwap == SwapType.Left || weaponToSwap == SwapType.Both) {
                
                if (unit.holdingHandler) {
                    
                    if (unit.holdingHandler.leftObject) {
                        
                        var dropped = unit.holdingHandler.leftObject.gameObject;
                        unit.holdingHandler.LetGoOfWeapon(dropped);
                        Destroy(dropped);
                    }

                    if (weaponL)
                    {
                        var weaponLSpawned = unit.unitBlueprint.SetWeapon(unit, unit.Team, weaponL, new PropItemData(), HoldingHandler.HandType.Left, unit.data.mainRig.rotation, new List<GameObject>()).gameObject;
                        weaponLSpawned.GetComponent<Rigidbody>().mass *= unit.unitBlueprint.massMultiplier;
                        left = true;
                    }
                    
                    else if (unit.unitBlueprint.holdinigWithTwoHands) unit.holdingHandler.leftHandActivity = HoldingHandler.HandActivity.HoldingRightObject;
                }
            }

            if ((left && right) || (right && !left))
            {
                unit.m_AttackDistance = unit.WeaponHandler.rightWeapon.maxRange;
                unit.m_PreferedDistance = unit.WeaponHandler.rightWeapon.maxRange - 0.3f;
            }
            else if (left && !right)
            {
                unit.m_AttackDistance = unit.WeaponHandler.leftWeapon.maxRange;
                unit.m_PreferedDistance = unit.WeaponHandler.leftWeapon.maxRange - 0.3f;
            }
            
            swapEvent.Invoke();
            unit.api.UpdateECSValues();
            hasSwapped = true;
        }

        public void Reset() { hasSwapped = false; }

        public enum SwapType {
            
            Both,
            Right,
            Left
        }

        private Unit unit;

        public GameObject weaponR;

        public GameObject weaponL;

        public SwapType weaponToSwap;

        public UnityEvent swapEvent = new UnityEvent();

        public bool hasSwapped = true;
    }
}
