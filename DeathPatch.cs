using HarmonyLib;
using Landfall.TABS;

namespace HiddenUnits 
{
    [HarmonyPatch(typeof(HealthHandler), "Die", new System.Type[] { typeof(Unit) })]
    class DeathPatch 
    {
        [HarmonyPrefix]
        public static bool Prefix(HealthHandler __instance, Unit damager = null)
        {
            return !__instance.transform.root.GetComponentInChildren<Unkillable>();
        }
    }
}