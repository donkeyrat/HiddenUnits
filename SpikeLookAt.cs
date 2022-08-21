using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HiddenUnits {

    public class SpikeLookAt : MonoBehaviour {

        void Update() {

            if (target) { transform.LookAt(target); }
        }

        public Transform target;
    }
}