using UnityEngine;
using Landfall.TABS;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace HiddenUnits {
    
    public class HammerBounceRig : MonoBehaviour {

        public void Start() 
        {
            target = GetComponent<TeamHolder>().spawner.GetComponent<Unit>().data.targetData.unit.data.mainRig.transform;
            weapon = transform.GetComponentInParent<Weapon>() ? transform.GetComponentInParent<Weapon>() : transform.root.GetComponent<Unit>().WeaponHandler.rightWeapon;
            returnObject = weapon.transform.FindChildRecursive(objectToReturnTo);
        }
        
        public void Update()
        {
            counter += Time.deltaTime;
            if (!target && !returning) SetTarget();
            else if (target)
            {
                GetComponent<Rigidbody>().AddForce((target.position - transform.position).normalized * flightSpeed * GetComponent<Rigidbody>().mass * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, target.position - transform.position, Time.deltaTime * rotationSpeed, 0f));
            }

            if (returning)
            {
                if (returnCounter >= 1f)
                {
                    weapon.GetComponent<DelayEvent>().Go();
                    Destroy(gameObject);
                    returning = false;
                    return;
                }
                transform.position = Vector3.Lerp(returnPosition,
                    returnObject.position, returnCounter);
                transform.rotation = Quaternion.Lerp(returnRotation,
                    returnObject.rotation, returnCounter);
                returnCounter += Time.deltaTime * returnSpeed;
            }
        }

        public void OnCollisionEnter(Collision col) {

            var enemyUnit = col.transform.root.GetComponent<Unit>();
            if (counter < cooldown || !enemyUnit || !col.rigidbody || (enemyUnit && hitList.Contains(enemyUnit)) || (enemyUnit && enemyUnit.Team == GetComponent<TeamHolder>().team)) return;
            counter = 0f;

            var flag = col.transform.IsChildOf(enemyUnit.data.transform);
            
            enemyUnit.data.healthHandler.TakeDamage(damage * (flag ? 1f : 0f), Vector3.zero);

            var goldenNumber = Mathf.Clamp(col.impulse.magnitude / (GetComponent<Rigidbody>().mass + 10f) * 0.3f * impactMultiplier, 0f, 2f);
            if (ScreenShake.Instance) ScreenShake.Instance.AddForce(transform.forward * Mathf.Sqrt(goldenNumber * 0.5f) * 0.5f * impactScreenShake, col.contacts[0].point);
            WilhelmPhysicsFunctions.AddForceWithMinWeight(enemyUnit.data.mainRig, Mathf.Sqrt(goldenNumber * 50f) * transform.forward * impactForce * (flag ? 1f : 0f), ForceMode.Impulse, massCap);
            WilhelmPhysicsFunctions.AddForceWithMinWeight(col.rigidbody, Mathf.Sqrt(goldenNumber * 50f) * transform.forward * impactForce, ForceMode.Impulse, massCap);
            
            foreach (var effect in GetComponents<CollisionWeaponEffect>()) effect.DoEffect(col.transform, col);

            hitCount += 1 * (!col.transform.name.Contains("Mjolnir") ? 1 : 100);
            hitList.Add(enemyUnit);
            SetTarget();
            if (hitCount >= hitLimit)
            {
                finishEvent.Invoke();
            }
        }
        
        public void SetTarget() {
            
            var hits = Physics.SphereCastAll(transform.position, maxRange, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
            List<Unit> foundUnits = new List<Unit>();
            foreach (var hit in hits) {
                
                if (hit.transform.root.GetComponent<Unit>() && !foundUnits.Contains(hit.transform.root.GetComponent<Unit>())) foundUnits.Add(hit.rigidbody.transform.root.GetComponent<Unit>());
            }
            Unit[] query
                = (
                    from Unit unit
                    in foundUnits
                    where !unit.data.Dead && unit.Team != transform.root.GetComponent<Unit>().Team && !hitList.Contains(unit)
                    orderby (unit.data.mainRig.transform.position - transform.position).magnitude
                    select unit
                ).ToArray();
            
            if (query.Length > 0) { target = query[0].data.mainRig.transform; }
            else { finishEvent.Invoke(); }
        }

        public void Return()
        {
            returning = true;
            returnPosition = transform.position;
            returnRotation = transform.rotation;
        }

        public void TriggerFinish()
        {
            finishEvent.Invoke();
        }
        
        private float counter;
        private Transform target;
        private List<Unit> hitList = new List<Unit>();
        private int hitCount;
        private bool returning;
        
        [Header("Projectile Settings")] 
        
        public int hitLimit = 10;
        public float maxRange = 50f;
        
        public float flightSpeed = 1f;
        public float rotationSpeed = 30f;


        [Header("Return Settings")] 
        
        public float returnSpeed = 0.5f;
        public string objectToReturnTo;

        private float returnCounter;
        
        
        private Transform returnObject;
        private Weapon weapon;
        
        private Vector3 returnPosition;
        private Quaternion returnRotation;
        
        public UnityEvent finishEvent = new UnityEvent();
        
        [Header("Damage Settings")]
        
        public float cooldown = 0.01f;

        public float damage;

        public float impactMultiplier = 1f;
        public float impactForce;
        public float impactScreenShake = 1f;
        public float massCap;
    }
}