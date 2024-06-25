using System.Collections;
using UnityEngine;
using Landfall.TABS;
using System.Linq;
using UnityEngine.Events;

namespace HiddenUnits {

    public class HadesTotem : MonoBehaviour
    {

        private void Start()
        {
            TeamHolder = GetComponentInParent<TeamHolder>();
            Egg = TeamHolder.spawner.GetComponentInChildren<HadesEgg>();
        }

        public void Drain()
        {
            StartCoroutine(DoDrain());
        }

        public IEnumerator DoDrain() {

            var targets = SetTargets();
            if (targets.Length > 0)
            {
                var limit = Mathf.Min(targets.Length, limitPerDrain);
                for (var i = 0; i < limit; i++) 
                {
                    var spawnedObject = Instantiate(objectToSpawn, transform.position, transform.rotation);
                    TeamHolder.AddTeamHolder(spawnedObject, null, TeamHolder);
                    foreach (var targetableEffect in spawnedObject.GetComponents<TargetableEffect>()) 
                    {
                        targetableEffect.DoEffect(transform, targets[i].data.mainRig.transform);
                        targetableEffect.DoEffect(transform.position, targets[i].data.mainRig.position, targets[i].data.mainRig);
                    }
                    
                    Egg.AddHealth(healthToDrain);
                    if (addToHitList) Egg.hitList.Add(targets[i]);
                    
                    drainEvent.Invoke();
                    StartCoroutine(RemoveUnitFromList(targets[i]));
                    
                    yield return new WaitForSeconds(delayPerDrain);
                }
                if (!Egg.hasHatched) 
                {
                    var spawnedObjectEgg = Instantiate(objectToSpawn, transform.position, transform.rotation);
                    TeamHolder.AddTeamHolder(spawnedObjectEgg, null, TeamHolder);
                    
                    Destroy(spawnedObjectEgg.GetComponent<AddTargetableEffect>());
                    foreach (var targetableEffect in spawnedObjectEgg.GetComponents<TargetableEffect>()) 
                    {
                        targetableEffect.DoEffect(transform, Egg.transform);
                    }
                }
            }
        }

        public Unit[] SetTargets() {

            var hits = Physics.SphereCastAll(transform.position, radius, Vector3.up, 0.1f, LayerMask.GetMask("MainRig"));
            return hits
                .Select(hit => hit.transform.root.GetComponent<Unit>())
                .Where(x => GetComponentInParent<TeamHolder>() && x && !x.data.Dead && x.Team != GetComponentInParent<TeamHolder>().team && !Egg.hitList.Contains(x))
                .OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
                .Distinct()
                .ToArray();
        }

        public IEnumerator RemoveUnitFromList(Unit unit) {

            yield return new WaitForSeconds(1f);
            if (unit) Egg.hitList.Remove(unit);
        }
        
        private TeamHolder TeamHolder;
        private HadesEgg Egg;

        public UnityEvent drainEvent = new();

        public GameObject objectToSpawn;
        public float radius = 4f;
        public float delayPerDrain = 0.05f;
        public float healthToDrain = 150f;
        
        public int limitPerDrain = 3;
        
        public bool addToHitList;
    }
}
