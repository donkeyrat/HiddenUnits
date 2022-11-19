using UnityEngine;
using Landfall.TABS;
using System.Collections.Generic;
using System.Linq;

namespace HiddenUnits
{
	class FlyTowardsNonTeam : MonoBehaviour
	{
		void Start()
		{
			rig = GetComponent<Rigidbody>();
			if (transform.root.GetComponent<Unit>()) SetTarget(true);
			else SetTarget(false);
		}

		void Update()
		{
			if (target && !target.data.Dead) { rig.AddForce((target.data.mainRig.position - transform.position).normalized * forwardForce * Time.deltaTime); }
			else {
				if (transform.root.GetComponent<Unit>()) { SetTarget(true); }
				else { SetTarget(false); }
			}
		}

		
		void SetTarget(bool doTeamCheck)
		{
			var hits = Physics.SphereCastAll(transform.position, 80f, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
			var foundUnits = hits
				.Select(hit => hit.transform.root.GetComponent<Unit>())
				.Where(x => x && !x.data.Dead && (doTeamCheck && x.Team != transform.root.GetComponent<Unit>().Team || !doTeamCheck))
				.OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
				.Distinct()
				.ToArray();
			if (foundUnits.Length != 0) target = foundUnits[0];
		}

		public float forwardForce = 20000f;

		private Unit target;

		private Rigidbody rig;
	}
}
