using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HiddenUnits
{
	public class AngelBubble : MonoBehaviour
	{
		private readonly List<Rigidbody> rigs = new List<Rigidbody>();

		private readonly List<Vector3> randomOffset = new List<Vector3>();

		private readonly List<float> floatiness = new List<float>();

		private Transform effectTransform;

		public float radius = 5f;

		public float force = 1000f;

		public float damage;

		public AnimationCurve curve;

		public AnimationCurve upCurve;

		private Damagable[] healths;

		private void Start()
		{
			effectTransform = transform.GetChild(0);
			Collider[] array = Physics.OverlapSphere(effectTransform.position, radius);
			foreach (var t in array)
			{
				if ((bool)t.attachedRigidbody && !rigs.Contains(t.attachedRigidbody))
				{
					rigs.Add(t.attachedRigidbody);
					randomOffset.Add(Random.insideUnitSphere * 2f);
					floatiness.Add(Random.Range(0.5f, 1f));
				}
			}
			healths = new Damagable[rigs.Count];
			for (int j = 0; j < rigs.Count; j++)
			{
				healths[j] = rigs[j].GetComponentInParent<Damagable>();
			}
			StartCoroutine(AnimateSpell());
			transform.rotation = Quaternion.identity;
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
					var position = effectTransform.position;
					float time = Vector3.Distance(rigs[i].position, position);
					float num = curve.Evaluate(time);
					WilhelmPhysicsFunctions.AddForceWithMinWeight(rigs[i], floatiness[i] * force * num * (position + randomOffset[i] - rigs[i].position).normalized, ForceMode.Force, 60f);
					if ((bool)healths[i])
					{
						healths[i].TakeDamage(damage * Time.deltaTime, Vector3.zero, null);
					}
					rigs[i].AddForce(-200f * floatiness[i] * Time.deltaTime * rigs[i].velocity, ForceMode.Acceleration);
				}
			}
		}
	}
}
