using System.Linq;
using HarmonyLib;
using Landfall.TABS;
using TGCore;
using UnityEngine;

namespace HiddenUnits.HarmonyPatches
{
    [HarmonyPatch(typeof(SecretUnlock), "UnlockSecret")]
    internal class SecretUnlockPatch
    {
        [HarmonyPostfix]
        public static void Postfix(SecretUnlock __instance)
        {
            var saveLoader = ServiceLocator.GetService<ISaveLoaderService>();

            var unlockCondition = HUMain.campaignUnlocker.m_unlockConditions[0];
            
            if (saveLoader.HasUnlockedSecret(unlockCondition.m_unlock)) return;
            
            var allUnlocked = true;
            foreach (var unlock in unlockCondition.m_conditionUnlocks)
            {
                if (!saveLoader.HasUnlockedSecret(unlock))
                {
                    allUnlocked = false;
                }
            }

            if (allUnlocked)
            {
                saveLoader.UnlockSecret(unlockCondition.m_unlock);
                ServiceLocator.GetService<ModalPanel>().OpenUnlockPanel(unlockCondition.m_unlockDescription, unlockCondition.m_unlockImage);
            }
        }
    }
}