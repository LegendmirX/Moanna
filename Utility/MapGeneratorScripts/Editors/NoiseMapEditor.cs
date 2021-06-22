using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapPreview))]
public class NoiseMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapPreview mapPreview = (MapPreview)target;

        if (DrawDefaultInspector())
        {
            if(mapPreview.AutoUpdate == true)
            {
                mapPreview.DrawMapInEditor();
            }
        }

        if(GUILayout.Button("Generate"))
        {
            mapPreview.DrawMapInEditor();
        }
    }
}
