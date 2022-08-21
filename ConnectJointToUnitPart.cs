using UnityEngine;

public class ConnectJointToUnitPart : MonoBehaviour
{
	public enum Bodypart
	{
		Head,
		Torso,
		Hip,
		ArmLeft,
		ArmRight,
		ElbowLeft,
		ElbowRight,
		LegLeft,
		LegRight,
		KneeLeft,
		KneeRight
	}

	public Bodypart bodypart;

	private bool done;

	public void Go()
	{
		if (done)
		{
			return;
		}
		done = true;
		ConfigurableJoint component = GetComponent<ConfigurableJoint>();
		Rigidbody rigidbody = null;
		if (bodypart == Bodypart.Head && transform.root.GetComponentInChildren<Head>())
		{
			rigidbody = transform.root.GetComponentInChildren<Head>().GetComponent<Rigidbody>();
		}
		if (bodypart == Bodypart.Torso && transform.root.GetComponentInChildren<Torso>())
		{
			rigidbody = transform.root.GetComponentInChildren<Torso>().GetComponent<Rigidbody>();
		}
		if (bodypart == Bodypart.Hip && transform.root.GetComponentInChildren<Hip>())
		{
			rigidbody = transform.root.GetComponentInChildren<Hip>().GetComponent<Rigidbody>();
		}
		if (bodypart == Bodypart.ArmLeft && transform.root.GetComponentInChildren<ArmLeft>())
		{
			rigidbody = transform.root.GetComponentInChildren<ArmLeft>().GetComponent<Rigidbody>();
		}
		if (bodypart == Bodypart.ArmRight && transform.root.GetComponentInChildren<ArmRight>())
		{
			rigidbody = transform.root.GetComponentInChildren<ArmRight>().GetComponent<Rigidbody>();
		}
		if (bodypart == Bodypart.ElbowLeft && transform.root.GetComponentInChildren<HandLeft>())
		{
			rigidbody = transform.root.GetComponentInChildren<HandLeft>().GetComponent<Rigidbody>();
		}
		if (bodypart == Bodypart.ElbowRight && transform.root.GetComponentInChildren<HandRight>())
		{
			rigidbody = transform.root.GetComponentInChildren<HandRight>().GetComponent<Rigidbody>();
		}
		if (bodypart == Bodypart.LegLeft && transform.root.GetComponentInChildren<LegLeft>())
		{
			rigidbody = transform.root.GetComponentInChildren<LegLeft>().GetComponent<Rigidbody>();
		}
		if (bodypart == Bodypart.LegRight && transform.root.GetComponentInChildren<LegRight>())
		{
			rigidbody = transform.root.GetComponentInChildren<LegRight>().GetComponent<Rigidbody>();
		}
		if (bodypart == Bodypart.KneeLeft && transform.root.GetComponentInChildren<KneeLeft>())
		{
			rigidbody = transform.root.GetComponentInChildren<KneeLeft>().GetComponent<Rigidbody>();
		}
		if (bodypart == Bodypart.KneeRight && transform.root.GetComponentInChildren<KneeRight>())
		{
			rigidbody = transform.root.GetComponentInChildren<KneeRight>().GetComponent<Rigidbody>();
		}
		if ((bool)rigidbody && (bool)component)
		{
			component.connectedBody = rigidbody;
		}
	}
}
