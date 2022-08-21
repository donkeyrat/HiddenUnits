using System.Reflection;
using System;
using UnityEngine;
using HarmonyLib;

namespace HiddenUnits
{
    [HarmonyPatch(typeof(Water), "OnTriggerStay")]
    class FromTheDeep
    {
        [HarmonyPrefix]
        public static bool Prefix(Water __instance, Collider other)
        {
			Rigidbody componentInParent = other.GetComponentInParent<Rigidbody>();
			if (!componentInParent)
			{
				return false;
			}
			DataHandler componentInChildren = other.transform.root.GetComponentInChildren<DataHandler>();
			if (componentInChildren && !componentInChildren.unit.GetComponentInChildren<CannotDrown>())
			{
				if (__instance.damageOverTime != 0f && !componentInChildren.Dead && other.gameObject.layer == LayerMask.NameToLayer("MainRig"))
				{
					componentInChildren.healthHandler.TakeDamage(__instance.damageOverTime * Time.deltaTime * (0.01f * componentInChildren.maxHealth), Vector3.zero, null, DamageType.Default);
				}
				if (componentInChildren.Dead && __instance.deadForce != 0f)
				{
					componentInParent.AddForce(Vector3.up * Mathf.Clamp((__instance.transform.position.y + __instance.offet - other.transform.position.y) * 10f, 0f, 10f) * __instance.deadForce * Time.deltaTime, ForceMode.Acceleration);
				}
				componentInChildren.sinceGrounded = Mathf.Clamp(componentInChildren.sinceGrounded, float.NegativeInfinity, 1f);
				componentInParent.AddForce(Vector3.up * Mathf.Clamp((__instance.transform.position.y + __instance.offet - other.transform.position.y) * 10f, 0f, 10f) * __instance.force * Time.deltaTime, ForceMode.Acceleration);
				componentInParent.AddForce(__instance.transform.forward * Mathf.Clamp((__instance.transform.position.y + __instance.offet - other.transform.position.y) * 10f, 0f, 10f) * __instance.streamForce * Time.deltaTime, ForceMode.Acceleration);
			}
			else if (componentInChildren && componentInChildren.unit.GetComponentInChildren<CannotDrown>()) {

				if (componentInChildren.Dead && __instance.deadForce != 0f) {
					componentInParent.AddForce(Vector3.up * Mathf.Clamp((__instance.transform.position.y + __instance.offet - other.transform.position.y) * 10f, 0f, 10f) * __instance.deadForce * Time.deltaTime, ForceMode.Acceleration);
				}
				else {
					componentInParent.AddForce(Vector3.up * 4000f * Time.deltaTime, ForceMode.Acceleration);
				}
				return false;
			}
 			componentInParent.angularVelocity *= 0.9f;
			componentInParent.velocity *= 0.9f;
			return false;
        }
    }
}