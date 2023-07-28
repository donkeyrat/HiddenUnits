using System.Collections;
using System.Linq;
using Landfall.TABS;
using UnityEngine;

namespace HiddenUnits
{
	public class AngelBubble : MonoBehaviour
	{
		private void Start()
		{
			EffectTransform = transform.GetChild(0);
			var hits = Physics.OverlapSphere(EffectTransform.position, radius);
			HitRigs = hits
				.Select(x => x.attachedRigidbody)
				.Where(x => x)
				.Distinct()
				.ToArray();
			
			HitUnits = hits
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
			var startPos = EffectTransform.position;
			while (t < time)
			{
				t += Time.deltaTime;
				
				Pull();
				EffectTransform.position = startPos + Vector3.up * upCurve.Evaluate(t);
				yield return null;
			}
		}

		private void Pull()
		{
			foreach (var rig in HitRigs.Where(x => x)) 
			{
				var position = EffectTransform.position;
				var num = influenceCurve.Evaluate(Vector3.Distance(rig.position, position));
				var randomFloat = Random.Range(0.5f, 1f);
				WilhelmPhysicsFunctions.AddForceWithMinWeight(rig, randomFloat * force * num * (position + Random.insideUnitSphere * 2f - rig.position).normalized, ForceMode.Force, minMassCap);

				rig.AddForce(-200f * randomFloat * Time.deltaTime * rig.velocity, ForceMode.Acceleration);
			}

			foreach (var unit in HitUnits)
			{
				unit.data.healthHandler.TakeDamage(damageOverTime * Time.deltaTime, Vector3.up);
			}
		}
		
		private Rigidbody[] HitRigs;
		private Unit[] HitUnits;
		private Transform EffectTransform;

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
