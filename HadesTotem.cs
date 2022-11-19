using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Landfall.TABS;
using System.Linq;
using UnityEngine.Events;

namespace HiddenUnits {

    public class HadesTotem : MonoBehaviour
    {

        void Start() {

            egg = GetComponentInParent<TeamHolder>().spawner.GetComponentInChildren<HadesEgg>();
        }

        public void Drain() { StartCoroutine(DoDrain()); }

        public IEnumerator DoDrain() {

            var targets = SetTargets();
            if (targets.Length > 0) { 

                for (int i = 0; i < targets.Length; i++) {

                    if (i >= limitPerDrain || egg.hitList.Contains(targets[i])) { yield break; }

                    var spawnedObject = Instantiate(objectToSpawn, transform.position, transform.rotation);
                    foreach (var targetableEffect in spawnedObject.GetComponents<TargetableEffect>()) {

                        targetableEffect.DoEffect(transform, targets[i].data.mainRig.transform);
                        targetableEffect.DoEffect(transform.position, targets[i].data.mainRig.position, targets[i].data.mainRig);
                    }
                    egg.AddHealth(healthToDrain);
                    drainEvent.Invoke();
                    egg.hitList.Add(targets[i]);
                    StartCoroutine(RemoveUnitFromList(targets[i]));
                    yield return new WaitForSeconds(delayPerDrain);
                }
                if (!egg.hasHatched) {
                    var spawnedObjectEgg = Instantiate(objectToSpawn, transform.position, transform.rotation);
                    Destroy(spawnedObjectEgg.GetComponent<AddTargetableEffect>());
                    foreach (var targetableEffect in spawnedObjectEgg.GetComponents<TargetableEffect>()) {

                        targetableEffect.DoEffect(transform, egg.transform);
                    }
                }
            }
        }

        public Unit[] SetTargets() {

            var hits = Physics.SphereCastAll(transform.position, radius, Vector3.up, 0.1f, layerMask);
            return hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => GetComponentInParent<TeamHolder>() && x && !x.data.Dead && x.Team != GetComponentInParent<TeamHolder>().team)
                .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
                .Distinct()
                .ToArray();
        }

        public IEnumerator RemoveUnitFromList(Unit unit) {

            yield return new WaitForSeconds(1f);
            egg.hitList.Remove(unit);
        }

        public UnityEvent drainEvent = new UnityEvent();

        private HadesEgg egg;

        public LayerMask layerMask;

        public GameObject objectToSpawn;

        public float radius = 4f;

        public float delayPerDrain = 0.05f;

        public float healthToDrain = 150f;

        public int limitPerDrain = 3;
    }
}
