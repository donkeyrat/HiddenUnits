using Landfall.TABS;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;

namespace HiddenUnits {

    public class AxeTargeting : ProjectileHitEffect {

        public void Start() {

            if (!GetComponent<TeamHolder>() || !GetComponent<TeamHolder>().spawner) { Destroy(this); }
            target = GetComponent<TeamHolder>().spawner.GetComponent<Unit>().data.targetData.unit;
        }

        public override bool DoEffect(HitData hit) {
            if (hit != null && hit.rigidbody != null && hit.rigidbody.transform.root.GetComponentInChildren<Unit>() != null)
            {
                hitList.Add(hit.rigidbody.transform.root.GetComponentInChildren<Unit>());
            }
            if (hitList.Count >= hitLimit)
            {
                finishEvent.Invoke();
                return false;
            }
            SetTarget();
            return false;
        }

        public void Update()
        {
            Transform go = null;
            if (autoTarget && target != null && target.data && !target.data.Dead)
            {
                if (target.data.mainRig) { go = target.data.mainRig.transform; }
            }
            else if (returned)
            {
                if (GetComponent<TeamHolder>() && GetComponent<TeamHolder>().spawnerWeapon) { go = GetComponent<TeamHolder>().spawnerWeapon.transform; }
                if (go != null && Vector3.Distance(transform.position, go.position) < returnRange)
                {
                    Destroy(gameObject);
                    if (GetComponent<TeamHolder>() && GetComponent<TeamHolder>().spawnerWeapon && GetComponent<TeamHolder>().spawnerWeapon.GetComponent<DelayEvent>()) { GetComponent<TeamHolder>().spawnerWeapon.GetComponent<DelayEvent>().Go(); }
                }
            }
            if (!returned && (target == null || target.data.Dead))
            {
                SetTarget();
            }
            if (go != null)
            {
                GoTowards(go);
            }
        }
        public void Return()
        {
            target = null;
            returned = true;
        }

        public void GoTowards(Transform target)
        {
            GetComponent<MoveTransform>().velocity = (target.position - transform.position).normalized * GetComponent<MoveTransform>().selfImpulse.magnitude;
        }

        public void SetTarget()
        {
            var hits = Physics.SphereCastAll(transform.position, maxRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
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
              where GetComponent<TeamHolder>() && !unit.data.Dead && unit.Team != GetComponent<TeamHolder>().team && !hitList.Contains(unit)
              orderby (unit.data.mainRig.transform.position - transform.position).magnitude
              select unit
            ).ToArray();
            if (query.Length > 0)
            {
                target = query[0];
            }
            else
            {
                finishEvent.Invoke();
            }
        }

        private List<Unit> hitList = new List<Unit>();

        private Unit target;

        public float maxRange = 20f;

        public int hitLimit = 10;

        public float returnRange = 0.3f;

        public UnityEvent finishEvent = new UnityEvent();

        public bool autoTarget = true;

        private bool returned;
    }
}
