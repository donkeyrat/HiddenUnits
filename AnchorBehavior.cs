using Landfall.TABS;
using TGCore.Library;
using UnityEngine;

namespace HiddenUnits;

public class AnchorBehavior : MonoBehaviour
{
	private Rope Rope;
	private TeamHolder Team;
	private Unit SpawnerUnit;
	private Rigidbody OwnRig;
	private CollisionStick Stick;
	private AudioSource Audio;
	private float Counter;

	public float force;
	public float closeToWeaponThreshold = 3f;
	public float projectilePosOffset = -0.2f;
	public float basePosOffset = 0.3f;
	public float timeTillDisable = 3f;

	private void Start()
	{
		Stick = GetComponent<CollisionStick>();
		Team = GetComponent<TeamHolder>();
		Rope = GetComponentInChildren<Rope>();
		OwnRig = GetComponent<Rigidbody>();
		SpawnerUnit = Team?.spawner?.GetComponent<Unit>();
		
		Audio = GetComponent<AudioSource>();
		Audio.volume = 0f;
	}

	private void Update()
	{
		var num = 0f;
		if (Rope && !Rope.done)
		{
			num = Rope.stiffnes;
		}
		Audio.volume = Mathf.Lerp(Audio.volume, num * 0.1f, Time.deltaTime * 5f);
	}

	private void LateUpdate()
	{
		var weaponTransform = Team.spawnerWeapon.transform;
		if (!Rope) return;

		Counter += Time.deltaTime;
		if (Counter > timeTillDisable || !Team.spawnerWeapon || !SpawnerUnit || (SpawnerUnit && SpawnerUnit.data.Dead))
		{
			Rope.done = true;
			return;
		}
		
		Rope.position1 = transform.position + transform.forward * projectilePosOffset;
		Rope.Position2 = weaponTransform.position + weaponTransform.forward * basePosOffset;
		Rope.middleVelocity += Vector3.up * (Mathf.Clamp(transform.forward.y, 0f, 1f) * Time.deltaTime * 250f);
		
		var directionToWeapon = (weaponTransform.position - transform.position).normalized * Rope.stiffnes;
		if (Stick.hitList.Count > 0)
		{
			var distanceToWeapon = Vector3.Distance(weaponTransform.position, transform.position);
			if (distanceToWeapon <= closeToWeaponThreshold)
			{
				Rope.done = true;
				return;
			}
			
			Rope.stiffnes = Mathf.Lerp(Rope.stiffnes, 1f, Time.deltaTime);

			OwnRig.AddForce(directionToWeapon * (Time.deltaTime * force), ForceMode.Force);
		}
	}
}