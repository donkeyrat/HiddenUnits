using UnityEngine;
using Landfall.TABS;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace HiddenUnits
{
    public class MeleeWeaponLightning : CollisionWeaponEffect
    {
        private void Start()
        {
            ownUnit = GetComponent<Weapon>().connectedData.unit;
        }
        
        public override void DoEffect(Transform hitTransform, Collision collision)
        {
            if (!hitTransform.root.GetComponent<Unit>() || hitTransform.root.GetComponent<Unit>().Team == ownUnit.Team || hitTransform.root.GetComponent<Unit>().data.Dead)
            {
                return;
            }
            oldTarget = hitTransform.root.GetComponent<Unit>();
            hitList.Add(oldTarget);
            oldTarget.data.healthHandler.TakeDamage(damage, Vector3.zero);
            
            SetTarget(transform.position);
            StartCoroutine(DoLightning());
        }

        public IEnumerator DoLightning()
        {
            for (int i = 0; i < chainCount; i++)
            {
                for (int j = 0; j < consecutiveChains; j++)
                {
                    if (target && target.data && target.data.healthHandler && lineObject && oldTarget)
                    {
                        var line = Instantiate(lineObject, oldTarget.transform, true);
                        line.transform.FindChildRecursive("T1").position = oldTarget.data.mainRig.position;
                        line.transform.FindChildRecursive("T2").position = target.data.mainRig.position;
                        
                        target.data.healthHandler.TakeDamage(damage, Vector3.zero);
                        
                        hitList.Add(target);
                        oldTarget = target;
                    }
                    
                    if (oldTarget) SetTarget(oldTarget.data.mainRig.position);
                }

                yield return new WaitForSeconds(0.1f);
            }
            
            hitList.Clear();
        }
        
        public void SetTarget(Vector3 source)
        {
            target = null;
            var hits = Physics.SphereCastAll(transform.position, maxTargetRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            var foundUnits = hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => x && !x.data.Dead && x.Team != ownUnit.Team && !hitList.Contains(x))
                .OrderBy(x => (x.data.mainRig.transform.position - source).magnitude)
                .Distinct()
                .ToArray();
            if (foundUnits.Length > 0) target = foundUnits[0];
        }

        private Unit target;

        private Unit oldTarget;

        private Unit ownUnit;

        private List<Unit> hitList = new List<Unit>();

        public float maxTargetRange = 6f;

        public int chainCount = 20;

        public float damage = 1000f;

        public GameObject lineObject;

        public int consecutiveChains = 1;
    }
}
