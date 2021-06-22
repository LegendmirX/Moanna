using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator  
{
    public static Texture2D TextureFromTerrainPixels(int width, int height, TerrainPixel[] terrainPixels, float[,] noiseMap)
    {
        Color[] colourMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < terrainPixels.Length; i++)
                {
                    if (currentHeight <= terrainPixels[i].height)
                    {
                        colourMap[y * width + x] = terrainPixels[i].colour;
                        break;
                    }
                }
            }
        }

        return TextureFromColourMap(colourMap, width, height);
    }

    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();

        return texture;
    }

    public static Texture2D TextureFromHeightMap(HeightMap heightMap)
    {
        int width = heightMap.Values.GetLength(0);
        int height = heightMap.Values.GetLength(1);
        
        Color[] colourMap = new Color[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.red, Color.yellow, Mathf.InverseLerp(heightMap.MinValue, heightMap.MaxValue, heightMap.Values[x, y]));
            }
        }

        return TextureFromColourMap(colourMap, width, height);
    }

}

[System.Serializable]
public struct TerrainPixel
{
    public string name;
    public float height;
    public Color colour;
}


