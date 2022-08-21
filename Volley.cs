using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Landfall.TABS;
using UnityEngine;

public class Volley : MonoBehaviour {
	
	private void Start() {
		
		spawn = GetComponent<SpawnObject>();
		SetTarget();
		StartCoroutine(SpawnArrows());
	}
	
	private IEnumerator SpawnArrows() {
		
		yield return new WaitForSeconds(spawnDelay);
		int num;
		for (int l = 0; l < totalSpawns; l = num + 1) {
			
			SetTarget();
			for (int m = 0; m < arrowsPerSpawn; m = num + 1) {
				
				Vector3 vector = transform.position + Vector3.up * metersAboveToSpawwnAt + Random.insideUnitSphere * radius;
				Vector3 direction = transform.position - vector + Random.insideUnitSphere * 1f;
				if (Random.value > 0.1f && m_NearbyUnits.Count > 0) {
					
					int index = Random.Range(0, m_NearbyUnits.Count);
					if (m_NearbyUnits[index] != null && m_NearbyUnits[index].data != null && m_NearbyUnits[index].data.mainRig != null) { direction = m_NearbyUnits[index].data.mainRig.position + m_NearbyUnits[index].data.mainRig.velocity * 0.25f - vector; }
				}
				spawn.Spawn(vector, direction);
				num = m;
			}
			yield return new WaitForSeconds(timeBetweenSpawns);
			num = l;
		}
		yield break;
	}

	public void SetTarget() {
		
		var hits = Physics.SphereCastAll(transform.position, 8f, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
		List<Unit> foundUnits = new List<Unit>();
		foreach (var hit in hits) {
			
			if (hit.transform.root.GetComponent<Unit>() && !foundUnits.Contains(hit.transform.root.GetComponent<Unit>())) { foundUnits.Add(hit.rigidbody.transform.root.GetComponent<Unit>()); }
		}
		Unit[] query
		= (
		  from Unit unit
		  in foundUnits
		  where !unit.data.Dead && unit.Team != GetComponent<TeamHolder>().team
		  orderby (unit.data.mainRig.transform.position - transform.position).magnitude
		  select unit
		).ToArray();
		
		if (query.Length != 0) { m_NearbyUnits = query.ToList(); }
	}

	public float metersAboveToSpawwnAt = 10f;

	public float radius = 3f;

	public float timeBetweenSpawns = 0.05f;

	public int totalSpawns = 25;

	public int arrowsPerSpawn = 2;

	private SpawnObject spawn;

	public float spawnDelay = 0.3f;

	private List<Unit> m_NearbyUnits = new List<Unit>();
}
