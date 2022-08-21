using UnityEngine;
using Landfall.TABS;
using System.Collections;

namespace HiddenUnits {
    
    public class Transformation : MonoBehaviour {
        
        public void Start() { GetComponentInChildren<WerewolfSpawner>().unitBlueprint = transform.root.GetComponent<Unit>().unitBlueprint; }

        public void Transform() {
            
            var root = transform.root.GetComponent<Unit>();
            var unit = transformed.Spawn();
            storedHP = root.data.health;
            transform.SetParent(unit.transform);
            unit.transform.position = new Vector3(root.data.mainRig.position.x,
                root.data.mainRig.position.y - distanceBelowToSpawn, root.data.mainRig.position.z);
            unit.transform.rotation = root.data.mainRig.rotation;
            Destroy(root.gameObject);
            StartCoroutine(Revert());
        }

        public IEnumerator Revert() {
            
            yield return new WaitForSeconds(revertTime);
            var root = transform.root.GetComponent<Unit>();
            transformer.rootUnit = root;
            var unit = transformer.Spawn();
            unit.data.health = storedHP;
            unit.transform.position = new Vector3(root.data.mainRig.position.x, root.data.mainRig.position.y - distanceBelowToSpawn, root.data.mainRig.position.z);
            unit.transform.rotation = root.data.mainRig.rotation;
            Destroy(root.gameObject);
            Destroy(gameObject);
            yield break;
        }

        public WerewolfSpawner transformer;

        public WerewolfSpawner transformed;

        public float revertTime = 6f;

        private float storedHP;

        public float distanceBelowToSpawn = 1.347f;
    }
}