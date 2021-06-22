using Unity.Entities;
using Unity.Mathematics;

public struct PathFindingParams : IComponentData
{
    public int2 StartPosition;

    public int2 EndPosition;
}
