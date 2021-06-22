using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

public class MapVisuals
{
    EntityManager entityManager;
    EntityArchetype tileArchetype;

    public GameObject MapParent;
    public float maxViewDistance = 15f;
    int tileSize = 1;
    int tilesVisableinViewDistance;

    int mapSize;
    Vector3 mapStart;

    private Dictionary<TileGridObj, Entity> visualEntitys;
    private Dictionary<TileGridObj, GameObject> tileGameObjects;
    private Dictionary<Vector2, TileGridObj> tilesByPositionDictionary;

    Dictionary<int2, string> neighbourDictionary;

    List<TileGridObj> tilesVisableLastUpdate;

    public void SetUp(EntityManager entityManager)
    {
        this.entityManager = entityManager;
        tileArchetype = entityManager.CreateArchetype(typeof(RenderBounds), typeof(RenderMesh), typeof(LocalToWorld), typeof(Translation));
        visualEntitys = new Dictionary<TileGridObj, Entity>();
        tileGameObjects = new Dictionary<TileGridObj, GameObject>();
        tilesByPositionDictionary = new Dictionary<Vector2, TileGridObj>();
        tilesVisableLastUpdate = new List<TileGridObj>();
        tilesVisableinViewDistance = Mathf.RoundToInt(maxViewDistance / tileSize);

        MapParent = new GameObject("Map");
        MapParent.transform.position = Vector3.zero;

        neighbourDictionary = new Dictionary<int2, string>();
        neighbourDictionary.Add(new int2(0, 1), "_N");
        neighbourDictionary.Add(new int2(1, 0), "_E");
        neighbourDictionary.Add(new int2(0, -1), "_S");
        neighbourDictionary.Add(new int2(-1, 0), "_W");
        neighbourDictionary.Add(new int2(1, 1), "_NE");
        neighbourDictionary.Add(new int2(1, -1), "_SE");
        neighbourDictionary.Add(new int2(-1, -1), "_SW");
        neighbourDictionary.Add(new int2(-1, 1), "_NW");
    }

    public void UpdateVisableTiles(Vector2 viewerPosition)
    {
        for (int i = 0; i < tilesVisableLastUpdate.Count; i++)
        {
            OnIsVisableChanged(tilesVisableLastUpdate[i], false);
        }
        tilesVisableLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / tileSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / tileSize);

        for (int yOffset = -tilesVisableinViewDistance; yOffset < tilesVisableinViewDistance; yOffset++)
        {
            for (int xOffset = -tilesVisableinViewDistance; xOffset < tilesVisableinViewDistance; xOffset++)
            {
                Vector2 viewedTileCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (tilesByPositionDictionary.ContainsKey(viewedTileCoord))
                {
                    float viewerDistanceFromTile = Vector2.Distance(viewerPosition, tilesByPositionDictionary[viewedTileCoord].Position);
                    bool visable = viewerDistanceFromTile <= maxViewDistance;
                    OnIsVisableChanged(tilesByPositionDictionary[viewedTileCoord], visable);
                    if(visable == true)
                    {
                        tilesVisableLastUpdate.Add(tilesByPositionDictionary[viewedTileCoord]);
                    }
                }
                else
                {
                    CreateTile(viewedTileCoord);
                }
            }
        }
    }

    public GameObject GetTileObject(Vector3 position)
    {
        return GetTileObject(new Vector2(position.x, position.y));
    }
    public GameObject GetTileObject(int x, int y)
    {
        return GetTileObject(new Vector2(x, y));
    }
    public GameObject GetTileObject(Vector2 position)
    {
        if (tilesByPositionDictionary.ContainsKey(position) == false)
        {
            return null;
        }
        if (tileGameObjects.ContainsKey(tilesByPositionDictionary[position]) == false)
        {
            return null;
        }
        return tileGameObjects[tilesByPositionDictionary[position]];
    }
    
    public void BuildVisuals(GridUtil<GridObj> grid)
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                GridObj gridObj = grid.GetGridObject(x, y);
                Entity entity = entityManager.Instantiate(PrefabEntities.PrefabEntity["Tile"]);

                //set material
                Material newMaterial = GameAssets.i.TileMaterial;
                newMaterial.mainTexture = SpriteManager.current.GetSprite(SpriteManager.SpriteCatagory.Tiles, gridObj.Type.ToString()).texture;

                //add material to mesh
                RenderMesh renderMesh = entityManager.GetSharedComponentData<RenderMesh>(entity);
                renderMesh.material = newMaterial;

                //put mesh back
                entityManager.SetSharedComponentData<RenderMesh>(entity, renderMesh);

                //SetPosition
                entityManager.SetComponentData<Translation>(entity, new Translation { Value = new float3(x, y, 0f) });
                
            }
        }
    }

    public void BuildMapVisuals(GridUtil<IslandMapGridObj> grid, int chunkSize, Vector3 mapStart, int mapSize)
    {
        this.mapStart = mapStart;
        this.mapSize = mapSize;

        Transform parent = new GameObject("MiniMap").transform;
        parent.position = Vector3.zero;

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                IslandMapGridObj obj = grid.GetGridObject(x, y);
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.transform.position = new Vector3(x * chunkSize + (chunkSize/2) - 0.5f, y * chunkSize + (chunkSize / 2) - 0.5f);
                go.transform.SetParent(parent);
                go.transform.localScale = Vector3.one * chunkSize;
                go.layer = 13;

                if (obj.Type == IslandMapGridObj.TileType.WATER)
                {
                    go.GetComponent<MeshRenderer>().material = new Material(GameAssets.i.blue);
                    go.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = TextureGenerator.TextureFromColourMap(new Color[1] { Color.blue }, 1, 1);
                }
                else if (obj.Type == IslandMapGridObj.TileType.LAND)
                {
                    go.GetComponent<MeshRenderer>().material = new Material(GameAssets.i.green);
                    go.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = TextureGenerator.TextureFromColourMap(obj.MapData.colourMap, obj.MapData.heightMap.GetLength(0), obj.MapData.heightMap.GetLength(1));
                }
                else
                {
                    Debug.Log(obj.Type + " Not found");
                }
            }
        }
    }

    public void BuildIslandVisuals(IslandMapGridObj islandMapGridObj)
    {
        GridUtil<TileGridObj> grid = islandMapGridObj.MapData.grid;
        int width = grid.GetWidth();
        int height = grid.GetHeight();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileGridObj obj = grid.GetGridObject(x, y);

                obj.SetPosition(new Vector2(islandMapGridObj.RegionStart.x + x, islandMapGridObj.RegionStart.y + y));
                
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = "Tile " + obj.Position.x + "," + obj.Position.y;
                Material material = go.GetComponent<MeshRenderer>().material = new Material(GameAssets.i.TileMaterial);
                material.mainTexture = SpriteManager.current.GetSprite(SpriteManager.SpriteCatagory.Tiles, obj.Type.ToString()).texture;

                go.transform.position = new Vector3(obj.Position.x, obj.Position.y, 0);
                go.SetActive(false);

                tileGameObjects.Add(obj, go);
                tilesByPositionDictionary.Add(new Vector2(obj.Position.x, obj.Position.y), obj);
            }
        }

    }

    void OnIsVisableChanged(TileGridObj obj, bool isVisable)
    {
        tileGameObjects[obj].SetActive(isVisable);
    }
    
    public void CreateTile(Vector2 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);

        TileGridObj obj = WorldController.current.bigDaddyGrid.GetGridObject(x, y);
        if(obj == null)
        {
            Debug.Log("Null");
            return;
        }

        GameObject go = WorldController.current.InstantiateForMe(GameAssets.i.TilePrefab);
        
        go.transform.SetParent(MapParent.transform);
        go.name = "Tile " + obj.Position.x + "," + obj.Position.y;
        go.transform.position = new Vector3(obj.Position.x, obj.Position.y, 0);

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

        string spriteName = GetTileSpriteName(obj);
        sr.sprite = SpriteManager.current.GetSprite(SpriteManager.SpriteCatagory.Tiles, spriteName);

        go.SetActive(true);
        obj.SetGameObjct(go);

        if(obj.plant != null)
        {
            WorldController.current.plantManager.plantVisuals.CreatePlant(obj.plant, go.transform.position, go.transform);
        }

        OnTileCreated(obj);
    }

    public void OnTileCreated(TileGridObj gridObj)
    {
        GameObject go = gridObj.gameObject;

        tileGameObjects.Add(gridObj, go);
        tilesByPositionDictionary.Add(new Vector2(gridObj.Position.x, gridObj.Position.y), gridObj);
        tilesVisableLastUpdate.Add(gridObj);
    }

    bool IsInMap(Vector3 position)
    {
        return IsInMap(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
    bool IsInMap(Vector2 position)
    {
        return IsInMap(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
    bool IsInMap(int x, int y)
    {
        if (x < mapStart.x || x >= (mapStart.x + mapSize) || y < mapStart.y || y >= (mapStart.y + mapSize))
        {
            return false;
        }

        return true;
    }
    
    public string GetTileSpriteName(TileGridObj tileObj)
    {
        if(tileObj.Type == TileGridObj.TileType.WATER ||tileObj.Type == TileGridObj.TileType.SNOW)
        {
            return tileObj.Type.ToString();
        }
        string name = tileObj.Type.ToString();

        int2 pos = WorldController.current.RoundPositionToInt(tileObj.Position);
        int2 offset = new int2(0, 0);
        TileGridObj neighbourObj;

        bool n = false;
        bool e = false;
        bool s = false;
        bool w = false;

        offset = new int2(0, 1);
        neighbourObj = WorldController.current.bigDaddyGrid.GetGridObject(pos.x + offset.x, pos.y + offset.y);
        if(neighbourObj != null && isCorrectType(neighbourObj.Type) == true)
        {
            n = true;
            name += neighbourDictionary[offset];
        }
        offset = new int2(1, 0);
        neighbourObj = WorldController.current.bigDaddyGrid.GetGridObject(pos.x + offset.x, pos.y + offset.y);
        if (neighbourObj != null && isCorrectType(neighbourObj.Type) == true)
        {
            e = true;
            name += neighbourDictionary[offset];
        }
        offset = new int2(0, -1);
        neighbourObj = WorldController.current.bigDaddyGrid.GetGridObject(pos.x + offset.x, pos.y + offset.y);
        if (neighbourObj != null && isCorrectType(neighbourObj.Type) == true)
        {
            s = true;
            name += neighbourDictionary[offset];
        }
        offset = new int2(-1, 0);
        neighbourObj = WorldController.current.bigDaddyGrid.GetGridObject(pos.x + offset.x, pos.y + offset.y);
        if (neighbourObj != null && isCorrectType(neighbourObj.Type) == true)
        {
            w = true;
            name += neighbourDictionary[offset];
        }

        if(n == true && e == true)
        {
            offset = new int2(1, 1);
            neighbourObj = WorldController.current.bigDaddyGrid.GetGridObject(pos.x + offset.x, pos.y + offset.y);
            if (neighbourObj != null && isCorrectType(neighbourObj.Type) == true)
            {
                name += neighbourDictionary[offset];
            }
        }
        if (e == true && s == true)
        {
            offset = new int2(1, -1);
            neighbourObj = WorldController.current.bigDaddyGrid.GetGridObject(pos.x + offset.x, pos.y + offset.y);
            if (neighbourObj != null && isCorrectType(neighbourObj.Type) == true)
            {
                name += neighbourDictionary[offset];
            }
        }
        if (w == true && s == true)
        {
            offset = new int2(-1, -1);
            neighbourObj = WorldController.current.bigDaddyGrid.GetGridObject(pos.x + offset.x, pos.y + offset.y);
            if (neighbourObj != null && isCorrectType(neighbourObj.Type) == true)
            {
                name += neighbourDictionary[offset];
            }
        }
        if (w == true && n == true)
        {
            offset = new int2(-1, 1);
            neighbourObj = WorldController.current.bigDaddyGrid.GetGridObject(pos.x + offset.x, pos.y + offset.y);
            if (neighbourObj != null && isCorrectType(neighbourObj.Type) == true)
            {
                name += neighbourDictionary[offset];
            }
        }


        return name;

        bool isCorrectType(TileGridObj.TileType neighbourType)
        {
            switch (tileObj.Type)
            {
                case TileGridObj.TileType.SAND:
                    if (neighbourType == TileGridObj.TileType.SAND || neighbourType == TileGridObj.TileType.GRASS)
                    {
                        return true;
                    }
                    break;
                case TileGridObj.TileType.GRASS:
                    if (neighbourType == TileGridObj.TileType.GRASS || neighbourType == TileGridObj.TileType.ROCK)
                    {
                        return true;
                    }
                    break;
                case TileGridObj.TileType.ROCK:
                    if (neighbourType == TileGridObj.TileType.ROCK || neighbourType == TileGridObj.TileType.SNOW)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}
