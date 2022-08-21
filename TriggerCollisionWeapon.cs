﻿using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;

namespace HiddenUnits {
    public class TriggerCollisionWeapon : MonoBehaviour {
        public void Start() {
            
            collisionEffects = GetComponentsInChildren<CollisionWeaponEffect>();
            
            if (meleeWeapon == null && GetComponent<MeleeWeapon>()) { meleeWeapon = GetComponent<MeleeWeapon>(); }
        }

        public void Update() {
            
            if (startedCounting) { counter += Time.deltaTime; }
            
            if (counter >= cooldown) {
                
                counter = 0f;
                startedCounting = false;
                canDealDamage = true;
            }
        }

        public void OnTriggerEnter(Collider col) {
            
            if (!col.attachedRigidbody || !col.attachedRigidbody.transform.root.GetComponent<Unit>() || !canDealDamage || (col.attachedRigidbody.transform.root.GetComponent<Unit>().Team == transform.root.GetComponent<Unit>().Team && !canDealDamageToTeammates) || col.attachedRigidbody.transform.root == transform.root) {
                return;
            }
            if (col.attachedRigidbody.transform.parent.name != "Rigidbodies") {
                return;
            }
            if (meleeWeapon && !meleeWeapon.canDealDamage) {
                return;
            }
            
            foreach (var effect in collisionEffects) { effect.DoEffect(col.transform, new Collision()); }
            collisionEvent.Invoke();
            col.attachedRigidbody.transform.root.GetComponent<Unit>().data.healthHandler.TakeDamage(damage, Vector3.zero, transform.root.GetComponent<Unit>(), DamageType.Piercing);
            AddForceToTarget(col.attachedRigidbody);
            
            if (GetComponent<CollisionSound>()) { DoEffect(col.transform, col, 100f); }
            
            startedCounting = true;
            canDealDamage = false;
        }

        public void AddForceToTarget(Rigidbody rig, float m = 1f) {
            
            WilhelmPhysicsFunctions.AddForceWithMinWeight(rig, transform.forward * knockback * m, ForceMode.Impulse, 10f);
            rig.velocity *= 0.7f;
        }

        public void DoEffect(Transform hitTransform, Collider col, float impact) {
            
            if (impact * 0.5f < 0.1f) { return; }
            
            if (GetComponent<CollisionSound>().onlySoundOnRig && !col.attachedRigidbody) { return; }
            
            if (meleeWeapon) { ServiceLocator.GetService<SoundPlayer>().PlaySoundEffect(GetComponent<CollisionSound>().soundEffectRef, impact * 0.5f, transform.position, SoundEffectVariations.GetMaterialType(col.gameObject, col.attachedRigidbody), null, 1f); }
        }

        public float damage = 100f;

        public float knockback = 100f;

        public float cooldown;

        private float counter;

        private bool canDealDamage = true;

        public bool canDealDamageToTeammates;

        private bool startedCounting;

        private CollisionWeaponEffect[] collisionEffects;

        public UnityEvent collisionEvent;

        public MeleeWeapon meleeWeapon;
    }
}
