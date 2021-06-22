using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IslandGenerator : MonoBehaviour
{
    public bool AutoUpdate = false;

    public float radius = 5;
    public float displayRadius = 2.5f;
    public Vector2 regeionSize = Vector2.one;
    public int rejectionSamples = 30;

    public GridUtil<IslandMapGridObj> IslandGrid;
    public List<IslandMapGridObj> Islands;

    public Material blue;
    public Material green;

    [Space]
    [Header("NoiseSettings")]
    public NoiseUtil.NormalizeMode NoiseNormaliseMode = NoiseUtil.NormalizeMode.Global;
    public int Seed = 8;
    public float NoiseMaxHeightMultiplaier = 2f;
    public float Scale = 100f;
    [Range(1, 20)]
    public int Octaves = 4;
    [Range(0, 1)]
    public float Persistance = 0.28f;
    public float Lacunarity = 1f;
    public bool useFalloff = true;
    private float[,] fallOffMap;

    [Space]
    public TerrainPixel[] terrainPixels;

    public void SetUp()
    {
        fallOffMap = FalloffGenerator.GenerateFalloffMap(100);
    }

    void OnValidate()
    {
        //grid = new GridUtil<GridObj>(Mathf.FloorToInt(regeionSize.x), Mathf.FloorToInt(regeionSize.y), 1f, Vector3.zero, CreateGridObj);
        //points = PoissonDiscSampling.GeneratePoints(radius, regeionSize, rejectionSamples);
    }

    void OnDrawGizmos()
    {
        //Gizmos.DrawWireCube(regeionSize / 2, regeionSize); 
        //if(points != null)
        //{
        //    foreach(Vector2 point in points)
        //    {
        //        Gizmos.DrawSphere(point, displayRadius);
        //    }
        //}
    }

    public GridUtil<IslandMapGridObj> GenerateIslandMap(int chunkSize)
    {
        fallOffMap = FalloffGenerator.GenerateFalloffMap(chunkSize);

        IslandGrid = new GridUtil<IslandMapGridObj>(Mathf.FloorToInt(regeionSize.x), Mathf.FloorToInt(regeionSize.y), 1f, Vector3.zero, CreateGridObj);
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius, regeionSize, rejectionSamples);
        Islands = new List<IslandMapGridObj>();

        int width = IslandGrid.GetWidth();
        int height = IslandGrid.GetHeight();

        for (float x = 0; x < width ; x++)
        {
            for (float y = 0; y < height; y++)
            {
                int gridX = Mathf.RoundToInt(x);
                int gridY = Mathf.RoundToInt(y);
                IslandMapGridObj obj =  IslandGrid.GetGridObject(gridX, gridY);
                obj.SetRegion(new Vector2(x * chunkSize, y * chunkSize), new Vector2((x * chunkSize) + (chunkSize - 1), (y * chunkSize) + (chunkSize - 1)));
                IslandGrid.SetGridObject(gridX, gridY, obj);
            }
        }

        foreach(Vector2 point in points)
        {
            int x = Mathf.FloorToInt(point.x);
            int y = Mathf.FloorToInt(point.y);
            IslandMapGridObj obj = IslandGrid.GetGridObject(x, y);
            MapData mapData = GenerateMapData(chunkSize, chunkSize, point, obj.RegionStart);

            obj.SetTileType(IslandMapGridObj.TileType.LAND);
            obj.SetMapData(mapData);
            obj.SetPosition(new Vector2(x, y));
            IslandGrid.SetGridObject(x, y, obj);
            Islands.Add(obj);
        }

        for (int x = 0; x < IslandGrid.GetWidth(); x++)
        {
            for (int y = 0; y < IslandGrid.GetHeight(); y++)
            {
                IslandMapGridObj obj = IslandGrid.GetGridObject(x, y);

                if(obj.Type == IslandMapGridObj.TileType.WATER)
                {
                    MapData mapData = GenerateWaterMapData(chunkSize, chunkSize, obj.RegionStart);

                    obj.SetMapData(mapData);
                    obj.SetPosition(new Vector2(x, y));
                }
            }
        }

        return IslandGrid;
    }

    MapData GenerateMapData(int width, int height, Vector2 centre, Vector2 regionStart)
    {
        float[,] noiseMap = NoiseUtil.GenerateNoiseMap(height, width, new NoiseSettings(NoiseNormaliseMode, GenerateSeed(centre), NoiseMaxHeightMultiplaier, Scale, Octaves, Persistance, Lacunarity, centre), Vector2.one);
        Color[] colourMap = new Color[width * height];
        GridUtil<TileGridObj> grid = new GridUtil<TileGridObj>(width, height, 1f, Vector3.zero, CreateTileGridObj);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOffMap[x, y]);
                }
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < terrainPixels.Length; i++)
                {
                    if (currentHeight >= terrainPixels[i].height)
                    {
                        colourMap[y * width + x] = terrainPixels[i].colour;
                        SetGridOBj(x, y, grid, terrainPixels[i], regionStart);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return new MapData(noiseMap, colourMap, grid);
    }

    MapData GenerateWaterMapData(int width, int height, Vector2 regionStart)
    {
        float[,] noiseMap = new float[width, height];
        Color[] colourMap = new Color[width * height];
        GridUtil<TileGridObj> grid = new GridUtil<TileGridObj>(width, height, 1f, Vector3.zero, CreateTileGridObj);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                TileGridObj obj = grid.GetGridObject(x, y);
                obj.SetPosition(new Vector2(regionStart.x + x, regionStart.y + y));
                grid.SetGridObject(x, y, obj);
            }
        }

        return new MapData(noiseMap, colourMap, grid);
    }

    void SetGridOBj(int x, int y, GridUtil<TileGridObj> grid, TerrainPixel terrainPixel, Vector2 regionStart)
    {
        TileGridObj obj = grid.GetGridObject(x, y);
        switch (terrainPixel.name)
        {
            case "Water":
                obj.SetTileType(TileGridObj.TileType.WATER);
                break;
            case "Sand":
                obj.SetTileType(TileGridObj.TileType.SAND);
                break;
            case "Grass":
                obj.SetTileType(TileGridObj.TileType.GRASS);
                break;
            case "Rock":
                obj.SetTileType(TileGridObj.TileType.ROCK);
                break;
            case "Snow":
                obj.SetTileType(TileGridObj.TileType.SNOW);
                break;
        }
        obj.SetPosition(new Vector2(regionStart.x + x, regionStart.y + y));

        grid.SetGridObject(x, y, obj);
    }

    int GenerateSeed(Vector2 centre)
    {
        int seed = Seed;
        DateTime dateTime = DateTime.Now;

        float random = UnityEngine.Random.Range(Mathf.Min(centre.x, centre.y), Mathf.Max(centre.x, centre.y));

        if(dateTime.Day <= 10)
        {
            random = (random * dateTime.Second) / UnityEngine.Random.Range(Mathf.Min(centre.x, centre.y), Mathf.Max(centre.x, centre.y));
            seed = Mathf.FloorToInt(((seed * random) / ((seed + dateTime.Minute) - dateTime.Hour)) * (centre.x * centre.y));
        }
        else if(dateTime.Day <= 20)
        {
            random = (random * dateTime.Minute) / UnityEngine.Random.Range(Mathf.Min(dateTime.Millisecond, dateTime.Minute), Mathf.Max(dateTime.Millisecond, dateTime.Minute));
            seed = Mathf.FloorToInt(((seed * random) / ((seed + dateTime.Second) - dateTime.Day)) * (centre.x * centre.y));
        }
        else if(dateTime.Day <= 30)
        {
            random = (random * dateTime.Second) / UnityEngine.Random.Range(Mathf.Min(dateTime.Hour, dateTime.Second), Mathf.Max(dateTime.Hour, dateTime.Second));
            seed = Mathf.FloorToInt(((seed * random) / ((seed + dateTime.Hour) - dateTime.Day)) * (centre.x * centre.y));
        }
        else
        {
            random = (random * dateTime.Millisecond) / UnityEngine.Random.Range(centre.y, centre.x);
            seed = Mathf.FloorToInt(((seed * random) / ((seed + dateTime.Second) - dateTime.Minute)) * (centre.x * centre.y));
        }

        return seed;
    }

    TileGridObj CreateTileGridObj()
    {
        TileGridObj obj = new TileGridObj();
        obj.SetTileType(TileGridObj.TileType.WATER);
        return obj;
    }

    IslandMapGridObj CreateGridObj()
    {
        IslandMapGridObj obj = new IslandMapGridObj();
        obj.SetTileType(IslandMapGridObj.TileType.WATER);
        return obj;
    }
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;
    public readonly GridUtil<TileGridObj> grid;

    public MapData(float[,] heightMap, Color[] colourMap, GridUtil<TileGridObj> grid)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
        this.grid = grid;
    }
}

public class TileGridObj
{
    public enum TileType
    {
        WATER,
        SAND,
        GRASS,
        ROCK,
        SNOW
    }
    public TileType Type { get; protected set; }
    private bool isWalkable;
    public Vector2 Position { get; protected set; }
    public GameObject gameObject { get; protected set;}
    public InstalledObject installedObject { get; protected set; }
    public Plant plant { get; protected set; }

    public TileGridObj()
    {
        this.isWalkable = true;
        this.Type = TileType.WATER;
    }

    public void PlaceInstalledObject(InstalledObject installedObject)
    {
        this.installedObject = installedObject;
    }

    public void PlacePlant(Plant plant)
    {
        this.plant = plant;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public bool IsSailable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool value)
    {
        isWalkable = value;
    }

    public void SetTileType(TileType type)
    {
        Type = type;
    }

    public void SetPosition(Vector2 position)
    {
        this.Position = position;
    }

    public void SetGameObjct(GameObject gameObject)
    {
        this.gameObject = gameObject;
    }

    public override string ToString()
    {
        return isWalkable.ToString();
    }
}

public class IslandMapGridObj
{
    public enum TileType
    {
        LAND,
        WATER
    }
    public TileType Type { get; protected set; }
    private bool isWalkable;
    public Vector2 Position { get; protected set; }
    public Vector2 RegionStart { get; protected set; }
    public Vector2 RegionEnd { get; protected set; }
    public MapData MapData { get; protected set; }

    public IslandMapGridObj()
    {
        this.isWalkable = true;
        this.Type = TileType.WATER;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool value)
    {
        isWalkable = value;
    }

    public void SetTileType(TileType type)
    {
        Type = type;
    }

    public void SetRegion(Vector2 start, Vector2 end)
    {
        RegionStart = start;
        RegionEnd = end;
    }

    public void SetMapData(MapData mapData)
    {
        this.MapData = mapData;
    }

    public void SetPosition(Vector2 position)
    {
        this.Position = position;
    }

    public override string ToString()
    {
        return isWalkable.ToString();
    }
}
