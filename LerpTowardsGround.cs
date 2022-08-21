using UnityEngine;

namespace HiddenUnits {

    public class LerpTowardsGround : MonoBehaviour {

        void Update() {
            if (lerping) {
                if (counter * lerpTime > 1f) { lerping = false; return; }
                counter += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, groundPos + groundOffset, counter * lerpTime);
            }
        }

        public void DoLerping() {
            if (Physics.Raycast(new Ray(transform.position, Vector3.down), out var hitInfo, 100f, groundMask)) { Debug.Log("awoawfoaf"); groundPos = hitInfo.point; startPos = transform.position; lerping = true; }
        }

        private bool lerping;

        private float counter;

        private Vector3 groundPos, startPos;

        public float lerpTime;

        public Vector3 groundOffset;

        public LayerMask groundMask;
    }
}
