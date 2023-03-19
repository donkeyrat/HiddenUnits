using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Landfall.TABS;
using UnityEngine;

namespace HiddenUnits
{
	public class AngelBubble : MonoBehaviour
	{
		private void Start()
		{
			effectTransform = transform.GetChild(0);
			var hits = Physics.OverlapSphere(effectTransform.position, radius);
			hitRigs = hits
				.Select(x => x.attachedRigidbody)
				.Where(x => x)
				.Distinct()
				.ToArray();
			
			hitUnits = hits
				.Select(hit => hit.transform.root.GetComponent<Unit>())
				.Where(x => x && !x.data.Dead)
				.OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
				.Distinct()
				.ToArray();
			
			StartCoroutine(AnimateSpell());
		}

		private IEnumerator AnimateSpell()
		{
			var time = upCurve.keys[upCurve.keys.Length - 1].time;
			var t = 0f;
			var startPos = effectTransform.position;
			while (t < time)
			{
				t += Time.deltaTime;
				
				Pull();
				effectTransform.position = startPos + Vector3.up * upCurve.Evaluate(t);
				yield return null;
			}
		}

		private void Pull()
		{
			foreach (var rig in hitRigs.Where(x => x)) 
			{
				var position = effectTransform.position;
				var num = influenceCurve.Evaluate(Vector3.Distance(rig.position, position));
				var randomFloat = Random.Range(0.5f, 1f);
				WilhelmPhysicsFunctions.AddForceWithMinWeight(rig, randomFloat * force * num * (position + Random.insideUnitSphere * 2f - rig.position).normalized, ForceMode.Force, minMassCap);

				rig.AddForce(-200f * randomFloat * Time.deltaTime * rig.velocity, ForceMode.Acceleration);
			}

			foreach (var unit in hitUnits)
			{
				unit.data.healthHandler.TakeDamage(damageOverTime * Time.deltaTime, Vector3.up);
			}
		}
		
		private Rigidbody[] hitRigs;
		private Unit[] hitUnits;
		private Transform effectTransform;

		[Header("Damage Settings")]
		
		public float radius = 5f;
		public float force = 1000f;
		public float minMassCap = 60f;
		public float damageOverTime = 60f;
		
		[Header("Curve Settings")]

		public AnimationCurve influenceCurve;
		public AnimationCurve upCurve;
	}
}
