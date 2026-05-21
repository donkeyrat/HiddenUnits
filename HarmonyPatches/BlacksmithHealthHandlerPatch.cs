/*
using HarmonyLib;
using Landfall.TABS;
using Landfall.TABS.GameMode;
using Landfall.TABS.GameState;
using TGCore.Library;
using UnityEngine;

namespace HiddenUnits.HarmonyPatches
{
    [HarmonyPatch(typeof(HealthHandler), "TakeDamage")]
    class BlacksmithHealthHandlerPatch
    {
        [HarmonyPostfix]
        public static void Postfix(HealthHandler __instance, float __damage, Unit __damager, GameStateManager ___m_gameStateManager, bool ___isInvulnerable, ref DataHandler ___data, SettingsInstance ___m_bugUnitsDying, GameModeService ___gameModeService)
        {
            if ((___m_gameStateManager == null || ___m_gameStateManager.GameState == GameState.BattleState) &&
                !___isInvulnerable && (!(___data.immunityForSeconds > 0f) || !(__damage > 0f)) &&
                !(___data.lifeTime < 0.3f) && !___data.unit.WasDamaged(null, null) 
                && __damager)
            {
                var returnDamage = ___data.unit.GetComponentInChildren<Effect_ReturnDamage>();
                if (returnDamage) returnDamage.ReceiveDamage();
            }
        }
    }
}
*/