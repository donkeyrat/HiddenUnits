using Landfall.TABS;
using HarmonyLib;
using UnityEngine;

namespace HiddenUnits.HarmonyPatches
{
    [HarmonyPatch(typeof(CollisionWeapon), "OnCollisionEnter")]
    class ArmorWeaponPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(CollisionWeapon instance, Collision collision, ref MeleeWeapon meleeWeapon, ref Rigidbody rig, ref DataHandler connectedData)
        {
            var armoredUnit = collision.transform.root.GetComponent<AchillesArmor.UnitIsArmored>();
            if (collision.transform && armoredUnit && armoredUnit.armorActive && collision.rigidbody && rig && meleeWeapon && meleeWeapon.isSwinging && connectedData && armoredUnit.GetComponent<Unit>().Team != connectedData.unit.Team && armoredUnit.parryPower > meleeWeapon.requiredPowerToParry)
            {
                meleeWeapon.StopSwing();
                rig.AddForce(collision.contacts[0].point.normalized * -armoredUnit.parryForce, ForceMode.VelocityChange);
                
                Object.Instantiate(armoredUnit.weaponHitEffect, collision.contacts[0].point, Quaternion.identity);
            }
            return true;
        }
    }
}