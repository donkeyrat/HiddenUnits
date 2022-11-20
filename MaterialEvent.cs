using UnityEngine;
using System.Collections;
using Landfall.TABS;
using System.Linq;

namespace HiddenUnits {
    public class MaterialEvent : MonoBehaviour {

        public void Start()
        {
            originalMat = Instantiate(rend.materials[index]);
        }
        
        public void ChangeMaterial() {
            t = 0f;
            changing = true;
        }
        
        public void RevertMaterial() {
            t = 0f;
            changing = true;
            useOriginalMat = true;
        }

        void Update() {
            if (transform.root.GetComponentsInChildren<UnitEffectBase>().ToList().Find(x => x.effectID == 1987)) Reset();
            if (changing && t < lerpTime && Time.timeScale != 0f) {
                t += Mathf.Clamp(Time.deltaTime / lerpTime, 0f, 1f);
                if (useOriginalMat)
                {
                    rend.materials[index].Lerp(rend.materials[index], originalMat, t);
                }
                else
                {
                    rend.materials[index].Lerp(rend.materials[index], newMat, t);
                }
            }
            else if (t > lerpTime) { changing = false;
                useOriginalMat = false;
            }
            if (!changing) { t = 0f; }
        }

        public void Reset() { 
            changing = false;
            useOriginalMat = false;
        }

        private bool changing;

        private bool useOriginalMat;

        private float t;

        public Renderer rend;

        public int index;

        public Material newMat;

        private Material originalMat;

        public float lerpTime = 0.6f;
    }
}
