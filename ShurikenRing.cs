using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace HiddenUnits;

public class ShurikenRing : MonoBehaviour
{
	private void Awake()
	{
		SwordPoints = GetComponentsInChildren<ShootPosition>();
	}

	private void Start()
	{
		Data = transform.root.GetComponentInChildren<DataHandler>();
		var rendererList = new List<Renderer>();
		foreach (var point in SwordPoints)
		{
			var spookySword = CreateNewSword(point.transform.position + Vector3.up * 2f, point.transform.rotation, point);
			var renderers = spookySword.obj.GetComponentsInChildren<Renderer>();
			
			if (renderers != null && renderers.Length != 0)
			{
				rendererList.AddRange(renderers);
			}
			
			Shurikens.Add(spookySword);
		}
		if (Data != null && Data.unit != null)
		{
			Data.unit.AddRenderersToShowHide(rendererList.ToArray(), Data.unit.IsSpawnedInBlindPlacement);
		}
	}

	private void Update()
	{
		if (Done)
		{
			return;
		}
		if (Data && Data.Dead)
		{
			Done = true;
			for (var i = 0; i < Shurikens.Count; i++)
			{
				AttackID = i;
				Attack(Data.mainRig, AttackID);
			}
		}
		
		if ((bool)Data.weaponHandler)
		{
			AttackSpeedMulti = Data.weaponHandler.attackSpeedMultiplier;
		}
		var num2 = Data.unit == null || !Data.unit.IsRemotelyControlled;
		Counter += Time.deltaTime * AttackSpeedMulti;
		if (num2 && (bool)Data.targetMainRig && Counter > attackRate)
		{
			for (var i = 0; i < shurikensPerAttack; i++)
			{
				var leastAngle = 999f;
				for (var j = 0; j < SwordPoints.Length; j++)
				{
					var thisAngle = Vector3.Angle(Data.targetMainRig.position - transform.position, SwordPoints[j].gameObject.transform.position - transform.position);
					if (thisAngle < leastAngle && Shurikens[j].sinceSpawn > startDelay && Shurikens[j].sinceFired > attackRatePerShuriken)
					{
						leastAngle = thisAngle;
						AttackID = j;
					}
				}
				if ((bool)Data.targetMainRig && Data.distanceToTarget < attackDistance)
				{
					Attack(Data.targetMainRig, AttackID);
				}
			}
		}
		for (var k = 0; k < Shurikens.Count; k++)
		{
			if (Shurikens[k].obj != null)
			{
				Shurikens[k].sinceSpawn += Time.deltaTime;
				Shurikens[k].sinceFired += Time.deltaTime;
				Shurikens[k].obj.transform.position +=
					(SwordPoints[k].transform.position - Shurikens[k].obj.transform.position) * (followSpeed * Time.deltaTime);
				Shurikens[k].obj.transform.rotation = SwordPoints[k].transform.rotation;
			}
		}
	}

	public void Attack(Rigidbody target, int useAttackID)
	{
		Counter = 0f;
		
		if (useAttackID < 0 || useAttackID >= Shurikens.Count)
		{
			useAttackID = Random.Range(0, Shurikens.Count);
		}
		
		var shuriken = Shurikens[useAttackID];
		if (shuriken != null)
		{
			shuriken.sinceFired = 0f;
			
			StartCoroutine(DoAttack(shuriken, target));
			attackEvent.Invoke();
			
			if (!Done)
			{
				Shurikens[useAttackID] = CreateNewSword(SwordPoints[useAttackID].gameObject.transform.position, SwordPoints[useAttackID].gameObject.transform.rotation, SwordPoints[useAttackID]);
			}
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		foreach (var shuriken in Shurikens)
		{
			Destroy(shuriken.obj);
		}
	}

	private IEnumerator DoAttack(Shuriken attackShuriken, Rigidbody target)
	{
		if (!attackShuriken.obj)
		{
			yield break;
		}

		var spawnDirection = (target.position - attackShuriken.obj.transform.position).normalized;

		attackShuriken.obj.transform.rotation =
			Quaternion.LookRotation(spawnDirection + 0.01f * spread * Random.insideUnitSphere);
		SetProjectileStats(attackShuriken, spawnDirection, target,
			attackShuriken.obj.transform.forward, target.position, target.velocity);

		attackShuriken.move.enabled = true;
		attackShuriken.trail.enabled = true;
		attackShuriken.delay.enabled = true;
		var moveTransformCross = attackShuriken.obj.GetComponent<MoveTransformCross>();
		if (moveTransformCross) moveTransformCross.enabled = true;
	}

	private Shuriken CreateNewSword(Vector3 pos, Quaternion rot, ShootPosition point)
	{
		var shuriken = new Shuriken
		{
			obj = Instantiate(sourceShuriken, pos, rot)
		};
		shuriken.move = shuriken.obj.GetComponent<MoveTransform>();
		shuriken.move.enabled = false;
		
		shuriken.trail = shuriken.obj.GetComponent<RaycastTrail>();
		shuriken.trail.enabled = false;

		shuriken.delay = shuriken.obj.GetComponent<DelayEvent>();
		shuriken.delay.enabled = false;

		var moveTransformCross = shuriken.obj.GetComponent<MoveTransformCross>();
		if (moveTransformCross) moveTransformCross.enabled = false;
		
		var teamHolder = shuriken.obj.FetchComponent<TeamHolder>();
		teamHolder.team = Data.team;
		teamHolder.spawner = transform.root.gameObject;
		teamHolder.spawnerWeapon = point.gameObject;
		
		shuriken.obj.GetComponentInChildren<Landfall.TABC.CodeAnimation>().speedMultiplier = AttackSpeedMulti;
		return shuriken;
	}
	
	public void SetProjectileStats(Shuriken shuriken, Vector3 spawnDir, Rigidbody targetRig, Vector3 shootPositionForward, Vector3 targetRigPosition, Vector3 targetRigVelocity)
	{
		var compensation = shuriken.obj.GetComponentInChildren<Compensation>();
		if (compensation && targetRig)
		{
			compensation.transform.rotation = Quaternion.LookRotation(compensation.GetCompensation(targetRigPosition, targetRigVelocity, 0f) + Random.insideUnitSphere * 0.01f);
		}
		
		if (compensation && compensation.forwardCompensation > 0f && targetRig)
		{
			shuriken.move.selfImpulse.z += Mathf.Pow(Mathf.Clamp(Vector3.Distance(targetRigPosition, transform.position), 0f, compensation.clampDistance), compensation.rangePow) * compensation.forwardCompensation;
		}
	}
	
	private ShootPosition[] SwordPoints;
	private readonly List<Shuriken> Shurikens = new();
	private DataHandler Data;
	private bool Done;
	private float Counter;
	private float AttackSpeedMulti = 1f;
	private int AttackID;

	public GameObject sourceShuriken;

	public float followSpeed = 1f;

	public float startDelay = 1f;

	[Header("Attack")] 
	
	public int shurikensPerAttack = 3;
	
	public float attackRate = 0.5f;
	public float attackRatePerShuriken = 0.5f;
	public float attackDistance = 15f;
	public UnityEvent attackEvent;
	
	public float spread = 10f;

	[Serializable]
	public class Shuriken
	{
		public GameObject obj;
		public MoveTransform move;
		public RaycastTrail trail;
		public DelayEvent delay;
		public float sinceSpawn;
		public float sinceFired;
	}
}