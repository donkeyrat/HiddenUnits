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

namespace HiddenUnits
{
    public class Effect_Zombie : UnitEffectBase
    {
        public void Init()
        {
            unit = transform.root.GetComponent<Unit>();
            if (unit.data.weaponHandler && unit.data.weaponHandler.rightWeapon != null && unit.data.weaponHandler.rightWeapon.GetComponent<Holdable>() && !unit.data.weaponHandler.rightWeapon.GetComponent<Holdable>().ignoreDissarm)
            {
                weapon1 = unit.data.weaponHandler.rightWeapon.gameObject;
            }
            if (unit.data.weaponHandler && unit.data.weaponHandler.leftWeapon != null && unit.data.weaponHandler.leftWeapon.GetComponent<Holdable>() && !unit.data.weaponHandler.leftWeapon.GetComponent<Holdable>().ignoreDissarm)
            {
                weapon2 = unit.data.weaponHandler.leftWeapon.gameObject;
            }
        }

        public override void DoEffect()
        {
            if (!Initialized)
            {
                Initialized = true;
                Init();
            }
            StartCoroutine(Progress());
        }

        public override void Ping()
        {
            if (!done)
            {
                progress += progressToAdd / unit.data.health;
                if (zombieType != Type.Support)
                {
                    unit.GetComponentInChildren<UnitColorHandler>().SetColor(color, progress);
                }
                if (progress >= 0.5f)
                {
                    if (unit.GetComponentInChildren<AddRigidbodyOnDeath>())
                    {
                        foreach (var script in unit.GetComponentsInChildren<AddRigidbodyOnDeath>())
                        {
                            unit.data.healthHandler.RemoveDieAction(new System.Action(script.Die));
                            Destroy(script);
                        }
                    }
                    if (unit.GetComponentInChildren<SinkOnDeath>())
                    {
                        foreach (var script in unit.GetComponentsInChildren<SinkOnDeath>())
                        {
                            unit.data.healthHandler.RemoveDieAction(new System.Action(script.Sink));
                            Destroy(script);
                        }
                    }
                    if (unit.GetComponentInChildren<RemoveJointsOnDeath>())
                    {
                        foreach (var script in unit.GetComponentsInChildren<RemoveJointsOnDeath>())
                        {
                            unit.data.healthHandler.RemoveDieAction(new System.Action(script.Die));
                            Destroy(script);
                        }
                    }
                    if (unit.GetComponentInChildren<DisableAllSkinnedClothes>())
                    {
                        foreach (var script in unit.GetComponentsInChildren<DisableAllSkinnedClothes>())
                        {
                            Destroy(script);
                        }
                    }
                    unit.data.healthHandler.willBeRewived = true;
                }
                if (progress >= 1f)
                {
                    if (zombieType != Type.Support)
                    {
                        unit.GetComponentInChildren<UnitColorHandler>().SetColor(color, 1f);
                    }
                    if (!unit.data.Dead)
                    {
                        done = true;
                        unit.data.healthHandler.Die();
                    }
                }
            }
        }

        public void OnDeath()
        {
            if (unit.data.healthHandler.willBeRewived)
            {
                if (progress >= 0.5f)
                {
                    StartCoroutine(Revival());
                }
            }
        }

        public IEnumerator Progress()
        {
            yield return new WaitForSeconds(0.05f);
            Ping();
            yield break;
        }

        public IEnumerator Revival()
        {
            if (!unit.data.Dead)
            {
                ServiceLocator.GetService<GameModeService>().CurrentGameMode.OnUnitDied(unit);
            }
            Landfall.TABS.Team newTeam;
            if (zombieType == Type.Support)
            {
                newTeam = unit.data.team;
            }
            else
            {
                newTeam = unit.data.team == Landfall.TABS.Team.Red ? Landfall.TABS.Team.Blue : Landfall.TABS.Team.Red;
            }
            unit.data.team = newTeam;
            unit.Team = newTeam;
            unit.targetingPriorityMultiplier = 0.2f;
            var goe = unit.GetComponent<GameObjectEntity>();
            goe.EntityManager.RemoveComponent<IsDead>(goe.Entity);
            goe.EntityManager.AddComponent(goe.Entity, ComponentType.Create<UnitTag>());
            goe.EntityManager.SetSharedComponentData(goe.Entity, new Landfall.TABS.AI.Components.Team
            {
                Value = (int)unit.Team
            });
            World.Active.GetOrCreateManager<TeamSystem>().AddUnit(goe.Entity, unit.gameObject, unit.transform, unit.data.mainRig, unit.data, newTeam, unit, false);
            yield return new WaitForSeconds(reviveDelay);
            unit.data.Dead = false;
            unit.dead = false;
            unit.data.hasBeenRevived = true;
            unit.data.ragdollControl = 1f;
            unit.data.muscleControl = 1f;
            unit.data.health = unit.data.maxHealth / 2;
            if (zombieType == Type.Virus)
            {
                if (unit.GetComponentInChildren<HoldingHandler>())
                {
                    if (weapon1)
                    {
                        weapon1.AddComponent<RemoveAfterSeconds>().shrink = true;
                    }
                    if (weapon2)
                    {
                        weapon2.AddComponent<RemoveAfterSeconds>().shrink = true;
                    }
                    unit.GetComponentInChildren<HoldingHandler>().LetGoOfAll();
                    unit.unitBlueprint.SetWeapon(unit, newTeam, virusWeapon, new PropItemData(), HoldingHandler.HandType.Right, unit.data.mainRig.rotation, new List<GameObject>());
                    unit.unitBlueprint.SetWeapon(unit, newTeam, virusWeapon, new PropItemData(), HoldingHandler.HandType.Left, unit.data.mainRig.rotation, new List<GameObject>());
                }
                else if (unit.GetComponentInChildren<HoldingHandlerMulti>())
                {
                    if (unit.GetComponentInChildren<HoldingHandlerMulti>().spawnedWeapons.Count > 0)
                    {
                        foreach (var w in unit.GetComponentInChildren<HoldingHandlerMulti>().spawnedWeapons)
                        {
                            w.AddComponent<RemoveAfterSeconds>().shrink = true;
                        }
                        unit.GetComponentInChildren<HoldingHandlerMulti>().LetGoOfAll();
                        foreach (var left in unit.GetComponentInChildren<HoldingHandlerMulti>().otherHands)
                        {
                            unit.GetComponentInChildren<HoldingHandlerMulti>().SetWeapon(left.gameObject, Instantiate(virusWeapon, left.transform.position, left.transform.rotation, unit.transform));
                        }
                        foreach (var right in unit.GetComponentInChildren<HoldingHandlerMulti>().mainHands)
                        {
                            unit.GetComponentInChildren<HoldingHandlerMulti>().SetWeapon(right.gameObject, Instantiate(virusWeapon, right.transform.position, right.transform.rotation, unit.transform));
                        }
                    }
                }
            }
            else
            {
                if (weapon1)
                {
                    unit.unitBlueprint.SetWeapon(unit, newTeam, weapon1, new PropItemData(), HoldingHandler.HandType.Right, unit.data.mainRig.rotation, new List<GameObject>()).transform.SetParent(unit.transform);
                    if (unit.GetComponentInChildren<HoldingHandlerMulti>())
                    {
                        foreach (var right in unit.GetComponentInChildren<HoldingHandlerMulti>().mainHands)
                        {
                            unit.GetComponentInChildren<HoldingHandlerMulti>().SetWeapon(right.gameObject, Instantiate(weapon1, right.transform.position, right.transform.rotation, unit.transform));
                        }
                    }
                    else if (unit.unitBlueprint.holdinigWithTwoHands && !weapon2)
                    {
                        unit.holdingHandler.leftHandActivity = HoldingHandler.HandActivity.HoldingRightObject;
                    }
                    Destroy(weapon1);
                }
                if (weapon2)
                {
                    unit.unitBlueprint.SetWeapon(unit, newTeam, weapon2, new PropItemData(), HoldingHandler.HandType.Left, unit.data.mainRig.rotation, new List<GameObject>()).transform.SetParent(unit.transform);
                    if (unit.GetComponentInChildren<HoldingHandlerMulti>())
                    {
                        foreach (var left in unit.GetComponentInChildren<HoldingHandlerMulti>().otherHands)
                        {
                            unit.GetComponentInChildren<HoldingHandlerMulti>().SetWeapon(left.gameObject, Instantiate(weapon2, left.transform.position, left.transform.rotation, unit.transform));
                        }
                    }
                    Destroy(weapon2);
                }
            }
            if (unit.GetComponentInChildren<TeamColor>())
            {
                foreach (var tc in unit.GetComponentsInChildren<TeamColor>())
                {
                    tc.SetTeamColor(newTeam);
                }
            }
            unit.GetComponentInChildren<UnitColorHandler>().SetColor(color, 1f);
            unit.data.healthHandler.willBeRewived = false;
            ServiceLocator.GetService<UnitHealthbars>().HandleUnitSpawned(unit);
            if (unit.data.GetComponent<StandingHandler>())
            {
                var ran = unit.data.gameObject.AddComponent<RandomCharacterStats>();
                ran.minStandingOffset = stats.GetComponent<RandomCharacterStats>().minStandingOffset;
                ran.maxStandingOffset = stats.GetComponent<RandomCharacterStats>().maxStandingOffset;
                ran.minMovement = stats.GetComponent<RandomCharacterStats>().minMovement;
                ran.maxMovemenmt = stats.GetComponent<RandomCharacterStats>().maxMovemenmt;
                ran.randomCurve = stats.GetComponent<RandomCharacterStats>().randomCurve;
            }
            unit.api.SetTargetingType(unit.unitBlueprint.TargetingComponent);
            unit.api.UpdateECSValues();
            unit.InitializeUnit(newTeam);
            reviveEvent.Invoke();
            yield break;
        }

        public enum Type
        {
            Zombified,
            Virus,
            Support
        }

        public Type zombieType;

        public GameObject virusWeapon;

        public float progress;

        public float progressToAdd = 100f;

        public float reviveDelay = 4f;

        public UnityEvent reviveEvent;

        public UnitColorInstance color;

        public GameObject stats;

        private Unit unit;

        private GameObject weapon1;

        private GameObject weapon2;

        private bool done;

        private bool Initialized;
    }
}
