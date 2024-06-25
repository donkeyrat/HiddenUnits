using UnityEngine;

namespace HiddenUnits
{
    public class MoveTransformCross : MonoBehaviour
    {
        private void Start()
        {
            var teamHolder = GetComponent<TeamHolder>();
            if (teamHolder && teamHolder.spawnerWeapon)
            {
                Target = teamHolder.spawnerWeapon.transform;
            }
            else
            {
                TrackedPosition = transform.position;
            }
        }

        private void Update()
        {
            Counter += Time.deltaTime;
        
            if (updatePosition && Target != null)
            {
                TrackedPosition = Target.transform.position;
            }
        
            var direction = (TrackedPosition - transform.position).normalized;
            direction.y = 0f;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation,
                rotationSpeedOverTime.Evaluate(Counter) * rotationForce * Time.deltaTime);
        
            projectile.velocity += transform.forward * (force * Time.deltaTime);
            projectile.velocity -= projectile.velocity * (drag * Time.deltaTime);

            if (Counter > returnDelay && Vector3.Distance(transform.position, TrackedPosition) < returnThreshold)
            {
                if (Target != null) Target.GetComponent<DelayEvent>().Go();
                delayEvent.Go();
            }
        }


        private float Counter;
        private Transform Target;
        private Vector3 TrackedPosition;

        public bool updatePosition;
        
        public MoveTransform projectile;
        public DelayEvent delayEvent;
        public AnimationCurve rotationSpeedOverTime;
    
        [Header("Movement")]
    
        public float force = 1f;
        public float rotationForce = 1f;
        public float drag = 0.9f;
    
        [Header("Return")]
    
        public float returnThreshold = 0.2f;
        public float returnDelay = 2f;
    }
}