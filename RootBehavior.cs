using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Landfall.TABS;
using System.Linq;
using BitCode.Debug.Commands;
using RootMotion.FinalIK;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class RootBehavior : MonoBehaviour
    {
        public void Update()
        {
            counter += Time.deltaTime;
            if (unitTarget && !joint && Vector3.Distance(tip.position, unitTarget.data.mainRig.position) < attachDistance && unitTarget.data.mainRig.GetComponents<ConfigurableJoint>().Length < 3)
            {
                StartCoroutine(AttachJoint());
            }
            else if (unitTarget && joint && doConstantDamage)
            {
                unitTarget.data.healthHandler.TakeDamage(damage * Time.deltaTime, Vector3.zero);
            }
        }
        
        public void DoRooting()
        {
            StartCoroutine(ChooseTarget());
        }

        public IEnumerator ChooseTarget()
        {
            SetTarget();
            if (unitTarget)
            {
                var t = 0f;
                var beginPosition = moveTarget.position;
                while (t < 1f && !joint && unitTarget)
                {
                    t += Time.deltaTime * followSpeed;
                    moveTarget.position = Vector3.Lerp(beginPosition, unitTarget.data.mainRig.position,
                        Mathf.Clamp(t, 0f, 1f));
                    yield return null;
                }
            }
        }

        public IEnumerator AttachJoint()
        {
            if (!unitTarget) yield break;
            foreach (var rig in rigs)
            {
                rig.isKinematic = false;
            }
            GetComponent<CCDIK>().enabled = false;

            joint = unitTarget.data.mainRig.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = tip.GetComponent<Rigidbody>();
            joint.breakForce = breakForce;
            joint.breakTorque = breakForce;
            
            if (lerpToCenterOfTip) StartCoroutine(LerpJoint());
            
            if (!doConstantDamage) unitTarget.data.healthHandler.TakeDamage(damage, Vector3.zero);
            hitEvent.Invoke();

            var seconds = GetComponent<RemoveAfterSeconds>();
            yield return new WaitUntil(() => counter >= seconds.seconds - 1f);
            
            if (joint) Destroy(joint);
        }

        public IEnumerator LerpJoint()
        {
            if (!joint) yield break;
            var t = 0f;
            var initialVector = joint.connectedAnchor;
            while (t < 1f && joint)
            {
                joint.autoConfigureConnectedAnchor = false;
                t += Time.deltaTime * adjustTime;
                joint.connectedAnchor = Vector3.Lerp(initialVector, Vector3.zero, Mathf.Clamp(t, 0f, 1f));
                yield return null;
            }
        }
        
        public void SetTarget() 
        {
            var hits = Physics.SphereCastAll(transform.position, targetRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            var foundUnits = hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => x && !x.data.Dead && x.Team != GetComponent<TeamHolder>().team)
                .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
                .Distinct()
                .ToArray();

            if (foundUnits.Length > 0)
            {
                unitTarget = foundUnits[0]; 
            }
        }

        private float counter;
        
        private Unit unitTarget;
        
        private FixedJoint joint;
        
        [Header("Root Settings")]

        public List<Rigidbody> rigs = new List<Rigidbody>();
        
        public Transform moveTarget;

        public Transform tip;

        public float targetRange = 3f;

        public float followSpeed = 1f;
        
        [Header("Joint Settings")]

        public float adjustTime = 1f;

        public float attachDistance = 1f;
        
        public float breakForce = 100000f;

        public bool lerpToCenterOfTip;

        [Header("Damage Settings")] 
        
        public UnityEvent hitEvent = new UnityEvent();
        
        public float damage = 20f;

        public bool doConstantDamage;
    }
}