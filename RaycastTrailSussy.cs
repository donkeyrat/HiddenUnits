using System;
using UnityEngine;
using Landfall.TABS;

public class RaycastTrailSussy : MonoBehaviour, GameObjectPooling.IPoolable
{
	private Vector3 deltaPos;

	private Vector3 lastPos;

	private RaycastHit hit;

	private RaycastHit groundhit;

	private Ray ray;

	private RaycastHit[] hits;

	private ProjectileHit projectileHit;

	public LayerMask mask;

	[HideInInspector]
	public int ignoredFrames;

	public bool useSphereCastOnUnits;

	public float radius;

	public LayerMask groundMask;

	public bool ignoreArmor;

	public Action ReleaseSelf
	{
		get;
		set;
	}

	private void Start()
	{
		lastPos = base.transform.position;
		projectileHit = GetComponent<ProjectileHit>();
		Check();
		_ = ignoreArmor;
	}

	public void Initialize()
	{
	}

	public void Reset()
	{
		lastPos = base.transform.position;
	}

	public void Release()
	{
	}

	private void Update()
	{
		Check();
	}

	private void Check()
	{
		if (ignoredFrames > 0)
		{
			ignoredFrames--;
			lastPos = base.transform.position;
			return;
		}
		deltaPos = base.transform.position - lastPos;
		ray = new Ray(lastPos, deltaPos);
		if (useSphereCastOnUnits)
		{
			hits = Physics.SphereCastAll(lastPos, radius, deltaPos, Vector3.Distance(base.transform.position, lastPos), mask);
			Physics.Raycast(ray, out groundhit, Vector3.Distance(base.transform.position, lastPos), mask);
		}
		else
		{
			Physics.Raycast(ray, out hit, Vector3.Distance(base.transform.position, lastPos), mask);
		}
		if (useSphereCastOnUnits)
		{
			if (hits.Length != 0)
			{
				for (int i = 0; i < hits.Length; i++)
				{
					projectileHit.Hit(hits[i]);
				}
			}
			if ((bool)groundhit.transform)
			{
				projectileHit.Hit(groundhit);
			}
		}
		else if ((bool)hit.transform)
		{
			if (hit.transform.root.GetComponent<Unit>() && hit.transform.root.GetComponent<Unit>().Team == GetComponentInParent<TeamHolder>().team)
            {
				return;
            }
			projectileHit.Hit(hit);
		}
		lastPos = base.transform.position;
	}
}
