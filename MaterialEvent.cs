using UnityEngine;
using System.Collections;
using Landfall.TABS;

namespace HiddenUnits {
    public class MaterialEvent : MonoBehaviour {
        public void ChangeMaterial() {
            t = 0f;
            changing = true;
        }

        void Update() {
            if (transform.root.GetComponentInChildren<Effect_Apollo>()) { Reset(); }
            if (changing && t < lerpTime && Time.timeScale != 0f) {
                t += Time.deltaTime;
                rend.materials[index].Lerp(rend.materials[index], newMat, t / lerpTime);
            }
            else if (t > lerpTime) { changing = false; }
            if (!changing) { t = 0f; }
        }

        public void Reset() { changing = false; }

        private bool changing;

        private float t;

        public Renderer rend;

        public int index;

        public Material newMat;

        public float lerpTime = 0.6f;
    }
}
