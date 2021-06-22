using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public class PathJob
{
    public Action<object> callBack;

    public int2 startPoint;
    public int2 endPoint;

    public PathJob(Action<object> callBack, int2 startPoint, int2 endPoint)
    {
        this.callBack = callBack;
        this.startPoint = startPoint;
        this.endPoint = endPoint;
    }
}
