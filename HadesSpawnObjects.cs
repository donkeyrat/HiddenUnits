using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HiddenUnits {

    public class HadesSpawnObjects : MonoBehaviour {

        public void Start()
        {
            TeamHolder = GetComponent<TeamHolder>() ?? gameObject.AddComponent<TeamHolder>();
            if (spawnOnStart)
            {
                SpawnObjects();
            }
        }
    
        public void SpawnObjects() { StartCoroutine(DoSpawning()); }
    
        public IEnumerator DoSpawning() {
    
            for (var i = 0; i < amountToSpawn; i++) {
    
                var point = new Vector3(transform.position.x + Random.Range(-radius, radius), transform.position.y + 2f, transform.position.z + Random.Range(-radius, radius));
                var spawnedObject = Instantiate(objectsToSpawn[Random.Range(0, objectsToSpawn.Count)], point, Quaternion.LookRotation(Vector3.up));
                
                var team = spawnedObject.AddComponent<TeamHolder>();
                team.team = TeamHolder.team;
                team.spawner = TeamHolder.spawner;
                
                yield return new WaitForSeconds(delayPerSpawn);
            }
        }

        private TeamHolder TeamHolder;
        
        public List<GameObject> objectsToSpawn = new List<GameObject>();
    
        public int amountToSpawn = 10;
    
        public float radius = 8f;
    
        public float delayPerSpawn = 0.08f;

        public bool spawnOnStart = true;
    }
}