using UnityEngine;
using Landfall.TABS;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using TGCore.Library;

namespace HiddenUnits {
    
    public class HammerBounceRig : MonoBehaviour {

        public void Start()
        {
            OwnRig = GetComponent<Rigidbody>();
            TeamHolder = GetComponent<TeamHolder>();
            var rootWeapon = transform.GetComponentInParent<Weapon>();
            var rootUnit = transform.root.GetComponent<Unit>();
            if (rootWeapon)
            {
                Weapon = rootWeapon;
            }
            else if (rootUnit)
            {
                Weapon = rootUnit.WeaponHandler?.rightWeapon;
            }
            if (Weapon) ReturnObject = Weapon.transform.FindChildRecursive(objectToReturnTo);
            
            transform.SetParent(null);
            
            SetTarget(100f);
        }
        
        public void Update()
        {
            Counter += Time.deltaTime;
            if (!Target && !Finished) SetTarget();
            else if (Target)
            {
                var targetPos = Target.data.mainRig.position - transform.position;
                OwnRig.AddForce(targetPos.normalized * (flightSpeed * OwnRig.mass * Time.deltaTime));
                OwnRig.MoveRotation(Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetPos, Time.deltaTime * rotationSpeed, 0f)));
            }

            if (Returning && Weapon && ReturnObject)
            {
                if (ReturnCounter >= 1f)
                {
                    var returnEvent = Weapon.GetComponent<ReturnableProjectileEvent>();
                    if (returnEvent) returnEvent.Go();
                    Destroy(gameObject);
                    Returning = false;
                    return;
                }
                transform.position = Vector3.Lerp(ReturnPosition, ReturnObject.position, ReturnCounter);
                transform.rotation = Quaternion.Lerp(ReturnRotation, ReturnObject.rotation, ReturnCounter);
                ReturnCounter += Time.deltaTime * returnSpeed;
            }
            else if (Returning && (!Weapon || !ReturnObject))
            {
                Returning = false;
                returnFailedEvent.Invoke();
                Target = null;
            }
        }

        public void OnCollisionEnter(Collision col) {

            var enemyUnit = col.transform.root.GetComponent<Unit>();
            if (Counter < cooldown || !enemyUnit || !col.rigidbody 
                || (enemyUnit && HitList.Contains(enemyUnit)) 
                || (enemyUnit && TeamHolder && enemyUnit.Team == TeamHolder.team)
                || Finished) return;
            Counter = 0f;

            var flag = col.transform.IsChildOf(enemyUnit.data.transform) || col.gameObject.GetComponentInParent<Weapon>();
            
            enemyUnit.data.healthHandler.TakeDamage(damage * (flag ? 1f : 0f), Vector3.zero);

            var goldenNumber = Mathf.Clamp(col.impulse.magnitude / (OwnRig.mass + 10f) * 0.3f * impactMultiplier, 0f, 2f);
            if (ScreenShake.Instance) ScreenShake.Instance.AddForce(transform.forward * Mathf.Sqrt(goldenNumber * 0.5f) * 0.5f * impactScreenShake, col.contacts[0].point);
            WilhelmPhysicsFunctions.AddForceWithMinWeight(enemyUnit.data.mainRig, Mathf.Sqrt(goldenNumber * 50f) * transform.forward * impactForce * (flag ? 1f : 0f), ForceMode.Impulse, massCap);
            WilhelmPhysicsFunctions.AddForceWithMinWeight(col.rigidbody, Mathf.Sqrt(goldenNumber * 50f) * transform.forward * impactForce, ForceMode.Impulse, massCap);
            
            foreach (var effect in GetComponents<CollisionWeaponEffect>()) effect.DoEffect(col.transform, col);
            if (GetComponent<CollisionSound>()) GetComponent<CollisionSound>().DoEffect(col.transform, col, goldenNumber);

            HitCount += 1 * (!col.transform.name.Contains("Mjolnir") ? 1 : hitLimit);
            HitList.Add(enemyUnit);
            if (HitCount >= hitLimit)
            {
                Finish();
            }
            else
            {
                SetTarget();
            }
        }
        
        public void SetTarget(float radius = 0f) 
        {
            var hits = Physics.SphereCastAll(transform.position, radius != 0f ? radius : maxRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            var foundUnits = hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => x && !x.data.Dead && (!TeamHolder || x.Team != TeamHolder.team) && !HitList.Contains(x))
                .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
                .Distinct()
                .ToArray();

            if (foundUnits.Length > 0) Target = foundUnits[0];
            else Finish();
        }

        public void Return()
        {
            Returning = true;
            ReturnPosition = transform.position;
            ReturnRotation = transform.rotation;
        }

        public void Finish()
        {
            if (Finished) return;
            
            finishEvent.Invoke();
            Finished = true;
        }
        
        private float Counter;
        private Rigidbody OwnRig;
        private Unit Target;
        private TeamHolder TeamHolder;
        private List<Unit> HitList = new List<Unit>();
        private int HitCount;
        private bool Finished;
        
        [Header("Projectile Settings")] 
        
        public int hitLimit = 10;
        public float maxRange = 50f;
        
        public float flightSpeed = 1f;
        public float rotationSpeed = 30f;


        [Header("Return Settings")] 
        
        public float returnSpeed = 0.5f;
        public string objectToReturnTo;

        private bool Returning;
        
        private float ReturnCounter;

        private Transform ReturnObject;
        private Weapon Weapon;
        
        private Vector3 ReturnPosition;
        private Quaternion ReturnRotation;
        
        public UnityEvent finishEvent = new UnityEvent();
        public UnityEvent returnFailedEvent;
        
        [Header("Damage Settings")]
        
        public float cooldown = 0.01f;

        public float damage;

        public float impactMultiplier = 1f;
        public float impactForce;
        public float impactScreenShake = 1f;
        public float massCap;
    }
}