using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public Transform TextPopUp;
    public Mesh TileMesh;
    public Material TileMaterial;
    public GameObject Error;
    public GameObject TilePrefab;
    public GameObject CameraRig;

    [Space]
    [Header("UI")]
    public RectTransform selectionBox;

    [Space]
    [Header("MiniMapColours")]
    public Material blue;
    public Material green;

    [Space]
    [Header("MapTiles")]
    public Material SAND;
    public Material GRASS;
    public Material ROCK;
    public Material SNOW;
    public Material WATER;

    [Space]
    [Header("Characters")]
    public GameObject Shamen;
    public GameObject MiniMapIcon;
    public GameObject NPC;

    [Space]
    [Header("InstalledObjects")]
    public GameObject House;
    public GameObject StoreHouse;
    public GameObject StoreHouseJob;
    public GameObject Storage;
    public GameObject StorageJob;

    [Space]
    [Header("Plants")]
    public GameObject PalmTree;

    public static GameAssets i;

    public void SetUp()
    {
        i = this;
    }

    public Material GetTileMaterial(TileGridObj.TileType type)
    {
        switch (type)
        {
            case TileGridObj.TileType.WATER:
                return WATER;
            case TileGridObj.TileType.SAND:
                return SAND;
            case TileGridObj.TileType.GRASS:
                return GRASS;
            case TileGridObj.TileType.ROCK:
                return ROCK;
            case TileGridObj.TileType.SNOW:
                return SNOW;
        }

        return Error.GetComponent<MeshRenderer>().material;
    }

    public GameObject GetInstalledObject(string key)
    {
        switch (key)
        {
            case "House":
                return House;
            case "Storage":
                return Storage;
            case "StorageJob":
                return StorageJob;
            case "StoreHouse":
                return StoreHouse;
            case "StoreHouseJob":
                return StoreHouseJob;
        }

        return Error;
    }

    public GameObject GetPlantObject(string key)
    {
        switch (key)
        {
            case "PalmTree":
                return PalmTree;
        }

        return Error;
    }
}
