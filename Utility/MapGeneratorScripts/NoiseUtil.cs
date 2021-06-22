using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class NoiseUtil
{
    public enum NormalizeMode
    {
        Local,
        Global
    }
    
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(settings.Seed);
        Vector2[] octaveOffsets = new Vector2[settings.Octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int o = 0; o < settings.Octaves; o++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.Offset.x + sampleCentre.x;
            float offsetY = prng.Next(-100000, 100000) - settings.Offset.y - sampleCentre.y;
            octaveOffsets[o] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.Persistance;
        }

        float MAX_LOCAL_NOISE_HEIGHT = float.MinValue;
        float MIN_LOCAL_NOISE_HEIGHT = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int o = 0; o < settings.Octaves; o++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[o].x) / settings.Scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[o].y) / settings.Scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.Persistance;
                    frequency *= settings.Lacunarity;
                }

                #region Check n set min n max noise heights
                if (noiseHeight > MAX_LOCAL_NOISE_HEIGHT)
                {
                    MAX_LOCAL_NOISE_HEIGHT = noiseHeight;
                }

                if(noiseHeight < MIN_LOCAL_NOISE_HEIGHT)
                {
                    MIN_LOCAL_NOISE_HEIGHT = noiseHeight;
                }
                #endregion

                noiseMap[x, y] = noiseHeight;

                if (settings.NoiseNormaliseMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight * settings.NoiseMaxHeightMultiplaier); //This is important for height sharpness and how often it hits high mountain size. be careful. test regularly
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        if (settings.NoiseNormaliseMode == NormalizeMode.Local)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(MIN_LOCAL_NOISE_HEIGHT, MAX_LOCAL_NOISE_HEIGHT, noiseMap[x, y]);
                }
            }
        }

        return noiseMap; 
    }
    
}

[Serializable]
public class NoiseSettings
{
    public NoiseUtil.NormalizeMode NoiseNormaliseMode = NoiseUtil.NormalizeMode.Global;
    public int Seed = 8;
    public float NoiseMaxHeightMultiplaier = 2f;
    public float Scale = 100f;
    [Range(1, 20)]
    public int Octaves = 4;
    [Range(0, 1)]
    public float Persistance = 0.28f;
    public float Lacunarity = 1f;
    public Vector2 Offset = new Vector2(0, 0);

    public NoiseSettings(NoiseUtil.NormalizeMode noiseNormaliseMode, int seed, float noiseMaxHeightMultiplaier, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        NoiseNormaliseMode = noiseNormaliseMode;
        Seed = seed;
        NoiseMaxHeightMultiplaier = noiseMaxHeightMultiplaier;
        Scale = scale;
        Octaves = octaves;
        Persistance = persistance;
        Lacunarity = lacunarity;
        Offset = offset;
    }

    public void ValidateValues()
    {
        Scale = Mathf.Max(Scale, 0.001f);
        Octaves = Mathf.Max(Octaves, 1);
        Lacunarity = Mathf.Max(Lacunarity, 1);
        Persistance = Mathf.Clamp01(Persistance);
    }
}
