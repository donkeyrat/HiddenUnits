using EzECS.Barriers;
using Landfall.TABS.AI.Components;
using Landfall.TABS.AI.Components.Modifiers;
using Landfall.TABS.AI.Components.Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Landfall.TABS.AI.Systems.Modifiers {
	
	[UpdateAfter(typeof(UpdateBarrier))]
	[UpdateBefore(typeof(PreLateUpdateBarrier))]
	public class CircleTargetSystem : JobComponentSystem 
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps) {
			
			Job jobData = default(Job);
			jobData.Entities = m_filter.Entities;
			jobData.Directions = m_filter.Directions;
			jobData.CircleTargets = m_filter.CircleTargets;
			jobData.TargetDatas = m_filter.TargetDatas;
			jobData.HasTargetTags = m_filter.HasTargetTags;
			jobData.CommandBuffer = m_barrier.CreateCommandBuffer().ToConcurrent();
			return jobData.Schedule(m_filter.Length, 12, inputDeps);
		}

		[Inject]
		private Filter m_filter;

		[Inject]
		private PreLateUpdateBarrier m_barrier;

		private struct Filter {
			
			public EntityArray Entities;

			public ComponentDataArray<Direction> Directions;

			[ReadOnly]
			public ComponentDataArray<CircleTarget> CircleTargets;

			[ReadOnly]
			public ComponentDataArray<HasTargetTag> HasTargetTags;

			[ReadOnly]
			public ComponentDataArray<TargetData> TargetDatas;

			[ReadOnly]
			public SubtractiveComponent<IsInPool> IsInPool;

			public readonly int Length;
		}

		private struct Job : IJobParallelFor {
			
			public void Execute(int index) {
				
				if (HasTargetTags[index].Target == Entity.Null) { return; }
				
				var e = Entities[index];
				var component = Directions[index];
				var circleTarget = CircleTargets[index];
				var distanceToTarget = TargetDatas[index].DistanceToTarget;
				if (distanceToTarget <= circleTarget.maxCircleDistance && distanceToTarget >= circleTarget.minCircleDistance) {
					
					var initial = component.Value;
					var better = new float3(initial.x, 0f, initial.z);
					var right = math.cross(math.normalize(better), new float3(0, 1, 0));
					var scaled = math.length(better) * right;
					component.Value = new float3(scaled.x, initial.y, scaled.z);
				}
				CommandBuffer.SetComponent(index, e, component);
			}

			public EntityArray Entities;

			public ComponentDataArray<Direction> Directions;

			[ReadOnly]
			public ComponentDataArray<CircleTarget> CircleTargets;

			[ReadOnly]
			public ComponentDataArray<HasTargetTag> HasTargetTags;

			[ReadOnly]
			public ComponentDataArray<TargetData> TargetDatas;

			[ReadOnly]
			public SubtractiveComponent<IsInPool> IsInPool;

			public EntityCommandBuffer.Concurrent CommandBuffer;
		}
	}
}
