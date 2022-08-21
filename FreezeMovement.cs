using System.Collections.Generic;
using UnityEngine;

public class FreezeMovement : MonoBehaviour
{
	public bool freezeOnStart;

	public bool freezePosisiton;

	public bool freezeRotation;

	public bool unFreezeWeaponsOnDeath;

	public bool isCurrentlyFrozen;

	private List<Rigidbody> rigs;

	private RigidbodyHolder allRigs;

	private WeaponHandler weaponHandler;

	private DataHandler data;

	private HealthHandler healthHandler;

	public RigidbodyConstraints restraint;

	private void Awake()
	{
		data = base.transform.root.GetComponentInChildren<DataHandler>();
		healthHandler = base.transform.root.GetComponentInChildren<HealthHandler>();
		allRigs = base.transform.root.GetComponentInChildren<RigidbodyHolder>();
		rigs = new List<Rigidbody>();
		if (allRigs != null)
		{
			rigs.AddRange(allRigs.AllRigs);
		}
		weaponHandler = base.transform.root.GetComponentInChildren<WeaponHandler>();
		if ((bool)weaponHandler)
		{
			if ((bool)weaponHandler.rightWeapon)
			{
				rigs.Add(weaponHandler.rightWeapon.rigidbody);
			}
			if ((bool)weaponHandler.leftWeapon)
			{
				rigs.Add(weaponHandler.leftWeapon.rigidbody);
			}
		}
		if (freezeOnStart)
		{
			Freeze();
		}
	}

	public void Freeze()
	{
		for (int i = 0; i < rigs.Count; i++)
		{
			if ((bool)rigs[i])
			{
				if (freezePosisiton)
				{
					rigs[i].constraints = RigidbodyConstraints.FreezePosition;
				}
				if (freezeRotation)
				{
					//rigs[i].constraints = RigidbodyConstraints.FreezeRotation;
					rigs[i].freezeRotation = true;
				}
			}
		}
		data.mainRig.constraints = restraint;
		isCurrentlyFrozen = true;
		if (data != null && data.Dead)
		{
			UnFreezeWeapons();
		}
	}

	public void UnFreeze()
	{
		for (int i = 0; i < rigs.Count; i++)
		{
			if ((bool)rigs[i])
			{
				rigs[i].constraints = RigidbodyConstraints.None;
			}
		}
		isCurrentlyFrozen = false;
	}

	public void UnFreezeWeapons()
	{
		if ((bool)weaponHandler && isCurrentlyFrozen)
		{
			if ((bool)weaponHandler.rightWeapon && (bool)weaponHandler.rightWeapon.rigidbody)
			{
				weaponHandler.rightWeapon.rigidbody.constraints = RigidbodyConstraints.None;
			}
			if ((bool)weaponHandler.leftWeapon && (bool)weaponHandler.leftWeapon.rigidbody)
			{
				weaponHandler.leftWeapon.rigidbody.constraints = RigidbodyConstraints.None;
			}
		}
	}
}
