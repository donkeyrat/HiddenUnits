using Unity.Entities;
using Unity.Mathematics;

namespace HiddenUnits;

public struct PredictedPositionSpawner : IComponentData
{
    public float3 Value;
}