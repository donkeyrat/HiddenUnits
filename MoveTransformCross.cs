using TGCore.Library;
using UnityEngine;
using UnityEngine.Events;

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

            MoveTransform = GetComponent<MoveTransform>();
        }

        private void Update()
        {
            Counter += Time.deltaTime;
        
            if (updatePosition && Target != null)
            {
                TrackedPosition = Target.transform.position;
            }
        
            var direction = (TrackedPosition - transform.position).normalized;
            if (setYPosition) direction.y = TrackedPosition.y;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation,
                rotationSpeedOverTime.Evaluate(Counter) * rotationForce * Time.deltaTime);
        
            MoveTransform.velocity += transform.forward * (force * Time.deltaTime);
            MoveTransform.velocity -= MoveTransform.velocity * (drag * Time.deltaTime);

            if (Counter > returnDelay && Vector3.Distance(transform.position, TrackedPosition) < returnThreshold)
            {
                if (Target != null)
                {
                    var returnableProjectileEvent = Target.GetComponentInParent<ReturnableProjectileEvent>();
                    if (returnableProjectileEvent) returnableProjectileEvent.Go();
                }
                returnEvent.Invoke();
            }
        }

        private float Counter;
        private Transform Target;
        private Vector3 TrackedPosition;
        private MoveTransform MoveTransform;

        public bool updatePosition;
        
        public UnityEvent returnEvent;
        public AnimationCurve rotationSpeedOverTime;
    
        [Header("Movement")]
    
        public float force = 1f;
        public float rotationForce = 1f;
        public float drag = 0.9f;
        public bool setYPosition;
    
        [Header("Return")]
    
        public float returnThreshold = 0.2f;
        public float returnDelay = 2f;
    }
}