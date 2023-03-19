using UnityEngine;

namespace HiddenUnits
{
    public class ShrinkParent : MonoBehaviour
    {
        public void Shrink()
        {
            scaling = true;
        }
        
        private void Update()
        {
            counter += Time.deltaTime;
            if (scaling && counter < 1f)
            {
                transform.parent.localScale -= Vector3.one * (scaler * Time.deltaTime);
            }
        }

        private float counter;

        private bool scaling;

        public float scaler = 0.5f;
    }
}
