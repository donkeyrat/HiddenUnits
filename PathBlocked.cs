using Unity.Entities;

namespace HiddenUnits;

public struct PathBlocked : IComponentData {
	
    public int pathBlocked;
    public int raycastMask;
    public float distance;
}