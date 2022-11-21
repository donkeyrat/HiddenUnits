using UnityEngine;
using Landfall.TABS;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class AchillesArmor : MonoBehaviour
    {
        public void Start()
        {
            unit = transform.root.GetComponent<Unit>();
            unit.WasDealtDamageAction += Armor;
            maxArmorHealth = armorHealth;
        }

        public void Armor(float damage)
        {
            if (armorDisabled)
            {
                return;
            }

            unit.data.health += damage;
            armorHealth -= damage;
            armorHealth = Mathf.Clamp(armorHealth, 0f, maxArmorHealth);
            if (armorHealth <= 0f)
            {
                armorDisabled = true;
                armorDisableEvent.Invoke();
            }
        }

        public void Update()
        {
            
            if (armorDisabled)
            {
                armorDisabledCounter += Time.deltaTime;
                if (armorDisabledCounter >= armorDisabledTime)
                {
                    armorDisabledCounter = 0f;
                    armorDisabled = false;
                    armorHealth = maxArmorHealth;
                    armorEnableEvent.Invoke();
                }
            }
            else
            {
                if (armorRegenerate)
                {
                    armorHealth += Time.deltaTime * armorRegenerationRate;
                    armorHealth = Mathf.Clamp(armorHealth, 0f, maxArmorHealth);
                }

                if (healthRegenerate)
                {
                    unit.data.health += Time.deltaTime * healthRegenerationRate;
                    unit.data.health = Mathf.Clamp(unit.data.health, 0f, unit.data.maxHealth);
                }
            }
        }
        
        private Unit unit;

        public UnityEvent armorDisableEvent = new UnityEvent();

        public UnityEvent armorEnableEvent = new UnityEvent();
        
        private bool armorDisabled;

        private float armorDisabledCounter;

        public float armorDisabledTime = 3f;
        
        public float armorHealth = 500f;

        private float maxArmorHealth;

        public bool armorRegenerate;
        
        public float armorRegenerationRate = 5f;

        public bool healthRegenerate;
        
        public float healthRegenerationRate = 50f;
    }
}