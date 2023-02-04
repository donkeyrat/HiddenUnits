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
	
		public bool Meshparticle = true;
	
		[Tooltip("Enable for units that have erratic movement after teleporting in a ProjectMars game.")]
		public bool setUnitMainRigKinematic;
	
		public UnityEvent poofEvent;
	
		private TeamSystem m_teamSystem;
	
		private Unit unit;
	
		private ParticleSystem.ShapeModule emiss;
	
		private ParticleSystem part;
	
		private List<PhysicsFollowBodyPart> followers;
	
		private float currentDistance;
	
		private CanDoUnitEvents canDo;
	
		private bool done;
	
		private float counter;
	
		private void Start()
		{
			unit = GetComponentInParent<Unit>();
			m_teamSystem = World.Active.GetOrCreateManager<TeamSystem>();
			canDo = GetComponent<CanDoUnitEvents>();
			counter = Random.Range(0f, 0.5f);
			if (Meshparticle)
			{
				part = GetComponentInChildren<ParticleSystem>();
				emiss = part.shape;
				emiss.skinnedMeshRenderer = base.transform.root.GetComponentInChildren<SkinnedMeshRenderer>();
			}
			if (setUnitMainRigKinematic && BoltNetwork.IsClient)
			{
				unit.data.mainRig.isKinematic = true;
			}
		}
	
		private void Update()
		{
			if (automatic && canDo.canDoStuff && !done)
			{
				counter += Time.deltaTime;
				if (counter > 1f)
				{
					done = true;
					StartCoroutine(DoPoof());
				}
			}
		}
	
		private IEnumerator DoPoof()
		{
			if ((bool)part)
			{
				part.Emit(25);
			}
			yield return new WaitForSeconds(moveDelay);
			List<Unit> list = ((this.unit.Team == Team.Blue) ? m_teamSystem.GetTeamUnits(Team.Red) : m_teamSystem.GetTeamUnits(Team.Blue));
			Unit unit = null;
			for (int i = 0; i < list.Count; i++)
			{
				float num = Vector3.Distance(base.transform.position, list[i].data.mainRig.position);
				if ((bool)unit)
				{
					if (!(Random.value > 0.2f) || !useRandom)
					{
						if (unitTarget == UnitTarget.Furthest && num > currentDistance)
						{
							currentDistance = num;
							unit = list[i];
						}
						if (unitTarget == UnitTarget.Closest && num < currentDistance)
						{
							currentDistance = num;
							unit = list[i];
						}
					}
				}
				else
				{
					currentDistance = num;
					unit = list[i];
				}
			}
			if ((bool)unit)
			{
				_ = base.transform.root.position - this.unit.data.mainRig.position;
				Vector3 vector = (unit.data.mainRig.transform.position - this.unit.data.mainRig.position).normalized * ((unit.data.mainRig.transform.position - this.unit.data.mainRig.position).magnitude + distanceFromUnit);
				Debug.DrawLine(base.transform.position, unit.data.mainRig.transform.position, Color.blue, 1.5f);
				DataHandler componentInChildren = base.transform.root.GetComponentInChildren<DataHandler>();
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
				followers = new List<PhysicsFollowBodyPart>();
				followers.AddRange(base.transform.root.GetComponentsInChildren<PhysicsFollowBodyPart>());
				for (int k = 0; k < followers.Count; k++)
				{
					followers[k].transform.position += vector + Vector3.up * distanceAboveUnit;
				}
			}
			poofEvent?.Invoke();
			if ((bool)part)
			{
				part.Play();
			}
		}
	
		public void DoThePoof()
		{
			StartCoroutine(DoPoof());
		}
	}
}