using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdateableData
{
    public NoiseSettings noiseSettings;

    public float HeightMultiplyer = 15f;
    public AnimationCurve HeightCurve;
    public bool useFalloffMap;


    public float minHeight
    {
        get
        {
            return  HeightMultiplyer * HeightCurve.Evaluate(0);
        }
    }
    public float maxHeight
    {
        get
        {
            return  HeightMultiplyer * HeightCurve.Evaluate(1);
        }
    }

    #if UNITY_EDITOR

    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }

    #endif
}
