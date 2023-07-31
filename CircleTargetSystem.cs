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
			
			return new Job {
				entities = MFilter.entities,
				directions = MFilter.directions,
				circleTargets = MFilter.circleTargets,
				targetDatas = MFilter.targetDatas,
				hasTargetTags = MFilter.hasTargetTags,
				commandBuffer = MBarrier.CreateCommandBuffer().ToConcurrent()
			}.Schedule(MFilter.length, 12, inputDeps);
		}

		[Inject]
		private Filter MFilter;

		[Inject]
		private PreLateUpdateBarrier MBarrier;

		private struct Filter {
			
			public EntityArray entities;

			public ComponentDataArray<Direction> directions;

			[ReadOnly]
			public ComponentDataArray<CircleTarget> circleTargets;

			[ReadOnly]
			public ComponentDataArray<HasTargetTag> hasTargetTags;

			[ReadOnly]
			public ComponentDataArray<TargetData> targetDatas;

			public readonly int length;
		}

		private struct Job : IJobParallelFor {
			
			public void Execute(int index) {
				
				if (hasTargetTags[index].Target == Entity.Null) { return; }
				
				var e = entities[index];
				var component = directions[index];
				var circleTarget = circleTargets[index];
				if (targetDatas[index].DistanceToTarget <= circleTarget.circleDistance) {
					
					var initial = component.Value;
					var better = new float3(initial.x, 0f, initial.z);
					var right = math.cross(math.normalize(better), new float3(0, 1, 0));
					var scaled = math.length(better) * right;
					component.Value = new float3(scaled.x, initial.y, scaled.z);
				}
				commandBuffer.SetComponent(index, e, component);
			}

			public EntityArray entities;

			public ComponentDataArray<Direction> directions;

			[ReadOnly]
			public ComponentDataArray<CircleTarget> circleTargets;

			[ReadOnly]
			public ComponentDataArray<HasTargetTag> hasTargetTags;

			[ReadOnly]
			public ComponentDataArray<TargetData> targetDatas;

			public EntityCommandBuffer.Concurrent commandBuffer;
		}
	}
}
