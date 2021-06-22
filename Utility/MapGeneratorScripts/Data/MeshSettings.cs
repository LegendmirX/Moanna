using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdateableData
{
    public const int NumSupportedLODs = 5;
    public const int NumSupportedChunkSizes = 9;
    public const int NumOfSupportedFlatshadedChunkSizes = 3;
    public static readonly int[] SupportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };
    public static readonly int[] SupportedFlatshadedChunkSizes = { 48, 72, 96 };

    public float MeshScale = 3f;
    public bool UseFlatShading;

    [Range(0, NumSupportedChunkSizes - 1)]
    public int ChunkSizeIndex;
    [Range(0, NumOfSupportedFlatshadedChunkSizes - 1)]
    public int FlatshadedChunkSizeIndex;


    //num of verts per line of mesh rendered at LOd = 0. Includes the 2 extra verts that are excluded from final mesh, but used for cal normals
    public int NumVercitiesPerLine //this number needs to be divisable by LOD amount. Then -1 for boarder
    {
        get
        {
            return SupportedChunkSizes[(UseFlatShading) ? FlatshadedChunkSizeIndex : ChunkSizeIndex] + 5;
        }
    }

    public float MeshWorldSize
    {
        get
        {
            return (NumVercitiesPerLine - 3) * MeshScale;
        }
    }
}
