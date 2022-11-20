using UnityEngine;
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
	public class CircleTargetSystem : JobComponentSystem {
		
		protected override JobHandle OnUpdate(JobHandle inputDeps) {
			
			return new CircleTargetSystem.Job {
				Entities = this.m_filter.Entities,
				Directions = this.m_filter.Directions,
				CircleTargets = this.m_filter.CircleTargets,
				TargetDatas = this.m_filter.TargetDatas,
				HasTargetTags = this.m_filter.HasTargetTags,
				CommandBuffer = this.m_barrier.CreateCommandBuffer().ToConcurrent()
			}.Schedule(this.m_filter.Length, 12, inputDeps);
		}

		[Inject]
		private CircleTargetSystem.Filter m_filter;

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

			public readonly int Length;
		}

		private struct Job : IJobParallelFor {
			
			public void Execute(int index) {
				
				if (this.HasTargetTags[index].Target == Entity.Null) { return; }
				
				Entity e = this.Entities[index];
				Direction component = this.Directions[index];
				CircleTarget circleTarget = this.CircleTargets[index];
				if (this.TargetDatas[index].DistanceToTarget <= circleTarget.CircleDistance) {
					
					float3 initial = component.Value;
					float3 better = new float3(initial.x, 0f, initial.z);
					float3 right = math.cross(math.normalize(better), new float3(0, 1, 0));
					float3 scaled = math.length(better) * right;
					component.Value = new float3(scaled.x, initial.y, scaled.z);
				}
				this.CommandBuffer.SetComponent<Direction>(index, e, component);
			}

			public EntityArray Entities;

			public ComponentDataArray<Direction> Directions;

			[ReadOnly]
			public ComponentDataArray<CircleTarget> CircleTargets;

			[ReadOnly]
			public ComponentDataArray<HasTargetTag> HasTargetTags;

			[ReadOnly]
			public ComponentDataArray<TargetData> TargetDatas;

			public EntityCommandBuffer.Concurrent CommandBuffer;
		}
	}
}
