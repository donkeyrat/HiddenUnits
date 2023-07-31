using System.Collections;
using UnityEngine;
using Landfall.TABS;

public class ClubSpell : TargetableEffect 
{
	public AnimationCurve upCurve;

	public AnimationCurve forceCurve;

	public float force;

	private float RockStartY;

	private Rigidbody Rig;

	private Vector3 Direction;

	private Vector3 StartPos;

	public float prediction = 1f;

	private Rigidbody Target;

	public void Start()
	{
		StartPos = transform.position;
	}

    private IEnumerator Go() 
    {
	    var t2 = upCurve.keys[upCurve.keys.Length - 1].time;
		var c2 = 0f;
		while (c2 < t2) 
		{
			c2 += Time.deltaTime;
			Rig.transform.localPosition = new Vector3(0f, upCurve.Evaluate(c2) + RockStartY, 0f);
			yield return null;
		}
		Rig.isKinematic = false;
		t2 = forceCurve.keys[forceCurve.keys.Length - 1].time;
		c2 = 0f;
		GetDirection(Rig.position, Target.position, Target);
		Rig.useGravity = false;
		while (c2 < t2) 
		{
			c2 += Time.deltaTime;
			Rig.velocity = Direction * (forceCurve.Evaluate(c2) * force);
			yield return null;
		}
		Rig.useGravity = true;
	}

	public override void DoEffect(Transform startPoint, Transform endPoint)
	{
	}

	public override void DoEffect(Vector3 startPoint, Vector3 endPoint, Rigidbody targetRig = null)
	{
		Target = targetRig;
		Rig = GetComponentInChildren<Rigidbody>();
		RockStartY = Rig.transform.localPosition.y;
		StartCoroutine(Go());
	}

	public void DoSpell()
	{
		var enemyMainRig = transform.root.GetComponent<Unit>().data.targetMainRig;
		DoEffect(StartPos, enemyMainRig.position, enemyMainRig);
	}

	private void GetDirection(Vector3 startPoint, Vector3 endPoint, Rigidbody targetRig) 
	{
		var vector = endPoint;
		vector += targetRig.velocity * (prediction * 0.1f * Vector3.Distance(startPoint, vector));
		Direction = (vector - startPoint).normalized;
	}
}
