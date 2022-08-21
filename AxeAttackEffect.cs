using UnityEngine;

namespace HiddenUnits {

    public abstract class AxeAttackEffect : MonoBehaviour {

        public abstract void DoEffect(Rigidbody target);
    }
}
