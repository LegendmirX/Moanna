using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        Mesh,
        FalloffMap
    }
    [Space]
    [Header("References")]
    public Renderer textureRenderer;
    public MeshFilter MapMeshFilter;
    public MeshRenderer MapMeshRenderer;
    public Material terrainMaterial;

    [Space]
    [Header("Settings")]
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;

    public DrawMode drawMode;
    [SerializeField]
    [Range(0, MeshSettings.NumSupportedLODs - 1)]
    private int editorPreviewLOD;

    [SerializeField]
    private AnimationCurve IslandMeshCurve;
    [SerializeField]
    private AnimationCurve MountainMeshHeightCurve;

    public bool AutoUpdate;

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);

        textureRenderer.gameObject.SetActive(true);
        MapMeshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        MapMeshFilter.sharedMesh = meshData.CreateMesh();

        textureRenderer.gameObject.SetActive(false);
        MapMeshFilter.gameObject.SetActive(true);
    }
    
    public void DrawMapInEditor()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.NumVercitiesPerLine, meshSettings.NumVercitiesPerLine, heightMapSettings, Vector2.zero);

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                //make texture from noise map to see what it looks like
                DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
                break;
            case DrawMode.Mesh:
                //3D Terrain
                DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.Values, meshSettings, editorPreviewLOD));
                break;
            case DrawMode.FalloffMap:
                //make a texture from the falloff map to see what the falloff map looks like
                DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.NumVercitiesPerLine), 0, 1)));
                break;
        }
    }
    
    void OnValidate()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }

    }

    void OnValuesUpdated()
    {
        if (Application.isPlaying == false)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

}
