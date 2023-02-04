using System.Collections;
using Landfall.TABS;
using UnityEngine;

namespace HiddenUnits
{
    public class AddRigidbodyThenSinkOnDeath : MonoBehaviour
    {
        private void Start()
        {
            ownUnit = transform.root.GetComponent<Unit>();
            ownUnit.data.healthHandler.AddDieAction(Die);
        }

        public void Die()
        {
            if (hasDied) return;

            if (addRigidbody) rig = gameObject.AddComponent<Rigidbody>();
            else rig = GetComponent<Rigidbody>();
            rig.mass = mass;
            rig.drag = drag;
            rig.angularDrag = angularDrag;
            rig.interpolation = interpolation;

            hasDied = true;
            
            transform.SetParent(ownUnit.data.transform, true);

            StartCoroutine(DoSink());
        }

        private IEnumerator DoSink()
        {
            yield return new WaitForSeconds(sinkDelay);
            var t = 0f;
            while (t < 3f)
            {
                rig.drag *= 2f;
                if (rig.velocity.magnitude < 1f) t += Time.deltaTime;

                yield return null;
            }

            rig.isKinematic = true;

            t = 0f;
            while (t < 30f)
            {
                transform.position += Vector3.down * Mathf.Clamp(t * 0.1f, 0f, 1f) * Time.deltaTime * sinkMultiplier;
                t += Time.deltaTime;
                if (scaleAfterDelay && t > scaleDelay)
                {
                    scaleMultiplier += Time.deltaTime * 0.35f;
                    transform.localScale *= Mathf.Lerp(1f, 0f, scaleMultiplier);
                }

                yield return null;
            }
        }
        
        private Unit ownUnit;
        private bool hasDied;
        private Rigidbody rig;

        [Header("Rigidbody Settings")] 
        
        public bool addRigidbody;
        public float mass = 200f;
        public float drag;
        public float angularDrag;
        public RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;

        [Header("Sink Settings")] 
        
        public float sinkDelay = 2f;

        public float sinkMultiplier = 0.3f;
        
        public bool scaleAfterDelay = true;
        public float scaleDelay = 15f;
        private float scaleMultiplier;
    }
}