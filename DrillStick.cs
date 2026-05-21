using System.Collections;
using System.Collections.Generic;
using Landfall.TABS;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits;

public class DrillStick : CollisionWeaponEffect
{
	private Rigidbody Rig;
	private StickPosition StickPosition;
	private Unit OwnUnit;
	private GeneralInput Input;
	private Holdable Holdable;
	private static List<Unit> HitList;
	
	public bool hardStick = true;

	public float fixPositionAmount;

	public float breakForce = 20000f;

	[HideInInspector] 
	public ConfigurableJoint joint;

	public bool walkBackwardsWhenStuck;

	public float downwardsForceOnStuckRig;

	public float time = 3f;

	public UnityEvent stickEvent;

	private void Start()
	{
		Rig = GetComponent<Rigidbody>();
		OwnUnit = transform.root.GetComponent<Unit>();

		StickPosition = GetComponentInChildren<StickPosition>();
		Input = transform.root.GetComponentInChildren<GeneralInput>();
		Holdable = GetComponent<Holdable>();
		if (Holdable)
		{
			Holdable.AddWasGrabbedAction(Grab);
		}

		if (HitList == null) HitList = new List<Unit>();
	}

	public void Grab()
	{
		OwnUnit = Holdable.holderData.GetComponentInParent<Unit>();
	}

	private void FixedUpdate()
	{
		if ((bool)joint && (bool)joint.connectedBody && (!OwnUnit || !OwnUnit.data.Dead))
		{
			joint.connectedBody.AddForce(Vector3.down * downwardsForceOnStuckRig, ForceMode.Force);
			if (walkBackwardsWhenStuck && Input)
			{
				Input.inputDirection = Vector3.forward * -1f;
			}
		}
	}

	public override void DoEffect(Transform hitTransform, Collision collision)
	{
		if (joint)
		{
			return;
		}

		var collisionUnit = collision.transform.GetComponentInParent<Unit>();
		if (collisionUnit)
		{
			if (HitList.Contains(collisionUnit) || OwnUnit.Team == collisionUnit.Team)
			{
				return;
			}
			HitList.Add(collisionUnit);
		}

		var sqrMagnitude = (StickPosition.transform.position - collision.GetContact(0).point).sqrMagnitude;
		if ((bool)collision.rigidbody && sqrMagnitude < StickPosition.radius * StickPosition.radius)
		{
			joint = AttachJoint(Rig, collision.rigidbody, collision.GetContact(0).point, fixPositionAmount,
				StickPosition.transform, hardStick);
			joint.breakForce = breakForce;
			stickEvent.Invoke();
			StartCoroutine(KillJoint(joint, collisionUnit, Random.Range(time - 1f, time + 1f)));
		}
	}

	private static ConfigurableJoint AttachJoint(Rigidbody myRig, Rigidbody otherRig, Vector3 hitPos, float fix,
		Transform stickPos, bool hardStick)
	{
		if (fix != 0f)
		{
			otherRig.position = otherRig.transform.position + (stickPos.position - otherRig.position).normalized * fix;
		}

		ConfigurableJoint configurableJoint = myRig.gameObject.AddComponent<ConfigurableJoint>();
		configurableJoint.anchor = myRig.transform.InverseTransformPoint(hitPos);
		configurableJoint.xMotion = ConfigurableJointMotion.Locked;
		configurableJoint.yMotion = ConfigurableJointMotion.Locked;
		configurableJoint.zMotion = ConfigurableJointMotion.Locked;
		if (hardStick)
		{
			configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
			configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
		}
		else
		{
			configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
			configurableJoint.angularYMotion = ConfigurableJointMotion.Free;
			configurableJoint.angularZMotion = ConfigurableJointMotion.Free;
		}

		if ((bool)otherRig)
		{
			configurableJoint.connectedBody = otherRig;
		}

		configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
		var angularXDrive = configurableJoint.angularXDrive;
		angularXDrive.positionSpring = 10f;
		angularXDrive.positionDamper = 2f;
		configurableJoint.angularXDrive = angularXDrive;
		configurableJoint.angularYZDrive = angularXDrive;
		configurableJoint.enablePreprocessing = false;
		return configurableJoint;
	}

	public void RemoveStickJoint()
	{
		Destroy(joint);
	}

	private static IEnumerator KillJoint(ConfigurableJoint joint, Unit collisionUnit, float time)
	{
		yield return new WaitForSeconds(time);
		if (joint)
		{
			Destroy(joint);
		}

		if (collisionUnit) HitList.Remove(collisionUnit);
	}
}