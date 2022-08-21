using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Bolt;
using TFBGames;
using UnityEngine;
using Landfall.TABS;

namespace HiddenUnits {
	
	public class WerewolfSpawner : MonoBehaviour {
		
		public bool spawnOnStart = true;

		public UnitBlueprint unitBlueprint;

		public int amountToSpawn = 1;

		public float spawnDelay;

		public Team team;

		public bool useUnitTeam = true;

		public float unitInvincibleFor;

		public bool transferHp;

		private DataHandler rootData;

		public Unit rootUnit;

		private float originalSize = 1f;

		private float originalMass = 1f;

		private float sizeMultiplier = 1f;

		private float massMultiplier = 1f;

		private bool originalHoldingWithTwoHands = true;

		private float originalanimationMultiplier = 1f;

		private float originalStepMultiplier = 1f;

		private float originalBalanceMultiplier = 1f;

		private float originalBalanceForceMultiplier = 1f;

		public bool addRemoveAfterSecons;

		public bool destroyroot;

		public float time = 1f;

		public float eventtimer;

		public GameObject poofEffect;

		private RiderHolder riders;

		private TeamHolder teamHolder;

		private EffectSpawnedUnit effectSpawnedUnit;

		public UnitBlueprint rangedUnitBlueprint;

		public bool switchIfRanged;

		public bool useHealthOfRootForAmount;

		public bool spawnWithRootScaleAndMass;

		public bool useRootWeapon;

		public bool useSpawnerBlueprint;

		public bool spawnRandomUnits;

		public List<UnitBlueprint> units = new List<UnitBlueprint>();

		private Vector3 spawnPos;

		private bool isHumanoid = true;

		private bool isRangedUnit;

		private int allowedUnitsToSpawn = 1;

		private int unitsSpawnedSinceWait;

		private GameObject originalRightWeapon;

		private GameObject originalLeftWeapon;

		private IceGiant isIceGiant;

		public Action<GameObject> spawnUnitAction;

		private SpawnerBlueprintHolder blueprintHolder;

		private bool didInitializeForMultiplayer;

		private Projectile parentProjectile;

		private Unit parentUnit;

		public bool spawnRiders;

		private void Start()
		{
			teamHolder = GetComponentInParent<TeamHolder>();
			effectSpawnedUnit = GetComponentInParent<EffectSpawnedUnit>();
			rootData = base.transform.root.GetComponentInChildren<DataHandler>();
			rootUnit = base.transform.root.GetComponent<Unit>();
			isIceGiant = base.transform.root.GetComponent<IceGiant>();
			blueprintHolder = base.transform.root.GetComponentInChildren<SpawnerBlueprintHolder>();
			if ((bool)rootUnit && rootUnit.RigType != Stitcher.TransformCatalog.RigType.Human && rootUnit.RigType != Stitcher.TransformCatalog.RigType.Halfling && rootUnit.RigType != Stitcher.TransformCatalog.RigType.Minotaur)
			{
				isHumanoid = false;
			}
			if (spawnRandomUnits && units.Count > 0)
			{
				unitBlueprint = units[UnityEngine.Random.Range(0, units.Count)];
			}
			if (useHealthOfRootForAmount && !isHumanoid)
			{
				amountToSpawn += Mathf.RoundToInt(rootData.maxHealth / 500f);
			}
			if (spawnOnStart)
			{
				Spawnerino();
			}
		}

		public void Spawnerino()
		{
			StartCoroutine(Spawnfixer());
		}

		private IEnumerator Spawnfixer()
		{
			int amountspawned = 0;
			while (amountspawned < amountToSpawn)
			{
				if (unitsSpawnedSinceWait < allowedUnitsToSpawn)
				{
					if (amountspawned > 1)
					{
						spawnPos = new Vector3(base.transform.position.x + UnityEngine.Random.Range(-2.5f, 2.5f), base.transform.position.y + UnityEngine.Random.Range(-0.5f, 0.5f), base.transform.position.z + UnityEngine.Random.Range(-2.5f, 2.5f));
					}
					else
					{
						spawnPos = base.transform.position;
					}
					Spawn();
					unitsSpawnedSinceWait++;
					amountspawned++;
				}
				else
				{
					unitsSpawnedSinceWait = 0;
					yield return new WaitForSeconds(spawnDelay);
				}
			}
		}

		public Unit Spawn()
		{
			if (!IsAllowedToSpawnInMultiplayer())
			{
				return null;
			}
			if (switchIfRanged && isHumanoid)
			{
				if ((bool)rootData.weaponHandler.rightWeapon && !isRangedUnit && rootData.weaponHandler.rightWeapon.isRange)
				{
					isRangedUnit = true;
				}
				if ((bool)rootData.weaponHandler.leftWeapon && !isRangedUnit && rootData.weaponHandler.leftWeapon.isRange)
				{
					isRangedUnit = true;
				}
				if (isRangedUnit)
				{
					unitBlueprint = rangedUnitBlueprint;
				}
			}
			if ((bool)teamHolder)
			{
				team = teamHolder.team;
			}
			if (useUnitTeam && (bool)rootUnit)
			{
				team = rootUnit.Team;
			}
			if ((bool)isIceGiant)
			{
				sizeMultiplier = base.transform.root.localScale.x * 3f;
				massMultiplier = 1.5f;
			}
			else
			{
				sizeMultiplier = base.transform.root.localScale.x;
			}
			if ((bool)blueprintHolder && useSpawnerBlueprint)
			{
				unitBlueprint = blueprintHolder.unitBlueprint;
			}
			Unit unitToSpawn;
			if (spawnWithRootScaleAndMass && isHumanoid)
			{
				originalSize = unitBlueprint.sizeMultiplier;
				originalMass = unitBlueprint.massMultiplier;
				originalanimationMultiplier = unitBlueprint.animationMultiplier;
				originalStepMultiplier = unitBlueprint.stepMultiplier;
				originalBalanceMultiplier = unitBlueprint.balanceMultiplier;
				originalBalanceForceMultiplier = unitBlueprint.balanceForceMultiplier;
				unitBlueprint.sizeMultiplier = sizeMultiplier;
				unitBlueprint.massMultiplier = rootUnit.unitBlueprint.massMultiplier * massMultiplier;
				unitBlueprint.animationMultiplier = rootUnit.unitBlueprint.animationMultiplier;
				unitBlueprint.stepMultiplier = rootUnit.unitBlueprint.stepMultiplier;
				unitBlueprint.balanceMultiplier = rootUnit.unitBlueprint.balanceMultiplier;
				unitBlueprint.balanceForceMultiplier = rootUnit.unitBlueprint.balanceForceMultiplier;
				if (useRootWeapon)
				{
					originalRightWeapon = unitBlueprint.RightWeapon;
					originalLeftWeapon = unitBlueprint.LeftWeapon;
					originalHoldingWithTwoHands = unitBlueprint.holdinigWithTwoHands;
					if ((bool)rootData.weaponHandler.rightWeapon)
					{
						unitBlueprint.RightWeapon = rootUnit.unitBlueprint.RightWeapon;
					}
					else
					{
						unitBlueprint.RightWeapon = null;
					}
					if ((bool)rootData.weaponHandler.leftWeapon)
					{
						unitBlueprint.LeftWeapon = rootUnit.unitBlueprint.LeftWeapon;
					}
					else
					{
						unitBlueprint.LeftWeapon = null;
					}
					unitBlueprint.holdinigWithTwoHands = rootUnit.unitBlueprint.holdinigWithTwoHands;
				}
				unitBlueprint.Spawn(spawnPos, base.transform.rotation, team, out unitToSpawn, 1f, false, spawnRiders);
				unitBlueprint.sizeMultiplier = originalSize;
				unitBlueprint.massMultiplier = originalMass;
				unitBlueprint.animationMultiplier = originalanimationMultiplier;
				unitBlueprint.stepMultiplier = originalStepMultiplier;
				unitBlueprint.balanceMultiplier = originalBalanceMultiplier;
				unitBlueprint.balanceForceMultiplier = originalBalanceForceMultiplier;
				if (useRootWeapon)
				{
					if ((bool)originalRightWeapon)
					{
						unitBlueprint.RightWeapon = originalRightWeapon;
					}
					else
					{
						unitBlueprint.RightWeapon = null;
					}
					if ((bool)originalLeftWeapon)
					{
						unitBlueprint.LeftWeapon = originalLeftWeapon;
					}
					else
					{
						unitBlueprint.LeftWeapon = null;
					}
					unitBlueprint.holdinigWithTwoHands = originalHoldingWithTwoHands;
				}
			}
			else
			{
				unitBlueprint.Spawn(spawnPos, base.transform.rotation, team, out unitToSpawn, 1f, false, spawnRiders);
			}
			if (addRemoveAfterSecons)
			{
				KillAfterSeconds killAfterSeconds = unitToSpawn.gameObject.AddComponent<KillAfterSeconds>();
				killAfterSeconds.seconds = time;
				killAfterSeconds.destroyRoot = destroyroot;
				if (eventtimer != 0f)
				{
					killAfterSeconds.eventTimer = eventtimer;
					SpawnObject spawnObject = unitToSpawn.transform.GetComponentInChildren<Hip>().gameObject.AddComponent<SpawnObject>();
					spawnObject.spawnIfUnitDead = false;
					spawnObject.followMe = true;
					if ((bool)poofEffect)
					{
						spawnObject.objectToSpawn = poofEffect;
					}
					killAfterSeconds.killEvent.AddListener(spawnObject.SpawnCharacterForward);
				}
				riders = unitToSpawn.GetComponent<RiderHolder>();
				if ((bool)riders)
				{
					foreach (GameObject rider in riders.riders)
					{
						killAfterSeconds = rider.AddComponent<KillAfterSeconds>();
						killAfterSeconds.seconds = time;
						killAfterSeconds.destroyRoot = destroyroot;
					}
				}
			}
			PlacementSpawnEffects componentInChildren = unitToSpawn.GetComponentInChildren<PlacementSpawnEffects>();
			if ((bool)componentInChildren)
			{
				UnityEngine.Object.Destroy(componentInChildren.gameObject);
			}
			if ((bool)effectSpawnedUnit)
			{
				effectSpawnedUnit.Spawn(unitToSpawn.gameObject);
			}
			spawnUnitAction?.Invoke(unitToSpawn.gameObject);
			if (transferHp && (bool)unitToSpawn && (bool)rootData)
			{
				float healthDiff = rootData.health / rootData.maxHealth;
				unitToSpawn.gameObject.AddComponent<HealthTransfer>().TransferHealth(unitToSpawn, healthDiff);
			}
			if (unitInvincibleFor != 0f)
			{
				UnitDontTakeDamageForSeconds unitDontTakeDamageForSeconds = unitToSpawn.gameObject.AddComponent<UnitDontTakeDamageForSeconds>();
				unitDontTakeDamageForSeconds.seconds = unitInvincibleFor;
				unitDontTakeDamageForSeconds.playOnStart = true;
			}
			SpawnerBlueprintHolder componentInChildren2 = unitToSpawn.transform.root.GetComponentInChildren<SpawnerBlueprintHolder>();
			if ((bool)rootUnit && (bool)componentInChildren2)
			{
				componentInChildren2.unitBlueprint = rootUnit.unitBlueprint;
			}
			return unitToSpawn;
		}

		private bool IsAllowedToSpawnInMultiplayer()
		{
			InitializeForMultiplayer();
			if (parentProjectile != null && parentProjectile.IsRemotelyControlled)
			{
				return false;
			}
			if (parentUnit != null && parentUnit.IsRemotelyControlled)
			{
				return false;
			}
			return true;
		}

		private void InitializeForMultiplayer()
		{
			if (!didInitializeForMultiplayer)
			{
				didInitializeForMultiplayer = true;
				if (BoltNetwork.IsRunning)
				{
					parentProjectile = GetComponentInParent<Projectile>();
					parentUnit = GetComponentInParent<Unit>();
				}
			}
		}
	}
}
