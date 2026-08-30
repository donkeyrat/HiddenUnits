using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Landfall.TABS;
using TGCore.Library;
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
            OriginalTargetingPriority = OwnUnit.targetingPriorityMultiplier;

            var multi = OwnUnit.data.GetComponent<HoldingHandlerMulti>();
            if (OwnUnit.data.weaponHandler)
            {
                if (OwnUnit.data.weaponHandler.rightWeapon) Weapons.Add(OwnUnit.data.weaponHandler.rightWeapon.rigidbody);
                if (OwnUnit.data.weaponHandler.leftWeapon) Weapons.Add(OwnUnit.data.weaponHandler.leftWeapon.rigidbody);
            }
            else if (multi)
            {
                Weapons.AddRange(multi.spawnedWeapons.Select(x => x.GetComponent<Rigidbody>()));
            }

            TrueDelay = Mathf.Clamp(unPetrifyDelay - OwnUnit.data.maxHealth / 250 + 0.4f, 1f, unPetrifyDelay);

            StartCoroutine(DoPetrifying());
        }

        public override void Ping()
        {
            pingEvent.Invoke();
        }

        private IEnumerator DoPetrifying()
        {
            if (!OwnUnit || !ColorHandler || !DragHandler || OwnUnit.data.Dead || OwnUnit.data.healthHandler.willBeRewived) yield break;
            
            petrifyEvent.Invoke();
            OwnUnit.targetingPriorityMultiplier = targetingPriority;
            OwnUnit.api.UpdateECSValues();
            
            var t = 0f;
            while (t < 1f && (!OwnUnit.data.healthHandler.willBeRewived || OwnUnit.data.Dead))
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

            if (OwnUnit.data.Dead && OwnUnit.data.healthHandler.willBeRewived)
            {
                ResetUnit();
                yield break;
            }
            
            OwnUnit.data.healthHandler.TakeDamage(petrifyDamage, Vector3.zero);

            OriginalDrags = AllRigs.AllDrags.ToList();
            if (OwnUnit.WeaponHandler) OwnUnit.WeaponHandler.StopAttacksFor(TrueDelay);

            foreach (var rig in AllRigs.AllRigs)
            {
                if (!rig) continue;
                rig.isKinematic = true;
            }
            foreach (var weapon in Weapons.Where(weapon => weapon))
            {
                weapon.isKinematic = true;
            }

            StartCoroutine(DoUnPetrifying());
        }

        private IEnumerator DoUnPetrifying()
        {
            yield return new WaitForSeconds(TrueDelay);
            
            if (OwnUnit.data.Dead) yield break;
            
            unPetrifyEvent.Invoke();

            foreach (var rig in AllRigs.AllRigs) rig.isKinematic = false;
            foreach (var weapon in Weapons) weapon.isKinematic = false;
            
            OwnUnit.targetingPriorityMultiplier = OriginalTargetingPriority;
            
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
        }

        private void ResetUnit()
        {
            ColorHandler.SetColor(petrifyColor, 0f);
            for (var i = 0; i < AllRigs.AllDrags.Length; i++)
            {
                AllRigs.AllDrags[i].x = OwnUnit.data.allRigs.defaultDrags[i].x;
                AllRigs.AllDrags[i].y = OwnUnit.data.allRigs.defaultDrags[i].y;
            }
            OwnUnit.targetingPriorityMultiplier = OriginalTargetingPriority;
        }

        private Unit OwnUnit;
        private UnitColorHandler ColorHandler;
        private DragHandler DragHandler;
        private RigidbodyHolder AllRigs;
        private List<Vector2> OriginalDrags;
        private List<Rigidbody> Weapons = new List<Rigidbody>();
        private float TrueDelay;
        private float OriginalTargetingPriority;

        public float petrifyDamage = 100f;
        
        [Header("Petrification")]
        
        public UnityEvent petrifyEvent = new UnityEvent();
        
        public UnitColorInstance petrifyColor;
        
        public float petrifySpeed = 2f;
        public float targetingPriority = 0.5f;

        [Header("De-Petrification")] 
        
        public UnityEvent unPetrifyEvent = new UnityEvent();
        
        public float unPetrifyDelay = 4f;
        public float unPetrifySpeed = 4f;
        
        [Header("Ping")]
        
        public UnityEvent pingEvent = new UnityEvent();
    }
}