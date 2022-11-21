using System.Reflection;
using Landfall.TABS.GameMode;
using System;
using UnityEngine;
using HarmonyLib;

namespace HiddenUnits.HarmonyPatches
{
    [HarmonyPatch(typeof(DataHandler), "Dead", MethodType.Setter)]
    class RevivePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(DataHandler __instance, ref bool value)
        {
            if (value && !(bool)HUAddons.GetField(typeof(DataHandler), __instance, "dead"))
            {
                GameModeService service = ServiceLocator.GetService<GameModeService>();
                if (service.CurrentGameMode == null)
                {
                    Debug.LogError("Could not find CurrentGameMode!");
                }
                else if (!__instance.healthHandler.willBeRewived)
                {
                    service.CurrentGameMode.OnUnitDied(__instance.unit);
                }
            }
            HUAddons.SetField(__instance, "dead", value);
            return false;
        }
    }
}