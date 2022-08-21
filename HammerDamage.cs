using UnityEngine;
using Landfall.TABS;

namespace HiddenUnits {
    
    public class HammerDamage : MonoBehaviour {

        public void Update() { counter += Time.deltaTime; }

        public void OnCollisionEnter(Collision col) {

            var enemyUnit = col.transform.root.GetComponent<Unit>();
            if (counter < cooldown || !enemyUnit || !col.rigidbody || (enemyUnit && GetComponentInParent<HammerBounce>().hitList.Contains(enemyUnit)) || (enemyUnit && enemyUnit.Team == GetComponentInParent<TeamHolder>().team)) return;
            counter = 0f;
            GetComponentInParent<HammerBounce>().Hit(enemyUnit);
            
            enemyUnit.data.healthHandler.TakeDamage(damage, Vector3.zero);

            var goldenNumber = Mathf.Clamp(col.impulse.magnitude / (GetComponent<Rigidbody>().mass + 10f) * 0.3f * impactMultiplier, 0f, 2f);
            if (ScreenShake.Instance) ScreenShake.Instance.AddForce(transform.forward * Mathf.Sqrt(goldenNumber * 0.5f) * 0.5f * impactScreenShake, col.contacts[0].point);
            WilhelmPhysicsFunctions.AddForceWithMinWeight(enemyUnit.data.mainRig, Mathf.Sqrt(goldenNumber * 50f) * transform.forward * impactForce, ForceMode.Impulse, massCap);
            WilhelmPhysicsFunctions.AddForceWithMinWeight(col.rigidbody, Mathf.Sqrt(goldenNumber * 50f) * transform.forward * impactForce, ForceMode.Impulse, massCap);
            
            foreach (var effect in GetComponents<CollisionWeaponEffect>()) effect.DoEffect(col.transform, col);
        }
        
        private float counter;
        public float cooldown = 0.01f;

        public float damage;

        public float impactMultiplier = 1f;
        public float impactForce;
        public float impactScreenShake = 1f;
        public float massCap;
    }
}