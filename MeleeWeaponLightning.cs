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
            OwnUnit = GetComponent<Weapon>().connectedData.unit;
        }
        
        public override void DoEffect(Transform hitTransform, Collision collision)
        {
            if (!hitTransform.root.GetComponent<Unit>() || hitTransform.root.GetComponent<Unit>().Team == OwnUnit.Team || hitTransform.root.GetComponent<Unit>().data.Dead)
            {
                return;
            }
            OldTarget = hitTransform.root.GetComponent<Unit>();
            HitList.Add(OldTarget);
            OldTarget.data.healthHandler.TakeDamage(damage, Vector3.zero);
            
            SetTarget(transform.position);
            StartCoroutine(DoLightning());
        }

        public IEnumerator DoLightning()
        {
            for (int i = 0; i < chainCount; i++)
            {
                for (int j = 0; j < consecutiveChains; j++)
                {
                    if (Target && Target.data && Target.data.healthHandler && lineObject && OldTarget)
                    {
                        var line = Instantiate(lineObject, OldTarget.transform, true);
                        line.transform.FindChildRecursive("T1").position = OldTarget.data.mainRig.position;
                        line.transform.FindChildRecursive("T2").position = Target.data.mainRig.position;
                        
                        Target.data.healthHandler.TakeDamage(damage, Vector3.zero);
                        
                        HitList.Add(Target);
                        OldTarget = Target;
                    }
                    
                    if (OldTarget) SetTarget(OldTarget.data.mainRig.position);
                }

                yield return new WaitForSeconds(0.1f);
            }
            
            HitList.Clear();
        }
        
        public void SetTarget(Vector3 source)
        {
            Target = null;
            var hits = Physics.SphereCastAll(transform.position, maxTargetRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            var foundUnits = hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => x && !x.data.Dead && x.Team != OwnUnit.Team && !HitList.Contains(x))
                .OrderBy(x => (x.data.mainRig.transform.position - source).magnitude)
                .Distinct()
                .ToArray();
            if (foundUnits.Length > 0) Target = foundUnits[0];
        }

        private Unit Target;

        private Unit OldTarget;

        private Unit OwnUnit;

        private List<Unit> HitList = new List<Unit>();

        public float maxTargetRange = 6f;

        public int chainCount = 20;

        public float damage = 1000f;

        public GameObject lineObject;

        public int consecutiveChains = 1;
    }
}
