using Landfall.TABS.AI.Components.Modifiers;
using Unity.Entities;

namespace HiddenUnits {
	
	public struct CircleTarget : IMovementComponent, IComponentData {
		
		public float minCircleDistance;
		public float maxCircleDistance;
	}
}
