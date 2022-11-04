using UnityEngine;
using Landfall.TABS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Unity.Entities;
using Landfall.TABS.AI;
using System.Reflection;
using Landfall.TABS.GameMode;
using System.Linq;

namespace HiddenUnits
{
    public class Revive : MonoBehaviour
    {
        public void Start()
        {
            unit = transform.root.GetComponent<Unit>();
            unit.data.healthHandler.willBeRewived = true;
            if (unit.data.weaponHandler.rightWeapon != null && unit.data.weaponHandler.rightWeapon.GetComponent<Holdable>() && !unit.data.weaponHandler.rightWeapon.GetComponent<Holdable>().ignoreDissarm)
            {
                spawnedWeapon1 = unit.data.weaponHandler.rightWeapon.gameObject;
                spawnedWeapon1.GetComponent<Holdable>().ignoreDissarm = false;
            }
            if (unit.data.weaponHandler.leftWeapon != null && unit.data.weaponHandler.leftWeapon.GetComponent<Holdable>() && !unit.data.weaponHandler.leftWeapon.GetComponent<Holdable>().ignoreDissarm)
            {
                spawnedWeapon2 = unit.data.weaponHandler.leftWeapon.gameObject;
                spawnedWeapon2.GetComponent<Holdable>().ignoreDissarm = false;
            }
            if (weaponToSpawn1) { weaponToUse1 = weaponToSpawn1; } else { weaponToUse1 = spawnedWeapon1; }
            if (weaponToSpawn2) { weaponToUse2 = weaponToSpawn2; } else { weaponToUse2 = spawnedWeapon2; }
        }

        public void DoRevive()
        {
            StartCoroutine(Revival());
        }

        public IEnumerator Revival()
        {
            var effect = unit.GetComponentsInChildren<UnitEffectBase>().ToList().Find(x => x.effectID == 1984 || x.effectID == 1987);
            if (unit.data.health > 0f || effect)
            {
                unit.data.healthHandler.willBeRewived = false;
                ServiceLocator.GetService<GameModeService>().CurrentGameMode.OnUnitDied(unit);
                yield break;
            }
            beforeReviveEvent.Invoke();
            yield return new WaitForSeconds(reviveDelay);
            unit.data.Dead = false;
            unit.dead = false;
            unit.data.hasBeenRevived = true;
            reviveEvent.Invoke();
            unit.data.ragdollControl = 1f;
            unit.data.muscleControl = 1f;
            unit.data.health = unit.data.maxHealth * healthPercentage;
            if (weaponToUse1)
            {
                var w = unit.unitBlueprint.SetWeapon(unit, unit.Team, weaponToUse1, new PropItemData(), HoldingHandler.HandType.Right, unit.data.mainRig.rotation, new List<GameObject>());
                w.GetComponent<Holdable>().ignoreDissarm = false;
                if (weaponToUse1 == weaponToSpawn1) { w.GetComponent<Rigidbody>().mass *= unit.unitBlueprint.massMultiplier; }
                if (unit.unitBlueprint.holdinigWithTwoHands)
                {
                    unit.holdingHandler.leftHandActivity = HoldingHandler.HandActivity.HoldingRightObject;
                    weaponToUse2 = null;
                }
            }
            if (spawnedWeapon1 && removeWeapons) { 
                Destroy(spawnedWeapon1); 
            } 
            else if (spawnedWeapon1 && removeWeaponsAfterSeconds) {
                spawnedWeapon1.transform.SetParent(null); 
                var sec = spawnedWeapon1.AddComponent<RemoveAfterSeconds>(); 
                sec.shrink = true; 
                sec.seconds = 1f; 
            }
            if (weaponToUse2)
            {
                var w = unit.unitBlueprint.SetWeapon(unit, unit.Team, weaponToUse2, new PropItemData(), HoldingHandler.HandType.Left, unit.data.mainRig.rotation, new List<GameObject>());
                w.GetComponent<Holdable>().ignoreDissarm = false;
                if (weaponToUse2 == weaponToSpawn2) { w.GetComponent<Rigidbody>().mass *= unit.unitBlueprint.massMultiplier; }
            }
            if (spawnedWeapon2 && removeWeapons) { 
                Destroy(spawnedWeapon2); 
            } 
            else if (spawnedWeapon2 && removeWeaponsAfterSeconds) {
                spawnedWeapon2.transform.SetParent(null); 
                var sec = spawnedWeapon2.AddComponent<RemoveAfterSeconds>(); 
                sec.shrink = true; 
                sec.seconds = 1f; 
            }
            if (unit.GetComponentInChildren<EyeSpawner>() && unit.GetComponentInChildren<EyeSpawner>().spawnedEyes != null) {
                foreach (var eye in unit.GetComponentInChildren<EyeSpawner>().spawnedEyes) {
                    eye.dead.SetActive(false);
                    eye.currentEyeState = GooglyEye.EyeState.Open;
                    eye.SetState(GooglyEye.EyeState.Open);
                    GooglyEyes.instance.AddEye(eye);
                }
            }
            var goe = unit.GetComponent<GameObjectEntity>();
            if (unit.unitBlueprint.MovementComponents != null && unit.unitBlueprint.MovementComponents.Count > 0)
            {
                foreach (var mov in unit.unitBlueprint.MovementComponents)
                {
                    var mi = (MethodInfo)typeof(UnitAPI).GetMethod("CreateGenericRemoveComponentData", (BindingFlags)(-1)).Invoke(unit.api, new object[] { mov.GetType() });
                    mi.Invoke(goe.EntityManager, new object[] { goe.Entity });
                }
            }
            unit.data.healthHandler.willBeRewived = false;
            ServiceLocator.GetService<UnitHealthbars>().HandleUnitSpawned(unit);
            unit.api.SetTargetingType(unit.unitBlueprint.TargetingComponent);
            unit.api.UpdateECSValues();
            unit.InitializeUnit(unit.Team);
            unit.data.healthHandler.deathEvent.RemoveAllListeners();
            foreach (var rigidbodyOnDeath in unit.GetComponentsInChildren<AddRigidbodyOnDeath>()) {

                unit.data.healthHandler.RemoveDieAction(rigidbodyOnDeath.Die);
            }
            foreach (var deathEvent in unit.GetComponentsInChildren<DeathEvent>()) {

                unit.data.healthHandler.RemoveDieAction(deathEvent.Die);
            }
            afterReviveEvent.Invoke();
            yield break;
        }

        private Unit unit;

        public UnityEvent beforeReviveEvent = new UnityEvent();

        public UnityEvent reviveEvent = new UnityEvent();

        public UnityEvent afterReviveEvent = new UnityEvent();

        public float reviveDelay = 4f;

        public GameObject weaponToSpawn1;

        public GameObject weaponToSpawn2;

        private GameObject spawnedWeapon1;

        private GameObject spawnedWeapon2;

        private GameObject weaponToUse1;

        private GameObject weaponToUse2;

        public bool removeWeapons = true;

        public bool removeWeaponsAfterSeconds;

        [Range(0f, 1f)]
        public float healthPercentage = 0.5f;
    }
}
