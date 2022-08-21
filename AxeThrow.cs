using Landfall.TABS;
using System.Collections;
using UnityEngine;
using System.Linq;
using Landfall.TABS.AI.Components;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace HiddenUnits
{
    public class AxeThrow : MonoBehaviour
    {
		void Awake() {
			if (GetComponent<RangeWeapon>().ObjectToSpawn) { var dummy = objectToSpawn; objectToSpawn = GetComponent<RangeWeapon>().ObjectToSpawn; GetComponent<RangeWeapon>().ObjectToSpawn = dummy; }
		}

		void Start() {

			hammerEffects = GetComponents<AxeAttackEffect>();
			myLevel = GetComponent<Level>();
			if (GetComponentInChildren<ShootPosition>()) { shootPosition = GetComponentInChildren<ShootPosition>().transform; }
        }

        public void Throw()
        {
            StartCoroutine(DelayedSwing());
        }

        public IEnumerator DelayedSwing()
        {
			var target = GetComponent<Weapon>().connectedData.targetData.unit;
			if (callHammerEffects)
			{
				foreach (var effect in hammerEffects)
				{
					if (target != null)
					{
						effect.DoEffect(target.data.mainRig);
					}
				}
			}

			yield return new WaitForSeconds(spawnDelay);

			if (GetComponent<AxeShowProjectile>()) { GetComponent<AxeShowProjectile>().pivot.gameObject.SetActive(false); }

			spawnedObject = Instantiate(objectToSpawn, shootPosition.position, shootPosition.rotation);
			spawnedObject.GetComponent<MoveTransform>().velocity = (target.data.mainRig.position - transform.position).normalized * spawnedObject.GetComponent<MoveTransform>().selfImpulse.magnitude;
			SetProjectileStats(spawnedObject, GetSpawnDirection((target.data.mainRig.position - shootPosition.position).normalized, target.data.mainRig, new Vector3(0f, 0f, 0f)), (target.data.mainRig.position - shootPosition.position).normalized, target.data.mainRig, shootPosition.forward, target.data.mainRig.position, target.data.mainRig.velocity);
			
			var team = spawnedObject.AddComponent<TeamHolder>();
			team.spawner = GetComponent<Weapon>().connectedData.unit.gameObject;
			team.spawnerWeapon = gameObject;
			team.team = GetComponent<Weapon>().connectedData.unit.Team;
			team.target = target.data.mainRig;
			foreach (var teamc in spawnedObject.GetComponentsInChildren<TeamColor>())
			{
				teamc.SetTeamColor(team.team);
			}

			yield break;
        }

		public void SetProjectileStats(GameObject spawnedObject, Vector3 spawnDir, Vector3 directionToTarget, Rigidbody targetRig, Vector3 shootPositionForward, Vector3 targetRigPosition, Vector3 targetRigVelocity)
		{
			Transform[] componentsInChildren = spawnedObject.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (1f != 0f)
				{
					Rigidbody component = componentsInChildren[i].GetComponent<Rigidbody>();
					if (component)
					{
						component.AddForce(spawnDir * 1f, ForceMode.VelocityChange);
						if (myLevel)
						{
							component.mass *= Mathf.Pow((float)myLevel.level, 1.5f);
						}
					}
				}
				Compensation componentInChildren = componentsInChildren[i].GetComponentInChildren<Compensation>();
				if (componentInChildren && targetRig)
				{
					componentInChildren.transform.rotation = Quaternion.LookRotation(componentInChildren.GetCompensation(targetRigPosition, targetRigVelocity, shootHelpAngleCurve.Evaluate(Vector3.Angle(directionToTarget, shootPositionForward))) + UnityEngine.Random.insideUnitSphere * 1f * 0.01f);
				}
				MoveTransform componentInChildren2 = spawnedObject.GetComponentInChildren<MoveTransform>();
				if (componentInChildren2)
				{
					if (componentInChildren && componentInChildren.forwardCompensation > 0f && targetRig)
					{
						MoveTransform moveTransform = componentInChildren2;
						moveTransform.selfImpulse.z = moveTransform.selfImpulse.z + Mathf.Pow(Mathf.Clamp(Vector3.Distance(targetRigPosition, transform.position), 0f, componentInChildren.clampDistance), componentInChildren.rangePow) * componentInChildren.forwardCompensation;
					}
				}
				AddForce component2 = spawnedObject.GetComponent<AddForce>();
				if (component2 && componentInChildren && componentInChildren.forwardCompensation > 0f && targetRig)
				{
					AddForce addForce = component2;
					addForce.force.z = addForce.force.z + Mathf.Pow(Mathf.Clamp(Vector3.Distance(targetRigPosition, transform.position), 0f, componentInChildren.clampDistance), componentInChildren.rangePow) * componentInChildren.forwardCompensation;
				}
				ProjectileHit componentInChildren3 = componentsInChildren[i].GetComponentInChildren<ProjectileHit>();
				if (componentInChildren3)
				{
					componentInChildren3.damage *= levelMultiplier;
					if (GetComponent<Weapon>().connectedData && GetComponent<Weapon>().connectedData.input.hasControl)
					{
						componentInChildren3.alwaysHitTeamMates = true;
					}
					if (myLevel)
					{
						componentInChildren3.ignoreTeamMates = myLevel.ignoreTeam;
					}
				}
				CollisionWeapon componentInChildren4 = componentsInChildren[i].GetComponentInChildren<CollisionWeapon>();
				if (componentInChildren4)
				{
					componentInChildren4.damage *= levelMultiplier;
				}
				if (1f != 0f)
				{
					if (componentInChildren2)
					{
						componentInChildren2.selfImpulse *= 1f;
						componentInChildren2.worldImpulse.y = 0f;
					}
					if (componentInChildren3)
					{
						componentInChildren3.damage *= 1f;
						componentInChildren3.force *= 1f;
					}
				}
			}
			Level level = spawnedObject.GetComponent<Level>();
			if (level == null)
			{
				level = spawnedObject.AddComponent<Level>();
			}
			level.levelMultiplier = levelMultiplier;
			if (myLevel)
			{
				level.ignoreTeam = myLevel.ignoreTeam;
				level.level = myLevel.level;
			}
		}

		private Vector3 GetSpawnDirection(Vector3 directionToTarget, Rigidbody targetRig, Vector3 forcedDirection)
		{
			Vector3 result = directionToTarget.normalized;
			result = Vector3.Lerp(directionToTarget, shootPosition.forward, this.shootHelpAngleCurve.Evaluate(Vector3.Angle(directionToTarget, shootPosition.forward))).normalized;
			if (GetComponent<Weapon>().connectedData && GetComponent<Weapon>().connectedData.input.hasControl)
			{
				if (!targetRig)
				{
					result = forcedDirection;
				}
			}
			return result;
		}

		private Transform shootPosition;

		public GameObject objectToSpawn;

        private GameObject spawnedObject;

		private Level myLevel;

        public float spawnDelay = 0.4f;

        public bool callHammerEffects = true;

        private AxeAttackEffect[] hammerEffects;

		private float levelMultiplier = 1f;

		private AnimationCurve shootHelpAngleCurve = new AnimationCurve();
	}
}
