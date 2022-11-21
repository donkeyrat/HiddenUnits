using UnityEngine;
using Landfall.TABS;
using System.Collections;

namespace HiddenUnits {
    
    public class Transformation : MonoBehaviour {

        private void Start()
        {
            transformer.unitBlueprint = transform.root.GetComponent<Unit>().unitBlueprint;
            root = transform.root.GetComponent<Unit>();
        }

        public void Transform() {
            
            storedHP = root.data.health;
            var unit = transformed.Spawn();
            transform.SetParent(unit.transform);
            
            unit.transform.position = new Vector3(root.data.mainRig.position.x,
                root.data.mainRig.position.y - distanceBelowToSpawn, root.data.mainRig.position.z);
            unit.transform.rotation = root.data.mainRig.rotation;

            Destroy(root.gameObject);
            StartCoroutine(Revert());
        }

        public IEnumerator Revert() {
            
            yield return new WaitForSeconds(revertTime);
            
            var werewolf = transform.root.GetComponent<Unit>();
            root = transformer.Spawn();
            
            root.data.healthHandler.TakeDamage(root.data.maxHealth - storedHP, Vector3.down);
            root.transform.position = new Vector3(werewolf.data.mainRig.position.x, werewolf.data.mainRig.position.y - distanceBelowToSpawn, werewolf.data.mainRig.position.z);
            root.transform.rotation = werewolf.data.mainRig.rotation;
            
            Destroy(werewolf.gameObject);
            Destroy(gameObject);
        }

        private Unit root;

        public WerewolfSpawner transformer;

        public WerewolfSpawner transformed;

        public float revertTime = 6f;

        private float storedHP;

        public float distanceBelowToSpawn = 1.347f;
    }
}