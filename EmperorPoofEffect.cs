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
			if ((bool)Part)
			{
				Part.Emit(25);
			}
			yield return new WaitForSeconds(moveDelay);
			List<Unit> list = ((Unit.Team == Team.Blue) ? MTeamSystem.GetTeamUnits(Team.Red) : MTeamSystem.GetTeamUnits(Team.Blue));
			Unit unit = null;
			for (int i = 0; i < list.Count; i++)
			{
				float num = Vector3.Distance(transform.position, list[i].data.mainRig.position);
				if ((bool)unit)
				{
					if (!(Random.value > 0.2f) || !useRandom)
					{
						if (unitTarget == UnitTarget.Furthest && num > CurrentDistance)
						{
							CurrentDistance = num;
							unit = list[i];
						}
						if (unitTarget == UnitTarget.Closest && num < CurrentDistance)
						{
							CurrentDistance = num;
							unit = list[i];
						}
					}
				}
				else
				{
					CurrentDistance = num;
					unit = list[i];
				}
			}
			if ((bool)unit)
			{
				_ = transform.root.position - Unit.data.mainRig.position;
				Vector3 vector = (unit.data.mainRig.transform.position - Unit.data.mainRig.position).normalized * ((unit.data.mainRig.transform.position - Unit.data.mainRig.position).magnitude + distanceFromUnit);
				Debug.DrawLine(transform.position, unit.data.mainRig.transform.position, Color.blue, 1.5f);
				DataHandler componentInChildren = transform.root.GetComponentInChildren<DataHandler>();
				for (int j = 0; j < componentInChildren.transform.childCount; j++)
				{
					Transform child = componentInChildren.transform.GetChild(j);
					child.position += vector + Vector3.up * distanceAboveUnit;
					if (unitTarget == UnitTarget.Furthest)
					{
						child.Rotate(Vector3.up * 180f);
					}
				}
				WeaponHandler component = componentInChildren.GetComponent<WeaponHandler>();
				if ((bool)component)
				{
					if ((bool)component.rightWeapon)
					{
						component.rightWeapon.transform.position += vector + Vector3.up * distanceAboveUnit;
					}
					if ((bool)component.leftWeapon)
					{
						component.leftWeapon.transform.position += vector + Vector3.up * distanceAboveUnit;
					}
				}
				Followers = new List<PhysicsFollowBodyPart>();
				Followers.AddRange(transform.root.GetComponentsInChildren<PhysicsFollowBodyPart>());
				for (int k = 0; k < Followers.Count; k++)
				{
					Followers[k].transform.position += vector + Vector3.up * distanceAboveUnit;
				}
			}
			poofEvent?.Invoke();
			if ((bool)Part)
			{
				Part.Play();
			}
		}
	
		public void DoThePoof()
		{
			StartCoroutine(DoPoof());
		}
	}
}