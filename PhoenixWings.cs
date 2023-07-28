using Landfall.TABS.GameState;
using Photon.Bolt;
using UnityEngine;

public class PhoenixWings : MonoBehaviour
{
	public LayerMask mask;

	public AnimationCurve flightCurve;

	public float heightVariance = 0.5f;

	public float variationSpeed = 0.5f;

	public float flightForce;

	public float legForceMultiplier = 1f;

	private DataHandler Data;

	private RigidbodyHolder RigHolder;

	private Rigidbody RightFootRig;

	private Rigidbody LeftFootRig;

	private Rigidbody HipRig;

	private Rigidbody HeadRig;

	public float headM = 0.5f;

	private float Time;

	public bool useWings = true;

	public bool useWingsInPlacement = true;

	[Tooltip("Enable if units move erratically on the client side of ProjectMars games. Only enable if you are sure Wings.cs is causing erratic movement.")]
	public bool setUnitMainRigKinematic;

	private GameStateManager MGameStateManager;

	public float rotationTorque = 10f;

	private void Start()
	{
		Data = transform.root.GetComponentInChildren<DataHandler>();
		RigHolder = Data.GetComponent<RigidbodyHolder>();
		Data.takeFallDamage = false;
		Data.canFall = false;
		if ((bool)Data.footRight)
		{
			RightFootRig = Data.footRight.GetComponent<Rigidbody>();
		}
		if ((bool)Data.footLeft)
		{
			LeftFootRig = Data.footLeft.GetComponent<Rigidbody>();
		}
		HipRig = Data.hip.GetComponent<Rigidbody>();
		if ((bool)Data.head)
		{
			HeadRig = Data.head.GetComponent<Rigidbody>();
		}
		AnimationHandler component = Data.GetComponent<AnimationHandler>();
		if ((bool)component)
		{
			component.multiplier = 0.5f;
		}
		heightVariance *= Random.value;
		Time = Random.Range(0f, 1000f);
		Balance component2 = Data.GetComponent<Balance>();
		if ((bool)component2)
		{
			component2.enabled = false;
		}
		MGameStateManager = ServiceLocator.GetService<GameStateManager>();
		if (setUnitMainRigKinematic && BoltNetwork.IsClient)
		{
			Data.mainRig.isKinematic = true;
		}
	}

	private void FixedUpdate()
	{
		if ((!useWingsInPlacement && MGameStateManager.GameState != GameState.BattleState) || !useWings)
		{
			return;
		}
		bool value = Data.unit.m_PreferedDistance > Data.distanceToTarget;
		Physics.Raycast(new Ray(transform.position, Vector3.down), out var hitInfo, flightCurve.keys[flightCurve.keys.Length - 1].time, mask);
		if ((bool)hitInfo.transform)
		{
			float num = hitInfo.distance + Mathf.Cos((UnityEngine.Time.time + Time) * variationSpeed) * heightVariance;
			Data.mainRig.AddTorque(rotationTorque * Vector3.Angle(Data.mainRig.transform.up, Data.groundedMovementDirectionObject.forward) * Vector3.Cross(Data.mainRig.transform.up, Data.groundedMovementDirectionObject.forward), ForceMode.Acceleration);
			if ((bool)HeadRig)
			{
				HeadRig.AddForce(Vector3.up * flightForce * headM * flightCurve.Evaluate(num), ForceMode.Acceleration);
			}
			Data.mainRig.AddForce(Vector3.up * flightForce * flightCurve.Evaluate(num), ForceMode.Acceleration);
			if ((bool)RightFootRig)
			{
				RightFootRig.AddForce(Vector3.up * flightForce * legForceMultiplier * 0.5f * flightCurve.Evaluate(num), ForceMode.Acceleration);
			}
			if ((bool)RightFootRig)
			{
				LeftFootRig.AddForce(Vector3.up * flightForce * legForceMultiplier * 0.5f * flightCurve.Evaluate(num), ForceMode.Acceleration);
			}
			Data.TouchGround(hitInfo.point, hitInfo.normal);
		}
	}

	public void EnableFlight()
	{
		useWings = true;
	}

	public void DiableFlight()
	{
		useWings = false;
	}
}
