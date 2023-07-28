using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits {
    public class TriggerCollisionWeapon : MonoBehaviour {
        public void Start() {
            
            CollisionEffects = GetComponentsInChildren<CollisionWeaponEffect>();
            
            if (meleeWeapon == null && GetComponent<MeleeWeapon>()) { meleeWeapon = GetComponent<MeleeWeapon>(); }
        }

        public void Update() {
            
            if (StartedCounting) { Counter += Time.deltaTime; }
            
            if (Counter >= cooldown) {
                
                Counter = 0f;
                StartedCounting = false;
                CanDealDamage = true;
            }
        }

        public void OnTriggerStay(Collider col) {
            
            DoDamage(col);
        }
        
        public void OnTriggerEnter(Collider col) {
            
            DoDamage(col);
        }

        public void DoDamage(Collider col)
        {
            if (!col.attachedRigidbody || !col.attachedRigidbody.transform.root.GetComponent<Unit>() || !CanDealDamage || (col.attachedRigidbody.transform.root.GetComponent<Unit>().Team == transform.root.GetComponent<Unit>().Team && !canDealDamageToTeammates) || col.attachedRigidbody.transform.root == transform.root) {
                return;
            }
            if (col.attachedRigidbody.transform.parent.name != "Rigidbodies") {
                return;
            }
            if (meleeWeapon && !meleeWeapon.canDealDamage) {
                return;
            }
            
            foreach (var effect in CollisionEffects) { effect.DoEffect(col.transform, new Collision()); }
            collisionEvent.Invoke();
            col.attachedRigidbody.transform.root.GetComponent<Unit>().data.healthHandler.TakeDamage(damage, Vector3.zero, transform.root.GetComponent<Unit>(), DamageType.Piercing);
            AddForceToTarget(col.attachedRigidbody);
            
            if (GetComponent<CollisionSound>()) { DoEffect(col.transform, col, 100f); }
            
            StartedCounting = true;
            CanDealDamage = false;
        }

        public void AddForceToTarget(Rigidbody rig, float m = 1f) {
            
            WilhelmPhysicsFunctions.AddForceWithMinWeight(rig, transform.forward * knockback * m, ForceMode.Impulse, minMassCap);
            rig.velocity *= 0.7f;
        }

        public void DoEffect(Transform hitTransform, Collider col, float impact) {
            
            if (impact * 0.5f < 0.1f) { return; }
            
            if (GetComponent<CollisionSound>().onlySoundOnRig && !col.attachedRigidbody) { return; }
            
            if (meleeWeapon) { ServiceLocator.GetService<SoundPlayer>().PlaySoundEffect(GetComponent<CollisionSound>().SoundEffectRef, impact * 0.5f, transform.position, SoundEffectVariations.GetMaterialType(col.gameObject, col.attachedRigidbody)); }
        }

        public float damage = 100f;

        public float knockback = 100f;

        public float minMassCap = 10f;

        public float cooldown;

        private float Counter;

        private bool CanDealDamage = true;

        public bool canDealDamageToTeammates;

        private bool StartedCounting;

        private CollisionWeaponEffect[] CollisionEffects;

        public UnityEvent collisionEvent;

        public MeleeWeapon meleeWeapon;
    }
}
