using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Landfall.TABS;
using System.Linq;

namespace HiddenUnits 
{

    public class SpikeBehavior : MonoBehaviour 
    {
        void Start() 
        {
            if (trigger == RiseType.OnStart) DoSpike();
            
            if (GetComponent<TeamHolder>()) team = GetComponent<TeamHolder>().team;
            else if (GetComponentInParent<TeamHolder>()) team = GetComponentInParent<TeamHolder>().team;
            else if (GetComponentInChildren<TeamHolder>()) team = GetComponentInChildren<TeamHolder>().team;
            else if (transform.root.GetComponent<Unit>()) team = transform.root.GetComponent<Unit>().Team == Team.Red ? Team.Blue : Team.Red;
        }
    
        public void DoSpike() 
        {
            rising = true;
        }
        
        void Update() 
        {
            if (joint) 
            {
                target.data.healthHandler.TakeDamage(stickDamage * Time.deltaTime, Vector3.zero);
               
                if (adjustCounter < 1f) 
                {
                    adjustCounter += Time.deltaTime * adjustTime;
                    joint.connectedAnchor = Vector3.Lerp(joint.connectedAnchor, Vector3.zero, adjustCounter);
                }
            }
    
            if (!target) 
            {
                if (trigger == RiseType.WhenTargetNear) { SetTarget(); }
                else if ((trigger == RiseType.OnStart || trigger == RiseType.Triggerable) && transform.root.GetComponent<Unit>()) { target = transform.root.GetComponent<Unit>(); DoSpike(); foreach (var rig in target.GetComponentsInChildren<Rigidbody>()) rig.velocity *= 0f; }
            }
            
            if (rising) 
            {
                if (impaleType == SpikeType.Scale) transform.localScale += Vector3.up * riseSpeed * Time.deltaTime;
                else tip.localPosition += Vector3.up * riseSpeed * Time.deltaTime;
                
                if (target && Vector3.Distance(tip.position, target.data.mainRig.position) < stickDistance && !joint) {
                    StartCoroutine(DelayEnd());
                    StartCoroutine(Stick());
                }
                else if (target && Vector3.Distance(tip.position, target.data.mainRig.position) > 6f) { Destroy(this); }
            }
        }
    
        public IEnumerator Stick() 
        {
            joint = tip.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = target.data.mainRig;
            joint.autoConfigureConnectedAnchor = false;
            
            target.data.healthHandler.TakeDamage(damage, Vector3.zero);
            
            yield return new WaitForSeconds(stickTime);
            Destroy(joint);
        }
    
        public IEnumerator DelayEnd() 
        {
            yield return new WaitForSeconds(stopSpikeDelay);
            rising = false;
        }
    
        public void SetTarget() {
    
            var hits = Physics.SphereCastAll(tip.position, targetDistance, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            var foundUnits = hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => x && !x.data.Dead && x.Team != team)
                .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
                .Distinct()
                .ToArray();

            if (foundUnits.Length > 0)
            {
                target = foundUnits[0]; 
                DoSpike(); 
                foreach (var rig in target.GetComponentsInChildren<Rigidbody>()) rig.velocity *= 0f;
            }
        }
    
        public enum SpikeType
        {
            Scale,
            MoveTip
        }
    
        public enum RiseType
        {
            OnStart,
            Triggerable,
            WhenTargetNear
        }
    
        private Unit target;
    
        private float adjustCounter;
    
        private FixedJoint joint;
    
        private Team team;
    
        private bool rising;
    
        public SpikeType impaleType;
    
        public RiseType trigger;
    
        public Transform tip;
    
        public float damage = 100f;
    
        public float stickDamage;
    
        public float stickTime = 1f;
    
        public float riseSpeed = 1f;
    
        public float stopSpikeDelay;
    
        public float stickDistance = 0.5f;
    
        public float adjustTime = 1f;
    
        public float targetDistance = 1f;
    }
}