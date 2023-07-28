using UnityEngine;

namespace HiddenUnits
{
    public class ShrinkParent : MonoBehaviour
    {
        public void Shrink()
        {
            Scaling = true;
        }
        
        private void Update()
        {
            Counter += Time.deltaTime;
            if (Scaling && Counter < 1f)
            {
                transform.parent.localScale -= Vector3.one * (scaler * Time.deltaTime);
            }
        }

        private float Counter;

        private bool Scaling;

        public float scaler = 0.5f;
    }
}
