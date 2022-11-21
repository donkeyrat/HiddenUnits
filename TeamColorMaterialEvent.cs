using UnityEngine;
using System.Collections;
using Landfall.TABS;
using System.Linq;

namespace HiddenUnits 
{
    public class TeamColorMaterialEvent : MonoBehaviour 
    {

        public void Start()
        {
            originalMat = Instantiate(rend.materials[index]);
            if (GetComponent<TeamHolder>())
            {
                team = GetComponent<TeamHolder>().team;
            }
            else if (GetComponentInChildren<TeamHolder>())
            {
                team = GetComponentInChildren<TeamHolder>().team;
            }
            else if (GetComponentInParent<TeamHolder>())
            {
                team = GetComponentInParent<TeamHolder>().team;
            }
            else if (transform.root.GetComponent<Unit>())
            {
                team = transform.root.GetComponent<Unit>().Team;
            }
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
            if (transform.root.GetComponentsInChildren<UnitEffectBase>().ToList().Find(x => x.effectID == 1987)) { Reset(); }
            if (changing && t < lerpTime && Time.timeScale != 0f) {
                t += Mathf.Clamp(Time.deltaTime / lerpTime, 0f, 1f);
                if (useOriginalMat)
                {
                    rend.materials[index].Lerp(rend.materials[index], originalMat, t);
                }
                else
                {
                    rend.materials[index].Lerp(rend.materials[index], team == Team.Red ? newMatRed : newMatBlue, t);
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

        private Team team;

        private bool changing;

        private bool useOriginalMat;

        private float t;

        public Renderer rend;

        public int index;

        public Material newMatRed;
        
        public Material newMatBlue;

        private Material originalMat;

        public float lerpTime = 0.6f;
    }
}