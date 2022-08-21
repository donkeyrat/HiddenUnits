using UnityEngine;
using UnityEngine.Events;

namespace Unitgrad
{
    public class OdinShield : ProjectileSurfaceEffect
    {
        public void Start()
        {
            health = maxHealth;
            ReflectionRune();
        }

        public void ReflectionRune()
        {
            shieldState = ShieldStates.ReflectionRune;
            activateEvent.Invoke();
        }

        public void Deactivate()
        {
            shieldState = ShieldStates.Deactivated;
            deactivateEvent.Invoke();
        }

        public override bool DoEffect(HitData hit, GameObject projectile)
        {
            if (shieldState != ShieldStates.ReflectionRune)
            {
                return false;
            }
            if (projectile.GetComponent<ProjectileHit>())
            {
                health -= projectile.GetComponent<ProjectileHit>().damage;
            }
            if (projectile.GetComponent<CollisionWeapon>())
            {
                health -= projectile.GetComponent<CollisionWeapon>().damage;
            }
            if (health <= 0f)
            {
                counter = 0f;
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
            counter += Time.deltaTime;
            if (counter > rechargeTime && GetComponent<OdinShield>().health <= 0f)
            {
                GetComponent<OdinShield>().health = GetComponent<OdinShield>().maxHealth;
                ReflectionRune();
            }
        }

        private float counter = 0f;

        public float rechargeTime = 6f;

        private ShieldStates shieldState;

        public UnityEvent activateEvent = new UnityEvent();

        public UnityEvent deactivateEvent = new UnityEvent();

        public UnityEvent reflectEvent = new UnityEvent();

        private float health;

        public float maxHealth = 1500f;

        public enum ShieldStates
        {
            ReflectionRune,
            Deactivated
        }
    }
}
