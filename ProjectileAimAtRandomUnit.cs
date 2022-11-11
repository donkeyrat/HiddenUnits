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
            TeamHolder component = GetComponent<TeamHolder>();
            if (!component)
            {
                return;
            }

            SetTarget();

            if (target)
            {
                Compensation component1 = GetComponent<Compensation>();
                MoveTransform component2 = GetComponent<MoveTransform>();
                if ((bool)component2 && (bool)component)
                {
                    base.transform.rotation = Quaternion.LookRotation(component1.GetCompensation(target.data.mainRig.position, target.data.mainRig.velocity, 0f));
                    component2.Initialize();
                }
            }
        }

        public void SetTarget()
        {
            var hits = Physics.SphereCastAll(transform.position, radius, Vector3.up, 0.1f, mainRigMask);
            List<Unit> foundUnits = new List<Unit>();
            foreach (var hit in hits)
            {
                if (hit.transform.root.GetComponent<Unit>() && !foundUnits.Contains(hit.transform.root.GetComponent<Unit>()))
                {
                    foundUnits.Add(hit.rigidbody.transform.root.GetComponent<Unit>());
                }
            }
            Unit[] query
                = (
                    from Unit unit
                        in foundUnits
                    where GetComponent<TeamHolder>() && !unit.data.Dead && unit.Team != GetComponent<TeamHolder>().team
                    orderby (unit.data.mainRig.transform.position - transform.position).magnitude
                    select unit
                ).ToArray();
            if (query.Length > 0)
            {
                target = query[Random.Range(0, query.Length - 1)];
            }
        }

        private Unit target;
    }

}