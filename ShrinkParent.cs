using UnityEngine;

namespace HiddenUnits
{
    public class ShrinkParent : MonoBehaviour
    {
        public void Shrink()
        {
            scaling = true;
        }
        public void Update()
        {
            counter += Time.deltaTime;
            if (scaling && scaleTime > counter)
            {
                transform.parent.localScale -= new Vector3(scaler, scaler, scaler) * Time.deltaTime;
            }
        }

        private float counter;

        private bool scaling;

        private float scaleTime = 1f;

        public float scaler = 0.5f;
    }
}
