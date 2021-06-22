using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant
{
    public Vector3 Position { get; protected set; }

    public List<TileGridObj.TileType> AcceptableTiles { get; protected set; }

    public string Name { get; protected set; }
    public string HarvestItem { get; protected set; }
    public string RemovalItem { get; protected set; }
    public int HarvestQuantity { get; protected set; }
    public bool Regrows { get; protected set; }
    public int MaxGrowthStage { get; protected set; }
    public int GrowthStage { get; protected set; }

    public bool IsWalkable { get; protected set; }

    private int daysPerGrowthStage;
    private int growthStageTimer;
    
    #region BuildFuncs
    public Plant()
    {
        
    }

    protected Plant(Plant other)
    {
        this.Name               = other.Name;
        this.HarvestItem        = other.HarvestItem;
        this.RemovalItem        = other.RemovalItem;
        this.AcceptableTiles    = other.AcceptableTiles;
        this.IsWalkable         = other.IsWalkable;
        this.HarvestQuantity    = other.HarvestQuantity;
        this.Regrows            = other.Regrows;
        this.MaxGrowthStage     = other.MaxGrowthStage;
        this.daysPerGrowthStage = other.daysPerGrowthStage;
    }

    virtual public Plant Clone()
    {
        return new Plant(this);
    }

    static public Plant CreatePrototype(string plantType, string harvestItem, string removalItem, List<TileGridObj.TileType> acceptableTiles, bool isWalkable = false, int harvestQuantity = 2, bool regrows = false, int maxGrowthStage = 4, int daysPerGrowthStage = 1)
    {
        Plant plant             = new Plant();
        plant.Name              = plantType;
        plant.HarvestItem       = harvestItem;
        plant.RemovalItem       = removalItem;
        plant.AcceptableTiles   = acceptableTiles;
        plant.IsWalkable        = isWalkable;
        plant.HarvestQuantity   = harvestQuantity;
        plant.Regrows           = regrows;
        plant.MaxGrowthStage    = maxGrowthStage;
        plant.daysPerGrowthStage = daysPerGrowthStage;

        return plant;
    }
    #endregion

    static public Plant CreatePlant(Plant proto, Vector3 position, int growthStage = 1, int growthStageTimer = 0)
    {
        Plant plant = proto.Clone();
        plant.Position = position;
        plant.GrowthStage = growthStage;
        plant.growthStageTimer = growthStageTimer;

        return plant;
    }

    public void GetOlder(int days, bool watered)
    {
        growthStageTimer += days;
        if(growthStageTimer >= daysPerGrowthStage && GrowthStage != MaxGrowthStage)
        {
            GrowthStage++;
        }
    }

    public bool ReadyToHarvest()
    {
        if(GrowthStage == 6)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Harvest()
    {
        GrowthStage = MaxGrowthStage / 2;
    }

    public string GetPlantSpriteName()
    {
        string name = null;

        name = Name + GrowthStage;

        return name;
    }

    //public bool IsPlantDead()
    //{
    //    if(daysWithoutWater > survivalTimeWithoutWater)
    //    {
    //        return true;
    //    }
    //    if(Age > Lifespan)
    //    {
    //        return true;
    //    }
    //    return false;
    //}

}
