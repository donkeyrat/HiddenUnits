using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

public class Volley : MonoBehaviour 
{
	
	private void Start() 
	{
		
		spawn = GetComponent<SpawnObject>();
		SetTarget();
		StartCoroutine(SpawnArrows());
	}
	
	private IEnumerator SpawnArrows() 
	{
		yield return new WaitForSeconds(spawnDelay);
		
		int num;
		for (int l = 0; l < totalSpawns; l = num + 1) 
		{
			SetTarget();
			for (int m = 0; m < arrowsPerSpawn; m = num + 1) 
			{
				Vector3 vector = transform.position + Vector3.up * metersAboveToSpawwnAt + Random.insideUnitSphere * radius;
				Vector3 direction = transform.position - vector + Random.insideUnitSphere * 1f;
				if (Random.value > 0.1f && nearbyUnits.Count > 0) 
				{
					int index = Random.Range(0, nearbyUnits.Count);
					if (nearbyUnits[index] != null && nearbyUnits[index].data != null && nearbyUnits[index].data.mainRig != null) { direction = nearbyUnits[index].data.mainRig.position + nearbyUnits[index].data.mainRig.velocity * 0.25f - vector; }
				}
				
				spawn.Spawn(vector, direction);
				num = m;
			}
			yield return new WaitForSeconds(timeBetweenSpawns);
			num = l;
		}
	}

	public void SetTarget() 
	{
		
		var hits = Physics.SphereCastAll(transform.position, 8f, Vector3.up, 0.1f, LayerMask.GetMask(new string[] { "MainRig" }));
		nearbyUnits = hits
			.Select(hit => hit.transform.root.GetComponent<Unit>())
			.Where(x => GetComponent<TeamHolder>() && x && !x.data.Dead && x.Team != GetComponent<TeamHolder>().team)
			.OrderBy(x => (x.data.mainRig.transform.position - transform.position).magnitude)
			.Distinct()
			.ToList();
	}

	private List<Unit> nearbyUnits = new List<Unit>();
	
	public float metersAboveToSpawwnAt = 10f;

	public float radius = 3f;

	public float timeBetweenSpawns = 0.05f;

	public int totalSpawns = 25;

	public int arrowsPerSpawn = 2;

	private SpawnObject spawn;

	public float spawnDelay = 0.3f;
}
