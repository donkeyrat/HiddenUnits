using System.Collections;
using System.Collections.Generic;
using Landfall.TABS;
using UnityEngine;
using System.Linq;

public class HadezHands : MonoBehaviour
{
	public string swingRef;

	public string hitRef;

	private Transform TargetTrans;

	private List<AttackArm> AttackArms = new List<AttackArm>();

	public AnimationCurve reachCurve;

	public AnimationCurve goBackToHoldCurve;

	public AnimationCurve goBackToHoldCurveFollowMainRigAmount;

	public AnimationCurve throwCurve;

	private float FollowMainRigAmount;

	private Unit Unit;

	private float SnapSpeed;

	private List<Unit> HitList = new List<Unit>();

	public void Start()
	{
		for (var i = 0; i < transform.childCount; i++)
		{
			var attackArm = new AttackArm();
			attackArm.targetObj = transform.GetChild(i).Find("Target").gameObject;
			attackArm.restPosObj = transform.GetChild(i).Find("RestPos").gameObject;
			attackArm.lerpSpeed = Random.Range(0.5f, 1.5f);
			attackArm.smoothTargetPos = transform.position;
			attackArm.targetPos = transform.position;
			AttackArms.Add(attackArm);
		}
		Unit = GetComponentInParent<Weapon>().connectedData.unit;
		TargetTrans = transform.parent;
		transform.SetParent(Unit.transform);
	}

	private void Update()
	{
		if (TargetTrans == null || Unit.data.Dead)
		{
			Destroy(gameObject);
			return;
		}
		transform.position = TargetTrans.position;
		transform.rotation = Quaternion.Lerp(transform.rotation, TargetTrans.rotation, Time.deltaTime * 2.5f);
		for (var i = 0; i < AttackArms.Count; i++)
		{
			AttackArms[i].counter += Time.deltaTime;
			var vector = Vector3.Lerp(AttackArms[i].smoothTargetPos, AttackArms[i].targetPos, Time.deltaTime * 10f);
			if (AttackArms[i].armState == AttackArm.ArmState.Free)
			{
				AttackArms[i].targetPos = Vector3.Lerp(AttackArms[i].targetPos, AttackArms[i].restPosObj.transform.position + GetPerlinPos(AttackArms[i].lerpSpeed) * 2f, Time.deltaTime * 3f * AttackArms[i].lerpSpeed);
				AttackArms[i].smoothTargetPos = vector;
				SnapSpeed = 0f;
			}
			else
			{
				SnapSpeed += Time.deltaTime;
				AttackArms[i].smoothTargetPos = Vector3.Lerp(vector, AttackArms[i].targetPos, SnapSpeed);
			}
			if (AttackArms[i].heldUnit)
			{
				AttackArms[i].targetObj.transform.position = Vector3.Lerp(AttackArms[i].smoothTargetPos, AttackArms[i].heldUnit.data.mainRig.position, FollowMainRigAmount);
			}
			else
			{
				AttackArms[i].targetObj.transform.position = AttackArms[i].smoothTargetPos;
			}
		}
		CheckAttack(AttackArms[Random.Range(0, AttackArms.Count)]);
	}

	private void CheckAttack(AttackArm attack)
	{
		var targ = SetTarget();
		if (attack.counter > 3f && Unit.data.distanceToTarget <= 10f && attack.armState == AttackArm.ArmState.Free && targ != null)
		{
			HitList.Add(targ);
			attack.counter = 0f;
			StartCoroutine(Attack(attack, targ));
		}
	}

	private IEnumerator Attack(AttackArm attack, Unit targetUnit)
	{
		attack.armState = AttackArm.ArmState.Holding;
		attack.heldUnit = targetUnit;
		var c2 = 0f;
		if (swingRef != "")
		{
			ServiceLocator.GetService<SoundPlayer>().PlaySoundEffect(swingRef, 1f, transform.position);
		}
		while (c2 < 3f && targetUnit && targetUnit.data.mainRig && !targetUnit.data.Dead)
		{
			var a = targetUnit.data.mainRig.position - attack.restPosObj.transform.position;
			attack.targetPos = attack.restPosObj.transform.position + a * reachCurve.Evaluate(c2);
			targetUnit.data.healthHandler.TakeDamage(100f * Time.deltaTime, Vector3.up);
			c2 += Time.deltaTime * Time.timeScale;
			yield return null;
		}
		attack.armState = AttackArm.ArmState.Free;
		c2 = 0f;
		var t2 = AnimationCurveFunctions.GetAnimLength(goBackToHoldCurve);
		while (c2 < t2 && targetUnit && targetUnit.data.mainRig)
		{
			var a2 = targetUnit.data.mainRig.position - attack.restPosObj.transform.position;
			attack.targetPos = attack.restPosObj.transform.position + a2 * goBackToHoldCurve.Evaluate(c2);
			c2 += Time.deltaTime;
			FollowMainRigAmount = goBackToHoldCurveFollowMainRigAmount.Evaluate(c2);
			yield return null;
		}
		HitList.Remove(attack.heldUnit);
		attack.heldUnit = null;
	}

	private Vector3 GetPerlinPos(float input)
	{
		input *= 0.2f;
		var result = new Vector3(0f, 0f, 0f);
		result.x += Mathf.PerlinNoise(Time.time * input, 0f);
		result.y += Mathf.PerlinNoise(Time.time * input, Time.time * input);
		result.z += Mathf.PerlinNoise(0f, Time.time * input);
		result.x -= 0.5f;
		result.y -= 0.5f;
		result.z -= 0.5f;
		return result;
	}

	public Unit SetTarget()
	{
		var query
		= (
		  from Unit unit
		  in FindObjectsOfType<Unit>()
		  where !unit.data.Dead && unit.Team != transform.root.GetComponent<Unit>().Team && !HitList.Contains(unit) && (unit.data.mainRig.transform.position - transform.position).magnitude <= 10f
		  orderby (unit.data.mainRig.transform.position - transform.position).magnitude
		  select unit
		).ToArray();
		if (query.Length > 0)
		{
			return query[0];
		}
		else
        {
			return null;
        }
	}
}
