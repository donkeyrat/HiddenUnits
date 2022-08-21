using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Landfall.TABS;

namespace HiddenUnits {

    public class HadesSpawnObjects : MonoBehaviour {
    
        void Start() { SpawnObjects(); }
    
        public void SpawnObjects() { StartCoroutine(DoSpawning()); }
    
        public IEnumerator DoSpawning() {
    
            for (int i = 0; i < amountToSpawn; i++) {
    
                var point = new Vector3(transform.position.x + Random.Range(-radius, radius), transform.position.y + 2f, transform.position.z + Random.Range(-radius, radius));
                var spawnedObject = Instantiate(objectsToSpawn[Random.Range(0, objectsToSpawn.Count)], point, Quaternion.LookRotation(Vector3.up));
                var team = spawnedObject.AddComponent<TeamHolder>();
                team.team = GetComponent<TeamHolder>().team;
                team.spawner = GetComponent<TeamHolder>().spawner;
                yield return new WaitForSeconds(delayPerSpawn);
            }
            yield break;
        }
    
        public List<GameObject> objectsToSpawn = new List<GameObject>();
    
        public int amountToSpawn = 10;
    
        public float radius = 8f;
    
        public float delayPerSpawn = 0.08f;
    }
}