using System.Linq;
using HarmonyLib;
using Landfall.TABS;
using TGCore;
using TGCore.Library;
using UnityEngine;

namespace HiddenUnits.HarmonyPatches
{
    [HarmonyPatch(typeof(SecretUnlockPiece), "UnlockSecret")]
    internal class SecretUnlockPiecePatch
    {
        [HarmonyPostfix]
        public static void Postfix(SecretUnlock __instance)
        {
            HUMain.CheckUnlockConditions();
        }
    }
}