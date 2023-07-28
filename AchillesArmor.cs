using UnityEngine;
using Landfall.TABS;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class AchillesArmor : MonoBehaviour
    {
        public void Start()
        {
            Unit = transform.root.GetComponent<Unit>();
            Unit.WasDealtDamageAction += Armor;
            
            ArmoredUnit = Unit.gameObject.AddComponent<UnitIsArmored>();
            ArmoredUnit.projectileHitEffect = projectileHitEffect;
            ArmoredUnit.weaponHitEffect = weaponHitEffect;
            ArmoredUnit.parryForce = parryForce;
            ArmoredUnit.parryPower = parryPower;
            ArmoredUnit.blockPower = blockPower;
            
            MaxArmorHealth = armorHealth;
            ArmorListeners = Unit.GetComponentsInChildren<AchillesArmorEvent>();
            foreach (var armor in ArmorListeners) armor.OnArmorActivated();
        }

        public void Armor(float damage)
        {
            if (ArmorDisabled)
            {
                return;
            }

            Unit.data.health += damage;
            armorHealth -= damage;
            armorHealth = Mathf.Clamp(armorHealth, 0f, MaxArmorHealth);
            if (armorHealth <= 0f)
            {
                ArmorDisabled = true;
                armorDisableEvent.Invoke();
                
                foreach (var armor in ArmorListeners) armor.OnArmorDeactivated();
                ArmoredUnit.armorActive = false;
            }
        }

        public void Update()
        {
            if (ArmorDisabled)
            {
                ArmorDisabledCounter += Time.deltaTime;
                if (ArmorDisabledCounter >= armorDisabledTime)
                {
                    ArmorDisabledCounter = 0f;
                    ArmorDisabled = false;
                    armorHealth = MaxArmorHealth;
                    armorEnableEvent.Invoke();
                    
                    foreach (var armor in ArmorListeners) armor.OnArmorActivated();
                    ArmoredUnit.armorActive = true;
                }
            }
            else
            {
                if (armorRegenerate)
                {
                    armorHealth += Time.deltaTime * armorRegenerationRate;
                    armorHealth = Mathf.Clamp(armorHealth, 0f, MaxArmorHealth);
                }
                if (healthRegenerate)
                {
                    Unit.data.health += Time.deltaTime * healthRegenerationRate;
                    Unit.data.health = Mathf.Clamp(Unit.data.health, 0f, Unit.data.maxHealth);
                }
            }
        }
        
        private Unit Unit;
        private UnitIsArmored ArmoredUnit;

        private AchillesArmorEvent[] ArmorListeners;
        
        private bool ArmorDisabled;
        private float ArmorDisabledCounter;
        
        private float MaxArmorHealth;
        
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