using System.Collections;
using System.Collections.Generic;
using Landfall.TABS;
using UnityEngine;
using System.Linq;

public class HadezHands : MonoBehaviour
{
	public string swingRef;

	public string hitRef;

	private Transform targetTrans;

	private List<AttackArm> attackArms = new List<AttackArm>();

	public AnimationCurve reachCurve;

	public AnimationCurve goBackToHoldCurve;

	public AnimationCurve goBackToHoldCurveFollowMainRigAmount;

	public AnimationCurve throwCurve;

	[SerializeField]
	private Renderer[] armRenderers;

	private float followMainRigAmount;

	private Unit unit;

	private float snapSpeed;

	private List<Unit> hitList = new List<Unit>();

	public void Start()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			AttackArm attackArm = new AttackArm();
			attackArm.targetObj = transform.GetChild(i).Find("Target").gameObject;
			attackArm.restPosObj = transform.GetChild(i).Find("RestPos").gameObject;
			attackArm.lerpSpeed = Random.Range(0.5f, 1.5f);
			attackArm.smoothTargetPos = transform.position;
			attackArm.targetPos = transform.position;
			attackArms.Add(attackArm);
		}
		unit = GetComponentInParent<Weapon>().connectedData.unit;
		if (armRenderers != null && unit != null)
		{
			unit.AddRenderersToShowHide(armRenderers);
		}
		targetTrans = transform.parent;
		transform.SetParent(unit.transform);
	}

	private void Update()
	{
		if (targetTrans == null || unit.data.Dead)
		{
			Object.Destroy(gameObject);
			return;
		}
		transform.position = targetTrans.position;
		transform.rotation = Quaternion.Lerp(transform.rotation, targetTrans.rotation, Time.deltaTime * 2.5f);
		for (int i = 0; i < attackArms.Count; i++)
		{
			attackArms[i].counter += Time.deltaTime;
			Vector3 vector = Vector3.Lerp(attackArms[i].smoothTargetPos, attackArms[i].targetPos, Time.deltaTime * 10f);
			if (attackArms[i].armState == AttackArm.ArmState.Free)
			{
				attackArms[i].targetPos = Vector3.Lerp(attackArms[i].targetPos, attackArms[i].restPosObj.transform.position + GetPerlinPos(attackArms[i].lerpSpeed) * 2f, Time.deltaTime * 3f * attackArms[i].lerpSpeed);
				attackArms[i].smoothTargetPos = vector;
				snapSpeed = 0f;
			}
			else
			{
				snapSpeed += Time.deltaTime;
				attackArms[i].smoothTargetPos = Vector3.Lerp(vector, attackArms[i].targetPos, snapSpeed);
			}
			if (attackArms[i].heldUnit)
			{
				attackArms[i].targetObj.transform.position = Vector3.Lerp(attackArms[i].smoothTargetPos, attackArms[i].heldUnit.data.mainRig.position, followMainRigAmount);
			}
			else
			{
				attackArms[i].targetObj.transform.position = attackArms[i].smoothTargetPos;
			}
		}
		CheckAttack(attackArms[Random.Range(0, attackArms.Count)]);
	}

	private void CheckAttack(AttackArm attack)
	{
		var targ = SetTarget();
		if (attack.counter > 3f && unit.data.distanceToTarget <= 10f && attack.armState == AttackArm.ArmState.Free && targ != null)
		{
			hitList.Add(targ);
			attack.counter = 0f;
			StartCoroutine(Attack(attack, targ));
		}
	}

	private IEnumerator Attack(AttackArm attack, Unit targetUnit)
	{
		attack.armState = AttackArm.ArmState.Holding;
		attack.heldUnit = targetUnit;
		float c2 = 0f;
		if (swingRef != "")
		{
			ServiceLocator.GetService<SoundPlayer>().PlaySoundEffect(swingRef, 1f, transform.position);
		}
		while (c2 < 3f && targetUnit && targetUnit.data.mainRig && !targetUnit.data.Dead)
		{
			Vector3 a = targetUnit.data.mainRig.position - attack.restPosObj.transform.position;
			attack.targetPos = attack.restPosObj.transform.position + a * reachCurve.Evaluate(c2);
			targetUnit.data.healthHandler.TakeDamage(100f * Time.deltaTime, Vector3.up, null);
			c2 += Time.deltaTime * Time.timeScale;
			yield return null;
		}
		attack.armState = AttackArm.ArmState.Free;
		c2 = 0f;
		var t2 = AnimationCurveFunctions.GetAnimLength(goBackToHoldCurve);
		while (c2 < t2 && targetUnit && targetUnit.data.mainRig)
		{
			Vector3 a2 = targetUnit.data.mainRig.position - attack.restPosObj.transform.position;
			attack.targetPos = attack.restPosObj.transform.position + a2 * goBackToHoldCurve.Evaluate(c2);
			c2 += Time.deltaTime;
			followMainRigAmount = goBackToHoldCurveFollowMainRigAmount.Evaluate(c2);
			yield return null;
		}
		hitList.Remove(attack.heldUnit);
		attack.heldUnit = null;
	}

	private Vector3 GetPerlinPos(float input)
	{
		input *= 0.2f;
		Vector3 result = new Vector3(0f, 0f, 0f);
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
		Unit[] query
		= (
		  from Unit unit
		  in FindObjectsOfType<Unit>()
		  where !unit.data.Dead && unit.Team != transform.root.GetComponent<Unit>().Team && !hitList.Contains(unit) && (unit.data.mainRig.transform.position - transform.position).magnitude <= 10f
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
