using System;
using Unity.Entities;

namespace Landfall.TABS.AI.Components.Modifiers {
	
	public struct CircleTarget : IMovementComponent, IComponentData {
		
		public float CircleDistance;
	}
}
