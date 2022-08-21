using Landfall.TABS;
using UnityEngine;

namespace Unitgrad
{
    public class ProjectileHitScale : ProjectileHitEffect
    {
        public override bool DoEffect(HitData hit)
        {
            if (!hit.rigidbody || !hit.rigidbody.transform.root.GetComponent<Unit>() || hit.rigidbody.transform.root.GetComponent<Unit>().data.Dead || hit.rigidbody.transform.root.GetComponent<Unit>().Team == GetComponent<TeamHolder>().spawner.GetComponent<Unit>().Team)
            {
                return false;
            }
            this.hit = hit;
            startedScaling = true;
            return true;
        }
        
        public void Update()
        {
            counter += Time.deltaTime;
            if (scaleTime > counter && startedScaling)
            {
                if (plus)
                {
                    hit.rigidbody.transform.localScale += scale * Time.deltaTime;
                }
                else if (hit.rigidbody.transform.localScale.magnitude > 0.01f)
                {
                    hit.rigidbody.transform.localScale -= scale * Time.deltaTime;
                }
            }
        }

        private float counter;

        public float scaleTime = 1f;

        public Vector3 scale = new Vector3(1.6f, 1.6f, 1.6f);

        private bool startedScaling;

        private HitData hit;

        public bool plus = true;
    }
}
