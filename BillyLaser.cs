using System.Collections.Generic;
using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits 
{
    public class BillyLaser : MonoBehaviour 
    {
        public void Start() { collisionEffects = GetComponentsInChildren<CollisionWeaponEffect>(); }

        public void OnTriggerStay(Collider col) { DoDamage(col); }
        
        public void OnTriggerEnter(Collider col) { DoDamage(col); }

        public void DoDamage(Collider col)
        {
            if (!col.attachedRigidbody || !col.attachedRigidbody.transform.root.GetComponent<Unit>() || col.attachedRigidbody.transform.root.GetComponent<Unit>().Team == transform.root.GetComponent<Unit>().Team || col.attachedRigidbody.transform.root == transform.root || !col.attachedRigidbody.transform.IsChildOf(col.attachedRigidbody.transform.root.GetComponent<Unit>().data.transform) || (col.attachedRigidbody.transform.root.GetComponent<Unit>() && hitList.Contains(col.attachedRigidbody.transform.root.GetComponent<Unit>()))) 
            {
                return;
            }

            var enemyUnit = col.attachedRigidbody.transform.root.GetComponent<Unit>();
            
            foreach (var effect in collisionEffects) { effect.DoEffect(col.transform, new Collision()); }
            collisionEvent.Invoke();
            enemyUnit.data.healthHandler.TakeDamage(damage, Vector3.zero, transform.root.GetComponent<Unit>(), DamageType.Piercing);
            var num2 = Mathf.Clamp(col.attachedRigidbody.drag / 3f, 0.1f, 1f);
            WilhelmPhysicsFunctions.AddAxplosionForceWithMinWeight(col.attachedRigidbody, knockback * num2, base.transform.position, 3f, ForceMode.Impulse, massCap);
            col.attachedRigidbody.velocity *= 0.9f;
            WilhelmPhysicsFunctions.AddAxplosionForceWithMinWeight(enemyUnit.data.mainRig, knockback * num2, base.transform.position, 3f, ForceMode.Impulse, massCap);
            enemyUnit.data.mainRig.velocity *= 0.9f;

            if (!canHitMultipleTimes)
            {
                hitList.Add(enemyUnit);
            }
        }

        public void ClearHits()
        {
            hitList.Clear();
        }

        private List<Unit> hitList = new List<Unit>();

        public float damage = 100f;

        public float knockback = 100f;

        public float massCap = 10f;

        public bool canHitMultipleTimes = true;

        private CollisionWeaponEffect[] collisionEffects;

        public UnityEvent collisionEvent;
    }
}
