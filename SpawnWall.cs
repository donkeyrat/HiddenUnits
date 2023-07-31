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

	private bool DidInitialize;

	public UnityEvent spawnEvent;

	private ProjectilesSpawnManager ProjectilesSpawnManager;

	private bool IsProjectile;

	private GameObject RootObject;

	private DataHandler Data;

	private CheckClosestUnitTargets TargetChecker;

	private float AllowedToSpawn = 1f;

	private float SpawnedSinceDelay;

	private List<Unit> Targets;

	private Unit CurrentTarget;

	private List<AlternatingSpawnPos> AltSpawnPos;

	private int TargetPlace;

	private int CurrentSpawnPosNumber;

	[HideInInspector]
	public GameObject spawnerProjecile;

	private bool DidInitializeForMultiplayer;

	private bool IsUnit;

	public bool IsRemotelyControlled { get; private set; }

	private void Start()
	{
		Data = transform.root.GetComponentInChildren<DataHandler>();
		RootObject = transform.root.gameObject;
		if (giveTarget)
		{
			TargetChecker = GetComponent<CheckClosestUnitTargets>();
		}
		if (spawnOnAwake)
		{
			StartCoroutine(SpawnUnit(transform.position, transform.rotation));
		}
	}

	public void SpawnUpwardsOnLastGroundPos()
	{
		if ((bool)Data)
		{
			StartCoroutine(SpawnUnit(Data.groundMapPosition, Quaternion.LookRotation(Vector3.up)));
		}
		else
		{
			SpawnUpwards();
		}
	}

	public void SpawnUpwardsOnTarget()
	{
		var position = transform.position;
		if (!Data)
		{
			Data = RootObject.GetComponent<Unit>().data;
		}
		if ((bool)Data && (bool)Data.targetData)
		{
			position = Data.targetData.mainRig.position;
			if (maxRange != 0f && Data.distanceToTarget > maxRange)
			{
				return;
			}
		}
		StartCoroutine(SpawnUnit(position, Quaternion.LookRotation(Vector3.up + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void SpawnOnAndTowardsTarget()
	{
		var position = transform.position;
		if (!Data)
		{
			Data = RootObject.GetComponent<Unit>().data;
		}
		if ((bool)Data && (bool)Data.targetData)
		{
			position = Data.targetData.mainRig.position;
			if (maxRange != 0f && Data.distanceToTarget > maxRange)
			{
				return;
			}
		}
		var vector = new Vector3(Data.targetData.mainRig.position.x - Data.mainRig.position.x, 0f, Data.targetData.mainRig.position.z - Data.mainRig.position.z);
		StartCoroutine(SpawnUnit(position, Quaternion.LookRotation(vector + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void SpawnUpwardsOnSpawnerTarget()
	{
		var position = transform.position;
		DataHandler dataHandler = null;
		var component = transform.root.GetComponent<TeamHolder>();
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
		StartCoroutine(SpawnUnit(transform.position, Quaternion.LookRotation(Vector3.up + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void SpawnAtObjectRotation()
	{
		var vector = transform.rotation * Vector3.forward;
		StartCoroutine(SpawnUnit(transform.position, Quaternion.LookRotation(vector + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void SpawnTowardsTarget()
	{
		if (!Data)
		{
			Data = RootObject.GetComponent<Unit>().data;
		}
		if ((bool)Data.targetData)
		{
			StartCoroutine(SpawnUnit(transform.position, Quaternion.LookRotation((Data.targetData.mainRig.position - Data.mainRig.position).normalized + Random.insideUnitSphere * spread * 0.01f)));
		}
	}

	public void SpawnTowardsTargetWithoutY()
	{
		if (!Data)
		{
			Data = RootObject.GetComponent<Unit>().data;
		}
		if ((bool)Data.targetData)
		{
			StartCoroutine(SpawnUnit(rotation: Quaternion.LookRotation(new Vector3(Data.targetData.mainRig.position.x - Data.mainRig.position.x, 0f, Data.targetData.mainRig.position.z - Data.mainRig.position.z).normalized + Random.insideUnitSphere * spread * 0.01f), position: Vector3.Lerp(Data.targetData.mainRig.position, transform.position, percentage)));
		}
	}

	public void SpawnCharacterForward()
	{
		StartCoroutine(SpawnUnit(transform.position, Quaternion.LookRotation(transform.root.GetComponent<Unit>().data.mainRig.transform.forward + Random.insideUnitSphere * spread * 0.01f)));
	}

	public void Spawn(Vector3 position, Vector3 direction)
	{
		StartCoroutine(SpawnUnit(position, Quaternion.LookRotation(direction + Random.insideUnitSphere * spread * 0.01f)));
	}

	private IEnumerator SpawnUnit(Vector3 position, Quaternion rotation)
	{
		if ((bool)TargetChecker)
		{
			Targets = new List<Unit>();
			Targets = TargetChecker.GetTargets(maxRange);
		}
		if (waitBeforeFirstSpawn)
		{
			SpawnedSinceDelay = AllowedToSpawn;
		}
		var numberSpawned = 0;
		while (numberSpawned < numberToSpawn)
		{
			InitializeSpawn();
			if (!IsAllowedToSpawnInMultiplayer())
			{
				yield return null;
			}
			if ((bool)Data && Data.Dead && !spawnIfUnitDead)
			{
				yield return null;
			}
			if (useAlternatingSpawnPos)
			{
				AltSpawnPos = new List<AlternatingSpawnPos>();
				AltSpawnPos.AddRange(transform.parent.GetComponentsInChildren<AlternatingSpawnPos>());
			}
			if (SpawnedSinceDelay < AllowedToSpawn)
			{
				if (useAlternatingSpawnPos && AltSpawnPos.Count > 0)
				{
					if (CurrentSpawnPosNumber > AltSpawnPos.Count - 1)
					{
						CurrentSpawnPosNumber = 0;
					}
					position = AltSpawnPos[CurrentSpawnPosNumber].transform.position;
					if (numberToSpawn - numberSpawned <= AltSpawnPos.Count && AltSpawnPos[CurrentSpawnPosNumber].useLastSpawnEvent)
					{
						AltSpawnPos[CurrentSpawnPosNumber].InvokeLastSpawnEvent();
					}
					else
					{
						AltSpawnPos[CurrentSpawnPosNumber].InvokeSpawnEvent();
					}
				}
				var gameObject = ((!IsProjectile || !(ProjectilesSpawnManager != null)) ? Instantiate(objectToSpawn, position, rotation) : ProjectilesSpawnManager.SpawnProjectile(objectToSpawn, position, rotation));
				TeamHolder.AddTeamHolder(gameObject, transform.gameObject);
				var component = gameObject.GetComponent<TeamHolder>();
				if (giveSpawnerWeapon)
				{
					component.spawnerWeapon = transform.GetComponentInParent<Weapon>().gameObject;
				}
				SpawnedSinceDelay += 1f;
				if (parentToMe)
				{
					gameObject.transform.SetParent(transform, worldPositionStays: true);
				}
				if (followMe)
				{
					var followTransform = gameObject.gameObject.AddComponent<FollowTransform>();
					followTransform.target = transform;
					followTransform.destroyOnTargetNull = false;
				}
				if (useRootSizeOfSpawner)
				{
					gameObject.transform.localScale = transform.root.localScale;
				}
				var component2 = gameObject.GetComponent<SpellTarget>();
				if ((bool)TargetChecker && Targets.Count > 0)
				{
					if (numberSpawned < Targets.Count)
					{
						TargetPlace = numberSpawned;
					}
					else
					{
						TargetPlace = numberSpawned - Targets.Count * Mathf.FloorToInt(numberSpawned / Targets.Count);
					}
				}
				if ((bool)component2)
				{
					if ((bool)TargetChecker && giveTarget && TargetPlace < Targets.Count)
					{
						CurrentTarget = Targets[TargetPlace];
						var componentInChildren = CurrentTarget.GetComponentInChildren<DataHandler>();
						var position2 = transform.position;
						var position3 = componentInChildren.mainRig.position;
						var mainRig = componentInChildren.mainRig;
						component2.DoEffect(position2, position3, mainRig);
					}
					else
					{
						component2.GetTarget();
					}
				}
				var component3 = gameObject.GetComponent<TeslaCannon>();
				if ((bool)component3 && (bool)spawnerProjecile)
				{
					component3.maxTargetChecker = spawnerProjecile.transform.GetComponent<TeslaCannon>().maxTargetChecker;
					if ((bool)TargetChecker && giveTarget && (bool)component3.maxTargetChecker && component3.maxTargetChecker.CheckIfAllowedToHit())
					{
						var component4 = transform.root.GetComponent<Unit>();
						if (Targets.Count > 0)
						{
							if (!allowRootTarget && (bool)component4 && Targets[TargetPlace] == component4)
							{
								Targets.Remove(Targets[TargetPlace]);
							}
							CurrentTarget = Targets[TargetPlace];
							var componentInChildren2 = CurrentTarget.GetComponentInChildren<DataHandler>();
							var component5 = GetComponent<TeamHolder>();
							component.team = component5.team;
							component.spawner = component5.spawner;
							component3.PlayEffect(componentInChildren2.mainRig.transform, transform, component5.spawner);
						}
						else
						{
							Destroy(this.gameObject);
						}
					}
					else
					{
						Destroy(this.gameObject);
					}
				}
				numberSpawned++;
				CurrentSpawnPosNumber++;
				spawnEvent?.Invoke();
			}
			else
			{
				SpawnedSinceDelay = 0f;
				var seconds = ((!useRandom) ? timeBetweenSpawns : Random.Range(timeBetweenSpawns * minRandom, timeBetweenSpawns * maxRandom));
				yield return new WaitForSeconds(seconds);
			}
		}
		TargetPlace = 0;
	}

	private void InitializeSpawn()
	{
		if (!DidInitialize)
		{
			DidInitialize = true;
			if (objectToSpawn != null && objectToSpawn.GetComponent<Projectile>() != null)
			{
				IsProjectile = true;
				ProjectilesSpawnManager = ServiceLocator.GetService<ProjectilesSpawnManager>();
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
		if (!IsUnit && !IsProjectile)
		{
			return true;
		}
		return !IsRemotelyControlled;
	}

	private void InitializeForMultiplayer()
	{
		if (!DidInitializeForMultiplayer)
		{
			DidInitializeForMultiplayer = true;
			if (BoltNetwork.IsRunning && objectToSpawn != null)
			{
				IsUnit = objectToSpawn.GetComponent<Unit>() != null;
			}
		}
	}
}
