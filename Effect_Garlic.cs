using Landfall.TABS;
using TGCore.Library;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
	public class Effect_Garlic : UnitEffectBase
	{
		private void Init()
		{
			OwnUnit = transform.root.GetComponent<Unit>();
		}

		public override void DoEffect()
		{
			doEffectEvent.Invoke();
		}

		public override void Ping()
		{
			pingEvent.Invoke();
		}

		private void Update()
		{
			if (!OwnUnit || Done) return;
			
			Counter += Time.deltaTime;
			if (Counter < runTime)
			{
				OwnUnit.unitConfusion = Amount * multiplier * Direction;
				Counter += Time.deltaTime;
				
				if (OwnUnit.data.weaponHandler) OwnUnit.data.weaponHandler.StopAttacksFor(1f);
				
				var forward = OwnUnit.data.mainRig.transform.forward;
				OwnUnit.data.mainRig.AddTorque(torque * Vector2.Angle(forward, Direction) * Time.deltaTime * Vector3.up, ForceMode.Acceleration);
				var allRigs = OwnUnit.data.allRigs.AllRigs;
				foreach (var rig in allRigs)
				{
					if ((bool)rig)
					{
						rig.AddForce(force * Time.deltaTime * Mathf.Clamp(rig.drag, 0f, 1f) * Direction, ForceMode.Acceleration);
					}
				}
				OwnUnit.data.mainRig.AddForce(force * Mathf.Clamp(OwnUnit.data.mainRig.drag, 0f, 1f) * Time.deltaTime * Direction, ForceMode.Acceleration);
			}
			else if (!Done)
			{
				Done = true;
				OwnUnit.unitConfusion = Vector3.zero;
			}
		}

		public void DoTheThing()
		{
			Init();
			
			if (OwnUnit && OwnUnit.data && OwnUnit.data.targetData && !OwnUnit.GetComponentInChildren<IsVampire>())
			{
				Amount += 1.5f / Mathf.Clamp(OwnUnit.data.maxHealth * 0.02f, 0.1f, float.PositiveInfinity);
				
				Direction = -(OwnUnit.data.targetData.mainRig.position - OwnUnit.data.mainRig.position).normalized;
				Direction.y = 0.2f;
				Direction = Direction.normalized;
				
				Counter = 0f;
			}
			else if (OwnUnit && OwnUnit.data && OwnUnit.data.targetData)
			{
				var killed = OwnUnit.data.health <= vampireDamage;
				OwnUnit.data.healthHandler.TakeDamage(vampireDamage, Vector3.zero, OwnUnit, DamageType.Magic);
				
				if (killed) banishVampireEvent.Invoke();
				else hurtVampireEvent.Invoke();
			}
		}
		
		private Unit OwnUnit;
		private float Amount;
		private float Counter;
		private Vector3 Direction;
		private bool Done;
		
		public UnityEvent doEffectEvent = new();
		public UnityEvent pingEvent = new();

		[Header("Vampire")] 
		
		public UnityEvent banishVampireEvent = new();
		public UnityEvent hurtVampireEvent = new();
		
		public float vampireDamage;

		[Header("Non-Vampire")]
		
		public float multiplier = 1f;
		public float torque = 1f;
		public float force = 1f;
		public float runTime = 3f;
	}
}
