using System.Reflection;
using System;
using UnityEngine;
using HarmonyLib;

namespace HiddenUnits {
    
    [HarmonyPatch(typeof(UnitBowAnimation), "ResetPos")]
    class WussyWaka {
        
        [HarmonyPrefix]
        public static bool Prefix(UnitBowAnimation __instance, ref Transform ___originalParent, ref Transform ___startPos, ref ConfigurableJoint ___rightHandJoint) {
            //return false;
            if (___originalParent)
            {
                __instance.transform.SetParent(___originalParent);
            }
            if (___startPos)
            {
                __instance.transform.localPosition = ___startPos.localPosition;
                __instance.transform.localRotation = ___startPos.localRotation;
            }
            if (___rightHandJoint)
            {
                ___rightHandJoint.zMotion = ConfigurableJointMotion.Locked;
            }
            if (__instance)
            {
                __instance.stopAim = true;
                __instance.drawn = false;
            }
            return false;
        }
    }
}