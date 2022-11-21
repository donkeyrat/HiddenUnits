using System.Collections.Generic;
using Landfall.TABS;
using UnityEngine;
using System.Linq;

namespace HiddenUnits
{

    public class ProjectileAimAtRandomUnit : MonoBehaviour
    {
        public LayerMask mainRigMask;

        public float radius = 5f;

        private void Start()
        {
            if (!GetComponent<TeamHolder>())
            {
                return;
            }

            SetTarget();

            if (target)
            {
                Compensation component1 = GetComponent<Compensation>();
                MoveTransform component2 = GetComponent<MoveTransform>();
                if (!component2 || !GetComponent<TeamHolder>()) return;
                transform.rotation = Quaternion.LookRotation(component1.GetCompensation(target.data.mainRig.position, target.data.mainRig.velocity, 0f));
                component2.Initialize();
            }
        }

        public void SetTarget()
        {
            var hits = Physics.SphereCastAll(transform.position, radius, Vector3.up, 0.1f, mainRigMask);
            var foundUnits = hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => GetComponent<TeamHolder>() && x && !x.data.Dead && x.Team != GetComponent<TeamHolder>().team)
                .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
                .Distinct()
                .ToArray();
            if (foundUnits.Length > 0) target = foundUnits[Random.Range(0, foundUnits.Length - 1)];
        }

        private Unit target;
    }

}