﻿using UnityEngine;
using Landfall.TABS;
using System.Collections.Generic;
using System.Linq;

namespace HiddenUnits
{
    public class AddEffectIfTargetingSelf : MonoBehaviour
    {
        public void DoCheck()
        {
            var hits = Physics.SphereCastAll(transform.position, checkRadius, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            var foundUnits = new List<Unit>();
            foreach (var hit in hits) {
                
                if (hit.transform.root.GetComponent<Unit>() && !foundUnits.Contains(hit.transform.root.GetComponent<Unit>())) foundUnits.Add(hit.rigidbody.transform.root.GetComponent<Unit>());
            }
            var query
                = (
                    from Unit unit
                        in foundUnits
                    where !unit.data.Dead && (unit.data.targetData.unit && unit.data.targetData.unit == transform.root.GetComponent<Unit>())
                    select unit
                ).ToArray();

            if (query.Length > 0)
            {
                foreach (var unit in query)
                {
                    var unitEffectBase = UnitEffectBase.AddEffectToTarget(unit.transform.gameObject, effect);
                    if (unitEffectBase == null) {

                        var obj = Instantiate(effect.gameObject, unit.transform.root);
                        obj.transform.position = unit.transform.position;
                        unitEffectBase = obj.GetComponent<UnitEffectBase>();
                        TeamHolder.AddTeamHolder(obj, transform.root.gameObject);
                        unitEffectBase.DoEffect();
                    }
                    else if (!onlyOnce) {

                        unitEffectBase.Ping();
                    }
                }
            }
        }

        public float checkRadius;

        public bool onlyOnce;
        
        public UnitEffectBase effect;
    }
}