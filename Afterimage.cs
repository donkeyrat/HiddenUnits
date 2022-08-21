using UnityEngine;
using System.Collections.Generic;
using Landfall.TABS;
using Landfall.TABS.AI;
using System.Collections;

namespace HiddenUnits {

    public class Afterimage : MonoBehaviour {

        public void Start() { GetComponent<UnitSpawner>().unitBlueprint = transform.root.GetComponent<Unit>().unitBlueprint; }

        public void SpawnAfterimage() { StartCoroutine(Spawn()); }

        public IEnumerator Spawn() {

            transform.position = transform.root.GetComponent<Unit>().data.mainRig.position;
            var u = GetComponent<UnitSpawner>().Spawn();
            u.name = "AFTERIMAGE";
            u.transform.position = new Vector3(transform.root.GetComponent<Unit>().data.mainRig.position.x, transform.root.GetComponent<Unit>().data.mainRig.position.y - 1.353508f, transform.root.GetComponent<Unit>().data.mainRig.position.z);
            
            if (GetComponent<UnitSpawner>().poofEffect) { Instantiate(GetComponent<UnitSpawner>().poofEffect, u.data.mainRig.position, GetComponent<UnitSpawner>().poofEffect.transform.rotation); }
            foreach (var rend in u.GetComponentsInChildren<Renderer>()) {
                if (!rend.GetComponent<ParticleSystemRenderer>()) {
                    var list = new List<Material>();
                    foreach (var mat in rend.materials) { list.Add(Instantiate(imgMaterial)); }
                    rend.materials = list.ToArray();
                }
            }
            u.GetComponent<UnitAPI>().forceSupressFromWinCondition = true;
            u.targetingPriorityMultiplier = 0f;
            foreach (var ai in u.GetComponentsInChildren<ConditionalEvent>()) { Destroy(ai.gameObject); }

            yield return new WaitForSeconds(0.05f);
            foreach (var rend in u.GetComponentsInChildren<Renderer>()) {
                if (!rend.GetComponent<ParticleSystemRenderer>()) {
                    var list = new List<Material>();
                    foreach (var mat in rend.materials) { list.Add(Instantiate(imgMaterial)); }
                    rend.materials = list.ToArray();
                }
            }

            yield return new WaitForSeconds(0.05f);
            if (GetComponent<UnitSpawner>().poofEffect) {

                Instantiate(GetComponent<UnitSpawner>().poofEffect, transform.root.GetComponent<Unit>().data.mainRig.position, GetComponent<UnitSpawner>().poofEffect.transform.rotation);
            }

            yield return new WaitForSeconds(fadeTime - 0.1f);
            if (GetComponent<UnitSpawner>().poofEffect) {

                Instantiate(GetComponent<UnitSpawner>().poofEffect, u.data.mainRig.position, GetComponent<UnitSpawner>().poofEffect.transform.rotation);
            }

            yield return new WaitForSeconds(0.2f);
            u.transform.position = Vector3.down * 1000f;
            yield break;
        }

        public Material imgMaterial;

        public float fadeTime;
    }
}
