using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Landfall.TABS;
using System.Linq;
using RootMotion.FinalIK;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class RootBehavior : MonoBehaviour
    {
        private void Start()
        {
            Team = GetComponent<TeamHolder>();
        }
        
        private void Update()
        {
            Counter += Time.deltaTime;
            if (UnitTarget && !Joint && Vector3.Distance(tip.position, UnitTarget.data.mainRig.position) < attachDistance && UnitTarget.data.mainRig.GetComponents<ConfigurableJoint>().Length < 3)
            {
                StartCoroutine(AttachJoint());
            }
            else if (UnitTarget && Joint && doConstantDamage)
            {
                UnitTarget.data.healthHandler.TakeDamage(damage * Time.deltaTime, Vector3.zero);
            }
        }
        
        public void DoRooting()
        {
            StartCoroutine(ChooseTarget());
        }

        private IEnumerator ChooseTarget()
        {
            SetTarget();
            if (UnitTarget)
            {
                var t = 0f;
                var beginPosition = moveTarget.position;
                while (t < 1f && !Joint && UnitTarget)
                {
                    t += Time.deltaTime * followSpeed;
                    moveTarget.position = Vector3.Lerp(beginPosition, UnitTarget.data.mainRig.position,
                        Mathf.Clamp(t, 0f, 1f));
                    yield return null;
                }
            }
        }

        public IEnumerator AttachJoint()
        {
            if (!UnitTarget) yield break;
            foreach (var rig in rigs)
            {
                rig.isKinematic = false;
            }
            GetComponent<CCDIK>().enabled = false;

            Joint = UnitTarget.data.mainRig.gameObject.AddComponent<FixedJoint>();
            Joint.connectedBody = tip.GetComponent<Rigidbody>();
            Joint.breakForce = breakForce;
            Joint.breakTorque = breakForce;
            
            if (lerpToCenterOfTip) StartCoroutine(LerpJoint());
            
            if (!doConstantDamage) UnitTarget.data.healthHandler.TakeDamage(damage, Vector3.zero);
            hitEvent.Invoke();

            var seconds = GetComponent<RemoveAfterSeconds>();
            yield return new WaitUntil(() => Counter >= seconds.seconds - 1f);
            
            if (Joint) Destroy(Joint);
        }

        public IEnumerator LerpJoint()
        {
            if (!Joint) yield break;
            var t = 0f;
            var initialVector = Joint.connectedAnchor;
            while (t < 1f && Joint)
            {
                Joint.autoConfigureConnectedAnchor = false;
                t += Time.deltaTime * adjustTime;
                Joint.connectedAnchor = Vector3.Lerp(initialVector, Vector3.zero, Mathf.Clamp(t, 0f, 1f));
                yield return null;
            }
        }
        
        public void SetTarget() 
        {
            var hits = Physics.SphereCastAll(transform.position, targetRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            var foundUnits = hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => x && !x.data.Dead && x.Team != Team.team)
                .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
                .Distinct()
                .ToArray();

            if (foundUnits.Length > 0)
            {
                UnitTarget = foundUnits[0]; 
            }
        }

        private float Counter;
        private Unit UnitTarget;
        private FixedJoint Joint;
        private TeamHolder Team;
        
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