using System.Collections;
using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class ProjectileHitScale : ProjectileHitEffect
    {
        private void Awake()
        {
            if (HUMain.InfiniteScalingEnabled) scaleLimit = 9999;
        }
        
        public override bool DoEffect(HitData hit)
        {
            if (!hit.rigidbody || !hit.transform.root.GetComponent<Unit>() || (hit.transform.root.GetComponent<Unit>() && hit.transform.root.GetComponent<Unit>().data.Dead) || (hit.transform.root.GetComponent<Unit>() && hit.transform.root.GetComponent<Unit>().Team == GetComponent<TeamHolder>().team) || hit.transform.GetComponent<Scaling>())
            {
                return false;
            }
            
            StartCoroutine(DoScaling(hit));
            return true;
        }

        private IEnumerator DoScaling(HitData hit)
        {
            hit.transform.gameObject.AddComponent<Scaling>();

            yield return new WaitForSeconds(scaleDelay);
            
            scaleEvent.Invoke();

            var t = 0f;
            var originalVector = hit.transform.localScale;
            while (t < scaleCurve.keys[scaleCurve.keys.Length - 1].time && hit.transform.localScale.magnitude > 0.1f)
            {
                hit.transform.localScale = originalVector * (scaleCurve.Evaluate(t) * scaleCurveMultiplier);
                t += Time.deltaTime;
                yield return null;
            }

            ScaleCount++;
            if (ScaleCount >= scaleLimit) Destroy(hit.transform.GetComponent<Scaling>());
        }

        private int ScaleCount;

        public float scaleDelay = 0.5f;

        public int scaleLimit = 1;
        
        public AnimationCurve scaleCurve;

        public float scaleCurveMultiplier = 1f;

        public UnityEvent scaleEvent = new UnityEvent();

        public class Scaling : MonoBehaviour { }
    }
}
