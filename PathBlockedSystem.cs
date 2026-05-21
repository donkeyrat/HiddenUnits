using EzECS.Barriers;
using Landfall.TABS.AI.Components;
using Landfall.TABS.AI.Components.Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace HiddenUnits {
	
	[UpdateAfter(typeof(PreUpdateBarrier))]
	[UpdateBefore(typeof(UpdateBarrier))]
	public class PathBlockedSystem : JobComponentSystem 
	{
		protected override JobHandle OnUpdate(JobHandle inputDeps) {
			
			Job jobData = default(Job);
			NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(m_filter.Length, Allocator.TempJob);
			NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(m_filter.Length, Allocator.TempJob);
			jobData.Entities = m_filter.Entities;
			jobData.Directions = m_filter.Directions;
			jobData.Positions = m_filter.Positions;
			jobData.PathBlocked = m_filter.PathBlocked;
			jobData.HasTargetTags = m_filter.HasTargetTags;
			jobData.CommandBuffer = m_barrier.CreateCommandBuffer().ToConcurrent();
			jobData.Commands = commands;
			jobData.Schedule(m_filter.Length, 12, inputDeps).Complete();
			RaycastCommand.ScheduleBatch(commands, results, 12, inputDeps).Complete();
			commands.Dispose();
			var entityCommandBuffer = m_barrier.CreateCommandBuffer();
			for (var i = 0; i < results.Length; i++)
			{
				var pathBlocked = m_filter.PathBlocked[i];
				var pathBlocked2 = default(PathBlocked);
				pathBlocked2.raycastMask = pathBlocked.raycastMask;
				var component = pathBlocked2;
				component.pathBlocked = results[i].collider == null ? 0 : 1;
				if (component.pathBlocked != pathBlocked.pathBlocked)
				{
					entityCommandBuffer.SetComponent(m_filter.Entities[i], component);
				}
			}
			results.Dispose();
			return inputDeps;
		}

		[Inject]
		private Filter m_filter;

		[Inject]
		private PreLateUpdateBarrier m_barrier;

		private struct Filter {
			
			public EntityArray Entities;

			public ComponentDataArray<Direction> Directions;
			
			[ReadOnly]
			public ComponentDataArray<HipPosition> Positions;

			[ReadOnly]
			public ComponentDataArray<PathBlocked> PathBlocked;

			[ReadOnly]
			public ComponentDataArray<HasTargetTag> HasTargetTags;
			
			[ReadOnly]
			public SubtractiveComponent<IsInPool> IsInPool;

			public readonly int Length;
		}

		private struct Job : IJobParallelFor {
			
			public void Execute(int index)
			{

				var hipPosition = Positions[index];
				var direction = Directions[index];
				var hasTargetTag = HasTargetTags[index];
				var pathBlocked = PathBlocked[index];
				if (hasTargetTag.Target != Entity.Null)
				{
					var initial = direction.Value;
					var better = new float3(initial.x, 0f, initial.z);
					var right = math.cross(math.normalize(better), new float3(0, 1, 0));
					var scaled = math.length(better) * right;
					var raycastDirection = new float3(scaled.x, initial.y, scaled.z);
					Commands[index] = new RaycastCommand(hipPosition.Value, raycastDirection, pathBlocked.distance, pathBlocked.raycastMask);
				}
			}

			public EntityArray Entities;

			public ComponentDataArray<Direction> Directions;
			
			[ReadOnly]
			public ComponentDataArray<HipPosition> Positions;

			[ReadOnly]
			public ComponentDataArray<PathBlocked> PathBlocked;

			[ReadOnly]
			public ComponentDataArray<HasTargetTag> HasTargetTags;

			[ReadOnly]
			public SubtractiveComponent<IsInPool> IsInPool;
			
			[WriteOnly]
			public NativeArray<RaycastCommand> Commands;

			public EntityCommandBuffer.Concurrent CommandBuffer;
		}
	}
}
