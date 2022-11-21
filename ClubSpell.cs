using System.Collections;
using UnityEngine;
using Landfall.TABS;

public class ClubSpell : TargetableEffect 
{
	public AnimationCurve upCurve;

	public AnimationCurve forceCurve;

	public float force;

	private float rockStartY;

	private Rigidbody rig;

	private Vector3 direction;

	private Vector3 startPos;

	public float prediction = 1f;

	private Rigidbody target;

	public void Start()
	{
		startPos = transform.position;
	}

    private IEnumerator Go() 
    {
	    float t2 = upCurve.keys[upCurve.keys.Length - 1].time;
		float c2 = 0f;
		while (c2 < t2) 
		{
			c2 += Time.deltaTime;
			rig.transform.localPosition = new Vector3(0f, upCurve.Evaluate(c2) + rockStartY, 0f);
			yield return null;
		}
		rig.isKinematic = false;
		t2 = forceCurve.keys[forceCurve.keys.Length - 1].time;
		c2 = 0f;
		GetDirection(rig.position, target.position, target);
		rig.useGravity = false;
		while (c2 < t2) 
		{
			c2 += Time.deltaTime;
			rig.velocity = direction * (forceCurve.Evaluate(c2) * force);
			yield return null;
		}
		rig.useGravity = true;
	}

	public override void DoEffect(Transform startPoint, Transform endPoint)
	{
	}

	public override void DoEffect(Vector3 startPoint, Vector3 endPoint, Rigidbody targetRig = null)
	{
		target = targetRig;
		rig = GetComponentInChildren<Rigidbody>();
		rockStartY = rig.transform.localPosition.y;
		StartCoroutine(Go());
	}

	public void DoSpell()
	{
		var enemyMainRig = transform.root.GetComponent<Unit>().data.targetMainRig;
		DoEffect(startPos, enemyMainRig.position, enemyMainRig);
	}

	private void GetDirection(Vector3 startPoint, Vector3 endPoint, Rigidbody targetRig) 
	{
		Vector3 vector = endPoint;
		vector += targetRig.velocity * (prediction * 0.1f * Vector3.Distance(startPoint, vector));
		direction = (vector - startPoint).normalized;
	}
}
