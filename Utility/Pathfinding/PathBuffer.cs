using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(50)]
public struct PathBuffer : IBufferElementData
{
    public int2 Position;
}
