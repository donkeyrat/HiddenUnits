﻿using System.Collections;
using System.Collections.Generic;
using Photon.Bolt;
using TFBGames;
using UnityEngine;

public class DodgeMovePhoenix : Move, IRemotelyControllable
{
	[HideInInspector]
	public Transform targetObject;

	public float animationSpeed = 1f;

	public float animationSpeedWhenPositiveCurve = 1f;

	public float forceMultiplier = 1f;

	public bool divideForceByMass;

	public bool cancelSelf;

	public bool usedAsMovement;

	public bool randomForceMultiplier;

	public float minRange;

	public float maxRange;

	public CombatMoveDataInstance[] moves;

	[SerializeField]
	[Tooltip("See the comments in code.")]
	protected bool allowForMartian;

	private float randomSeed;

	private DataHandler data;

	private RigidbodyHolder allRigs;

	private CameraAbilityPossess possess;

	public bool IsRemotelyControlled { get; private set; }

	private void Start()
	{
		data = base.transform.root.GetComponentInChildren<DataHandler>();
		allRigs = data.GetComponent<RigidbodyHolder>();
		for (int i = 0; i < moves.Length; i++)
		{
			if (moves[i].forceDirection == CombatMoveDataInstance.ForceDirection.RotateTowardsPossCamElseTarget)
			{
				possess = MainCam.instance.GetComponentInParent<CameraAbilityPossess>();
			}
		}
		if (randomForceMultiplier)
		{
			randomSeed = Random.Range(-1, 1);
			forceMultiplier = Random.Range(forceMultiplier * 0.7f, forceMultiplier);
			if (randomSeed < 0f)
			{
				forceMultiplier = 0f - forceMultiplier;
			}
		}
	}

	public void DoMove(Transform targetObj)
	{
		targetObject = targetObj;
		DoMove();
	}

	public void DoMove()
	{
		if (!data)
		{
			data = base.transform.root.GetComponentInChildren<DataHandler>();
			allRigs = data.GetComponent<RigidbodyHolder>();
		}
		DoMove(null, data.targetMainRig, data.targetData);
	}

	public override void DoMove(Rigidbody enemyWeapon, Rigidbody enemyTorso, DataHandler targetData)
	{
		if (cancelSelf)
		{
			StopAllCoroutines();
		}
		if (!enemyWeapon && !enemyTorso)
		{
			return;
		}
		for (int i = 0; i < moves.Length; i++)
		{
			CombatMoveDataInstance move = moves[i];
			if (IsAllowedToDoMoveInMultiplayer(move))
			{
				StartCoroutine(DoMoveSequence(move, enemyWeapon, enemyTorso, targetData));
			}
		}
	}

	private IEnumerator DoMoveSequence(CombatMoveDataInstance move, Rigidbody enemyWeapon, Rigidbody enemyTorso, DataHandler targetData)
	{
		float t = move.forceCurve.keys[move.forceCurve.keys.Length - 1].time;
		float c = 0f;
		if (move.useAlternateForceProjectMarsClient && BoltNetwork.IsClient)
		{
			move.force = move.alternateClientForce;
		}
		move.randomMultiplier = move.randomCurve.Evaluate(Random.value);
		List<Rigidbody> rigs = new List<Rigidbody>();
		if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.Head)
		{
			rigs.Add(data.head.GetComponent<Rigidbody>());
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.Torso)
		{
			if ((bool)data.torso)
			{
				rigs.Add(data.torso.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.Hip)
		{
			if ((bool)data.hip)
			{
				rigs.Add(data.hip.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.FootLeft)
		{
			if ((bool)data.footLeft)
			{
				rigs.Add(data.footLeft.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.FootRight)
		{
			if ((bool)data.footRight)
			{
				rigs.Add(data.footRight.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.HandRight)
		{
			if ((bool)data.rightHand)
			{
				rigs.Add(data.rightHand.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.HandLeft)
		{
			if ((bool)data.leftHand)
			{
				rigs.Add(data.leftHand.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.AllRigs)
		{
			rigs.AddRange(allRigs.AllRigs);
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.MainWeapon)
		{
			WeaponHandler weaponHandler = data.weaponHandler;
			if ((bool)weaponHandler)
			{
				if ((bool)weaponHandler.rightWeapon && (bool)weaponHandler.rightWeapon.rigidbody)
				{
					rigs.Add(weaponHandler.rightWeapon.rigidbody);
				}
				else if ((bool)weaponHandler.leftWeapon && (bool)weaponHandler.leftWeapon.rigidbody)
				{
					rigs.Add(weaponHandler.leftWeapon.rigidbody);
				}
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.This)
		{
			rigs.Add(GetComponent<Rigidbody>());
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.Specific && (bool)move.specificRig)
		{
			rigs.Add(move.specificRig);
		}
		if (move.includeWeapons)
		{
			WeaponHandler weaponHandler2 = data.weaponHandler;
			if ((bool)weaponHandler2)
			{
				if ((bool)weaponHandler2.leftWeapon && (bool)weaponHandler2.leftWeapon.rigidbody)
				{
					rigs.Add(weaponHandler2.leftWeapon.rigidbody);
				}
				if ((bool)weaponHandler2.rightWeapon && (bool)weaponHandler2.rightWeapon.rigidbody)
				{
					rigs.Add(weaponHandler2.rightWeapon.rigidbody);
				}
			}
		}
		Vector3 forceDirection = Vector3.zero;
		if (rigs.Count >= 1)
		{
			forceDirection = GetDirection(move, enemyWeapon, enemyTorso, rigs[0], targetData);
			for (int num = rigs.Count - 1; num >= 0; num--)
			{
				if (rigs[num] == null)
				{
					rigs.RemoveAt(num);
				}
			}
		}
		float massM = 1f;
		if (divideForceByMass)
		{
			float num2 = 0f;
			for (int i = 0; i < rigs.Count; i++)
			{
				num2 += rigs[i].mass / (float)rigs.Count;
			}
			massM = 1f / num2;
		}
		while (c < t)
		{
			if (CheckConditions())
			{
				for (int j = 0; j < rigs.Count; j++)
				{
					if (move.setDirectionContiniouiouss)
					{
						forceDirection = GetDirection(move, enemyWeapon, enemyTorso, rigs[0], targetData);
					}
					if (!usedAsMovement || !data || !data.GetComponent<AnimationHandler>() || data.GetComponent<AnimationHandler>().currentState != 0)
					{
						if (move.force != 0f && (bool)rigs[j])
						{
							rigs[j].AddForce(forceDirection * massM * move.randomMultiplier * forceMultiplier * move.force * move.forceCurve.Evaluate(c), ForceMode.Acceleration);
						}
						if (move.torque != 0f && (bool)rigs[j])
						{
							rigs[j].AddTorque(forceDirection * massM * forceMultiplier * move.torque * move.forceCurve.Evaluate(c), ForceMode.Acceleration);
						}
					}
				}
			}
			float num3 = 1f;
			num3 *= animationSpeed;
			if (move.forceCurve.Evaluate(c) > 0f)
			{
				num3 *= animationSpeedWhenPositiveCurve;
			}
			c += Time.fixedDeltaTime * num3;
			yield return new WaitForFixedUpdate();
		}
	}

	private bool CheckConditions()
	{
		bool result = true;
		if (maxRange != 0f && (bool)data && data.distanceToTarget > maxRange)
		{
			result = false;
		}
		if (minRange != 0f && (bool)data && data.distanceToTarget < minRange)
		{
			result = false;
		}
		return result;
	}

	private Vector3 GetDirection(CombatMoveDataInstance move, Rigidbody enemyWeapon, Rigidbody enemyTorso, Rigidbody ownRig, DataHandler targetData)
	{
		if (!enemyTorso)
		{
			enemyTorso = data.targetData.mainRig;
		}
		Vector3 result = Vector3.zero;
		if (ownRig == null)
		{
			return result;
		}
		if (move.forceDirection == CombatMoveDataInstance.ForceDirection.Up)
		{
			result = Vector3.up;
		}
		if (move.forceDirection == CombatMoveDataInstance.ForceDirection.TorwardTarget && (bool)ownRig && (bool)enemyTorso)
		{
			result = enemyTorso.position - ownRig.position;
			if (move.normalize)
			{
				result = result.normalized;
			}
		}
		if (move.forceDirection == CombatMoveDataInstance.ForceDirection.TowardsTargetHead && (bool)ownRig && (bool)targetData && (bool)targetData.head)
		{
			result = targetData.head.position + targetData.head.transform.forward * 0.1f + targetData.head.transform.up * 0.15f - ownRig.position;
			if (move.normalize)
			{
				result = result.normalized;
			}
		}
		if (move.forceDirection == CombatMoveDataInstance.ForceDirection.AwayFromTargetWeapon)
		{
			if ((bool)enemyWeapon)
			{
				result = ownRig.position - (enemyWeapon.worldCenterOfMass + enemyWeapon.velocity * move.predictionAmount);
				if (move.normalize)
				{
					result = result.normalized;
				}
				if (move.ignoreY)
				{
					result = new Vector3(result.x, 0f, result.y);
				}
				if (data.cantFallForSeconds < 0.5f)
				{
					data.cantFallForSeconds = 0.5f;
				}
			}
			else if ((bool)enemyTorso)
			{
				result = -(enemyTorso.position - ownRig.position);
				if (move.normalize)
				{
					result = result.normalized;
				}
			}
		}
		if (move.forceDirection == CombatMoveDataInstance.ForceDirection.CharacterForward)
		{
			result = data.characterForwardObject.forward;
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.CharacterRight)
		{
			result = data.characterForwardObject.right;
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.CrossUpAndAwayFromAttacker && (bool)ownRig && (bool)enemyTorso)
		{
			result = Vector3.Cross(Vector3.up, ownRig.position - enemyTorso.position);
			if (move.normalize)
			{
				result = result.normalized;
			}
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.CrossUpAndTowardsUnitTarget && (bool)ownRig && (bool)data.targetMainRig)
		{
			result = Vector3.Cross(Vector3.up, ownRig.position - data.targetMainRig.position);
			if (move.normalize)
			{
				result = result.normalized;
			}
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.RigUp && (bool)ownRig)
		{
			result = ownRig.transform.up;
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.RotateTowardsPossCamElseTarget && (bool)enemyTorso && (bool)ownRig)
		{
			result = ((!possess || !possess.currentUnit || !data || !data.unit || !(possess.currentUnit == data.unit)) ? (-Vector3.Cross(enemyTorso.position - ownRig.position, ownRig.transform.forward).normalized * Vector3.Angle(enemyTorso.position - ownRig.position, ownRig.transform.forward)) : (-Vector3.Cross(MainCam.instance.transform.forward, ownRig.transform.forward).normalized * Vector3.Angle(MainCam.instance.transform.forward, ownRig.transform.forward)));
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.RotateTowardsTarget && (bool)enemyTorso && (bool)ownRig)
		{
			result = -Vector3.Cross(enemyTorso.position - ownRig.position, ownRig.transform.forward).normalized * Vector3.Angle(enemyTorso.position - ownRig.position, ownRig.transform.forward);
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.RotateTowardsTargetHead && (bool)targetData && (bool)targetData.head && (bool)ownRig)
		{
			result = -Vector3.Cross(targetData.head.position + targetData.head.transform.forward * 0.1f + targetData.head.transform.up * 0.15f - ownRig.position, ownRig.transform.forward).normalized * Vector3.Angle(targetData.head.position + targetData.head.transform.forward * 0.1f + targetData.head.transform.up * 0.15f - ownRig.position, ownRig.transform.forward);
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.AwayFromTargetObject && (bool)targetObject && (bool)ownRig)
		{
			result = ownRig.transform.position - targetObject.transform.position;
			if (move.normalize)
			{
				result = result.normalized;
			}
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.CrossUpAndAwayFromTargetObject && (bool)targetObject && (bool)ownRig)
		{
			result = Vector3.Cross(Vector3.up, ownRig.position - targetObject.position);
			if (move.normalize)
			{
				result = result.normalized;
			}
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.InWalkDirection)
		{
			result = data.groundedMovementDirectionObject.forward;
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.RotateTowardsWalkDirection)
		{
			result = Vector3.Cross(ownRig.transform.forward, data.groundedMovementDirectionObject.forward).normalized * Vector3.Angle(ownRig.transform.forward, data.groundedMovementDirectionObject.forward);
		}
		else if (move.randomizeDirection && Random.value > 0.5f)
		{
			result *= -1f;
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.TowardTargetWithoutY && (bool)ownRig && (bool)enemyTorso)
		{
			result = new Vector3(enemyTorso.position.x - ownRig.position.x, 0f, enemyTorso.position.z - ownRig.position.z);
		}
		return result;
	}

	public void SetIsRemotelyControlled(bool isRemotelyControlled)
	{
		IsRemotelyControlled = isRemotelyControlled;
	}

	private bool IsAllowedToDoMoveInMultiplayer(CombatMoveDataInstance move)
	{
		if (!allowForMartian && IsRemotelyControlled)
		{
			return move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.MainWeapon;
		}
		return true;
	}
}
