using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Landfall.TABS;

namespace HiddenUnits {

    public class HadesEgg : MonoBehaviour
    {
        private float StartVolume;
        
        [HideInInspector]
        public bool hasHatched;
        
        [HideInInspector]
        public List<Unit> hitList = new();

        public UnityEvent hatchEvent = new UnityEvent();

        [Header("Particles")]
        public ParticleSystem souls;
        public ParticleSystemForceField soulField;
        public float particleMultiplier = 0.05f;

        [Header("Health")]
        public float currentHealth;
        public float requiredHealth = 777f;

        [Header("Audio")]
        public AudioSource eggLoop;
        public float healthVolumeMultiplier = 0.5f;
        public float audioLerpSpeed = 2f;

        private void Start()
        {
            StartVolume = eggLoop.volume;
        }

        private void Update()
        {
            if (hasHatched)
            {
                eggLoop.volume = Mathf.Lerp(eggLoop.volume, 0f, Time.deltaTime * audioLerpSpeed);
            }
        }
        
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

            eggLoop.volume = StartVolume + currentHealth / requiredHealth * healthVolumeMultiplier;
        }

        public void HatchEgg() 
        {
            hatchEvent.Invoke();
            hasHatched = true;
        }

        public void KillEgg()
        {
            hasHatched = true;
        }
    }
}