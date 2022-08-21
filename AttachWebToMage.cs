using UnityEngine;

namespace HiddenUnits {

   public class AttachWebToMage : MonoBehaviour {

        public void Start() { Attach(); }

        public void Attach() {

            if (!transform.parent.parent.GetComponent<TeamHolder>() || !transform.parent.parent.GetComponent<TeamHolder>().spawnerWeapon || !transform.parent.parent.GetComponent<TeamHolder>().spawnerWeapon.transform) { return; }
            transform.SetParent(transform.parent.parent.GetComponent<TeamHolder>().spawnerWeapon.transform);
        }
    }
}
