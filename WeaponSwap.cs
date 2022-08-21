using UnityEngine;
using Landfall.TABS;
using System.Collections.Generic;
using UnityEngine.Events;

namespace HiddenUnits {
    
    public class WeaponSwap : MonoBehaviour {
        
        public void Swap() {
            
            if (!hasSwapped) {
                
                var unit = transform.root.GetComponent<Unit>();
                
                if (unit.WeaponHandler) { unit.WeaponHandler.fistRefernce = null; }
                
                if (weaponToSwap == SwapType.Right || weaponToSwap == SwapType.Both) {
                    
                    if (unit.holdingHandler) {
                        
                        if (unit.holdingHandler.rightObject) {
                            
                            var dropped = unit.holdingHandler.rightObject.gameObject;
                            unit.holdingHandler.LetGoOfWeapon(dropped);
                            Destroy(dropped);
                        }
                        
                        if (weaponR) { unit.unitBlueprint.SetWeapon(unit, unit.Team, weaponR, new PropItemData(), HoldingHandler.HandType.Right, unit.data.mainRig.rotation, new List<GameObject>()); }
                    }
                    else if (unit.GetComponentInChildren<HoldingHandlerMulti>()) {
                        
                        unit.GetComponentInChildren<HoldingHandlerMulti>().LetGoOfSpecific(HoldingHandler.HandType.Right, true);
                        foreach (var right in unit.GetComponentInChildren<HoldingHandlerMulti>().mainHands) {
                            if (weaponR) { unit.GetComponentInChildren<HoldingHandlerMulti>().SetWeapon(right.gameObject, Instantiate(weaponR, right.transform.position, right.transform.rotation, unit.transform)); }
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
                        
                        if (weaponL) { unit.unitBlueprint.SetWeapon(unit, unit.Team, weaponL, new PropItemData(), HoldingHandler.HandType.Left, unit.data.mainRig.rotation, new List<GameObject>()); }
                        
                        else if (unit.unitBlueprint.holdinigWithTwoHands) { unit.holdingHandler.leftHandActivity = HoldingHandler.HandActivity.HoldingRightObject; }
                    }
                    else if (unit.GetComponentInChildren<HoldingHandlerMulti>()) {
                        
                        unit.GetComponentInChildren<HoldingHandlerMulti>().LetGoOfSpecific(HoldingHandler.HandType.Left, true);
                        foreach (var left in unit.GetComponentInChildren<HoldingHandlerMulti>().otherHands) {
                            
                            if (weaponL) { unit.GetComponentInChildren<HoldingHandlerMulti>().SetWeapon(left.gameObject, Instantiate(weaponL, left.transform.position, left.transform.rotation, unit.transform)); }
                        }
                    }
                }
                swapEvent.Invoke();
                unit.api.UpdateECSValues();
                hasSwapped = true;
            }
        }

        public void Reset() { hasSwapped = false; }

        public enum SwapType {
            
            Both,
            Right,
            Left
        }

        public GameObject weaponR;

        public GameObject weaponL;

        public SwapType weaponToSwap;

        public UnityEvent swapEvent = new UnityEvent();

        public bool hasSwapped = true;
    }
}
