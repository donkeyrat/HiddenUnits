using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Landfall.TABS;

namespace HiddenUnits
{
    public class Defense : MonoBehaviour
    {
        public void Start()
        {
            health = maxHealth;
        }

        public void OnCollisionEnter(Collision col)
        {
            if (col.collider.GetComponentInParent<CollisionWeapon>())
            {
                health -= col.collider.GetComponentInParent<CollisionWeapon>().damage;
                StartCoroutine(ArmorBreak());
            }
        }

        public IEnumerator ArmorBreak()
        {
            if (health <= 0f)
            {
                foreach (var arm in armor)
                {
                    arm.SetActive(false);
                }
                yield return new WaitForSeconds(repairTime);
                foreach (var arm in armor)
                {
                    arm.SetActive(true);
                    health = maxHealth;
                }
            }
            yield break;
        }

        public float health = 100f;

        public float maxHealth = 100f;

        public float repairTime = 6f;

        public List<GameObject> armor = new List<GameObject>();
    }
}
