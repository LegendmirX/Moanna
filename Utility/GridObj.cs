using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObj
{
    public enum TileType
    {
        GRASS,
        WATER
    }
    public TileType Type { get; protected set; }
    private bool isWalkable;
    public MapData mapData;

    public GridObj()
    {
        this.isWalkable = true;
        this.Type = TileType.GRASS;
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

    public override string ToString()
    {
        return isWalkable.ToString();
    }
}

