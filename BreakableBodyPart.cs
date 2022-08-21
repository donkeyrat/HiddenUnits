using UnityEngine;
using System.Collections.Generic;

namespace HiddenUnits {
    
    public class BreakableBodyPart : MonoBehaviour {
        
        public void Start() { GetComponent<DataHandler>().healthHandler.AssignDamageAction(BreakPart); }

        public void Update() {
            
            if (counter >= cooldown) {
                
                counter = 0f;
                startedCounting = false;
            }
            if (startedCounting) { counter += Time.deltaTime; }
        }

        public void BreakPart() {
            
            if (!startedCounting) {
                
                if (GetComponent<DataHandler>().health <= GetComponent<DataHandler>().maxHealth / 2 && Random.value > breakChance) {
                    
                    var selected = Random.Range(0, breakableParts.Count - 1);
                    breakableParts[selected].AddComponent<Rigidbody>().mass = 5f;
                    breakableParts[selected].transform.SetParent(transform.root);
                    breakableParts[selected].GetComponentInChildren<ParticleSystem>().Play();
                    breakableParts[selected].GetComponentInChildren<PlaySoundEffect>().Go();
                    startedCounting = true;
                    breakableParts.Remove(breakableParts[selected]);
                }
            }
        }

        public List<GameObject> breakableParts;

        public float breakChance = 0.5f;

        private bool startedCounting;

        private float counter;

        public float cooldown = 3f;
    }
}
