using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Landfall.TABS;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class ChickenSticks : MonoBehaviour
    {
        public void Start()
        {
            Rig = GetComponent<Rigidbody>();
            offset = GetComponentInParent<Holdable>().hl ? new Vector3(-offset.x, offset.y, offset.z) : offset;
        }
        
        public void Update()
        {
            DamageCounter += Time.deltaTime;
            if (currentState == ClubState.Idle && hoverTarget)
            {
                Rig.AddForce((hoverTarget.TransformPoint(offset) - transform.position) * idleForce, ForceMode.Acceleration);
                Rig.velocity *= idleDrag;
                Rig.angularVelocity *= idleDrag;
                Rig.AddTorque(Vector3.Cross(transform.forward, hoverTarget.forward).normalized * Vector3.Angle(transform.forward, hoverTarget.forward) * idleAngularForce, ForceMode.Acceleration);
                Rig.AddTorque(Vector3.Cross(transform.up, Vector3.up).normalized * Vector3.Angle(transform.up, Vector3.up) * idleAngularForce * 0.2f, ForceMode.Acceleration);
            }
            else if (currentState == ClubState.Roaming)
            {
                if (RoamingTarget)
                {
                    Rig.AddForce((RoamingTarget.data.mainRig.position - transform.position).normalized * roamingForce * Rig.mass * Time.deltaTime);
                    Rig.AddTorque((RoamingTarget.data.mainRig.position - transform.position).normalized * roamingTorque * Rig.mass * Time.deltaTime);
                }
                else SetTarget(targetingRadius);
            }
        }

        public void SetState(ClubState state)
        {
            currentState = state;
            HitList.Clear();
            if (state == ClubState.Idle) Rig.drag = idleDragAmount;
            if (state == ClubState.Swinging) Rig.drag = swingingDragAmount;
            if (state == ClubState.Roaming) Rig.drag = roamingDragAmount;
            if (state == ClubState.Disabled) Rig.drag = 0f;
        }

        public void SetStateDisabled()
        {
            SetState(ClubState.Disabled);
        }

        public void Swing()
        {
            StartCoroutine(DoSwing());
        }

        public IEnumerator DoSwing()
        {
            yield return new WaitUntil(() => currentState == ClubState.Idle);
            
            SetState(ClubState.Swinging);;
            CanDealDamage = true;
            
            yield return new WaitForSeconds(returnDelay);
            
            SetState(ClubState.Idle);

            if (!dealDamageOutsideOfSwing)
            {
                yield return new WaitForSeconds(disableDamageDelay);
                CanDealDamage = false;
            }
        }

        public void Roam()
        {
            StartCoroutine(StartRoaming());
        }

        public IEnumerator StartRoaming()
        {
            yield return new WaitUntil(() => currentState == ClubState.Idle);
            
            SetState(ClubState.Roaming);
            SetTarget(50f);
            roamingBeginEvent.Invoke();

            yield return new WaitForSeconds(roamingTimer);

            SetState(ClubState.Idle);
            roamingEndEvent.Invoke();
        }
        
        public void OnCollisionEnter(Collision col) 
        {
            var enemyUnit = col.transform.root.GetComponent<Unit>();
            if (DamageCounter < damageCooldown || !enemyUnit || !col.rigidbody || (enemyUnit && HitList.Contains(enemyUnit)) || (enemyUnit && enemyUnit.Team == transform.root.GetComponent<Unit>().Team) || !CanDealDamage)
            {
                return;
            }
            DamageCounter = 0f;

            var flag = col.transform.IsChildOf(enemyUnit.data.transform);
            
            enemyUnit.data.healthHandler.TakeDamage(damage * (flag ? 1f : 0f), Vector3.zero);

            var goldenNumber = Mathf.Clamp(col.impulse.magnitude / (GetComponent<Rigidbody>().mass + 10f) * 0.3f * impactMultiplier, 0f, 2f);
            if (ScreenShake.Instance) ScreenShake.Instance.AddForce(transform.forward * Mathf.Sqrt(goldenNumber * 0.5f) * 0.5f * screenShakeAmount, col.contacts[0].point);
            WilhelmPhysicsFunctions.AddForceWithMinWeight(enemyUnit.data.mainRig, Mathf.Sqrt(goldenNumber * 50f) * transform.forward * knockback * (flag ? 1f : 0f), ForceMode.Impulse, massCap);
            WilhelmPhysicsFunctions.AddForceWithMinWeight(col.rigidbody, Mathf.Sqrt(goldenNumber * 50f) * transform.forward * knockback, ForceMode.Impulse, massCap);

            foreach (var effect in GetComponents<CollisionWeaponEffect>()) effect.DoEffect(col.transform, col);
            if (GetComponent<CollisionSound>()) GetComponent<CollisionSound>().DoEffect(col.transform, col, goldenNumber);

            HitList.Add(enemyUnit);
            if (currentState == ClubState.Roaming) SetTarget(targetingRadius);
        }
        
        public void SetTarget(float range) {
            
            var hits = Physics.SphereCastAll(transform.position, range, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            var foundUnits = hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => x && !x.data.Dead && x.Team != transform.root.GetComponent<Unit>().Team && !HitList.Contains(x))
                .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
                .Distinct()
                .ToArray();

            if (foundUnits.Length > 0) RoamingTarget = foundUnits[Random.Range(0, foundUnits.Length - 1)];
        }
        
        private Rigidbody Rig;
        
        public enum ClubState
        {
            Idle,
            Swinging,
            Roaming,
            Disabled
        }

        public ClubState currentState;
        
        [Header("Idle")]

        public Transform hoverTarget;
        public Vector3 offset;
        public float idleDrag = 0.8f;
        public float idleForce;
        public float idleAngularForce;
        public float idleDragAmount;

        [Header("Swinging")] 
        
        public float returnDelay = 1f;
        public float swingingDragAmount; 

        [Header("Roaming")] 
        
        public UnityEvent roamingBeginEvent = new UnityEvent();
        public UnityEvent roamingEndEvent = new UnityEvent();
        public float roamingTimer = 5f;
        public float targetingRadius = 10f;
        public float roamingForce;
        public float roamingTorque;
        public float roamingDragAmount; 
        
        private Unit RoamingTarget;
        private List<Unit> HitList = new List<Unit>();

        [Header("Damage")] 
        
        public float damage = 120f;
        public float knockback = 100f;
        public float massCap = 25f;
        public float impactMultiplier = 1f;
        public float screenShakeAmount = 1f;
        public float damageCooldown = 0.01f;
        public float disableDamageDelay = 0.3f;
        public bool dealDamageOutsideOfSwing;

        private float DamageCounter;
        private bool CanDealDamage = true;
    }
}