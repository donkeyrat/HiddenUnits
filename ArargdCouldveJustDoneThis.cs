using System.Reflection;
using System;
using UnityEngine;
using HarmonyLib;
using TFBGames;
using System.Collections.Generic;

namespace HiddenUnits 
{
    [HarmonyPatch(typeof(CameraSpawnObject), "Start")]
    class ArargdCouldveJustDoneThis 
    {
        [HarmonyPrefix]
        public static bool Start(CameraSpawnObject __instance, InputService ___inputService, int ___currentSelectedIndex) 
        {
            var newObjectsToSpawn = new List<GameObject>();
            var newSounds = new List<string>();
            foreach (var proj in (List<GameObject>)typeof(Landfall.TABS.LandfallUnitDatabase).GetField("Projectiles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Landfall.TABS.LandfallUnitDatabase.GetDatabase())) { 
                if (proj != null && !proj.name.ToUpper().Contains("STORM")) { 
                    newObjectsToSpawn.Add(proj); 
                    newSounds.Add("Medieval Attacks/Bow"); 
                } 
            }
            __instance.soundToPlay = newSounds.ToArray();
            __instance.objectsToSpawn = newObjectsToSpawn.ToArray();

            __instance.rate.LocaleID = "FIREMODE_TAP";
            typeof(CameraSpawnObject).GetField("inputService", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, ServiceLocator.GetService<InputService>());
            typeof(CameraSpawnObject).GetField("m_playerActions", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, Landfall.TABS_Input.PlayerActions.Instance);
            var service = ServiceLocator.GetService<PlayerCamerasManager>();
            var mainCam = (service != null) ? service.GetMainCam(TFBGames.Player.One) : null;
            var mainCamTransform = ((mainCam != null) ? mainCam.transform : null);
            typeof(CameraSpawnObject).GetField("mainCamTransform", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, mainCamTransform);
            typeof(CameraSpawnObject).GetField("maxIndex", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, __instance.objectsToSpawn.Length - 1);
            __instance.objectToSpawn = __instance.objectsToSpawn[___currentSelectedIndex];
            return false;
        }
    }
}