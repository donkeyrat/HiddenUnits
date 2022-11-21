using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace HiddenUnits.HarmonyPatches
{
    [HarmonyPatch(typeof(Mount), "Fall")]
    class MountForcePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Mount __instance, ref MountPos ___myMountPos, ref DataHandler ___otherData, ref MovementHandler ___mountMove, ref bool ___isMounted, ref DataHandler ___data, ref Landfall.TABS.AI.UnitAPI ___m_unitApi, ref RigidbodyHolder ___rigHolder, ref ConfigurableJoint[] ___joints)
		{
			if (___myMountPos.setMassOnMountOnDrop != 0f)
			{
				___otherData.mainRig.mass = ___myMountPos.setMassOnMountOnDrop;
			}
			if (___myMountPos.upwardsDropForce != 0f)
			{
				___otherData.mainRig.AddForce(Vector3.up * ___myMountPos.upwardsDropForce, ForceMode.VelocityChange);
			}
			if (___myMountPos.forwardDropForce != 0f)
			{
				___otherData.mainRig.AddForce(___otherData.characterForwardObject.forward * ___myMountPos.forwardDropForce, ForceMode.VelocityChange);
			}
			if (___myMountPos.killVehicleIfRiderDies)
			{
				__instance.GetType().GetMethod("KillMount", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { });
			}
			if ((bool)___mountMove && ___mountMove.otherHolders.Contains(___rigHolder) && Random.value < 0.02f)
			{
				___mountMove.otherHolders.Remove(___rigHolder);
			}
			if (___myMountPos.removeArmColliders)
			{
				__instance.GetType().GetMethod("ToggleColliders", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { true });
			}
			for (int i = 0; i < ___joints.Length; i++)
			{
				Object.Destroy(___joints[i]);
			}
			___isMounted = false;
			___m_unitApi.SetIsRider(___isMounted);
			if (___myMountPos.inheritVelocity && Random.value > 0.01f)
			{
				___mountMove = ___myMountPos.GetComponentInChildren<MovementHandler>();
				if ((bool)___mountMove && ___mountMove.otherHolders != null && ___mountMove.otherHolders.Contains(___rigHolder))
				{
					___mountMove.otherHolders.Remove(___rigHolder);
				}
			}
			if (___myMountPos.upwardDismountForce != 0f)
			{
				___data.mainRig.AddForce(Vector3.up * ___myMountPos.upwardDismountForce, ForceMode.VelocityChange);
			}
			if (___myMountPos.forwardDismountForce != 0f)
			{
				___data.mainRig.AddForce(___otherData.characterForwardObject.forward * ___myMountPos.forwardDismountForce, ForceMode.VelocityChange);
			}
			if (___myMountPos.GetComponent<SidewaysForce>())
			{
				___data.mainRig.AddForce(___data.mainRig.transform.right * ___myMountPos.GetComponent<SidewaysForce>().sidewaysForce, ForceMode.VelocityChange);
			}
			return false;
        }
    }
}