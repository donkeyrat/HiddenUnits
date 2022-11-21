using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
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
            if (counter > rechargeTime && health <= 0f)
            {
                health = maxHealth;
                ReflectionRune();
            }
        }

        private enum ShieldStates
        {
            ReflectionRune,
            Deactivated
        }
        
        private ShieldStates shieldState;
        
        private float counter;
        
        private float health;

        public float rechargeTime = 6f;

        public UnityEvent activateEvent = new UnityEvent();

        public UnityEvent deactivateEvent = new UnityEvent();

        public UnityEvent reflectEvent = new UnityEvent();

        public float maxHealth = 1500f;
    }
}
