using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IslandGenerator))]
public class DiscSamplingEditor : Editor
{

    public override void OnInspectorGUI()
    {
        IslandGenerator pointsPreview = (IslandGenerator)target;

        if (DrawDefaultInspector())
        {
            if (pointsPreview.AutoUpdate == true)
            {
                pointsPreview.GenerateIslandMap(100);
            }
        }

        if (GUILayout.Button("Generate"))
        {
            Transform parent = new GameObject("Map").transform;
            parent.position = Vector3.zero;

            GridUtil<IslandMapGridObj> grid = pointsPreview.GenerateIslandMap(100);

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    IslandMapGridObj obj = grid.GetGridObject(x, y);
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    go.transform.position = new Vector3(x, y);
                    go.transform.SetParent(parent);

                    if (obj.Type == IslandMapGridObj.TileType.WATER)
                    {
                        go.GetComponent<MeshRenderer>().material = new Material(pointsPreview.blue);
                        go.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = TextureGenerator.TextureFromColourMap(new Color[1] { Color.blue }, 1, 1);
                    }
                    else if(obj.Type == IslandMapGridObj.TileType.LAND)
                    {
                        go.GetComponent<MeshRenderer>().material = new Material(pointsPreview.green);
                        go.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = TextureGenerator.TextureFromColourMap(obj.MapData.colourMap, obj.MapData.heightMap.GetLength(0), obj.MapData.heightMap.GetLength(1));
                    }
                    else
                    {
                        Debug.Log(obj.Type + "Not found");
                    }
                }
            }
        }
    }
}
