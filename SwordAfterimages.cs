using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HiddenUnits
{
    public class SwordAfterimages : MonoBehaviour
    {
        public void Update()
        {
            counter += Time.deltaTime;
            if (!canSpawn ||
                (requireThreshold && GetComponentInParent<Rigidbody>().velocity.magnitude < thresholdToSpawn) ||
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

        public GameObject objectToSpawn;

        public bool requireCooldown;

        public bool requireThreshold;
    
        public float cooldown;

        public float thresholdToSpawn;

        public bool canSpawn;
    }

}