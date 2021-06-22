using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UpdateableData : ScriptableObject
{
    public event Action OnValuesUpdated;
    public bool autoUpdate;

    #if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        if(autoUpdate == true)
        {
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
        }
    }

    public void NotifyOfUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

    #endif

}
