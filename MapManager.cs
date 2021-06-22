using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class MapManager : MonoBehaviour 
{
    public bool UseAltRendering;

    IslandGenerator islandGenerator;
    public MapVisuals mapVisuals;

    public int ChunkSize = 100;
    public int mapSize;
    public Vector3 mapStart = Vector3.zero;

    public GridUtil<IslandMapGridObj> IslandMap;
    public List<IslandMapGridObj> Islands;

    public Dictionary<Vector2, IslandMapGridObj> IslandMapByRegion;

    public Transform ViewerTransform;
    private Vector2 viewerOldPosition;

    public void SetUp(EntityManager entityManager)
    {
        this.mapVisuals = new MapVisuals();
        this.mapVisuals.SetUp(entityManager);
        islandGenerator = FindObjectOfType<IslandGenerator>();
    }

    public void BuildMap()
    {
        IslandMapByRegion = new Dictionary<Vector2, IslandMapGridObj>();
        IslandMap = islandGenerator.GenerateIslandMap(ChunkSize);
        Islands = islandGenerator.Islands;

        mapSize = IslandMap.GetWidth() * ChunkSize;

        for (int x = 0; x < IslandMap.GetWidth(); x++)
        {
            for (int y = 0; y < IslandMap.GetHeight(); y++)
            {
                IslandMapGridObj obj = IslandMap.GetGridObject(x, y);
                IslandMapByRegion.Add(obj.RegionStart, obj);
            }
        }

        BuildMapVisuals();
    }

    public void UpdateMapManager(float deltaTime)
    {
        Vector2 viewerPosition = new Vector2(ViewerTransform.position.x, ViewerTransform.position.y);
        if(viewerPosition != viewerOldPosition)
        {
            mapVisuals.UpdateVisableTiles(viewerPosition);

            viewerOldPosition = viewerPosition;
        }
    }

    public void BuildMapVisuals()
    {
        mapVisuals.BuildMapVisuals(IslandMap, ChunkSize, mapStart, mapSize);
        
    }

    public void BuildIslandVisuals(int index)
    {
        mapVisuals.BuildIslandVisuals(Islands[index]);
    }

    public GameObject GetTileObject(Vector3 position)
    {
        return mapVisuals.GetTileObject(position);
    }
    public GameObject GetTileObject(Vector2 position)
    {
        return mapVisuals.GetTileObject(position);
    }
    public GameObject GetTileObject(int x, int y)
    {
        return mapVisuals.GetTileObject(x, y);
    }

    public bool IsInMap(Vector3 position)
    {
        return IsInMap(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
    public bool IsInMap(Vector2 position)
    {
        return IsInMap(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
    public bool IsInMap(int x, int y)
    {
        if(x < mapStart.x || x >= (mapStart.x + mapSize) || y < mapStart.y || y >= (mapStart.y + mapSize))
        {
            return false;
        }

        return true;
    }

    public TileGridObj GetIslandTileGridObjFromWorldPos(Vector3 position)
    {
        return GetIslandTileGridObjFromWorldPos(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
    public TileGridObj GetTileGridObjFromWorldPos(Vector2 position)
    {
        return GetIslandTileGridObjFromWorldPos(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
    public TileGridObj GetIslandTileGridObjFromWorldPos(int x, int y)
    {
        Vector2 position = new Vector2(x, y);
        if(IsInMap(x,y) == true)
        {
            foreach(Vector2 regionStart in IslandMapByRegion.Keys)
            {
                if(position.x >= regionStart.x && position.x < regionStart.x + ChunkSize && position.y >= regionStart.y && position.y < regionStart.y + ChunkSize)
                {
                    int keyX = Mathf.Abs(Mathf.RoundToInt(x - regionStart.x));
                    int keyY = Mathf.Abs(Mathf.RoundToInt(y - regionStart.y));
                    
                    return IslandMapByRegion[regionStart].MapData.grid.GetGridObject(keyX, keyY);
                }
            }
        }
        //if we get here we are out side of the map
        return null;
    }

}
