using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TerrainGenerator : MonoBehaviour
{
    [Space]
    [Header("References")]
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData TextureSettings;
    public Transform viewer;
    [SerializeField]
    private Material mapMaterieal;
    [SerializeField]
    private Transform parent;

    [Space]
    [Header("Terrain Data")]

    [SerializeField]
    public int ColliderLODIndex;
    public LODinfo[] detailLevels = new LODinfo[3]
    {
        new LODinfo{ LOD = 0, visableDistanceThreashold = 120f},
        new LODinfo{ LOD = 1, visableDistanceThreashold = 200f},
        new LODinfo{ LOD = 4, visableDistanceThreashold = 400f}
    };
    
    const float viwerMoveThresholdForChunkUpdate = 25f;
    const float sqrViwerMoveThresholdForChunkUpdate = viwerMoveThresholdForChunkUpdate * viwerMoveThresholdForChunkUpdate;
    
    Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    float meshWorldSize;
    int chunksVisableInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visableTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        TextureSettings.ApplyToMaterial(mapMaterieal);
        TextureSettings.UpdateMeshHeights(mapMaterieal, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float MaxViewDistance = detailLevels[detailLevels.Length - 1].visableDistanceThreashold;
        meshWorldSize = meshSettings.MeshWorldSize;
        chunksVisableInViewDistance = Mathf.RoundToInt(MaxViewDistance / meshWorldSize);

        UpdateVisableChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if(viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk chunk in visableTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViwerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisableChunks();
        }
    }

    void UpdateVisableChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoods = new HashSet<Vector2>();
        
        for (int i = visableTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoods.Add(visableTerrainChunks[i].coord);
            visableTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisableInViewDistance; yOffset <= chunksVisableInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisableInViewDistance; xOffset <= chunksVisableInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (alreadyUpdatedChunkCoods.Contains(viewedChunkCoord) == false)
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, ColliderLODIndex, parent, viewer, mapMaterieal);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.OnVisabilityChanged += OnTerrainChunkVisabilityChanged;
                        newChunk.Load();
                    }
                }
            } 
        }
    }

    void OnTerrainChunkVisabilityChanged(TerrainChunk chunk, bool isVisable)
    {
        if(isVisable == true)
        {
            visableTerrainChunks.Add(chunk);
        }
        else
        {
            visableTerrainChunks.Remove(chunk);
        }
    }
}

[Serializable]
public struct LODinfo
{
    [Range(0,MeshSettings.NumSupportedLODs-1)]
    public int LOD;
    public float visableDistanceThreashold;

    public float sqrVisableDistanceThreshold
    {
        get
        {
            return visableDistanceThreashold * visableDistanceThreashold;
        }
    }
}


