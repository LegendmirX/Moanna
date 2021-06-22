using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TerrainChunk
{
    public event Action<TerrainChunk, bool> OnVisabilityChanged;
    const float colliderGenerationThreshold = 5f;

    public Vector2 coord;

    GameObject meshObj;
    Vector2 sampleCentre;
    Bounds bounds;
    LODinfo[] detailLevels;
    LODMesh[] LODMeshes;
    int colliderLODIndex;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    HeightMap heightMap;
    bool IsHeightMapRecived;

    int previousLODIndex = -1;
    bool hasSetCollider;
    float maxViewDistance;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;

    Transform viewer;

    public TerrainChunk(Vector2 pos, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODinfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material) // Build Chunk
    {
        this.coord = pos;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        sampleCentre = pos * meshSettings.MeshWorldSize / meshSettings.MeshScale;
        Vector2 position = pos * meshSettings.MeshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.MeshWorldSize);

        meshObj = new GameObject("Chunk " + sampleCentre.x + "," + sampleCentre.y);
        meshRenderer = meshObj.AddComponent<MeshRenderer>();
        meshFilter = meshObj.AddComponent<MeshFilter>();
        meshCollider = meshObj.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObj.transform.position = new Vector3(position.x, 0, position.y);
        meshObj.transform.SetParent(parent);

        SetVisable(false);

        LODMeshes = new LODMesh[detailLevels.Length];

        for (int i = 0; i < detailLevels.Length; i++)
        {
            LODMeshes[i] = new LODMesh(detailLevels[i].LOD);
            LODMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex)
            {
                LODMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDistance = detailLevels[detailLevels.Length - 1].visableDistanceThreashold;

        
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.NumVercitiesPerLine, meshSettings.NumVercitiesPerLine, heightMapSettings, sampleCentre), OnHeightMapRecived);
    }

    public void OnHeightMapRecived(object hightMap)
    {
        this.heightMap = (HeightMap)hightMap;
        IsHeightMapRecived = true;

        UpdateTerrainChunk();
    }

    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public void UpdateTerrainChunk()
    {
        if (IsHeightMapRecived == true)
        {

            float viewerDistanceFromNearEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool wasVisable = IsVisable();
            bool visable = viewerDistanceFromNearEdge <= maxViewDistance;

            if (visable == true)
            {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDistanceFromNearEdge > detailLevels[i].visableDistanceThreashold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex)
                {
                    LODMesh lODMesh = LODMeshes[lodIndex];

                    if (lODMesh.HasMesh == true)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lODMesh.Mesh;
                    }
                    else if (lODMesh.HasRequestedMesh == false)
                    {
                        lODMesh.RequestMesh(heightMap, meshSettings);
                    }
                }
            }

            if (wasVisable != visable)
            {
                SetVisable(visable);
                if(OnVisabilityChanged != null)
                {
                    OnVisabilityChanged(this, visable);
                }
            }

        }
    }

    public void UpdateCollisionMesh()
    {
        if (hasSetCollider == false)
        {
            float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisableDistanceThreshold)
            {
                if (LODMeshes[colliderLODIndex].HasRequestedMesh == false)
                {
                    LODMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if (sqrDstFromViewerToEdge < colliderGenerationThreshold * colliderGenerationThreshold)
            {
                if (LODMeshes[colliderLODIndex].HasMesh == true)
                {
                    meshCollider.sharedMesh = LODMeshes[colliderLODIndex].Mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    public void SetVisable(bool isVisable)
    {
        meshObj.SetActive(isVisable);
    }

    public bool IsVisable()
    {
        return meshObj.activeSelf;
    }
}

class LODMesh
{
    public Mesh Mesh;
    public bool HasRequestedMesh;
    public bool HasMesh;
    int LOD;

    public event Action updateCallback;

    public LODMesh(int lOD)
    {
        this.LOD = lOD;
    }

    public void OnMeshDataRecived(object meshData)
    {
        MeshData MeshData = (MeshData)meshData;
        this.Mesh = MeshData.CreateMesh();
        HasMesh = true;

        updateCallback();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        HasRequestedMesh = true;

        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.Values, meshSettings, LOD), OnMeshDataRecived);
    }
}

