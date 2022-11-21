using UnityEngine;
using HarmonyLib;

namespace HiddenUnits.HarmonyPatches
{
    
    [HarmonyPatch(typeof(UnitBowAnimation), "ResetPos")]
    class CentaurArrowPatch 
    {
        
        [HarmonyPrefix]
        public static bool Prefix(UnitBowAnimation __instance, ref Transform ___originalParent, ref Transform ___startPos, ref ConfigurableJoint ___rightHandJoint)
        {
            var transform = __instance.transform;
            if (!transform) return false;
            
            if (___originalParent) __instance.transform.SetParent(___originalParent);
            
            if (___startPos)
            {
                transform.localPosition = ___startPos.localPosition;
                transform.localRotation = ___startPos.localRotation;
            }
            
            if (___rightHandJoint) ___rightHandJoint.zMotion = ConfigurableJointMotion.Locked;
            
            if (__instance)
            {
                __instance.stopAim = true;
                __instance.drawn = false;
            }
            
            return false;
        }
    }
}