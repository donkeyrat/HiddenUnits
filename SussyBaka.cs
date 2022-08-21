using System.Reflection;
using Landfall.TABS.GameMode;
using System;
using UnityEngine;
using HarmonyLib;

namespace HiddenUnits
{
    [HarmonyPatch(typeof(DataHandler), "Dead", MethodType.Setter)]
    class SussyBaka
    {
        [HarmonyPrefix]
        public static bool Prefix(DataHandler __instance, ref bool value)
        {
            if (value && !(bool)GetField(typeof(DataHandler), __instance, "dead"))
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
            SetField(__instance, "dead", value);
            return false;
        }

        public static object GetField(Type type, object instance, string fieldName)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo field = type.GetField(fieldName, bindingAttr);
            return field.GetValue(instance);
        }

        public static void SetField<T>(object originalObject, string fieldName, T newValue)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo field = originalObject.GetType().GetField(fieldName, bindingAttr);
            field.SetValue(originalObject, newValue);
        }
    }
}