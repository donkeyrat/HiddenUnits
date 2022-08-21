using Landfall.TABS;
using UnityEngine;

public class PhysicsFollowBodyPart1 : MonoBehaviour
{
	public float force;

	public float angularForce;

	public float drag = 0.8f;

	public Vector3 offset;

	public Rigidbody targetPart;

	public bool playOnStart;

	public bool setRotation;

	public bool useCenterOfMass;

	private bool done;

	[HideInInspector]
	public Transform target;

	private Rigidbody rig;

	private Vector3 startPos;

	public void Start()
	{
		rig = GetComponent<Rigidbody>();
		startPos += offset;
		if (playOnStart)
		{
			GoToPart();
		}
	}

	public void GoToPart()
	{
		if (done || !base.transform.root.GetComponent<Unit>())
		{
			return;
		}
		done = true;
		target = base.transform;
		if (targetPart)
        {
			target = targetPart.transform;
		}
		if ((bool)target)
		{
			Rigidbody component = target.GetComponent<Rigidbody>();
			if ((bool)component && useCenterOfMass)
			{
				base.transform.position = component.worldCenterOfMass;
			}
			else
			{
				base.transform.position = target.position;
			}
			if (setRotation)
			{
				base.transform.rotation = target.rotation;
			}
		}
	}

	private void FixedUpdate()
	{
		if ((bool)target)
		{
			rig.AddForce((target.TransformPoint(startPos) - base.transform.position) * force, ForceMode.Acceleration);
			rig.velocity *= drag;
			rig.angularVelocity *= drag;
			rig.AddTorque(Vector3.Cross(base.transform.forward, target.forward).normalized * Vector3.Angle(base.transform.forward, target.forward) * angularForce, ForceMode.Acceleration);
			rig.AddTorque(Vector3.Cross(base.transform.up, Vector3.up).normalized * Vector3.Angle(base.transform.up, Vector3.up) * angularForce * 0.2f, ForceMode.Acceleration);
		}
	}
}
