using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public interface IPathfindableInterface 
{
    void FollowPath(float deltaTime);
    PathJob SetPathJob(Vector3 destination, Action OnArrive = null );
    void OnPathReceived(object path);
}
