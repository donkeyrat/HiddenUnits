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
            
            armoredUnit = unit.gameObject.AddComponent<UnitIsArmored>();
            armoredUnit.projectileHitEffect = projectileHitEffect;
            armoredUnit.weaponHitEffect = weaponHitEffect;
            armoredUnit.parryForce = parryForce;
            armoredUnit.parryPower = parryPower;
            armoredUnit.blockPower = blockPower;
            
            maxArmorHealth = armorHealth;
            armorListeners = unit.GetComponentsInChildren<AchillesArmorEvent>();
            foreach (var armor in armorListeners) armor.OnArmorActivated();
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
                foreach (var armor in armorListeners) armor.OnArmorDeactivated();
                armoredUnit.armorActive = false;
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
                    foreach (var armor in armorListeners) armor.OnArmorActivated();
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
        private UnitIsArmored armoredUnit;

        private AchillesArmorEvent[] armorListeners;
        
        private bool armorDisabled;
        private float armorDisabledCounter;
        
        private float maxArmorHealth;
        
        [Header("Armor Settings")]

        public UnityEvent armorDisableEvent = new UnityEvent();
        public UnityEvent armorEnableEvent = new UnityEvent();
        
        public float armorDisabledTime = 3f;
        
        [Header("Hit Settings")]
        
        public GameObject projectileHitEffect;
        public GameObject weaponHitEffect;
        
        public float parryForce;
        public float parryPower;
        
        public float blockPower;
        
        [Header("Health Settings")]
        
        public float armorHealth = 500f;
        
        public bool armorRegenerate;
        public float armorRegenerationRate = 5f;
        public bool healthRegenerate;
        public float healthRegenerationRate = 50f;

        public class UnitIsArmored : MonoBehaviour
        {
            public bool armorActive = true;
            
            public GameObject projectileHitEffect;
            public GameObject weaponHitEffect;

            public float parryForce;
            public float parryPower;

            public float blockPower;
        }
    }
}