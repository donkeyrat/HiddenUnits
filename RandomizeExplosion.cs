using UnityEngine;
using System.Collections.Generic;

namespace HiddenUnits
{
    public class RandomizeExplosion : MonoBehaviour
    {
        public void Start()
        {
            Randomize();
        }

        public void Randomize()
        {
            if (GetComponent<SpawnObject>())
            {
                GetComponent<SpawnObject>().objectToSpawn = objectsToSpawn[Random.Range(0, objectsToSpawn.Count)];
            }
        }

        public List<GameObject> objectsToSpawn = new List<GameObject>();

        public float delay;
    }
}
