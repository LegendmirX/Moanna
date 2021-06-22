using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using TMPro;

public class GridUtil<TGridObject>
{
    public const int HeatMap_Max = 100;
    public const int HeatMap_Min = 0;

    public event EventHandler<OnGridObjectChangedArgs> OnGridObjectChanged;
    public class OnGridObjectChangedArgs: EventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    Vector3 gridOffset;
    private TGridObject[,] gridMap;
    private TextMeshPro[,] debugTextArray;

    public GridUtil(int width, int height, float cellSize, Vector3 originPosition, Func<TGridObject> createGridObject)
    {
        this.width          = width;
        this.height         = height;
        this.cellSize       = cellSize;
        this.originPosition = originPosition;

        gridMap         = new TGridObject[width, height];
        debugTextArray  = new TextMeshPro[width, height];

        gridOffset = new Vector3(cellSize, cellSize, 0) * 0.5f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridMap[x, y] = createGridObject();
            }
        }

        bool showDebug = false;
        if(showDebug == true)
        {
            for (int x = 0; x < gridMap.GetLength(0); x++)
            {
                for (int y = 0; y < gridMap.GetLength(1); y++)
                {
                    debugTextArray[x, y] = TextMeshUtil.CreateWorldText(gridMap[x, y]?.ToString(), null, GetWorldPosition(x,y), new Vector3(90,0,0), 3, Color.white, TMPro.TextAlignmentOptions.Center, "Characters");

                    Debug.DrawLine(GetWorldPosition(x, y) - gridOffset, GetWorldPosition(x, y + 1) - gridOffset, Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y) - gridOffset, GetWorldPosition(x + 1, y) - gridOffset, Color.white, 100f);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, height) - gridOffset, GetWorldPosition(width, height) - gridOffset, Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0) - gridOffset, GetWorldPosition(width, height) - gridOffset, Color.white, 100f);

            OnGridObjectChanged += (object sender, OnGridObjectChangedArgs eventArgs) => { debugTextArray[eventArgs.x, eventArgs.y].text = gridMap[eventArgs.x, eventArgs.y]?.ToString(); };
        }
    }

    public void TriggerGridObjectChanged(Vector3 position)
    {
        int x;
        int y;
        GetXY(position, out x, out y);

        TriggerGridObjectChanged(x, y);
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        if(OnGridObjectChanged != null)
        {
            OnGridObjectChanged(this, new OnGridObjectChangedArgs { x = x, y = y });
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y, 0) * cellSize + originPosition;
    }

    public void GetXY(float3 worldPosFloat, out int x, out int y)
    {
        Vector3 worldPosition = new Vector3(worldPosFloat.x, worldPosFloat.y, worldPosFloat.z);
        worldPosition = (worldPosition - originPosition) + gridOffset;
        x = Mathf.FloorToInt(worldPosition.x);
        y = Mathf.FloorToInt(worldPosition.y);
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        worldPosition = (worldPosition - originPosition) + gridOffset;
        x = Mathf.FloorToInt(worldPosition.x );
        y = Mathf.FloorToInt(worldPosition.y );
    }

    public void SetGridObject(int x, int y, TGridObject value)
    {
        if(x >= 0 && y >= 0 && x< width && y < height)
        {
            gridMap[x, y] = value;
            if (OnGridObjectChanged != null)
            {
                OnGridObjectChanged(this, new OnGridObjectChangedArgs { x = x, y = y });
            }
        }
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    //public void AddValue(int x, int z, TGridObject value)
    //{
    //    SetValue(x, z, GetValue(x, z) + value);
    //}

    //public void AddValueSquare(Vector3 worldPosition, TGridObject value, int range)
    //{
    //    GetXZ(worldPosition, out int originX, out int originZ);
    //    for (int x = 0; x < range; x++)
    //    {
    //        for (int z = 0; z < range; z++)
    //        {
    //            AddValue(originX + x, originZ + z, value);
    //        }
    //    }
    //}

    //public void AddValueDiamond(Vector3 worldPosition, TGridObject value, int range)
    //{
    //    GetXZ(worldPosition, out int originX, out int originZ);
    //    for (int x = 0; x < range; x++)
    //    {
    //        for (int z = 0; z < range - x; z++)
    //        {
    //            AddValue(originX + x, originZ + z, value);
    //            if(x != 0)
    //            {
    //                AddValue(originX - x, originZ + z, value);
    //            }
    //            if(z != 0)
    //            {
    //                AddValue(originX + x, originZ - z, value);
    //                if(x != 0)
    //                {
    //                    AddValue(originX - x, originZ - z, value);
    //                }
    //            }
    //        }
    //    }
    //}

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridMap[x, y];
        }
        return default(TGridObject);
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        GetXY(worldPosition, out int x, out int y);
        return GetGridObject(x, y);
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public void RegisterOnGridObjectChangedCallback(Action<int, int> func)
    {
        OnGridObjectChanged += (object sender, OnGridObjectChangedArgs eventArgs) => func(eventArgs.x, eventArgs.y);
    }
}
