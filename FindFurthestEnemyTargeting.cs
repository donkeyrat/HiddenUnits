using System.Collections;
using System.Collections.Generic;
using EzECS.Barriers;
using Landfall.TABS.AI.Components;
using Landfall.TABS.AI.Components.Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace HiddenUnits;

[UpdateBefore(typeof(PreUpdateBarrier))]
public class FindFurthestEnemyTargeting : JobComponentSystem
{
	private struct UnitFilter
	{
		public EntityArray Entities;

		[ReadOnly] public SharedComponentDataArray<Team> Teams;

		public ComponentDataArray<GroundPosition> Positions;

		public ComponentDataArray<PredictedPositionSpawner> PredictedPositions;

		public ComponentDataArray<Range> Ranges;

		public ComponentDataArray<UnitTag> UnitTags;

		[ReadOnly] public ComponentDataArray<FindFurthestEnemyTarget> TargetFurthestEnemyTag;

		[ReadOnly] public SubtractiveComponent<IsInPool> IsInPool;

		public readonly int Length;

		public readonly int GroupIndex;
	}

	private struct TargetFilter
	{
		public EntityArray Entities;

		[ReadOnly] public SharedComponentDataArray<Team> Teams;

		public ComponentDataArray<HipPosition> Position;

		[ReadOnly] public ComponentDataArray<PredictedPosition> PredictedPositions;

		[ReadOnly] public ComponentDataArray<TargetPriority> TargetPriorities;

		[ReadOnly] public ComponentDataArray<TargetThickness> TargetThicknesses;

		public ComponentDataArray<BeingTargetedBy> EnemiesTargetingMes;

		public ComponentDataArray<UnitTag> _UnitTag;

		public SubtractiveComponent<IsDead> _IsDead;

		[ReadOnly] public SubtractiveComponent<IsInPool> IsInPool;

		public readonly int Length;

		public readonly int GroupIndex;
	}

	private struct Job : IJob
	{
		public EntityArray UnitEntities;

		public ComponentDataArray<PredictedPositionSpawner> UnitPositions;

		public ComponentDataArray<Range> UnitRanges;

		[ReadOnly] public EntityArray TargetEntities;

		[ReadOnly] public ComponentDataArray<HipPosition> TargetPositions;

		[ReadOnly] public ComponentDataArray<TargetPriority> TargetPriorities;

		[ReadOnly] public ComponentDataArray<TargetThickness> TargetThicknesses;

		public NativeArray<BeingTargetedBy> TargetsBeingTargetedBy;

		public EntityCommandBuffer.Concurrent CommandBuffer;

		[ReadOnly] public ComponentDataFromEntity<HasTargetTag> HasTargetTag;

		[ReadOnly] public ComponentDataFromEntity<IsTarget> IsTargetTag;

		[ReadOnly] public SubtractiveComponent<IsInPool> IsInPool;

		public void Execute()
		{
			for (var i = 0; i < UnitEntities.Length; i++)
			{
				var entity = UnitEntities[i];
				var predictedPosition = UnitPositions[i];
				var range = UnitRanges[i];
				var target = Entity.Null;
				var targetIndex = -1;
				var minimumValue = float.MinValue;
				var value = 0f;
				for (var j = 0; j < TargetEntities.Length; j++)
				{
					var distanceToTargetMagnitude = math.length(TargetPositions[j].Value - predictedPosition.Value);
					var targetEntity = TargetEntities[j];
					var value2 = TargetsBeingTargetedBy[j].Value;
					var currentTargetValue = (distanceToTargetMagnitude) / TargetPriorities[j].Value - value2 * range.AttackRange * 0.05f;
					if (currentTargetValue > minimumValue)
					{
						minimumValue = currentTargetValue;
						target = targetEntity;
						targetIndex = j;
					}
				}

				if (targetIndex > -1)
				{
					CommandBuffer.SetComponent(i, entity, new AttackThickness
					{
						Value = value
					});
					var value3 = TargetsBeingTargetedBy[targetIndex];
					value3.Value += 1f;
					TargetsBeingTargetedBy[targetIndex] = value3;
					CommandBuffer.SetComponent(i, entity, new HasTargetTag
					{
						Target = target
					});
					CommandBuffer.SetComponent(i, target, new IsTarget
					{
						Targetee = entity
					});
				}
				else
				{
					CommandBuffer.SetComponent(i, entity, new AttackThickness
					{
						Value = 0f
					});
				}
			}
		}
	}

	[Inject] private UnitFilter m_filter;

	[Inject] private TargetFilter m_targetFilter;

	[Inject] private PreUpdateBarrier m_barrier;

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var hasTargetTag = GetComponentDataFromEntity<HasTargetTag>(isReadOnly: true);
		var isTarget = GetComponentDataFromEntity<IsTarget>(isReadOnly: true);
		var entityCommandBuffer = m_barrier.CreateCommandBuffer();
		
		var targetComponentGroup = ComponentGroups[m_targetFilter.GroupIndex];
		var componentGroup = ComponentGroups[m_filter.GroupIndex];
		
		componentGroup.SetFilter(new Team
		{
			Value = 0
		});
		targetComponentGroup.SetFilter(new Team
		{
			Value = 0
		});
		
		var entityArray = componentGroup.GetEntityArray();
		componentGroup.GetComponentDataArray<GroundPosition>();
		var componentDataArray = componentGroup.GetComponentDataArray<Range>();
		var componentDataArray2 = componentGroup.GetComponentDataArray<PredictedPositionSpawner>();
		var entityArray2 = targetComponentGroup.GetEntityArray();
		var componentDataArray3 = targetComponentGroup.GetComponentDataArray<HipPosition>();
		var componentDataArray4 = targetComponentGroup.GetComponentDataArray<TargetPriority>();
		var componentDataArray5 = targetComponentGroup.GetComponentDataArray<TargetThickness>();
		targetComponentGroup.GetComponentDataArray<PredictedPosition>();
		
		componentGroup.ResetFilter();
		componentGroup.SetFilter(new Team
		{
			Value = 1
		});
		
		targetComponentGroup.ResetFilter();
		targetComponentGroup.SetFilter(new Team
		{
			Value = 1
		});
		
		var entityArray3 = componentGroup.GetEntityArray();
		componentGroup.GetComponentDataArray<GroundPosition>();
		var componentDataArray6 = componentGroup.GetComponentDataArray<Range>();
		var componentDataArray7 = componentGroup.GetComponentDataArray<PredictedPositionSpawner>();
		var entityArray4 = targetComponentGroup.GetEntityArray();
		var componentDataArray8 = targetComponentGroup.GetComponentDataArray<HipPosition>();
		var componentDataArray9 = targetComponentGroup.GetComponentDataArray<TargetPriority>();
		var componentDataArray10 = targetComponentGroup.GetComponentDataArray<TargetThickness>();
		targetComponentGroup.GetComponentDataArray<PredictedPosition>();
		var targetsBeingTargetedBy = new NativeArray<BeingTargetedBy>(entityArray2.Length, Allocator.TempJob);
		var targetsBeingTargetedBy2 = new NativeArray<BeingTargetedBy>(entityArray4.Length, Allocator.TempJob);
		var dummyJob = default(Job);
		dummyJob.UnitEntities = entityArray;
		dummyJob.UnitPositions = componentDataArray2;
		dummyJob.UnitRanges = componentDataArray;
		dummyJob.TargetPositions = componentDataArray8;
		dummyJob.TargetEntities = entityArray4;
		dummyJob.TargetPriorities = componentDataArray9;
		dummyJob.TargetsBeingTargetedBy = targetsBeingTargetedBy2;
		dummyJob.TargetThicknesses = componentDataArray10;
		dummyJob.CommandBuffer = entityCommandBuffer.ToConcurrent();
		dummyJob.HasTargetTag = hasTargetTag;
		dummyJob.IsTargetTag = isTarget;
		var jobData = dummyJob;
		dummyJob = default(Job);
		dummyJob.UnitEntities = entityArray3;
		dummyJob.UnitPositions = componentDataArray7;
		dummyJob.UnitRanges = componentDataArray6;
		dummyJob.TargetEntities = entityArray2;
		dummyJob.TargetPositions = componentDataArray3;
		dummyJob.TargetPriorities = componentDataArray4;
		dummyJob.TargetsBeingTargetedBy = targetsBeingTargetedBy;
		dummyJob.TargetThicknesses = componentDataArray5;
		dummyJob.CommandBuffer = entityCommandBuffer.ToConcurrent();
		dummyJob.HasTargetTag = hasTargetTag;
		dummyJob.IsTargetTag = isTarget;
		var jobData2 = dummyJob;
		var dependsOn = jobData.Schedule(inputDeps);
		var result = jobData2.Schedule(dependsOn);
		dependsOn.Complete();
		result.Complete();
		for (var i = 0; i < entityArray2.Length; i++)
		{
			entityCommandBuffer.SetComponent(entityArray2[i], targetsBeingTargetedBy[i]);
		}

		for (var j = 0; j < entityArray4.Length; j++)
		{
			entityCommandBuffer.SetComponent(entityArray4[j], targetsBeingTargetedBy2[j]);
		}

		targetsBeingTargetedBy.Dispose();
		targetsBeingTargetedBy2.Dispose();
		return result;
	}
}
