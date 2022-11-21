using Landfall.TABS;
using HarmonyLib;
using TFBGames;

namespace HiddenUnits.HarmonyPatches
{
    [HarmonyPatch(typeof(Unit), "SetHighlight", MethodType.Setter)]
    class CustomOutlinePatch
    {
        [HarmonyPrefix]
        public static void Postfix(Unit __instance)
        {
            if (__instance.GetComponentInChildren<ChangeOutline>() != null)
            {
                var highlighter = (IHighlight)HUAddons.GetField(typeof(Unit), __instance, "m_highlighter");
                highlighter.BeginHighlight();
                highlighter.SetHighlightColor(__instance.GetComponentInChildren<ChangeOutline>().outlineColor);
            }
        }
    }
}