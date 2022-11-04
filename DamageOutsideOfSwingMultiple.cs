using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HiddenUnits
{
    public class EnableDamageOutOfSwing : MonoBehaviour
    {
        private List<MeleeWeapon> meleeWeapons = new List<MeleeWeapon>();

        private void Start()
        {
            meleeWeapons = transform.root.GetComponentsInChildren<MeleeWeapon>().ToList();
        }

        public void EnableDamage()
        {
            if (meleeWeapons.Count > 0)
            {
                foreach (var weapon in meleeWeapons)
                {
                    if (weapon != null)
                    {
                        weapon.EnableDamageOutOfSwing();
                    }
                }
            }
        }

        public void DisableDamage()
        {
            if (meleeWeapons.Count > 0)
            {
                foreach (var weapon in meleeWeapons)
                {
                    if (weapon != null)
                    {
                        weapon.DisableDamageOutOfSwing();
                    }
                }
            }
        }
    }

}