using System.Collections;
using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class Effect_Apollo : UnitEffectBase
    {
        private void Awake()
        {
            ownUnit = transform.root.GetComponent<Unit>();
            colorHandler = ownUnit.data.GetComponent<UnitColorHandler>();
        }
        
        public override void DoEffect()
        {
            StartCoroutine(AddEffect());
        }

        public override void Ping()
        {
            StartCoroutine(AddEffect());
        }

        public IEnumerator AddEffect()
        {
            yield return new WaitForSeconds(effectDelay);
            
            ownUnit.data.healthHandler.TakeDamage(damage, Vector3.zero);
            
            StartCoroutine(DestroyUnit());
        }

        public IEnumerator DestroyUnit()
        {
            if (destroying) yield break;
            destroying = true;
            
            if (ownUnit.data.Dead || alwaysDestroy)
            {
                destroyEvent.Invoke();
                
                yield return new WaitForSeconds(destroyDelay);
                
                colorHandler.SetMaterial(mat);
                
                if (destroyRoot) ownUnit.DestroyUnit();
            }
        }

        private Unit ownUnit;

        private UnitColorHandler colorHandler;

        private bool destroying;

        public float damage;

        public UnityEvent destroyEvent = new UnityEvent();
        
        public bool destroyRoot = true;

        public bool alwaysDestroy;

        public float destroyDelay;

        public float effectDelay = 0.01f;
        
        public Material mat;
    }
}
