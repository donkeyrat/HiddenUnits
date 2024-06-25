using UnityEngine;
using Landfall.TABS;
using Landfall.TABS.AI;
using System.Collections;

namespace HiddenUnits 
{

    public class Afterimage : MonoBehaviour 
    {
        public void Start()
        {
            Spawner = GetComponent<UnitSpawner>();
            Spawner.unitBlueprint = transform.root.GetComponent<Unit>().unitBlueprint;
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

            unit.data.GetComponent<UnitColorHandler>().SetMaterial(imgMaterial);
            u.GetComponent<UnitAPI>().forceSupressFromWinCondition = true;
            unit.targetingPriorityMultiplier = 0.1f;
            
            foreach (var move in u.GetComponentsInChildren<ConditionalEvent>())
            {
                Destroy(move.gameObject);
            }
            foreach (var projectileDodge in u.GetComponentsInChildren<ProjectileDodgeMove>())
            {
                Destroy(projectileDodge.gameObject);
            }

            Instantiate(poofEffect, unit.data.mainRig.position, poofEffect.transform.rotation);

            yield return new WaitForSeconds(0.1f);
            
            Instantiate(poofEffect, transform.root.GetComponent<Unit>().data.mainRig.position, poofEffect.transform.rotation);

            yield return new WaitForSeconds(fadeTime - 0.1f);
            
            Instantiate(poofEffect, unit.data.mainRig.position, poofEffect.transform.rotation);

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
    }
}
