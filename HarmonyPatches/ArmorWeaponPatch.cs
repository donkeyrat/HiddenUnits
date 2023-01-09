﻿using Landfall.TABS;
using HarmonyLib;
using UnityEngine;

namespace HiddenUnits.HarmonyPatches
{
    [HarmonyPatch(typeof(CollisionWeapon), "OnCollisionEnter", MethodType.Setter)]
    class ArmorWeaponPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(CollisionWeapon __instance, Collision col, ref MeleeWeapon ___meleeWeapon, ref Rigidbody ___rig, ref DataHandler ___connectedData)
        {
            var armoredUnit = col.transform.root.GetComponent<AchillesArmor.UnitIsArmored>();
            if (col.transform && armoredUnit && armoredUnit.armorActive && col.rigidbody && ___rig && ___meleeWeapon && ___meleeWeapon.isSwinging && ___connectedData && armoredUnit.GetComponent<Unit>().Team != ___connectedData.unit.Team && armoredUnit.parryPower > ___meleeWeapon.requiredPowerToParry)
            {
                ___meleeWeapon.StopSwing();
                ___rig.AddForce(col.contacts[0].point.normalized * -armoredUnit.parryForce, ForceMode.VelocityChange);
                
                Object.Instantiate(armoredUnit.weaponHitEffect, col.contacts[0].point, Quaternion.identity);
            }
            return true;
        }
    }
}