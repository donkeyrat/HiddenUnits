using HarmonyLib;

namespace HiddenUnits 
{
    [HarmonyPatch(typeof(HealthHandler), "Die")]
    class DeathPatch 
    {
        [HarmonyPrefix]
        public static bool Prefix(HealthHandler __instance)
        {
            return __instance.transform.root.GetComponentInChildren<Unkillable>();
        }
    }
}