using UnityEngine;

namespace HiddenUnits
{
    public class SwordAfterimages : MonoBehaviour
    {
        private void Start()
        {
            Rig = GetComponentInParent<Rigidbody>();
        }
        
        private void Update()
        {
            Counter += Time.deltaTime;
            
            if (!canSpawn || (requireThreshold && Rig.velocity.magnitude < thresholdToSpawn) || (requireCooldown && cooldown > Counter)) return;
            
            DoAfterimage();
        } 

        public void DoAfterimage()
        {
            Counter = 0f;
            Instantiate(objectToSpawn, transform.position, transform.rotation);
        }

        public void CanSpawn() { canSpawn = true; }
    
        public void CannotSpawn() { canSpawn = false; }

        private float Counter;

        private Rigidbody Rig;

        public GameObject objectToSpawn;

        public bool requireCooldown;

        public bool requireThreshold;
    
        public float cooldown;

        public float thresholdToSpawn;

        public bool canSpawn;
    }

}