using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
    public class Effect_Petrification : UnitEffectBase
    {
        public override void DoEffect()
        {
            OwnUnit = transform.root.GetComponent<Unit>();
            ColorHandler = OwnUnit.data.GetComponent<UnitColorHandler>();
            DragHandler = OwnUnit.data.GetComponent<DragHandler>();
            AllRigs = OwnUnit.data.allRigs;
            OriginalDrags = OwnUnit.data.allRigs.defaultDrags.ToList();

            TrueDelay = Mathf.Clamp(unPetrifyDelay - OwnUnit.data.maxHealth / 100 + 1f, 2f, unPetrifyDelay);

            StartCoroutine(DoPetrifying());
        }

        public override void Ping()
        {
        }

        private IEnumerator DoPetrifying()
        {
            if (OwnUnit.data.Dead) yield break;
            
            petrifyEvent.Invoke();
            
            var t = 0f;
            while (t < 1f && !OwnUnit.data.Dead)
            {
                t += Time.deltaTime * petrifySpeed;
                
                for (var i = 0; i < AllRigs.AllDrags.Length; i++)
                {
                    AllRigs.AllDrags[i].x = OriginalDrags[i].x + t;
                    AllRigs.AllDrags[i].y = OriginalDrags[i].y + t;
                }
                DragHandler.UpdateDrag();
                
                ColorHandler.SetColor(petrifyColor, t);
                
                yield return null;
            }

            if (OwnUnit.data.Dead)
            {
                ResetUnit();
                yield break;
            }

            OriginalDrags = AllRigs.AllDrags.ToList();
            foreach (var rig in AllRigs.AllRigs) rig.isKinematic = true;
            OwnUnit.WeaponHandler.StopAttacksFor(unPetrifyDelay);

            StartCoroutine(DoUnPetrifying());
        }

        private IEnumerator DoUnPetrifying()
        {
            yield return new WaitForSeconds(unPetrifyDelay);
            
            if (OwnUnit.data.Dead) yield break;
            
            unPetrifyEvent.Invoke();

            foreach (var rig in AllRigs.AllRigs) rig.isKinematic = false;
            
            var t = 0f;
            while (t < 1f && !OwnUnit.data.Dead)
            {
                t += Time.deltaTime * unPetrifySpeed;
                
                for (var i = 0; i < AllRigs.AllDrags.Length; i++)
                {
                    AllRigs.AllDrags[i].x = OriginalDrags[i].x - t;
                    AllRigs.AllDrags[i].y = OriginalDrags[i].y - t;
                }
                DragHandler.UpdateDrag();
                
                ColorHandler.SetColor(petrifyColor, 1 - t);

                yield return null;
            }

            if (OwnUnit.data.Dead) ResetUnit();
        }

        private void ResetUnit()
        {
            ColorHandler.SetColor(petrifyColor, 0f);
            for (var i = 0; i < AllRigs.AllDrags.Length; i++)
            {
                AllRigs.AllDrags[i].x = OwnUnit.data.allRigs.defaultDrags[i].x;
                AllRigs.AllDrags[i].y = OwnUnit.data.allRigs.defaultDrags[i].y;
            }
        }

        private Unit OwnUnit;
        private UnitColorHandler ColorHandler;
        private DragHandler DragHandler;
        private RigidbodyHolder AllRigs;
        private List<Vector2> OriginalDrags;
        private float TrueDelay;

        [Header("Petrification")]
        
        public UnityEvent petrifyEvent = new UnityEvent();
        
        public UnitColorInstance petrifyColor;
        
        public float petrifySpeed = 2f;

        [Header("De-Petrification")] 
        
        public UnityEvent unPetrifyEvent = new UnityEvent();
        
        public float unPetrifyDelay = 4f;
        public float unPetrifySpeed = 4f;
    }
}