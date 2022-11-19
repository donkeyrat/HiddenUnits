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
            oldTarget.data.healthHandler.TakeDamage(damage, new Vector3(0f, 0f, 0f), transform.root.GetComponent<Unit>(), DamageType.Magic);
            SetTarget(transform);
            StartCoroutine(Lightning());
        }

        public IEnumerator Lightning()
        {
            for (int i = 0; i < chainCount; i++)
            {
                for (int j = 0; j < consecutiveChains; j++)
                {
                    if (target != null && target.data && target.data.healthHandler && lineObject && oldTarget)
                    {
                        var line = Instantiate(lineObject, oldTarget.transform, true);
                        line.transform.FindChildRecursive("T1").position = oldTarget.data.mainRig.position;
                        line.transform.FindChildRecursive("T2").position = target.data.mainRig.position;
                        target.data.healthHandler.TakeDamage(damage, new Vector3(0f, 0f, 0f), transform.root.GetComponent<Unit>(), DamageType.Magic);
                        hitList.Add(target);
                        oldTarget = target;
                    }
                    if (oldTarget != null)
                    {
                        SetTarget(oldTarget.data.mainRig.transform);
                    }
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
        
        public void SetTarget(Transform source)
        {
            target = null;
            var hits = Physics.SphereCastAll(transform.position, maxTargetRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            var foundUnits = hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => x && !x.data.Dead && x.Team != transform.root.GetComponent<Unit>().Team && !hitList.Contains(x))
                .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
                .Distinct()
                .ToArray();
            if (foundUnits.Length > 0) target = foundUnits[0];
        }

        private Unit target;

        private Unit oldTarget;

        private List<Unit> hitList = new List<Unit>();

        public float maxTargetRange = 6f;

        public int chainCount = 20;

        public float damage = 1000f;

        public GameObject lineObject;

        public int consecutiveChains = 1;
    }
}
