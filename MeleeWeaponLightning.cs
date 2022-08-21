using UnityEngine;
using Landfall.TABS;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace HiddenUnits
{
    public class MeleeWeaponLightning : CollisionWeaponEffect
    {
        public override void DoEffect(Transform hitTransform, Collision collision)
        {
            if (!hitTransform.root.GetComponent<Unit>() || hitTransform.root.GetComponent<Unit>().Team == transform.root.GetComponent<Unit>().Team || hitTransform.root.GetComponent<Unit>().data.Dead)
            {
                return;
            }
            hitList.Add(hitTransform.root.GetComponent<Unit>());
            oldTarget = hitTransform.root.GetComponent<Unit>();
            hitTransform.root.GetComponent<Unit>().data.healthHandler.TakeDamage(damage, new Vector3(0f, 0f, 0f), transform.root.GetComponent<Unit>(), DamageType.Magic);
            SetTarget(transform);
            StartCoroutine(Lightning());
        }

        public void SetTarget(Transform source)
        {
            target = null;
            Unit[] query
            = (
              from Unit unit
              in FindObjectsOfType<Unit>()
              where (!unit.data.Dead && unit.Team != transform.root.GetComponent<Unit>().Team && !hitList.Contains(unit) && (unit.data.mainRig.transform.position - source.position).magnitude <= maxTargetRange)
              orderby (unit.data.mainRig.transform.position - transform.position).magnitude
              select unit
            ).ToArray();
            if (query.Length > 0)
            {
                target = query[0];
            }
        }

        public IEnumerator Lightning()
        {
            for (int i = 0; i < chainCount; i++)
            {
                if (target != null)
                {
                    var line = Instantiate(lineObject, oldTarget.transform, true);
                    line.transform.FindChildRecursive("T1").position = oldTarget.data.mainRig.position;
                    line.transform.FindChildRecursive("T2").position = target.data.mainRig.position;
                    target.data.healthHandler.TakeDamage(damage, new Vector3(0f, 0f, 0f), transform.root.GetComponent<Unit>(), DamageType.Magic);
                    hitList.Add(target);
                    oldTarget = target;
                    SetTarget(target.data.mainRig.transform);
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield break;
        }

        private Unit target;

        private Unit oldTarget;

        private List<Unit> hitList = new List<Unit>();

        public float maxTargetRange = 6f;

        public int chainCount = 20;

        public float damage = 1000f;

        public GameObject lineObject;
    }
}
