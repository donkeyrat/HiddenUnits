using System.Collections;
using System.Collections.Generic;
using Landfall.TABC;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class DreadSwords : MonoBehaviour
{
	public delegate void AttackedEventHandler(Rigidbody target, int useAttackID);

	public UnityEvent shootEvent;

	private ShootPosition[] swordPoints;

	private List<SpookySword> swords = new List<SpookySword>();

	public GameObject sourceSword;

	public float throwRange = 10f;

	public float followSpeed = 1f;

	public float throwSpeed = 100f;

	public AnimationCurve throwCurve;

	private float counter;

	private DataHandler data;

	private bool done;

	public float attackRate;

	private float attackspeedMulti = 1f;

	private int attackID;

	public event AttackedEventHandler Attacked;

	private void Awake()
	{
		swordPoints = GetComponentsInChildren<ShootPosition>();
	}

	private void Start()
	{
		data = base.transform.root.GetComponentInChildren<DataHandler>();
		List<Renderer> list = new List<Renderer>();
		for (int i = 0; i < swordPoints.Length; i++)
		{
			SpookySword spookySword = CreateNewSword(swordPoints[i].transform.position + Vector3.up * 2f, swordPoints[i].transform.rotation);
			Renderer[] componentsInChildren = spookySword.gameObject.GetComponentsInChildren<Renderer>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				list.AddRange(componentsInChildren);
			}
			swords.Add(spookySword);
		}
		if (data != null && data.unit != null)
		{
			data.unit.AddRenderersToShowHide(list.ToArray(), data.unit.IsSpawnedInBlindPlacement);
		}
		for (int k = 0; k < swords.Count; k++)
		{
			if (!(swords[k].gameObject == null))
			{
				swords[k].gameObject.transform.position = swordPoints[k].transform.position;
				swords[k].gameObject.transform.rotation = swordPoints[k].transform.rotation;
			}
		}
	}

	private void Update()
	{
		if (done)
		{
			return;
		}
		if ((bool)data && data.Dead)
		{
			done = true;
			for (int i = 0; i < swords.Count; i++)
			{
				attackID = i;
				Attack(data.mainRig, attackID);
			}
		}
		float num = Mathf.Clamp(Time.deltaTime, 0f, 0.02f);
		if ((bool)data.weaponHandler)
		{
			attackspeedMulti = data.weaponHandler.attackSpeedMultiplier;
		}
		bool num2 = data.unit == null || !data.unit.IsRemotelyControlled;
		counter += Time.deltaTime * attackspeedMulti;
		if (num2 && (bool)data.targetMainRig && counter > attackRate)
		{
			float num3 = 999f;
			for (int j = 0; j < swordPoints.Length; j++)
			{
				float num4 = Vector3.Angle(data.targetMainRig.position - base.transform.position, swordPoints[j].gameObject.transform.position - base.transform.position);
				if (num4 < num3 && swords[j].sinceSpawn > 1.5f)
				{
					num3 = num4;
					attackID = j;
				}
			}
			if ((bool)data.targetMainRig && data.distanceToTarget < throwRange)
			{
				Attack(data.targetMainRig, attackID);
			}
		}
		for (int k = 0; k < swords.Count; k++)
		{
			if (!(swords[k].gameObject == null))
			{
				swords[k].sinceSpawn += num;
				swords[k].gameObject.transform.position = swordPoints[k].transform.position;
				swords[k].gameObject.transform.rotation = swordPoints[k].transform.rotation;
			}
		}
	}

	public void Attack(Rigidbody target, int useAttackID)
	{
		counter = 0f;
		if (useAttackID < 0 || useAttackID >= swords.Count)
		{
			useAttackID = UnityEngine.Random.Range(0, swords.Count);
		}
		SpookySword spookySword = swords[useAttackID];
		if (spookySword != null)
		{
			StartCoroutine(DoAttack(spookySword, target));
			shootEvent.Invoke();
			if (!done)
			{
				swords[useAttackID] = CreateNewSword(swordPoints[useAttackID].gameObject.transform.position, swordPoints[useAttackID].gameObject.transform.rotation);
			}
			else
			{
				spookySword.gameObject.GetComponent<ProjectileHit>().ignoreTeamMates = false;
			}
			this.Attacked?.Invoke(target, useAttackID);
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		for (int i = 0; i < swords.Count; i++)
		{
			_ = swords[i].gameObject != null;
			Object.Destroy(swords[i].gameObject);
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
			gameObject = Object.Instantiate(sourceSword, pos, rot)
		};
		obj.move = obj.gameObject.GetComponent<MoveTransform>();
		obj.gameObject.FetchComponent<TeamHolder>().team = data.team;
		obj.gameObject.GetComponentInChildren<Landfall.TABC.CodeAnimation>().speedMultiplier = attackspeedMulti;
		return obj;
	}
}
