using UnityEngine;
using Landfall.TABS;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Landfall.TABS.AI.Components.Tags;
using Landfall.TABS.AI.Components;
using Landfall.TABS.AI.Systems;
using Landfall.TABS.GameMode;
using UnityEngine.Events;
using System.Linq;

namespace HiddenUnits
{
    public class Effect_Zombie : UnitEffectBase
    {
        public override void DoEffect()
        {
            unit = transform.root.GetComponent<Unit>();
            
            if (unit.holdingHandler)
            {
                weapon1 = unit.holdingHandler.rightObject ? unit.holdingHandler.rightObject.gameObject : null;
                weapon2 = unit.holdingHandler.leftObject ? unit.holdingHandler.leftObject.gameObject : null;
                if (zombieType != ZombificationType.Virus)
                {
                    if (weapon1 && weapon1.GetComponent<Holdable>()) weapon1.GetComponent<Holdable>().ignoreDissarm = true;
                    if (weapon2 && weapon2.GetComponent<Holdable>()) weapon2.GetComponent<Holdable>().ignoreDissarm = true;
                }
            }
            var effect = unit.GetComponentsInChildren<UnitEffectBase>().ToList().Find(x => x.effectID == 1984);
            if ((effect && effect != this) || unit.unitType == Unit.UnitType.Warmachine)
            {
                Destroy(gameObject);
                unit.data.healthHandler.RemoveDieAction(Revive);
            }
            else
            {
                unit.data.healthHandler.AddDieAction(Revive);
            }
            
            Ping();
        }
        
        public override void Ping()
        {
            if (done) return;
            
            currentProgress += Mathf.Clamp(progressToAdd / unit.data.health, 0f, 1f);
            if (zombieType != ZombificationType.Support) AddLerpProgress();
            else StartCoroutine(DoZombieChecks());
        }
    
        public void AddLerpProgress()
        {
            if (done && zombieType != ZombificationType.Support) return;
            
            StopCoroutine(DoLerp());
            StartCoroutine(DoLerp());
        }
    
        public IEnumerator DoLerp()
        {
            if (done && zombieType != ZombificationType.Support) yield break;
            
            float c = 0f;
            var startProgress = lerpProgress;
            while (c < 1f)
            {
                c += Mathf.Clamp(Time.deltaTime * lerpSpeed, 0f, 1f);
                lerpProgress = Mathf.Lerp(startProgress, currentProgress, c);
                yield return null;
            }
    
            StartCoroutine(DoZombieChecks());
            yield break;
        }
    
        public IEnumerator DoZombieChecks()
        {
            if (done) yield break;
            
            yield return new WaitForEndOfFrame();
    
            if (currentProgress >= 0.5f)
            {
                unit.data.healthHandler.willBeRewived = true;
                if (unit.GetComponentInChildren<AddRigidbodyOnDeath>())
                    foreach (var script in unit.GetComponentsInChildren<AddRigidbodyOnDeath>())
                    {
                        unit.data.healthHandler.RemoveDieAction(new System.Action(script.Die)); 
                        Destroy(script);
                    }
                if (unit.GetComponentInChildren<SinkOnDeath>())
                    foreach (var script in unit.GetComponentsInChildren<SinkOnDeath>())
                    {
                        unit.data.healthHandler.RemoveDieAction(new System.Action(script.Sink)); 
                        Destroy(script);
                    }
                if (unit.GetComponentInChildren<RemoveJointsOnDeath>())
                    foreach (var script in unit.GetComponentsInChildren<RemoveJointsOnDeath>())
                    {
                        unit.data.healthHandler.RemoveDieAction(new System.Action(script.Die)); 
                        Destroy(script);
                    }
                if (unit.GetComponentInChildren<DisableAllSkinnedClothes>())
                    foreach (var script in unit.GetComponentsInChildren<DisableAllSkinnedClothes>())
                    {
                        Destroy(script);
                    }
            }
    
            if (currentProgress >= 1f && zombieType != ZombificationType.Support)
            {
                unit.data.healthHandler.TakeDamage(unit.data.maxHealth, Vector3.zero, unit, DamageType.Magic);
            }
        }
    
        public void Revive()
        {
            if (!done)
            {
                done = true;
                StartCoroutine(DoRevive());
            }
        }
    
        public IEnumerator DoRevive()
        {
            if (!unit.data.Dead)
            {
                ServiceLocator.GetService<GameModeService>().CurrentGameMode.OnUnitDied(unit);
            }
            
            Landfall.TABS.Team newTeam;
            if (zombieType == ZombificationType.Support) newTeam = unit.data.team;
            else newTeam = unit.data.team == Landfall.TABS.Team.Red ? Landfall.TABS.Team.Blue : Landfall.TABS.Team.Red;
            unit.data.team = newTeam;
            unit.Team = newTeam;
            
            unit.targetingPriorityMultiplier = reviveTargetingPriority;
            
            var goe = unit.GetComponent<GameObjectEntity>();
            goe.EntityManager.RemoveComponent<IsDead>(goe.Entity);
            goe.EntityManager.AddComponent(goe.Entity, ComponentType.Create<UnitTag>());
            goe.EntityManager.SetSharedComponentData(goe.Entity, new Landfall.TABS.AI.Components.Team
            {
                Value = (int)unit.Team
            });
            World.Active.GetOrCreateManager<TeamSystem>().AddUnit(goe.Entity, unit.gameObject, unit.transform, unit.data.mainRig, unit.data, newTeam, unit, false);
            
            if (zombieType == ZombificationType.Support)
            {
                AddLerpProgress();
            }
            
            yield return new WaitForSeconds(reviveDelay);
    
            unit.data.Dead = false;
            unit.dead = false;
            unit.data.hasBeenRevived = true;
            unit.data.healthHandler.willBeRewived = false;
    
            unit.data.ragdollControl = 1f;
            unit.data.muscleControl = 1f;
    
            unit.data.health = unit.data.maxHealth * reviveHealthMultiplier;
    
            if (zombieType == ZombificationType.Virus)
            {
                if (unit.holdingHandler)
                {
                    if (weapon1) weapon1.AddComponent<RemoveAfterSeconds>().shrink = true;
                    if (weapon2) weapon1.AddComponent<RemoveAfterSeconds>().shrink = true;
                    unit.holdingHandler.LetGoOfAll();
                    unit.unitBlueprint.SetWeapon(unit, newTeam, reviveWeapon, new PropItemData(), HoldingHandler.HandType.Right, unit.data.mainRig.rotation, new List<GameObject>());
                    unit.unitBlueprint.SetWeapon(unit, newTeam, reviveWeapon, new PropItemData(), HoldingHandler.HandType.Left, unit.data.mainRig.rotation, new List<GameObject>());
                }
                else if (unit.GetComponentInChildren<HoldingHandlerMulti>())
                {
                    var multi = unit.GetComponentInChildren<HoldingHandlerMulti>();
                    foreach (var w in multi.spawnedWeapons)
                    {
                        w.AddComponent<RemoveAfterSeconds>().shrink = true;
                    }
                    multi.LetGoOfAll();
                    foreach (var left in multi.otherHands)
                    {
                        multi.SetWeapon(left.gameObject, Instantiate(reviveWeapon, left.transform.position, left.transform.rotation, unit.transform));
                    }
                    foreach (var right in multi.mainHands)
                    {
                        multi.SetWeapon(right.gameObject, Instantiate(reviveWeapon, right.transform.position, right.transform.rotation, unit.transform));
                    }
                }
            }
            else
            {
                if (weapon1 && weapon1.GetComponent<Holdable>()) weapon1.GetComponent<Holdable>().ignoreDissarm = false;
                if (weapon2 && weapon2.GetComponent<Holdable>()) weapon2.GetComponent<Holdable>().ignoreDissarm = false;
            }
            foreach (var ability in reviveAbilities)
            {
                Instantiate(ability, unit.transform.position, unit.transform.rotation, unit.transform);
            }
            
            if (unit.GetComponentInChildren<TeamColor>())
            {
                foreach (var tc in unit.GetComponentsInChildren<TeamColor>())
                {
                    tc.SetTeamColor(newTeam);
                }
            }
            
            if (unit.data.GetComponent<StandingHandler>())
            {
                var ran = unit.data.gameObject.AddComponent<RandomCharacterStats>();
                ran.minStandingOffset = zombieStats.GetComponent<RandomCharacterStats>().minStandingOffset;
                ran.maxStandingOffset = zombieStats.GetComponent<RandomCharacterStats>().maxStandingOffset;
                ran.minMovement = zombieStats.GetComponent<RandomCharacterStats>().minMovement;
                ran.maxMovemenmt = zombieStats.GetComponent<RandomCharacterStats>().maxMovemenmt;
                ran.randomCurve = zombieStats.GetComponent<RandomCharacterStats>().randomCurve;
            }
            
            unit.api.SetTargetingType(unit.unitBlueprint.TargetingComponent);
            ServiceLocator.GetService<UnitHealthbars>().HandleUnitSpawned(unit);
            unit.api.UpdateECSValues();
            unit.InitializeUnit(newTeam);
    
            reviveEvent.Invoke();
        }
        
        public void Update()
        {
            if (unit)
            {
                unit.data.GetComponent<UnitColorHandler>().SetColor(color, lerpProgress);
            }
        }
    
        public enum ZombificationType
        {
            Standard,
            Virus,
            Support
        }
    
        private Unit unit;
        private GameObject weapon1;
        private GameObject weapon2;
    
        private bool done;
        
        [Header("Zombification")]
        
        public ZombificationType zombieType;
        
        private float currentProgress;
    
        public float progressToAdd = 100f;
    
        [Header("Revive")] 
        
        public UnityEvent reviveEvent;
        
        public float reviveDelay;
    
        [Range(0f, 1f)]
        public float reviveHealthMultiplier = 0.5f;
    
        public float reviveTargetingPriority = 0.2f;
    
        public GameObject reviveWeapon;

        public List<GameObject> reviveAbilities = new List<GameObject>();
        
        public GameObject zombieStats;
    
        [Header("Color")] 
        
        public UnitColorInstance color = new UnitColorInstance();
        
        private float lerpProgress;
    
        public float lerpSpeed = 1f;
    }
}

