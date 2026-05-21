using EzECS.Barriers;
using Landfall.TABS.AI;
using Landfall.TABS.AI.Components;
using Landfall.TABS.AI.Components.Modifiers;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace HiddenUnits;

[UpdateInGroup(typeof(BeforeUpdate))]
	public class PredictedPositionSpawnerSystem : ComponentSystem
	{
		private struct Filter
		{
			public EntityArray Entities;
			
			public ComponentArray<SpawnedUnitEntityData.GetSpawnerPredictedPosition> SpawnerPositions;

			public ComponentDataArray<PredictedPositionSpawner> PredictedPositionSpawners;

			[ReadOnly]
			public SubtractiveComponent<IsInPool> IsInPool;

			public readonly int Length;
		}

		[Inject]
		private Filter m_filter;

		protected override void OnUpdate()
		{
			for (var i = 0; i < m_filter.Length; i++)
			{
				var entity = m_filter.Entities[i];
				var spawnerPosition = m_filter.SpawnerPositions[i];
				var predictedPositionSpawner = m_filter.PredictedPositionSpawners[i];
				predictedPositionSpawner.Value = spawnerPosition.GetPredictedPosition();
				EntityManager.SetComponentData(entity, predictedPositionSpawner);
			}
		}
	}