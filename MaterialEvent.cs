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
        
        public void ChangeMaterial()
        {
            StartCoroutine(DoMaterialChange());
        }
        
        public void RevertMaterial()
        {
            StartCoroutine(DoMaterialChange(true));
        }

        public void Reset()
        {
            resetting = true;
        }

        public IEnumerator DoMaterialChange(bool useOriginalMat = false)
        {
            var t = 0f;
            while (t < 1f && !resetting && !transform.root.GetComponentsInChildren<UnitEffectBase>().ToList().Find(x => x.effectID == 1987))
            {
                if (useOriginalMat) rend.materials[index].Lerp(rend.materials[index], originalMat, Mathf.Clamp(t, 0f, 1f));
                else rend.materials[index].Lerp(rend.materials[index], newMat, Mathf.Clamp(t, 0f, 1f));
                
                t += Time.deltaTime / lerpTime;
                
                yield return null;
            }

            resetting = false;
        }

        private bool resetting;

        public Renderer rend;

        public int index;

        public Material newMat;

        private Material originalMat;

        public float lerpTime = 0.6f;
    }
}
