using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HiddenUnits
{
    public class SwordAfterimages : MonoBehaviour
    {
        private void Start()
        {
            rig = GetComponentInParent<Rigidbody>();
        }
        
        private void Update()
        {
            counter += Time.deltaTime;
            if (!canSpawn ||
                (requireThreshold && rig.velocity.magnitude < thresholdToSpawn) ||
                (requireCooldown && cooldown > counter))
            {
                return;
            }
            DoAfterimage();
        } 

        public void DoAfterimage()
        {
            counter = 0f;
            Instantiate(objectToSpawn, transform.position, transform.rotation);
        }

        public void CanSpawn() { canSpawn = true; }
    
        public void CannotSpawn() { canSpawn = false; }

        private float counter;

        private Rigidbody rig;

        public GameObject objectToSpawn;

        public bool requireCooldown;

        public bool requireThreshold;
    
        public float cooldown;

        public float thresholdToSpawn;

        public bool canSpawn;
    }

}