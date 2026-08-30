using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BitCode.Extensions;
using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class ProjectileHitScale : ProjectileHitEffect
    {
        private int ID;
        
        public float scaleDelay = 0.5f;
        public int scaleLimit = 1;
        
        public AnimationCurve scaleCurve;
        public UnityEvent scaleEvent;
        
        private void Awake()
        {
            ID = GetComponent<ProjectileEntity>().Entity.GUID.m_ID;
            if (HUMain.InfiniteScalingEnabled) scaleLimit = 9999;
        }
        
        public override bool DoEffect(HitData hit)
        {
            var hitUnit = hit.transform.root.GetComponent<Unit>();
            var teamHolder = GetComponent<TeamHolder>();
            if (!hitUnit || hitUnit.data.Dead || (teamHolder && hitUnit.Team == teamHolder.team) || !hitUnit.data.allRigs)
            {
                return false;
            }

            var rigs = (from rig 
                in hitUnit.data.allRigs.AllRigs 
                let scalingComponents = rig.GetComponents<Scaling>() 
                where scalingComponents.Length <= 0 || !scalingComponents.Any(x => x.currentlyScaling) && !scalingComponents.Any(x => x.id == ID && x.scaleCount >= scaleLimit) 
                select rig).ToArray();

            if (rigs.Length <= 0) return false;
            
            var selectedPart = rigs[Random.Range(0, rigs.Length)];
            
            var scaling = FetchScaling(selectedPart.gameObject);
            if (!scaling) return false;
            
            scaling.currentlyScaling = true;
            scaling.scaleCount++;
            
            StartCoroutine(DoScaling(selectedPart.transform, scaling));
            return true;
        }

        private IEnumerator DoScaling(Transform bodyPart, Scaling scaling)
        {
            scaleEvent.Invoke();

            yield return new WaitForSeconds(scaleDelay);
            
            var t = 0f;
            var endTime = scaleCurve.keys[scaleCurve.keys.Length - 1].time;
            var originalScale = bodyPart.localScale;
            while (t < endTime && bodyPart.localScale.magnitude > 0.1f)
            {
                bodyPart.localScale = originalScale * (1 + scaleCurve.Evaluate(t) / scaling.scaleCount);
                t += Time.deltaTime;
                yield return null;
            }
            scaling.currentlyScaling = false;
        }
        
        private Scaling FetchScaling(GameObject obj)
        {
            var scalingComponents = obj.GetComponents<Scaling>();
            
            if (scalingComponents.Any(x => x.id == ID))
            {
                return scalingComponents.Single(x => x.id == ID);
            }
            
            var newScaling = obj.AddComponent<Scaling>();
            newScaling.id = ID;
            return newScaling;
        }

        public class Scaling : MonoBehaviour
        {
            public int id;
            public int scaleCount;
            public bool currentlyScaling;
        }
    }
}
