using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
    {
        float[,] values = NoiseUtil.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.HeightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                values[x, y] *= heightCurve_threadsafe.Evaluate(values[x, y]) * settings.HeightMultiplyer;

                if( values[x,y] > maxValue)
                {
                    maxValue = values[x, y];
                }
                if(values[x,y] < minValue)
                {
                    minValue = values[x, y];
                }
            }
        }

        return new HeightMap(values, minValue, maxValue);
    }
}

public struct HeightMap
{
    public readonly float[,] Values;
    public readonly float MinValue;
    public readonly float MaxValue;

    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.Values = values;
        this.MinValue = minValue;
        this.MaxValue = maxValue;
    }
}
