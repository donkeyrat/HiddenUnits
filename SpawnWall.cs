using System.Collections;
using System.Collections.Generic;
using Landfall.TABS;
using Photon.Bolt;
using TFBGames;
using UnityEngine;
using UnityEngine.Events;

public class SpawnWall : MonoBehaviour, IRemotelyControllable
{
	public GameObject objectToSpawn;

	public float percentage = 0.5f;

	public bool spawnOnAwake;

	public int numberToSpawn = 1;

	public bool spawnIfUnitDead = true;

	public bool parentToMe;

	public bool followMe;

	public bool useRootSizeOfSpawner;

	public bool giveTarget;

	public bool allowRootTarget;

	public bool useAlternatingSpawnPos;

	public bool giveSpawnerWeapon;

	public float spread;

	public float timeBetweenSpawns = 0.02f;

	public bool waitBeforeFirstSpawn;

	public bool useRandom;

	public float minRandom = 1f;

	public float maxRandom = 1f;

	public float maxRange;

	private bool didInitialize;

	public UnityEvent spawnEvent;

	private ProjectilesSpawnManager projectilesSpawnManager;

	private bool isProjectile;

	private GameObject rootObject;

	private DataHandler data;

	private CheckClosestUnitTargets TargetChecker;

	private float allowedToSpawn = 1f;

	private float spawnedSinceDelay;

	private List<Unit> targets;

	private Unit currentTarget;

	private List<AlternatingSpawnPos> altSpawnPos;

	private int targetPlace;

	private int currentSpawnPosNumber;

	[HideInInspector]
	public GameObject spawnerProjecile;

	private bool didInitializeForMultiplayer;

	private bool isUnit;

	public bool IsRemotelyControlled { get; private set; }

	private void Start()
	{
		data = base.transform.root.GetComponentInChildren<DataHandler>();
		rootObject = base.transform.root.gameObject;
		if (giveTarget)
		{
			TargetChecker = GetComponent<CheckClosestUnitTargets>();
		}
		if (spawnOnAwake)
		{
			StartCoroutine(SpawnUnit(base.transform.position, base.transform.rotation));
		}
	}

	public void SpawnUpwardsOnLastGroundPos()
	{
		if ((bool)data)
		{
			StartCoroutine(SpawnUnit(data.groundMapPosition, Quaternion.LookRotation(Vector3.up)));
		}
		else
		{
			SpawnUpwards();
		}
	}

	public void SpawnUpwardsOnTarget()
	{
		Vector3 position = base.transform.position;
		if (!data)
		{
			data = rootObject.GetComponent<Unit>().data;
		}
		if ((bool)data && (bool)data.targetData)
		{
			position = data.targetData.mainRig.position;
			if (maxRange != 0f && data.distanceToTarget > maxRange)
			{
				return;
			}
		}
		StartCoroutine(SpawnUnit(position, Quaternion.LookRotation(Vector3.up + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void SpawnOnAndTowardsTarget()
	{
		Vector3 position = base.transform.position;
		if (!data)
		{
			data = rootObject.GetComponent<Unit>().data;
		}
		if ((bool)data && (bool)data.targetData)
		{
			position = data.targetData.mainRig.position;
			if (maxRange != 0f && data.distanceToTarget > maxRange)
			{
				return;
			}
		}
		Vector3 vector = new Vector3(data.targetData.mainRig.position.x - data.mainRig.position.x, 0f, data.targetData.mainRig.position.z - data.mainRig.position.z);
		StartCoroutine(SpawnUnit(position, Quaternion.LookRotation(vector + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void SpawnUpwardsOnSpawnerTarget()
	{
		Vector3 position = base.transform.position;
		DataHandler dataHandler = null;
		TeamHolder component = base.transform.root.GetComponent<TeamHolder>();
		if ((bool)component.spawner)
		{
			dataHandler = component.spawner.transform.root.GetComponent<Unit>().data;
		}
		if ((bool)dataHandler && (bool)dataHandler.targetData)
		{
			position = dataHandler.targetData.mainRig.position;
			if (maxRange != 0f && dataHandler.distanceToTarget > maxRange)
			{
				return;
			}
		}
		StartCoroutine(SpawnUnit(position, Quaternion.LookRotation(Vector3.up + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void SpawnUpwards()
	{
		StartCoroutine(SpawnUnit(base.transform.position, Quaternion.LookRotation(Vector3.up + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void SpawnAtObjectRotation()
	{
		Vector3 vector = base.transform.rotation * Vector3.forward;
		StartCoroutine(SpawnUnit(base.transform.position, Quaternion.LookRotation(vector + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void SpawnTowardsTarget()
	{
		if (!data)
		{
			data = rootObject.GetComponent<Unit>().data;
		}
		if ((bool)data.targetData)
		{
			StartCoroutine(SpawnUnit(base.transform.position, Quaternion.LookRotation((data.targetData.mainRig.position - data.mainRig.position).normalized + Random.insideUnitSphere * spread * 0.01f)));
		}
	}

	public void SpawnTowardsTargetWithoutY()
	{
		if (!data)
		{
			data = rootObject.GetComponent<Unit>().data;
		}
		if ((bool)data.targetData)
		{
			StartCoroutine(SpawnUnit(rotation: Quaternion.LookRotation(new Vector3(data.targetData.mainRig.position.x - data.mainRig.position.x, 0f, data.targetData.mainRig.position.z - data.mainRig.position.z).normalized + Random.insideUnitSphere * spread * 0.01f), position: Vector3.Lerp(data.targetData.mainRig.position, transform.position, percentage)));
		}
	}

	public void SpawnCharacterForward()
	{
		StartCoroutine(SpawnUnit(base.transform.position, Quaternion.LookRotation(base.transform.root.GetComponent<Unit>().data.mainRig.transform.forward + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void Spawn(Vector3 position, Vector3 direction)
	{
		StartCoroutine(SpawnUnit(position, Quaternion.LookRotation(direction + Random.insideUnitSphere * spread * 0.01f)));
	}

	private IEnumerator SpawnUnit(Vector3 position, Quaternion rotation)
	{
		if ((bool)TargetChecker)
		{
			targets = new List<Unit>();
			targets = TargetChecker.GetTargets(maxRange);
		}
		if (waitBeforeFirstSpawn)
		{
			spawnedSinceDelay = allowedToSpawn;
		}
		int numberSpawned = 0;
		while (numberSpawned < numberToSpawn)
		{
			InitializeSpawn();
			if (!IsAllowedToSpawnInMultiplayer())
			{
				yield return null;
			}
			if ((bool)data && data.Dead && !spawnIfUnitDead)
			{
				yield return null;
			}
			if (useAlternatingSpawnPos)
			{
				altSpawnPos = new List<AlternatingSpawnPos>();
				altSpawnPos.AddRange(base.transform.parent.GetComponentsInChildren<AlternatingSpawnPos>());
			}
			if (spawnedSinceDelay < allowedToSpawn)
			{
				if (useAlternatingSpawnPos && altSpawnPos.Count > 0)
				{
					if (currentSpawnPosNumber > altSpawnPos.Count - 1)
					{
						currentSpawnPosNumber = 0;
					}
					position = altSpawnPos[currentSpawnPosNumber].transform.position;
					if (numberToSpawn - numberSpawned <= altSpawnPos.Count && altSpawnPos[currentSpawnPosNumber].useLastSpawnEvent)
					{
						altSpawnPos[currentSpawnPosNumber].InvokeLastSpawnEvent();
					}
					else
					{
						altSpawnPos[currentSpawnPosNumber].InvokeSpawnEvent();
					}
				}
				GameObject gameObject = ((!isProjectile || !(projectilesSpawnManager != null)) ? Object.Instantiate(objectToSpawn, position, rotation) : projectilesSpawnManager.SpawnProjectile(objectToSpawn, position, rotation));
				TeamHolder.AddTeamHolder(gameObject, base.transform.gameObject);
				TeamHolder component = gameObject.GetComponent<TeamHolder>();
				if (giveSpawnerWeapon)
				{
					component.spawnerWeapon = base.transform.GetComponentInParent<Weapon>().gameObject;
				}
				spawnedSinceDelay += 1f;
				if (parentToMe)
				{
					gameObject.transform.SetParent(base.transform, worldPositionStays: true);
				}
				if (followMe)
				{
					FollowTransform followTransform = gameObject.gameObject.AddComponent<FollowTransform>();
					followTransform.target = base.transform;
					followTransform.destroyOnTargetNull = false;
				}
				if (useRootSizeOfSpawner)
				{
					gameObject.transform.localScale = base.transform.root.localScale;
				}
				SpellTarget component2 = gameObject.GetComponent<SpellTarget>();
				if ((bool)TargetChecker && targets.Count > 0)
				{
					if (numberSpawned < targets.Count)
					{
						targetPlace = numberSpawned;
					}
					else
					{
						targetPlace = numberSpawned - targets.Count * Mathf.FloorToInt(numberSpawned / targets.Count);
					}
				}
				if ((bool)component2)
				{
					if ((bool)TargetChecker && giveTarget && targetPlace < targets.Count)
					{
						currentTarget = targets[targetPlace];
						DataHandler componentInChildren = currentTarget.GetComponentInChildren<DataHandler>();
						Vector3 position2 = base.transform.position;
						Vector3 position3 = componentInChildren.mainRig.position;
						Rigidbody mainRig = componentInChildren.mainRig;
						component2.DoEffect(position2, position3, mainRig);
					}
					else
					{
						component2.GetTarget();
					}
				}
				TeslaCannon component3 = gameObject.GetComponent<TeslaCannon>();
				if ((bool)component3 && (bool)spawnerProjecile)
				{
					component3.maxTargetChecker = spawnerProjecile.transform.GetComponent<TeslaCannon>().maxTargetChecker;
					if ((bool)TargetChecker && giveTarget && (bool)component3.maxTargetChecker && component3.maxTargetChecker.CheckIfAllowedToHit())
					{
						Unit component4 = base.transform.root.GetComponent<Unit>();
						if (targets.Count > 0)
						{
							if (!allowRootTarget && (bool)component4 && targets[targetPlace] == component4)
							{
								targets.Remove(targets[targetPlace]);
							}
							currentTarget = targets[targetPlace];
							DataHandler componentInChildren2 = currentTarget.GetComponentInChildren<DataHandler>();
							TeamHolder component5 = GetComponent<TeamHolder>();
							component.team = component5.team;
							component.spawner = component5.spawner;
							component3.PlayEffect(componentInChildren2.mainRig.transform, base.transform, component5.spawner);
						}
						else
						{
							Object.Destroy(base.gameObject);
						}
					}
					else
					{
						Object.Destroy(base.gameObject);
					}
				}
				numberSpawned++;
				currentSpawnPosNumber++;
				spawnEvent?.Invoke();
			}
			else
			{
				spawnedSinceDelay = 0f;
				float seconds = ((!useRandom) ? timeBetweenSpawns : Random.Range(timeBetweenSpawns * minRandom, timeBetweenSpawns * maxRandom));
				yield return new WaitForSeconds(seconds);
			}
		}
		targetPlace = 0;
	}

	private void InitializeSpawn()
	{
		if (!didInitialize)
		{
			didInitialize = true;
			if (objectToSpawn != null && objectToSpawn.GetComponent<Projectile>() != null)
			{
				isProjectile = true;
				projectilesSpawnManager = ServiceLocator.GetService<ProjectilesSpawnManager>();
			}
		}
	}

	public void SetIsRemotelyControlled(bool isRemotelyControlled)
	{
		IsRemotelyControlled = isRemotelyControlled;
	}

	private bool IsAllowedToSpawnInMultiplayer()
	{
		InitializeForMultiplayer();
		if (!isUnit && !isProjectile)
		{
			return true;
		}
		return !IsRemotelyControlled;
	}

	private void InitializeForMultiplayer()
	{
		if (!didInitializeForMultiplayer)
		{
			didInitializeForMultiplayer = true;
			if (BoltNetwork.IsRunning && objectToSpawn != null)
			{
				isUnit = objectToSpawn.GetComponent<Unit>() != null;
			}
		}
	}
}
