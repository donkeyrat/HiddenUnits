using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class OdinShield : ProjectileSurfaceEffect
    {
        public void Start()
        {
            Health = maxHealth;
        }

        public void ReflectionRune()
        {
            ShieldState = ShieldStates.ReflectionRune;
            activateEvent.Invoke();
        }

        public void Deactivate()
        {
            ShieldState = ShieldStates.Deactivated;
            deactivateEvent.Invoke();
        }

        public override bool DoEffect(HitData hit, GameObject projectile)
        {
            if (ShieldState != ShieldStates.ReflectionRune)
            {
                return false;
            }
            if (projectile.GetComponent<ProjectileHit>())
            {
                Health -= projectile.GetComponent<ProjectileHit>().damage;
            }
            if (projectile.GetComponent<CollisionWeapon>())
            {
                Health -= projectile.GetComponent<CollisionWeapon>().damage;
            }
            if (Health <= 0f)
            {
                Counter = 0f;
                Deactivate();
                return false;
            }
            if (projectile.GetComponent<TeamHolder>())
            {
                projectile.GetComponent<TeamHolder>().SwitchTeam();
                projectile.GetComponent<TeamHolder>().spawner = null;
            }
            projectile.GetComponent<MoveTransform>().velocity *= -1f;
            projectile.GetComponent<RaycastTrail>().ignoredFrames = 2;
            reflectEvent.Invoke();
            return true;
        }

        public void Update()
        {
            Counter += Time.deltaTime;
            if (Counter > rechargeTime && Health <= 0f)
            {
                Health = maxHealth;
                ReflectionRune();
            }
        }

        private enum ShieldStates
        {
            ReflectionRune,
            Deactivated
        }
        
        private ShieldStates ShieldState;
        
        private float Counter;
        
        private float Health;

        public float rechargeTime = 6f;

        public UnityEvent activateEvent = new UnityEvent();

        public UnityEvent deactivateEvent = new UnityEvent();

        public UnityEvent reflectEvent = new UnityEvent();

        public float maxHealth = 1500f;
    }
}
