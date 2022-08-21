using Landfall.TABS;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;

namespace HiddenUnits {
    
    public class HammerBounce : MonoBehaviour {
        
        public void Start() {
            target = GetComponent<TeamHolder>().spawner.GetComponent<Unit>().data.targetData.unit;
            weapon = transform.root.GetComponent<Unit>().WeaponHandler.rightWeapon;
        }

        public void Hit(Unit unit) {
            
            hitList.Add(unit);
            hitLimit -= 1;
            if (hitLimit <= 0) {
                finishEvent.Invoke();
                hitLimit = 0;
                return;
            }
            SetTarget();
        }

        public void Update() {
            
            Transform go = null;
            if (autoTarget && target != null && !target.data.Dead) go = target.data.mainRig.transform;
            else if (returned) {
                
                go = weapon.transform;
                if (Vector3.Distance(transform.position, weapon.transform.position) < distanceToDespawn) {
                    
                    Destroy(gameObject);
                    weapon.GetComponent<DelayEvent>().Go();

                    if (transform.root.GetComponent<Unit>().data.Dead && weapon.GetComponent<DeathEvent>()) weapon.GetComponent<DeathEvent>().Die();
                }
            }
            else if (target == null || target.data.Dead) SetTarget();
            if (go != null) GoTowards(go);
        }
        public void Return() {
            
            target = null;
            returned = true;
        }

        public void GoTowards(Transform target) {
            
            GetComponent<MoveTransform>().velocity = (target.position - transform.position).normalized * GetComponent<MoveTransform>().selfImpulse.magnitude * 1f;
            transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, target.position - transform.position, Time.deltaTime * rotationSpeed, 0f));
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
            
            if (query.Length > 0) { target = query[0]; }
            else { finishEvent.Invoke(); }
        }

        [HideInInspector]
        public List<Unit> hitList = new List<Unit>();

        private Unit target;

        public float maxRange = 20f;

        public int hitLimit = 10;

        public UnityEvent finishEvent = new UnityEvent();

        public bool autoTarget = true;

        private bool returned;

        public float rotationSpeed;

        private Weapon weapon;

        public float distanceToDespawn;
    }
}
