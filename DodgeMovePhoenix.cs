using System.Collections;
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

	private float RandomSeed;

	private DataHandler Data;

	private RigidbodyHolder AllRigs;

	private CameraAbilityPossess Possess;

	public bool IsRemotelyControlled { get; private set; }

	private void Start()
	{
		Data = transform.root.GetComponentInChildren<DataHandler>();
		AllRigs = Data.GetComponent<RigidbodyHolder>();
		for (var i = 0; i < moves.Length; i++)
		{
			if (moves[i].forceDirection == CombatMoveDataInstance.ForceDirection.RotateTowardsPossCamElseTarget)
			{
				Possess = MainCam.instance.GetComponentInParent<CameraAbilityPossess>();
			}
		}
		if (randomForceMultiplier)
		{
			RandomSeed = Random.Range(-1, 1);
			forceMultiplier = Random.Range(forceMultiplier * 0.7f, forceMultiplier);
			if (RandomSeed < 0f)
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
		if (!Data)
		{
			Data = transform.root.GetComponentInChildren<DataHandler>();
			AllRigs = Data.GetComponent<RigidbodyHolder>();
		}
		DoMove(null, Data.targetMainRig, Data.targetData);
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
		for (var i = 0; i < moves.Length; i++)
		{
			var move = moves[i];
			if (IsAllowedToDoMoveInMultiplayer(move))
			{
				StartCoroutine(DoMoveSequence(move, enemyWeapon, enemyTorso, targetData));
			}
		}
	}

	private IEnumerator DoMoveSequence(CombatMoveDataInstance move, Rigidbody enemyWeapon, Rigidbody enemyTorso, DataHandler targetData)
	{
		var t = move.forceCurve.keys[move.forceCurve.keys.Length - 1].time;
		var c = 0f;
		if (move.useAlternateForceProjectMarsClient && BoltNetwork.IsClient)
		{
			move.force = move.alternateClientForce;
		}
		move.randomMultiplier = move.randomCurve.Evaluate(Random.value);
		var rigs = new List<Rigidbody>();
		if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.Head)
		{
			rigs.Add(Data.head.GetComponent<Rigidbody>());
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.Torso)
		{
			if ((bool)Data.torso)
			{
				rigs.Add(Data.torso.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.Hip)
		{
			if ((bool)Data.hip)
			{
				rigs.Add(Data.hip.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.FootLeft)
		{
			if ((bool)Data.footLeft)
			{
				rigs.Add(Data.footLeft.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.FootRight)
		{
			if ((bool)Data.footRight)
			{
				rigs.Add(Data.footRight.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.HandRight)
		{
			if ((bool)Data.rightHand)
			{
				rigs.Add(Data.rightHand.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.HandLeft)
		{
			if ((bool)Data.leftHand)
			{
				rigs.Add(Data.leftHand.GetComponent<Rigidbody>());
			}
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.AllRigs)
		{
			rigs.AddRange(AllRigs.AllRigs);
		}
		else if (move.rigidbodyToMove == CombatMoveDataInstance.RigidBodyToMove.MainWeapon)
		{
			var weaponHandler = Data.weaponHandler;
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
			var weaponHandler2 = Data.weaponHandler;
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
		var forceDirection = Vector3.zero;
		if (rigs.Count >= 1)
		{
			forceDirection = GetDirection(move, enemyWeapon, enemyTorso, rigs[0], targetData);
			for (var num = rigs.Count - 1; num >= 0; num--)
			{
				if (rigs[num] == null)
				{
					rigs.RemoveAt(num);
				}
			}
		}
		var massM = 1f;
		if (divideForceByMass)
		{
			var num2 = 0f;
			for (var i = 0; i < rigs.Count; i++)
			{
				num2 += rigs[i].mass / (float)rigs.Count;
			}
			massM = 1f / num2;
		}
		while (c < t)
		{
			if (CheckConditions())
			{
				for (var j = 0; j < rigs.Count; j++)
				{
					if (move.setDirectionContiniouiouss)
					{
						forceDirection = GetDirection(move, enemyWeapon, enemyTorso, rigs[0], targetData);
					}
					if (!usedAsMovement || !Data || !Data.GetComponent<AnimationHandler>() || Data.GetComponent<AnimationHandler>().currentState != 0)
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
			var num3 = 1f;
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
		var result = true;
		if (maxRange != 0f && (bool)Data && Data.distanceToTarget > maxRange)
		{
			result = false;
		}
		if (minRange != 0f && (bool)Data && Data.distanceToTarget < minRange)
		{
			result = false;
		}
		return result;
	}

	private Vector3 GetDirection(CombatMoveDataInstance move, Rigidbody enemyWeapon, Rigidbody enemyTorso, Rigidbody ownRig, DataHandler targetData)
	{
		if (!enemyTorso)
		{
			enemyTorso = Data.targetData.mainRig;
		}
		var result = Vector3.zero;
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
				if (Data.cantFallForSeconds < 0.5f)
				{
					Data.cantFallForSeconds = 0.5f;
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
			result = Data.characterForwardObject.forward;
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.CharacterRight)
		{
			result = Data.characterForwardObject.right;
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.CrossUpAndAwayFromAttacker && (bool)ownRig && (bool)enemyTorso)
		{
			result = Vector3.Cross(Vector3.up, ownRig.position - enemyTorso.position);
			if (move.normalize)
			{
				result = result.normalized;
			}
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.CrossUpAndTowardsUnitTarget && (bool)ownRig && (bool)Data.targetMainRig)
		{
			result = Vector3.Cross(Vector3.up, ownRig.position - Data.targetMainRig.position);
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
			result = ((!Possess || !Possess.currentUnit || !Data || !Data.unit || !(Possess.currentUnit == Data.unit)) ? (-Vector3.Cross(enemyTorso.position - ownRig.position, ownRig.transform.forward).normalized * Vector3.Angle(enemyTorso.position - ownRig.position, ownRig.transform.forward)) : (-Vector3.Cross(MainCam.instance.transform.forward, ownRig.transform.forward).normalized * Vector3.Angle(MainCam.instance.transform.forward, ownRig.transform.forward)));
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
			result = Data.groundedMovementDirectionObject.forward;
		}
		else if (move.forceDirection == CombatMoveDataInstance.ForceDirection.RotateTowardsWalkDirection)
		{
			result = Vector3.Cross(ownRig.transform.forward, Data.groundedMovementDirectionObject.forward).normalized * Vector3.Angle(ownRig.transform.forward, Data.groundedMovementDirectionObject.forward);
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
