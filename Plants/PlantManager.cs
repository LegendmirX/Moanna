using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class PlantManager : MonoBehaviour
{
    public PlantVisualsController plantVisuals;

    public Dictionary<string, Plant> PlantPrototypes;

    public List<Plant> Plants;
    public List<Plant> PlantsToRemove;

    public void SetUp()
    {
        plantVisuals = FindObjectOfType<PlantVisualsController>();
        plantVisuals.SetUp();
        PlantPrototypes = PrototypeManager.BuildPlantPrototypes();
        Plants = new List<Plant>();
    }

    public Plant CreatePlant(string type, int2 position)
    {
        Plant proto = PlantPrototypes[type];

        if(proto == null)
        {
            Debug.Log("PlantProtos did not contain " + type);
            return null;
        }

        if(isValidPlacement(proto, position) == false)
        {
            return null;
        }

        Vector3 positionV3 = new Vector3(position.x, position.y);

        Plant plant = Plant.CreatePlant(proto, positionV3);

        Plants.Add(plant);

        return plant;
    }

    public void AgePlants(int days, Dictionary<Plant, bool> toAge)
    {
        List<Plant> plantsToChange = new List<Plant>();
        PlantsToRemove = new List<Plant>();
        
        foreach(Plant plant in toAge.Keys)
        {
            int stage = plant.GrowthStage;
            plant.GetOlder(days, toAge[plant]);
            if(plant.GrowthStage != stage)
            {
                plantsToChange.Add(plant);
            }
        }

        foreach(Plant plant in plantsToChange)
        {
            plantVisuals.OnPlantChanged(plant);
        }
    }

    public string CutDown(Plant plant)
    {
        string item = plant.RemovalItem;
        OnPlantRemoved(plant);
        return item;
    }

    public void OnPlantChange(Plant plant)
    {
        plantVisuals.OnPlantChanged(plant);
    }

    public void OnPlantRemoved(Plant plant)
    {
        Plants.Remove(plant);
        plantVisuals.OnPlantRemoved(plant);
    }

    bool isValidPlacement(Plant plant, int2 position)
    {
        TileGridObj gridObj = WorldController.current.bigDaddyGrid.GetGridObject(position.x, position.y);

        bool isTileAccepted = false;

        foreach(TileGridObj.TileType tileType in plant.AcceptableTiles)
        {
            if (gridObj.Type == tileType && gridObj.plant == null)
            {
                isTileAccepted = true;
            }
        }

        return isTileAccepted;
    }
}
