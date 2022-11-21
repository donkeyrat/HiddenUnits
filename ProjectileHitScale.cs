using System.Collections;
using Landfall.TABS;
using UnityEngine;

namespace HiddenUnits
{
    public class ProjectileHitScale : ProjectileHitEffect
    {
        public override bool DoEffect(HitData hit)
        {
            if (!hit.rigidbody || !hit.transform.root.GetComponent<Unit>() || (hit.transform.root.GetComponent<Unit>() && hit.transform.root.GetComponent<Unit>().data.Dead) || (hit.transform.root.GetComponent<Unit>() && hit.transform.root.GetComponent<Unit>().Team == GetComponent<TeamHolder>().team) || hit.transform.GetComponent<Scaling>())
            {
                return false;
            }
            
            StartCoroutine(DoScaling(hit));
            return true;
        }
        
        public IEnumerator DoScaling(HitData hit)
        {
            hit.transform.gameObject.AddComponent<Scaling>();
            
            var t = 0f;
            var originalVector = hit.transform.localScale;
            while (t < 1f && hit.transform.localScale.magnitude > 0.1f)
            {
                hit.transform.localScale = Vector3.Lerp(originalVector, originalVector * scale, t);
                t += Time.deltaTime;
                yield return null;
            }
            
            Destroy(hit.transform.GetComponent<Scaling>());
        }

        public float scale = 1.6f;

        public class Scaling : MonoBehaviour { }
    }
}
