using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class WorldController : MonoBehaviour 
{
    public static WorldController current;
    public EntityManager EntityManager;
    public MapManager mapManager;
    public InstalledObjectManager installedObjectManager;
    public PlantManager plantManager;
    public PlayerController playerController;
    public JobManager jobManager;
    public InventoryManager inventoryManager;
    public NPCManager dudeManager;
    public UIManager uiManager;

    public PathFinding pathfinding;
    public List<PathJob> findPathJobsList;
    public GridUtil<TileGridObj> bigDaddyGrid;

    [Space]
    [Header("SpawnLocation")]
    public int2 SpawnAreaSize = new int2(5, 5);
    public List<TileGridObj.TileType> AcceptableSpawnTiles = new List<TileGridObj.TileType> { TileGridObj.TileType.SAND, TileGridObj.TileType.GRASS };
    public int2 SpawnBorder = new int2(1, 1);

    public void SetUp()
    {
        current = this;

        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        findPathJobsList = new List<PathJob>();

        mapManager              = FindObjectOfType<MapManager>();
        installedObjectManager  = FindObjectOfType<InstalledObjectManager>();
        plantManager            = FindObjectOfType<PlantManager>();
        jobManager              = FindObjectOfType<JobManager>();
        inventoryManager        = FindObjectOfType<InventoryManager>();
        playerController        = FindObjectOfType<PlayerController>();
        dudeManager             = FindObjectOfType<NPCManager>();
        uiManager               = FindObjectOfType<UIManager>();

        mapManager.SetUp(EntityManager);
        dudeManager.SetUp(EntityManager);
        uiManager.SetUp();

        mapManager.BuildMap();

        bigDaddyGrid = new GridUtil<TileGridObj>(mapManager.IslandMap.GetWidth() * mapManager.ChunkSize, mapManager.IslandMap.GetHeight() * mapManager.ChunkSize, 1f, mapManager.mapStart, CreateTileGridObj);
        

        for (int x = 0; x < bigDaddyGrid.GetWidth(); x++)
        {
            for (int y = 0; y < bigDaddyGrid.GetHeight(); y++)
            {
                TileGridObj obj = mapManager.GetIslandTileGridObjFromWorldPos(x, y);
                if(obj.Type == TileGridObj.TileType.WATER || obj.Type == TileGridObj.TileType.ROCK || obj.Type == TileGridObj.TileType.SNOW)
                {
                    obj.SetIsWalkable(false);
                }
                bigDaddyGrid.SetGridObject(x, y, obj);
            }
        }

        bigDaddyGrid.RegisterOnGridObjectChangedCallback(OnTileObjChanged);

        pathfinding = new PathFinding();

        Vector3 areaWeWantToSpawn = new Vector3(bigDaddyGrid.GetWidth() / 2, bigDaddyGrid.GetHeight() / 2);

        int2 SpawnLocation = FindSpawnLocation(areaWeWantToSpawn, SpawnAreaSize, AcceptableSpawnTiles);

        Spawn(SpawnLocation);

        List<int2> usedLocations = new List<int2>();
        for (int x = SpawnLocation.x; x < SpawnLocation.x + SpawnAreaSize.x; x++)
        {
            for (int y = SpawnLocation.y; y < SpawnLocation.y + SpawnAreaSize.y; y++)
            {
                usedLocations.Add(new int2(x, y)); //listing areas no to spawn things on
            }
        }

        foreach(IslandMapGridObj island in mapManager.Islands)
        {
            List<Vector2> points = PoissonDiscSampling.GeneratePoints(1.8f, new Vector2(mapManager.ChunkSize, mapManager.ChunkSize), 4);
            List<Vector2> actualPoints = new List<Vector2>();

            Vector2 regionStart = island.RegionStart;

            foreach(Vector2 point in points)
            {
                Vector2 tempPoint = regionStart + point;
                int2 p = new int2(Mathf.RoundToInt(tempPoint.x), Mathf.RoundToInt(tempPoint.y));
                TileGridObj gridObj = bigDaddyGrid.GetGridObject(p.x, p.y);
                if(gridObj == null || (gridObj.Type != TileGridObj.TileType.GRASS && gridObj.Type != TileGridObj.TileType.SAND))
                {
                    continue;
                }

                if(gridObj.Type == TileGridObj.TileType.SAND)
                {
                    int random = UnityEngine.Random.Range(1, 5);
                    if(random != 4)
                    {
                        continue;
                    }
                }

                if (usedLocations.Contains(RoundPositionToInt(tempPoint)) == true)
                {
                    continue;
                }

                actualPoints.Add(tempPoint);
            }

            foreach(Vector2 point in actualPoints)
            {
                PlacePlant("PalmTree", point);
            }
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        mapManager.UpdateMapManager(deltaTime);
        playerController.FrameUpdate(deltaTime);
        dudeManager.FrameUpdate(deltaTime);

        if(findPathJobsList.Count > 0)
        {
            int2 gridSize = new int2(bigDaddyGrid.GetWidth(), bigDaddyGrid.GetHeight());
            PathNode[] pathNodeArray = pathfinding.GetPathNodeArray();

            foreach (PathJob pathJob in findPathJobsList)
            {
                ThreadedDataRequester.RequestData(() => PathFinding.FindPathJob(pathJob.startPoint, pathJob.endPoint, pathNodeArray, gridSize), pathJob.callBack);
            }

            findPathJobsList.Clear();
        }
    }

    public void Spawn(int2 spawnLocation)
    {
        float2 halfSpawnArea = (SpawnAreaSize - (SpawnBorder * 2)) / 2;

        int2 spawnStart = spawnLocation + SpawnBorder;
        int2 spawnEnd = spawnLocation + (SpawnAreaSize - (SpawnBorder));
        
        int2 playerPosition = new int2(Mathf.RoundToInt(spawnStart.x + halfSpawnArea.x), Mathf.RoundToInt(spawnStart.y + halfSpawnArea.y));

        for (int x = spawnStart.x; x < spawnEnd.x ; x++)
        {
            for (int y = spawnStart.y; y < spawnEnd.y; y++)
            {
                int2 point = new int2(x, y);
                if(point.x == playerPosition.x && point.y == playerPosition.y)
                {
                    BuildPlayer(playerPosition);
                }
                else
                {
                    BuildNPC(point);
                }
            }
        }
    }

    public void BuildPlayer(int2 pos)
    {
        Vector2 position = new Vector2(pos.x, pos.y);

        Player player = playerController.BuildPlayer(position);

        mapManager.ViewerTransform = player.gameObject.transform;

        mapManager.mapVisuals.UpdateVisableTiles(new Vector3(position.x, position.y));
    }

    public void BuildNPC(int2 position)
    {
        dudeManager.CreateNPC("Bob", position);
    }

    public void PlacePlant(string type, Vector2 position)
    {
        PlacePlant(type, RoundPositionToInt(position));
    }
    public void PlacePlant(string type, Vector3 position)
    {
        PlacePlant(type, RoundPositionToInt(position));
    }
    public void PlacePlant(string type, int2 position)
    {
        Plant plant = plantManager.CreatePlant(type, position);
        
        TileGridObj gridObj = bigDaddyGrid.GetGridObject(position.x, position.y);

        gridObj.PlacePlant(plant);
        gridObj.SetIsWalkable(plant.IsWalkable);

        GameObject parent = mapManager.GetTileObject(position.x, position.y);

        if(parent != null)
        {
            plantManager.plantVisuals.CreatePlant(plant, parent.transform.position, parent.transform);
        }
    }

    public int2 FindItem(string item, Vector3 position)
    {
        //TODO: get reference to objs belonging to this village

        Dictionary<int2, InstalledObject> placesToLook = new Dictionary<int2, InstalledObject>();

        for (int i = 0; i < installedObjectManager.InstalledObjects.Count; i++)
        {
            InstalledObject obj = installedObjectManager.InstalledObjects[i];

            if(obj.Type == InstalledObject.ObjectType.Storage)
            {

                //obj.inventory
                placesToLook.Add(RoundPositionToInt(obj.Position), obj);
            }
        }

        return new int2(0, 0);
    }

    public void CutDown(Vector3 position)
    {
        int2 pos = RoundPositionToInt(position);

        TileGridObj gridObj = bigDaddyGrid.GetGridObject(pos.x, pos.y);
        
        if(gridObj == null || gridObj.plant == null)
        {
            return;
        }
        Plant plant = gridObj.plant;

        string harvestItem = null;
        int harvestQuantity = 0;

        if(plant.ReadyToHarvest() == true)
        {
            harvestItem = plant.HarvestItem;
            harvestQuantity = plant.HarvestQuantity;
        }

        int cutDownQuantity = plant.GrowthStage;
        string cutDownItem = plantManager.CutDown(plant);

        InventoryItem removeItem = inventoryManager.CreateItem(cutDownItem, cutDownQuantity);
        InventoryItem harvItem = null;

        int capacity = 1;
        if(harvestItem != null)
        {
            capacity++;
            harvItem = inventoryManager.CreateItem(harvestItem, harvestQuantity);
        }

        InventoryItem[] items = new InventoryItem[capacity];

        items[0] = removeItem;
        if(harvItem != null)
        {
            items[1] = harvItem;
        }

        GameObject parent = gridObj.gameObject;
        Inventory tempInv = inventoryManager.CreateInventory(capacity, Inventory.Type.Temp, "Temp", items, parent.transform);
        gridObj.SetIsWalkable(true);
    }

    public void PlaceJob(string jobName, int2 pos)
    {
        if (installedObjectManager.InstalledObjectPrototypes.ContainsKey(jobName) == false)
        {
            Debug.Log("InstalledObjectProtos dose not contain " + jobName);
            return;
        }

        List<TileGridObj> tileObjsList = new List<TileGridObj>();
        InstalledObject proto = installedObjectManager.InstalledObjectPrototypes[jobName];

        bool IsPositionOk = true; //First Check the object can be placed

        for (int x =  pos.x; x < pos.x + proto.Size.x; x++)
        {
            for (int y = pos.y; y < pos.y + proto.Size.y; y++)
            {
                TileGridObj obj = bigDaddyGrid.GetGridObject(pos.x, pos.y);

                if(obj == null)
                {
                    IsPositionOk = false;
                    continue;
                }

                if (installedObjectManager.ValidatePosition(proto, obj) == false)
                {
                    IsPositionOk = false;
                    continue;
                }

                tileObjsList.Add(obj);
            }
        }

        if(IsPositionOk == true) //Now Check the tile for the stockpile is ok
        {
            TileGridObj obj = bigDaddyGrid.GetGridObject(pos.x - 1, pos.y);
            if(obj == null)
            {
                return;
            }
            if(obj.IsWalkable() == false)
            {
                return;
            }

            tileObjsList.Add(obj);


        }
        else
        {
            Debug.Log("Cannot Place Here");
            return;
        }
        //If we get here we are ok to place the job.

        jobManager.CreateJob(jobName, pos);
    }

    public void PlaceTask(Task.Type type, Vector3 position)
    {
        PlaceTask(type, RoundPositionToInt(position));
    }
    public void PlaceTask(Task.Type type, Vector2 position)
    {
        PlaceTask(type, RoundPositionToInt(position));
    }
    public void PlaceTask(Task.Type type, int2 position)
    {
        TileGridObj gridObj = bigDaddyGrid.GetGridObject(position.x, position.y);

        if(gridObj == null)
        {
            return;
        }

        switch (type)
        {
            case Task.Type.Cut:
                if(gridObj.plant == null)
                {
                    return;
                }
                Task task = jobManager.CreateTask(type, new Vector3(position.x, position.y));
                jobManager.ReQueueTask(task);
                break;
        }
    }

    public void CreateInstalledObject(string installedObjectName, Vector2 position, Job job = null)
    {
        if(job != null)
        {
            Debug.Log("Removing Job");
            jobManager.RemoveJob(job);
        }

        installedObjectManager.CreateInstalledObject(installedObjectName, position, mapManager.GetTileGridObjFromWorldPos(position).gameObject.transform);
    }

    public GridUtil<TileGridObj> GetIslandGrid(int index)
    {
        return mapManager.Islands[index].MapData.grid;
    }

    GridObj gridObj()
    {
        return new GridObj();
    }

    TileGridObj CreateTileGridObj()
    {
        TileGridObj obj = new TileGridObj();
        obj.SetTileType(TileGridObj.TileType.WATER);
        return obj;
    }

    void OnTileObjChanged(int x, int y)
    {
        TileGridObj ogObj = bigDaddyGrid.GetGridObject(x, y);
        TileGridObj toChangeObj = mapManager.GetIslandTileGridObjFromWorldPos(x, y);
        toChangeObj.SetTileType(ogObj.Type);
        toChangeObj.SetIsWalkable(ogObj.IsWalkable());
        if(ogObj.installedObject != null)
        {
            toChangeObj.PlaceInstalledObject(ogObj.installedObject);
        }
    }

    int2 FindSpawnLocation(Vector3 positionToBeginSearch, int2 areaSize, List<TileGridObj.TileType> acceptableTiles)
    {
        Dictionary<float, IslandMapGridObj> candidateIslands = new Dictionary<float, IslandMapGridObj>();
        float key = float.PositiveInfinity;

        foreach (IslandMapGridObj island in mapManager.Islands)
        {
            float distance = Vector3.Distance(positionToBeginSearch, island.RegionStart + (island.RegionEnd - island.RegionStart));

            while(candidateIslands.ContainsKey(distance) == true)
            {
                distance += 0.01f;
            }

            candidateIslands.Add(distance, island);
        }

        int numSamplesBeforeRejection = 30;
        while (candidateIslands.Count > 0)
        {
            foreach (float distance in candidateIslands.Keys)
            {
                if (distance < key)
                {
                    key = distance;
                }
            }
            IslandMapGridObj candidateIsland = candidateIslands[key];
            Vector2 CheckLocation = candidateIsland.RegionStart + (candidateIsland.RegionEnd - candidateIsland.RegionStart);

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = UnityEngine.Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)); //these 2 lines pick a direction
                Vector2 candidatePointVector2 = CheckLocation + dir * UnityEngine.Random.Range(0, candidateIslands[key].MapData.grid.GetWidth() / 2);//this line picks a point along that direction to place our point on.
                int2 candidatePoint = new int2(Mathf.RoundToInt(candidatePointVector2.x), Mathf.RoundToInt(candidatePointVector2.y));

                if (IsValid(candidatePoint) == true)
                {
                    return candidatePoint;
                }
            }
        }

        //If We are here we cant find any location anywhere on the map.
        Debug.LogError("No Location found on map");
        return new int2(0,0);

        bool IsValid(int2 position)
        {
            List<TileGridObj> checkTiles = new List<TileGridObj>();
            for (int x = 0; x < areaSize.x; x++)
            {
                for (int y = 0; y < areaSize.y; y++)
                {
                    TileGridObj checkTile = bigDaddyGrid.GetGridObject(position.x + x, position.y + y);

                    if(checkTile == null)
                    {
                        return false;
                    }

                    checkTiles.Add(checkTile);
                }
            }

            foreach(TileGridObj checkTile in checkTiles)
            {
                bool isValid = false;
                foreach (TileGridObj.TileType type in acceptableTiles)
                {
                    if (checkTile.Type == type)
                    {
                        isValid = true;
                    }
                }

                if(isValid == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
    
    public int2 RoundPositionToInt(Vector2 position)
    {
        return RoundPositionToInt(new Vector3(position.x, position.y));
    }
    public int2 RoundPositionToInt(Vector3 position)
    {
        return new int2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    public GameObject InstantiateForMe(GameObject go)
    {
        return Instantiate(go);
    }
}
