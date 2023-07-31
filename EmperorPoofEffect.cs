using System.Collections;
using System.Collections.Generic;
using Landfall.TABS;
using Landfall.TABS.AI.Systems;
using Photon.Bolt;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace HiddenUnits
{
	public class EmperorPoofEffect : MonoBehaviour
	{
		public enum UnitTarget
		{
			Furthest,
			Closest
		}
	
		public UnitTarget unitTarget;
	
		public bool automatic = true;
	
		public float moveDelay = 0.05f;
	
		public float distanceFromUnit = 2f;
	
		public float distanceAboveUnit = 1.5f;
	
		public bool useRandom = true;
	
		public bool meshparticle = true;
	
		[Tooltip("Enable for units that have erratic movement after teleporting in a ProjectMars game.")]
		public bool setUnitMainRigKinematic;
	
		public UnityEvent poofEvent;
	
		private TeamSystem MTeamSystem;
	
		private Unit Unit;
	
		private ParticleSystem.ShapeModule Emiss;
	
		private ParticleSystem Part;
	
		private List<PhysicsFollowBodyPart> Followers;
	
		private float CurrentDistance;
	
		private CanDoUnitEvents CanDo;
	
		private bool Done;
	
		private float Counter;
	
		private void Start()
		{
			Unit = GetComponentInParent<Unit>();
			MTeamSystem = World.Active.GetOrCreateManager<TeamSystem>();
			CanDo = GetComponent<CanDoUnitEvents>();
			Counter = Random.Range(0f, 0.5f);
			if (meshparticle)
			{
				Part = GetComponentInChildren<ParticleSystem>();
				Emiss = Part.shape;
				Emiss.skinnedMeshRenderer = transform.root.GetComponentInChildren<SkinnedMeshRenderer>();
			}
			if (setUnitMainRigKinematic && BoltNetwork.IsClient)
			{
				Unit.data.mainRig.isKinematic = true;
			}
		}
	
		private void Update()
		{
			if (automatic && CanDo.canDoStuff && !Done)
			{
				Counter += Time.deltaTime;
				if (Counter > 1f)
				{
					Done = true;
					StartCoroutine(DoPoof());
				}
			}
		}
	
		private IEnumerator DoPoof()
		{
			if (Part) Part.Emit(25);
			
			yield return new WaitForSeconds(moveDelay);
			
			var list = Unit.Team == Team.Blue ? MTeamSystem.GetTeamUnits(Team.Red) : MTeamSystem.GetTeamUnits(Team.Blue);
			Unit unit = null;
			
			foreach (var enemy in list)
			{
				var num = Vector3.Distance(transform.position, enemy.data.mainRig.position);
				if (unit)
				{
					if (Random.value <= 0.2f || !useRandom)
					{
						switch (unitTarget)
						{
							case UnitTarget.Furthest when num > CurrentDistance:
								CurrentDistance = num;
								unit = enemy;
								break;
							case UnitTarget.Closest when num < CurrentDistance:
								CurrentDistance = num;
								unit = enemy;
								break;
						}
					}
				}
				else
				{
					CurrentDistance = num;
					unit = enemy;
				}
			}
			if (unit)
			{
				var vector = (unit.data.mainRig.transform.position - Unit.data.mainRig.position).normalized * ((unit.data.mainRig.transform.position - Unit.data.mainRig.position).magnitude + distanceFromUnit);
				Debug.DrawLine(transform.position, unit.data.mainRig.transform.position, Color.blue, 1.5f);
				var componentInChildren = transform.root.GetComponentInChildren<DataHandler>();
				for (var j = 0; j < componentInChildren.transform.childCount; j++)
				{
					var child = componentInChildren.transform.GetChild(j);
					child.position += vector + Vector3.up * distanceAboveUnit;
					if (unitTarget == UnitTarget.Furthest)
					{
						child.Rotate(Vector3.up * 180f);
					}
				}
				var component = componentInChildren.GetComponent<WeaponHandler>();
				if (component)
				{
					if (component.rightWeapon)
					{
						component.rightWeapon.transform.position += vector + Vector3.up * distanceAboveUnit;
					}
					if (component.leftWeapon)
					{
						component.leftWeapon.transform.position += vector + Vector3.up * distanceAboveUnit;
					}
				}
				
				Followers = new List<PhysicsFollowBodyPart>();
				Followers.AddRange(transform.root.GetComponentsInChildren<PhysicsFollowBodyPart>());
				
				foreach (var follower in Followers)
				{
					follower.transform.position += vector + Vector3.up * distanceAboveUnit;
				}
			}
			
			poofEvent?.Invoke();
			if (Part) Part.Play();
		}
	
		public void DoThePoof()
		{
			StartCoroutine(DoPoof());
		}
	}
}