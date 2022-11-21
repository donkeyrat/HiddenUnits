using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HiddenUnits
{
	public class AngelBubble : MonoBehaviour
	{
		private List<Rigidbody> rigs = new List<Rigidbody>();

		private List<Vector3> randomOffset = new List<Vector3>();

		private List<float> floatynes = new List<float>();

		private Transform effectTransform;

		public float radius = 5f;

		public float force = 1000f;

		public float damage;

		public AnimationCurve curve;

		public AnimationCurve upCurve;

		public AnimationCurve throwCurve;

		public float throwForce;

		private Damagable[] healths;

		private void Start()
		{
			effectTransform = base.transform.GetChild(0);
			Collider[] array = Physics.OverlapSphere(effectTransform.position, radius);
			for (int i = 0; i < array.Length; i++)
			{
				if ((bool)array[i].attachedRigidbody && !rigs.Contains(array[i].attachedRigidbody))
				{
					rigs.Add(array[i].attachedRigidbody);
					randomOffset.Add(Random.insideUnitSphere * 2f);
					floatynes.Add(Random.Range(0.5f, 1f));
				}
			}
			healths = new Damagable[rigs.Count];
			for (int j = 0; j < rigs.Count; j++)
			{
				healths[j] = rigs[j].GetComponentInParent<Damagable>();
			}
			StartCoroutine(AnimateSpell());
			base.transform.rotation = Quaternion.identity;
		}

		private IEnumerator AnimateSpell()
		{
			float t = upCurve.keys[upCurve.keys.Length - 1].time;
			float c = 0f;
			while (c < t)
			{
				Pull();
				effectTransform.localPosition = Vector3.zero + Vector3.up * upCurve.Evaluate(c);
				c += Time.deltaTime;
				yield return null;
			}
		}

		private void Pull()
		{
			for (int i = 0; i < rigs.Count; i++)
			{
				if (!(rigs[i] == null))
				{
					float time = Vector3.Distance(rigs[i].position, effectTransform.position);
					float num = curve.Evaluate(time);
					WilhelmPhysicsFunctions.AddForceWithMinWeight(rigs[i], floatynes[i] * force * num * (effectTransform.position + randomOffset[i] - rigs[i].position).normalized, ForceMode.Force, 60f);
					if ((bool)healths[i])
					{
						healths[i].TakeDamage(damage * Time.deltaTime, Vector3.zero, null);
					}
					rigs[i].AddForce(-200f * floatynes[i] * Time.deltaTime * rigs[i].velocity, ForceMode.Acceleration);
				}
			}
		}

		private void Throw(Vector3 target, float curveMultiplier)
		{
			for (int i = 0; i < rigs.Count; i++)
			{
				float time = Vector3.Distance(rigs[i].position, effectTransform.position);
				float num = curve.Evaluate(time);
				WilhelmPhysicsFunctions.AddForceWithMinWeight(rigs[i], curveMultiplier * floatynes[i] * num * throwForce * (target + randomOffset[i] * 0.5f - rigs[i].position).normalized, ForceMode.Force, 60f);
				rigs[i].AddForce(-400f * floatynes[i] * Time.deltaTime * rigs[i].velocity, ForceMode.Acceleration);
			}
		}
	}
}
