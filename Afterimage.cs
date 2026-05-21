using UnityEngine;
using Landfall.TABS;
using Landfall.TABS.AI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Landfall.TABS.AI.Systems;
using TFBGames;
using TGCore;
using Unity.Entities;

namespace HiddenUnits 
{

    public class Afterimage : MonoBehaviour 
    {
        public void Start()
        {
            Spawner = GetComponent<UnitSpawner>();
            if (useRootBlueprint) Spawner.unitBlueprint = transform.root.GetComponent<Unit>().unitBlueprint;
            Spawner.spawnUnitAction += SpawnAfterimage;
        }

        public void SpawnAfterimage(GameObject unit)
        {
            StartCoroutine(Spawn(unit));
        }

        private IEnumerator Spawn(GameObject u) 
        {
            u.name = "AFTERIMAGE";

            var unit = u.GetComponent<Unit>();

            unit.data.GetComponent<UnitColorHandler>().SetMaterial(useTeamColor ? (unit.Team == Team.Red ? imgMaterialRed : imgMaterialBlue) : imgMaterial);

            if (disableAbilities)
            {
                foreach (var move in u.GetComponentsInChildren<ConditionalEvent>())
                {
                    Destroy(move.gameObject);
                }
                foreach (var projectileDodge in u.GetComponentsInChildren<ProjectileDodgeMove>())
                {
                    Destroy(projectileDodge.gameObject);
                }
            }

            var spawnedPoof = Instantiate(poofEffect, unit.data.mainRig.position, poofEffect.transform.rotation);
            TeamHolder.AddTeamHolder(spawnedPoof, unit, null);

            yield return new WaitForSeconds(0.1f);

            var rootUnit = transform.root.GetComponent<Unit>();
            if (rootUnit) Instantiate(poofEffect, rootUnit.data.mainRig.position, poofEffect.transform.rotation);
            
            unit.api.forceSupressFromWinCondition = true;
            unit.targetingPriorityMultiplier = afterimageTargetingPriority;
            unit.api.UpdateECSValues();
            
            var teamSystem =  World.Active.GetOrCreateManager<TeamSystem>();
            var winConditionUnits = (Dictionary<Team, List<Unit>>)teamSystem.GetField("m_winConditionUnits");
            winConditionUnits[unit.Team].Remove(unit);
            teamSystem.SetField("m_winConditionUnits", winConditionUnits);

            yield return new WaitForSeconds(fadeTime - 0.1f);
            
            spawnedPoof = Instantiate(poofEffect, unit.data.mainRig.position, poofEffect.transform.rotation);
            TeamHolder.AddTeamHolder(spawnedPoof, unit, null);

            yield return new WaitForSeconds(destroyDelay);
            
            foreach (var trail in u.GetComponentsInChildren<TrailRenderer>())
            {
                trail.transform.SetParent(null);
                trail.emitting = false;
                trail.gameObject.AddComponent<RemoveAfterSeconds>().seconds = trail.time * 1.5f;
            }
            
            unit.DestroyUnit();
        }

        private UnitSpawner Spawner;

        public Material imgMaterial;
        public GameObject poofEffect;

        public float fadeTime = 5f;
        public float destroyDelay = 0.2f;
        public float afterimageTargetingPriority = 0.1f;
        public bool useRootBlueprint = true;
        public bool disableAbilities = true;

        [Header("Team Color")] 
        
        public bool useTeamColor;
        public Material imgMaterialRed;
        public Material imgMaterialBlue;
    }
}
