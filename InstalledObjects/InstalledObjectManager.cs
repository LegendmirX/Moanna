using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstalledObjectManager : MonoBehaviour
{
    public InstalledObjectVisuals installedObjectVisuals;

    public Dictionary<string, InstalledObject> InstalledObjectPrototypes;

    public List<InstalledObject> InstalledObjects;

    public void SetUp()
    {
        InstalledObjects = new List<InstalledObject>();
        InstalledObjectPrototypes = PrototypeManager.BuildInstalledObjectPrototypes();
        installedObjectVisuals = FindObjectOfType<InstalledObjectVisuals>();
        installedObjectVisuals.SetUp();
    }

    public void UpdateFrame(float deltaTime)
    {
        foreach(InstalledObject obj in InstalledObjects)
        {
            obj.Update(deltaTime);
        }
    }

    public InstalledObject CreateInstalledObject(string type, Vector3 position, Transform parent)
    {
        InstalledObject proto = InstalledObjectPrototypes[type];
        if (proto == null)
        {
            Debug.Log("InstalledObjectProtos did not contain " + type);
            return null;
        }
        List<TileGridObj> tiles = new List<TileGridObj>();

        for (int x = 0; x < proto.Size.x; x++)
        {
            for (int y = 0; y < proto.Size.y; y++)
            {
                Vector3 testLocation = position + new Vector3(x, y);
                TileGridObj tile = WorldController.current.bigDaddyGrid.GetGridObject(testLocation);

                if(tile == null)
                {
                    Debug.Log("Invalid Position");
                    return null;
                }

                tiles.Add(tile);
            }
        }

        foreach(TileGridObj tile in tiles)
        {
            if (ValidatePosition(proto, tile) == false)
            {
                Debug.Log("Invalid Position");
                return null;
            }
        }

        InstalledObject installedObject = InstalledObject.CreateInstalledObject(proto, position);

        InstalledObjects.Add(installedObject);

        GameObject GO = installedObjectVisuals.CreateInstalledObject(installedObject, position, parent);

        installedObject.SetGO(GO);
        foreach(TileGridObj tile in tiles)
        {
            tile.PlaceInstalledObject(installedObject);
            tile.SetIsWalkable(installedObject.IsWalkable);
        }

        return installedObject;
    }

    public void OnInstalledObjectRemoved(InstalledObject obj)
    {
        InstalledObjects.Remove(obj);
        installedObjectVisuals.OnInstalledObjectRemoved(obj);
    }
    
    public bool ValidatePosition(InstalledObject proto, TileGridObj tile)
    {
            if (tile == null || tile.installedObject != null)
            {
                return false;
            }

            bool isTileAcceptable = false;
            foreach(TileGridObj.TileType type in proto.AcceptableTiles)
            {
                if (tile.Type == type)
                {
                    isTileAcceptable = true;
                }
            }

        return isTileAcceptable;
    }
}
