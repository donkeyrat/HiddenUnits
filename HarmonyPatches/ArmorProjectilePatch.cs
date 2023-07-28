using HarmonyLib;
using TGCore.Library;
using UnityEngine;

namespace HiddenUnits.HarmonyPatches
{
    [HarmonyPatch(typeof(ProjectileHit), "Hit")]
    class ArmorProjectilePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ProjectileHit instance, RaycastHit sentHit, float multiplier, ref MoveTransform move, ref RaycastTrail trail, ref TeamHolder teamHolder)
        {
            var armoredUnit = sentHit.transform.root.GetComponent<AchillesArmor.UnitIsArmored>();
            
            if (!instance.GetComponent<ProjectileHoming>() && sentHit.transform && armoredUnit && armoredUnit.armorActive && sentHit.rigidbody && armoredUnit.blockPower > instance.blockPoweredNeeded)
            {
                if (move) move.velocity = Vector3.Reflect(move.velocity, sentHit.normal) * Random.Range(0.2f, 0.4f);
                if (trail) trail.ignoredFrames = 3;
                
                Object.Instantiate(armoredUnit.projectileHitEffect, sentHit.point, Quaternion.identity);

                return false;
            }
            return true;
        }
    }
}