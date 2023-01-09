using Landfall.TABS;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;

namespace HiddenUnits {

    public class AxeTargeting : ProjectileHitEffect {

        public void Start()
        {
            teamHolder = GetComponent<TeamHolder>();
            if (!teamHolder || !teamHolder.spawner || !teamHolder.spawner.GetComponent<Unit>() || !teamHolder.spawner.GetComponent<Unit>().data.targetData) { Destroy(this); }
            target = teamHolder.spawner.GetComponent<Unit>().data.targetData.unit;
            moveTransform = GetComponent<MoveTransform>();
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
                if (teamHolder && teamHolder.spawnerWeapon) { go = teamHolder.spawnerWeapon.transform; }
                if (go != null && Vector3.Distance(transform.position, go.position) < returnRange)
                {
                    Destroy(gameObject);
                    if (teamHolder && teamHolder.spawnerWeapon && teamHolder.spawnerWeapon.GetComponent<DelayEvent>()) { teamHolder.spawnerWeapon.GetComponent<DelayEvent>().Go(); }
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
            moveTransform.velocity = (target.position - transform.position).normalized * moveTransform.selfImpulse.magnitude;
        }

        public void SetTarget()
        {
            var hits = Physics.SphereCastAll(transform.position, maxRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            var foundUnits = hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => teamHolder && x && !x.data.Dead && x.Team != teamHolder.team && !hitList.Contains(x))
                .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
                .Distinct()
                .ToArray();
            if (foundUnits.Length > 0) target = foundUnits[0];
            else finishEvent.Invoke();
        }

        private List<Unit> hitList = new List<Unit>();

        private Unit target;

        private TeamHolder teamHolder;

        private MoveTransform moveTransform;

        public float maxRange = 20f;

        public int hitLimit = 10;

        public float returnRange = 0.3f;

        public UnityEvent finishEvent = new UnityEvent();

        public bool autoTarget = true;

        private bool returned;
    }
}
