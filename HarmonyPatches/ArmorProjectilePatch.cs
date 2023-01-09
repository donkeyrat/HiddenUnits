using Landfall.TABS;
using HarmonyLib;
using UnityEngine;

namespace HiddenUnits.HarmonyPatches
{
    [HarmonyPatch(typeof(ProjectileHit), "Hit", MethodType.Setter)]
    class ArmorProjectilePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ProjectileHit __instance, RaycastHit sentHit, float multiplier, ref MoveTransform ___move, ref RaycastTrail ___trail, ref TeamHolder ___teamHolder)
        {
            var armoredUnit = sentHit.transform.root.GetComponent<AchillesArmor.UnitIsArmored>();
            
            if (sentHit.transform && armoredUnit && armoredUnit.armorActive && sentHit.rigidbody && armoredUnit.blockPower > __instance.blockPoweredNeeded)
            {
                if (___move) ___move.velocity = Vector3.Reflect(___move.velocity, sentHit.normal) * Random.Range(0.2f, 0.4f);
                if (___trail) ___trail.ignoredFrames = 3;
                
                Object.Instantiate(armoredUnit.projectileHitEffect, sentHit.point, Quaternion.identity);

                return false;
            }
            return true;
        }
    }
}