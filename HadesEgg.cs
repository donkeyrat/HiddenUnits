using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Landfall.TABS;

namespace HiddenUnits {

    public class HadesEgg : MonoBehaviour {

        public void AddHealth(float amount)
        {
            if (hasHatched) return;

            currentHealth += amount;

            var emit = souls.emission;

            if (currentHealth >= requiredHealth) {
                emit.rateOverTime = requiredHealth * particleMultiplier; 
                HatchEgg(); 
            }
            else
            {
                emit.rateOverTime = emit.rateOverTime.constant + amount * particleMultiplier;
            }
        }

        public void HatchEgg() {
            
            hatchEvent.Invoke();
            hasHatched = true;
        }

        [HideInInspector]
        public bool hasHatched;

        public UnityEvent hatchEvent = new UnityEvent();

        public ParticleSystem souls;
        public float particleMultiplier = 0.05f;

        public float currentHealth;
        public float requiredHealth = 777f;

        [HideInInspector]
        public List<Unit> hitList = new();
    }
}