using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class DreadSwords : MonoBehaviour
{
	public delegate void AttackedEventHandler(Rigidbody target, int useAttackID);

	public UnityEvent shootEvent;

	private ShootPosition[] SwordPoints;

	private List<SpookySword> Swords = new List<SpookySword>();

	public GameObject sourceSword;

	public float throwRange = 10f;

	public float followSpeed = 1f;

	public float throwSpeed = 100f;

	public AnimationCurve throwCurve;

	private float Counter;

	private DataHandler Data;

	private bool Done;

	public float attackRate;

	private float AttackspeedMulti = 1f;

	private int AttackID;

	public event AttackedEventHandler Attacked;

	private void Awake()
	{
		SwordPoints = GetComponentsInChildren<ShootPosition>();
	}

	private void Start()
	{
		Data = transform.root.GetComponentInChildren<DataHandler>();
		List<Renderer> list = new List<Renderer>();
		for (int i = 0; i < SwordPoints.Length; i++)
		{
			SpookySword spookySword = CreateNewSword(SwordPoints[i].transform.position + Vector3.up * 2f, SwordPoints[i].transform.rotation);
			Renderer[] componentsInChildren = spookySword.gameObject.GetComponentsInChildren<Renderer>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				list.AddRange(componentsInChildren);
			}
			Swords.Add(spookySword);
		}
		if (Data != null && Data.unit != null)
		{
			Data.unit.AddRenderersToShowHide(list.ToArray(), Data.unit.IsSpawnedInBlindPlacement);
		}
		for (int k = 0; k < Swords.Count; k++)
		{
			if (!(Swords[k].gameObject == null))
			{
				Swords[k].gameObject.transform.position = SwordPoints[k].transform.position;
				Swords[k].gameObject.transform.rotation = SwordPoints[k].transform.rotation;
			}
		}
	}

	private void Update()
	{
		if (Done)
		{
			return;
		}
		if ((bool)Data && Data.Dead)
		{
			Done = true;
			for (int i = 0; i < Swords.Count; i++)
			{
				AttackID = i;
				Attack(Data.mainRig, AttackID);
			}
		}
		float num = Mathf.Clamp(Time.deltaTime, 0f, 0.02f);
		if ((bool)Data.weaponHandler)
		{
			AttackspeedMulti = Data.weaponHandler.attackSpeedMultiplier;
		}
		bool num2 = Data.unit == null || !Data.unit.IsRemotelyControlled;
		Counter += Time.deltaTime * AttackspeedMulti;
		if (num2 && (bool)Data.targetMainRig && Counter > attackRate)
		{
			float num3 = 999f;
			for (int j = 0; j < SwordPoints.Length; j++)
			{
				float num4 = Vector3.Angle(Data.targetMainRig.position - transform.position, SwordPoints[j].gameObject.transform.position - transform.position);
				if (num4 < num3 && Swords[j].sinceSpawn > 1.5f)
				{
					num3 = num4;
					AttackID = j;
				}
			}
			if ((bool)Data.targetMainRig && Data.distanceToTarget < throwRange)
			{
				Attack(Data.targetMainRig, AttackID);
			}
		}
		for (int k = 0; k < Swords.Count; k++)
		{
			if (!(Swords[k].gameObject == null))
			{
				Swords[k].sinceSpawn += num;
				Swords[k].gameObject.transform.position = SwordPoints[k].transform.position;
				Swords[k].gameObject.transform.rotation = SwordPoints[k].transform.rotation;
			}
		}
	}

	public void Attack(Rigidbody target, int useAttackID)
	{
		Counter = 0f;
		if (useAttackID < 0 || useAttackID >= Swords.Count)
		{
			useAttackID = UnityEngine.Random.Range(0, Swords.Count);
		}
		SpookySword spookySword = Swords[useAttackID];
		if (spookySword != null)
		{
			StartCoroutine(DoAttack(spookySword, target));
			shootEvent.Invoke();
			if (!Done)
			{
				Swords[useAttackID] = CreateNewSword(SwordPoints[useAttackID].gameObject.transform.position, SwordPoints[useAttackID].gameObject.transform.rotation);
			}
			else
			{
				spookySword.gameObject.GetComponent<ProjectileHit>().ignoreTeamMates = false;
			}
			Attacked?.Invoke(target, useAttackID);
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		for (int i = 0; i < Swords.Count; i++)
		{
			_ = Swords[i].gameObject != null;
			Destroy(Swords[i].gameObject);
		}
	}

	private IEnumerator DoAttack(SpookySword attackSword, Rigidbody targ)
	{
		if (!attackSword.gameObject)
		{
			yield break;
		}
		float counter2 = 0f;
		float t = throwCurve.keys[throwCurve.keys.Length - 1].time;
		ProjectileStick stick = attackSword.gameObject.GetComponent<ProjectileStick>();
		attackSword.gameObject.GetComponent<RaycastTrail>().enabled = true;
		while (counter2 < t && (!stick || !stick.stuck))
		{
			if (attackSword.gameObject == null || targ == null)
			{
				yield break;
			}
			float num = Mathf.Clamp(Time.deltaTime, 0f, 0.02f);
			counter2 += Time.deltaTime;
			attackSword.move.velocity = Vector3.Lerp(attackSword.move.velocity, (targ.position - attackSword.gameObject.transform.position).normalized * throwSpeed * throwCurve.Evaluate(counter2), num * 8f);
			attackSword.gameObject.transform.rotation = Quaternion.Lerp(attackSword.gameObject.transform.rotation, quaternion.LookRotation(targ.position - attackSword.gameObject.transform.position, Vector3.up), num * 7f);
			yield return null;
		}
		counter2 = 0f;
		while (counter2 < 0.5f && (!stick || !stick.stuck))
		{
			float num2 = Mathf.Clamp(Time.deltaTime, 0f, 0.02f);
			counter2 += Time.deltaTime;
			if (attackSword.gameObject != null)
			{
				attackSword.gameObject.transform.rotation = Quaternion.Lerp(attackSword.gameObject.transform.rotation, quaternion.LookRotation(attackSword.move.velocity, Vector3.up), num2 * 15f);
			}
			yield return null;
		}
	}

	private SpookySword CreateNewSword(Vector3 pos, quaternion rot)
	{
		SpookySword obj = new SpookySword
		{
			gameObject = Instantiate(sourceSword, pos, rot)
		};
		obj.move = obj.gameObject.GetComponent<MoveTransform>();
		obj.gameObject.FetchComponent<TeamHolder>().team = Data.team;
		obj.gameObject.GetComponentInChildren<Landfall.TABC.CodeAnimation>().speedMultiplier = AttackspeedMulti;
		return obj;
	}
}
